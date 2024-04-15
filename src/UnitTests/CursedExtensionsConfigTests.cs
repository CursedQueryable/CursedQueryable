using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.Extensions;
using CursedQueryable.Options;
using CursedQueryable.UnitTests.Fixtures;
using FluentAssertions;
using Moq;
using Xunit;

namespace CursedQueryable.UnitTests;

[Trait("Category", "Cursed Framework - Unit Tests")]
[Collection(nameof(StaticCollection))]
public sealed class CursedExtensionsConfigTests : IDisposable
{
    public CursedExtensionsConfigTests()
    {
        ResetGlobalOptions();
    }

    public void Dispose()
    {
        ResetGlobalOptions();

        // Force collection for any dynamic assemblies
        GC.Collect();
    }

    [Fact]
    public void Default_configuration()
    {
        CursedExtensionsConfig.Get().Provider.Should().BeOfType<NoEntityDescriptorProvider>();
        CursedExtensionsConfig.Get().NullBehaviour.Should().Be(NullBehaviour.SmallerThanNonNullable);
    }

    [Fact]
    public void Manual_configuration()
    {
        var mockProvider = new Mock<IEntityDescriptorProvider>();

        CursedExtensionsConfig.Configure(o =>
        {
            o.Provider = mockProvider.Object;
            o.NullBehaviour = NullBehaviour.LargerThanNonNullable;
        });

        CursedExtensionsConfig.Get().Provider.Should().Be(mockProvider.Object);
        CursedExtensionsConfig.Get().NullBehaviour.Should().Be(NullBehaviour.LargerThanNonNullable);
    }

    [Fact]
    public void Automatic_configuration()
    {
        GenerateDynamicAssemblyWithConfigurator("CursedQueryable.TestDynamic");

        CursedExtensionsConfig.Get().Provider.Should().BeOfType<DynamicProvider>();
    }

    private static void ResetGlobalOptions()
    {
        // Force the Cached static options back to null to ensure automatic behaviour
        var field = typeof(CursedExtensionsConfig).GetField("Cached", BindingFlags.Static | BindingFlags.NonPublic)!;
        field.SetValue(null, null);
    }

    private static void GenerateDynamicAssemblyWithConfigurator(string assemblyNameStr)
    {
        var assemblyName = new AssemblyName(assemblyNameStr);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name!);

        var interfaceMethod = typeof(ICursedConfigurator).GetMethod(nameof(ICursedConfigurator.Configure))!;
        var typeBuilder = moduleBuilder.DefineType("DynamicConfigurator", TypeAttributes.Public);

        typeBuilder.AddInterfaceImplementation(interfaceMethod.DeclaringType!);

        var methodBuilder = typeBuilder.DefineMethod(interfaceMethod.Name,
            MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual |
            MethodAttributes.NewSlot,
            CallingConventions.Standard,
            interfaceMethod.ReturnType,
            null);

        var il = methodBuilder.GetILGenerator();
        var local = il.DeclareLocal(typeof(FrameworkOptions));

        il.Emit(OpCodes.Newobj, typeof(FrameworkOptions).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Newobj, typeof(DynamicProvider).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Callvirt, typeof(FrameworkOptions).GetProperty(nameof(FrameworkOptions.Provider))!.SetMethod!);
        il.Emit(OpCodes.Stloc, local);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        typeBuilder.CreateType();
    }

    // ReSharper disable once MemberCanBePrivate.Global - needs to be public for dynamic assembly binding
    public class DynamicProvider : IEntityDescriptorProvider
    {
        public bool TryGetEntityDescriptor(Expression expression, out IEntityDescriptor entityDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}