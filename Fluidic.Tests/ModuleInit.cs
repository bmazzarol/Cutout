using System.Runtime.CompilerServices;

namespace Fluidic.Tests;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
