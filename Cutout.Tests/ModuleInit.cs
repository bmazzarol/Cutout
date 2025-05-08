using System.Runtime.CompilerServices;

namespace Cutout.Tests;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
