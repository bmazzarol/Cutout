using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cutout;

// ============================================================================
// Firebreak POC (ops-board#18): failures as values, never as generator crashes.
// Types inline on the firebreak-poc branch purely to prove the design against
// a real incremental generator (Cutout's TemplateSourceGenerator).
//
// Design decisions being proven here:
//   1. Every pipeline stage returns Result<T>; exceptions are captured at the
//      stage boundary, never allowed to escape to Roslyn (no AD0001, no
//      silently-dropped output for unrelated templates).
//   2. Err payloads are value-equatable (EquatableArray<GenerationError>) so
//      the incremental cache stays stable across driver runs.
//   3. Diagnostics carry precise syntax-node locations, not Location.None.
//   4. Descriptors are static singletons, never constructed per failure.
// ============================================================================

/// <summary>
/// Either an <see cref="Ok{T}"/> value or an <see cref="Err{T}"/> with located errors.
/// Implicit conversion from a plain value keeps pipeline code ceremony-free.
/// </summary>
public abstract record Result<T>
{
    /// <summary>Wraps a successful value.</summary>
    public static implicit operator Result<T>(T value) => new Ok<T>(value);

    /// <summary>Wraps a single located failure.</summary>
    public static implicit operator Result<T>(GenerationError error) => new Err<T>(EquatableArray<GenerationError>.Create(error));

    /// <summary>
    /// Runs the stage on the value, or passes the failure through untouched:
    /// one failed item never re-executes a stage, and its error payload caches
    /// as value-equal data.
    /// </summary>
    /// <param name="selector">The stage logic.</param>
    /// <param name="originSelector">Gives the syntax node a failure should point at.</param>
    /// <returns>An <see cref="Ok{U}"/> result, or a located <see cref="Err{U}"/>.</returns>
    public abstract Result<U> Map<U>(Func<T, U> selector, Func<T, SyntaxNode>? originSelector = null);

}

/// <summary>Pipeline combinators over <see cref="Result{T}"/> values.</summary>
public static class FirebreakPipeline
{
    /// <summary>
    /// Combines two independent lanes. Both Ok gives the pair; any Err(s) merge
    /// their diagnostics so each lane reports all of its own failures.
    /// </summary>
    /// <param name="left">First lane's result.</param>
    /// <param name="right">Second lane's result.</param>
    /// <returns>The zipped pair, or the merged errors.</returns>
    public static Result<(A, B)> Zip<A, B>(Result<A> left, Result<B> right)
    {
        if (left is Ok<A> a && right is Ok<B> b)
        {
            return new Ok<(A, B)>((a.Value, b.Value));
        }

        var errors = new List<GenerationError>();
        if (left is Err<A> leftError) errors.AddRange(leftError.Errors);
        if (right is Err<B> rightError) errors.AddRange(rightError.Errors);
        return new Err<(A, B)>(EquatableArray<GenerationError>.Create(errors));
    }
}

/// <summary>A successful pipeline value.</summary>
public sealed record Ok<T>(T Value) : Result<T>
{
    /// <inheritdoc />
    public override Result<U> Map<U>(Func<T, U> selector, Func<T, SyntaxNode>? originSelector = null)
    {
        try
        {
            return new Ok<U>(selector(Value));
        }
        catch (LocatedFailureException located)
        {
            // Explicit, already-located pipeline failure (e.g. bad template syntax).
            return new Err<U>(EquatableArray<GenerationError>.Create(located.Error));
        }
        catch (Exception ex)
        {
            var origin = originSelector == null ? null : originSelector(Value);
            return new Err<U>(EquatableArray<GenerationError>.Create(GenerationError.FromException(ex, origin)));
        }
    }
}

/// <summary>A failed pipeline item carrying located diagnostics.</summary>
public sealed record Err<T>(EquatableArray<GenerationError> Errors) : Result<T>
{
    /// <inheritdoc />
    public override Result<U> Map<U>(Func<T, U> selector, Func<T, SyntaxNode>? originSelector = null)
    {
        return new Err<U>(Errors);
    }
}

/// <summary>
/// A failure that is fully describable as data: value-equatable, cache-stable,
/// and convertible to a precisely located Roslyn diagnostic on demand.
/// </summary>
/// The SyntaxTree reference is the cheapest way to reconstruct a precise
/// location later: it is reference-stable across driver runs, so the record
/// stays value-equal for caching while the squiggle survives.
public sealed record GenerationError(
    DiagnosticDescriptor Descriptor,
    string Message,
    SyntaxTree? Tree,
    TextSpan Span)
{
    /// <summary>Builds a failure located at a concrete syntax node.</summary>
    /// <param name="descriptor">The static descriptor to report.</param>
    /// <param name="message">Human-readable failure message.</param>
    /// <param name="node">The syntax node to squiggle.</param>
    /// <returns>The located error.</returns>
    public static GenerationError At(DiagnosticDescriptor descriptor, string message, SyntaxNode node)
    {
        return new GenerationError(descriptor, message, node.SyntaxTree, node.Span);
    }

    /// <summary>Turns any exception into a value-equal, located failure.</summary>
    /// <param name="exception">The captured exception.</param>
    /// <param name="origin">The node the failing stage was working on.</param>
    /// <returns>The error as data.</returns>
    public static GenerationError FromException(Exception exception, SyntaxNode? origin)
    {
        if (origin == null)
        {
            return new GenerationError(FirebreakDescriptors.InternalError, exception.ToString(), null, default);
        }

        return At(FirebreakDescriptors.InternalError, exception.Message, origin);
    }

    /// <summary>Materialises the stored failure as a Roslyn diagnostic.</summary>
    /// <returns>A precise, located diagnostic.</returns>
    public Diagnostic ToDiagnostic()
    {
        var location = Tree == null ? Location.None : Tree.GetLocation(Span);
        return Diagnostic.Create(Descriptor, location, Message);
    }
}

/// <summary>Static descriptors — never constructed per failure (cache stability).</summary>
public static class FirebreakDescriptors
{
    // POC: production repo would ship a release-tracking file for RS2008.
    #pragma warning disable RS2008

    /// <summary>Reported when a template body cannot be parsed.</summary>
    public static readonly DiagnosticDescriptor TemplateParseError = new DiagnosticDescriptor(
        "CUTOUT001",
        "Template parsing error",
        "Cutout failed to parse the template: {0}",
        "Cutout",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Reported when an unexpected stage exception is captured.</summary>
    public static readonly DiagnosticDescriptor InternalError = new DiagnosticDescriptor(
        "CUTOUT999",
        "Cutout internal error",
        "Cutout caught an internal failure and kept the rest of generation alive: {0}",
        "Cutout",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    #pragma warning restore RS2008
}

/// <summary>
/// Value-equatable array wrapper: ImmutableArray itself does not provide
/// structural equality, so Err payloads would never cache-hit without this.
/// Hash is computed once at construction — the items never change.
/// </summary>
public sealed class EquatableArray<T> : IEnumerable<T>, IEquatable<EquatableArray<T>> where T : class
{
    private readonly ImmutableArray<T> _items;
    private readonly int _hashCode;

    /// <summary>Creates a value-equatable array and precomputes its hash.</summary>
    /// <param name="items">The immutable items.</param>
    public EquatableArray(ImmutableArray<T> items)
    {
        _items = items;
        var hash = 0;
        foreach (var item in items)
        {
            hash = hash * 17 + (item == null ? 0 : item.GetHashCode());
        }
        _hashCode = hash;
    }

    /// <summary>Convenience factory from a parameter list.</summary>
    /// <param name="items">The items.</param>
    /// <returns>The value-equatable array.</returns>
    public static EquatableArray<T> Create(params T[] items) => new EquatableArray<T>(items.ToImmutableArray());

    /// <summary>Convenience factory from any enumerable.</summary>
    /// <param name="items">The items.</param>
    /// <returns>The value-equatable array.</returns>
    public static EquatableArray<T> Create(IEnumerable<T> items) => new EquatableArray<T>(items.ToImmutableArray());

    /// <summary>Number of items.</summary>
    public int Count => _items.Length;

    /// <summary>Element-wise structural equality — the key to cache hits on Err payloads.</summary>
    /// <param name="other">The array to compare with.</param>
    /// <returns>True when every element is equal.</returns>
    public bool Equals(EquatableArray<T> other)
    {
        if (other == null) return false;
        if (_items.Length != other._items.Length) return false;
        for (var i = 0; i < _items.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_items[i], other._items[i])) return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Pipeline-local failure that already carries its located diagnostic payload.
/// Map surfaces it directly as an Err instead of treating it as an internal bug.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1194:Implement exception constructors")]
public sealed class LocatedFailureException : Exception
{
    /// <summary>The located failure to report.</summary>
    public GenerationError Error { get; }

    /// <summary>Creates a failure from an already-located error.</summary>
    /// <param name="error">The located error.</param>
    public LocatedFailureException(GenerationError error) : base(error.Message)
    {
        Error = error;
    }

    /// <summary>Creates a failure from an already-located error and its cause.</summary>
    /// <param name="error">The located error.</param>
    /// <param name="cause">The underlying exception.</param>
    public LocatedFailureException(GenerationError error, Exception cause) : base(error.Message, cause)
    {
        Error = error;
    }

    /// <summary>Creates a generic internal failure from a message.</summary>
    /// <param name="message">The failure message.</param>
    public LocatedFailureException(string message) : base(message)
    {
        Error = new GenerationError(FirebreakDescriptors.InternalError, message, null, default);
    }

    /// <summary>Creates a generic internal failure from a message and cause.</summary>
    /// <param name="message">The failure message.</param>
    /// <param name="cause">The underlying exception.</param>
    public LocatedFailureException(string message, Exception cause) : base(message, cause)
    {
        Error = new GenerationError(FirebreakDescriptors.InternalError, message, null, default);
    }
}

/// <summary>
/// POC instrumentation: counts stage invocations per generator instance so the
/// tests can PROVE that Err items are cached (no re-execution) and good items
/// are isolated from bad ones.
/// </summary>
public class FirebreakPocCounters
{
    /// <summary>Times the entry stage (attribute discovery) executed.</summary>
    public int EntryInvocations { get; set; }

    /// <summary>Times the parse stage executed for a non-failed item.</summary>
    public int ParseInvocations { get; set; }

    /// <summary>Times the output stage produced generated source.</summary>
    public int OutputInvocations { get; set; }

    /// <summary>Total diagnostics reported from captured failures.</summary>
    public int ReportedDiagnostics { get; set; }
}
