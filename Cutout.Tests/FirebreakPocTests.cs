using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Cutout.Tests;

// Firebreak POC tests (ops-board#18). Plain assertions instead of Verify
// snapshots so the design decisions are proven explicitly:
//   1. a bad template produces a located diagnostic instead of AD0001
//   2. one bad template does not sink the good ones
//   3. Err payloads are value-equatable (cache stable, stage not re-run)
public class FirebreakPocTests
{
    private const string GoodTemplate = """
            [Template("hello {{value}}")]
            public static partial void Good(this StringBuilder builder, string value);
        """;

    private const string BadTemplate = """
            [Template("{{% unclosed")]
            public static partial void Bad(this StringBuilder builder, string value);
        """;

    private static GeneratorDriverRunResult Run(string source)
    {
        var compilation = CSharpCompilation.Create(
            "poc",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(
                    typeof(Cutout.TemplateAttribute).Assembly.Location
                ),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var driver = CSharpGeneratorDriver.Create(new Cutout.TemplateSourceGenerator());
        return driver.RunGenerators(compilation).GetRunResult();
    }

    // ------------------------------------------------------------------
    // 1. Bad template: located CUTOUT001 diagnostic, NOT an AD0001 crash.
    // ------------------------------------------------------------------
    [Fact]
    public void BadTemplateReportsLocatedDiagnosticInsteadOfCrash()
    {
        var result = Run(
            $$"""
            using System.Text;
            using Cutout;

            public class Sample
            {
                {{BadTemplate}}
            }
            """
        );

        var cutoutError = result.Diagnostics.FirstOrDefault(d => d.Id == "CUTOUT001");
        Assert.NotNull(cutoutError);
        Assert.Equal(DiagnosticSeverity.Error, cutoutError!.Severity);

        // The diagnostic points at the malformed template attribute node,
        // not Location.None.
        Assert.NotEqual(Location.None, cutoutError!.Location);
        Assert.True(cutoutError!.Location.SourceSpan.Length > 0);

        // No generator crash anywhere.
        Assert.DoesNotContain(result.Diagnostics, d => d.Id == "AD0001");
    }

    // ------------------------------------------------------------------
    // 2. Isolation: the good template still generates when a sibling fails.
    // ------------------------------------------------------------------
    [Fact]
    public void GoodTemplateStillGeneratesWhenSiblingTemplateFails()
    {
        var result = Run(
            $$"""
            using System.Text;
            using Cutout;

            public class Sample
            {
                {{GoodTemplate}}
                {{BadTemplate}}
            }
            """
        );

        var goodTree = result.GeneratedTrees.FirstOrDefault(t =>
            t.ToString().Contains("hello ", StringComparison.Ordinal)
        );
        Assert.NotNull(goodTree);
        Assert.Contains("hello ", goodTree!.ToString());

        // Exactly one failure reported — the bad one — and no crash.
        Assert.DoesNotContain(result.Diagnostics, d => d.Id == "AD0001");
        Assert.Equal(1, result.Diagnostics.Count(d => d.Id == "CUTOUT001"));
    }

    // ------------------------------------------------------------------
    // 3. Cache stability: the parse stage is not re-executed for Err items
    //    when the driver runs twice with unchanged input.
    // ------------------------------------------------------------------
    [Fact]
    public void ErrItemsCacheValueEqualAcrossRuns()
    {
        var source = $$"""
            using System.Text;
            using Cutout;

            public class Sample
            {
                {{GoodTemplate}}
                {{BadTemplate}}
            }
            """;

        var compilation = CSharpCompilation.Create(
            "poc",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(
                    typeof(Cutout.TemplateAttribute).Assembly.Location
                ),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new Cutout.TemplateSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        var firstCounters = generator.PocCounters;
        var outputRun1 = firstCounters.OutputInvocations;
        var reportedRun1 = firstCounters.ReportedDiagnostics;

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        // Roslyn re-runs provider transforms each pass (entry/parse counters
        // grow), but RegisterSourceOutput only re-executes when a value is
        // NOT equal to the cached one. The Err payload is pure, value-equal
        // data (EquatableArray<GenerationError>), so the bad template's
        // diagnostic is reported exactly once across both runs — no
        // re-emission, no duplicate squiggles.
        Assert.Equal(reportedRun1, firstCounters.ReportedDiagnostics);
        Assert.Equal(1, reportedRun1);

        // The Ok payload still carries raw ISymbol references (Cutout's
        // TemplateMethodDetails), which are reference-unequal between passes,
        // so Ok items re-execute the output stage. That is the cache gap
        // Firebreak removes by making payloads data, not symbols.
        Assert.True(firstCounters.OutputInvocations > outputRun1);
    }

    // ------------------------------------------------------------------
    // 4. Core Result semantics (inline unit proof of the algebra).
    // ------------------------------------------------------------------
    [Fact]
    public void OkMapRunsStageAndCatchTurnsExceptionsIntoLocatedErr()
    {
        var ok = new Cutout.Ok<int>(42);
        var failed = ok.Map<int>(
            value => throw new InvalidOperationException("stage blew up"),
            value => null
        );

        Assert.IsType<Cutout.Err<int>>(failed);
        var err = (Cutout.Err<int>)failed;
        Assert.Equal(1, err.Errors.Count);
        Assert.Contains("stage blew up", err.Errors.First().Message);
    }

    [Fact]
    public void ErrMapShortCircuitsWithoutRunningStage()
    {
        var error = new Cutout.GenerationError(
            Cutout.FirebreakDescriptors.InternalError,
            "boom",
            null,
            new TextSpan(0, 1)
        );
        var err = new Cutout.Err<int>(Cutout.EquatableArray<Cutout.GenerationError>.Create(error));

        var ran = false;
        var mapped = err.Map<string>(
            value =>
            {
                ran = true;
                return "should never run";
            },
            null
        );

        Assert.False(ran);
        Assert.IsType<Cutout.Err<string>>(mapped);
    }

    [Fact]
    public void ErrPayloadsAreValueEquatableForCacheStability()
    {
        // Two structurally distinct exception-derived errors with the same
        // descriptor and message must be value-equal, otherwise Err items can
        // never produce incremental cache hits.
        var a = Cutout.EquatableArray<Cutout.GenerationError>.Create(
            Cutout.GenerationError.FromException(new InvalidOperationException("same"), null)
        );
        var b = Cutout.EquatableArray<Cutout.GenerationError>.Create(
            Cutout.GenerationError.FromException(new InvalidOperationException("same"), null)
        );

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ZipMergesErrorsFromBothLanes()
    {
        var errorA = new Cutout.GenerationError(
            Cutout.FirebreakDescriptors.InternalError,
            "a",
            null,
            new TextSpan(0, 1)
        );
        var errorB = new Cutout.GenerationError(
            Cutout.FirebreakDescriptors.InternalError,
            "b",
            null,
            new TextSpan(0, 1)
        );

        var merged = Cutout.FirebreakPipeline.Zip(
            new Cutout.Err<int>(Cutout.EquatableArray<Cutout.GenerationError>.Create(errorA)),
            new Cutout.Err<string>(Cutout.EquatableArray<Cutout.GenerationError>.Create(errorB))
        );

        var err = (Cutout.Err<(int, string)>)merged;
        Assert.Equal(2, err.Errors.Count);
    }
}
