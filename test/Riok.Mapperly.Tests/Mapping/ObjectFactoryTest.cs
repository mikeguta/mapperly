using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectFactoryTest
{
    [Fact]
    public void ShouldUseSimpleObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new B();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryForMultipleMaps()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new B();" + "partial B Map(A a);" + "partial B Map2(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodCount(2)
            .AllMethodsHaveBody(
                """
                var target = CreateB();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryWithSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB(A source) => new B(source.IntValue);"
                + "[MapperIgnoreSource(nameof(A.IntValue))] partial B Map(A a);",
            "class A { public int IntValue { get; set; } public string StringValue { get; set; } }",
            "class B { private readonly int _intValue; public B(int v) => _intValue = v; public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB(a);
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericTargetObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<T>() where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::B>();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericTargetObjectFactoryWithSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<T>(A source) where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::B>(a);
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericSourceTargetObjectFactoryTargetFirst()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<T, S>(S source) where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::B, global::A>(a);
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericSourceTargetObjectFactorySourceFirst()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<S, T>(S source) where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::A, global::B>(a);
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericSourceObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B Create<S>(S source) => new B();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::A>(a);
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseFirstMatchingObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] A CreateA() => new A();"
                + "[ObjectFactory] B CreateB() => new B();"
                + "[ObjectFactory] T Create<T>() where T : new() => new T();"
                + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericObjectFactoryWithTypeConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] private T CreateA<T>() where T : new(), A => new T();"
                + "[ObjectFactory] private T CreateStruct<T>() where T : new(), struct => new T();"
                + "[ObjectFactory] private T CreateB<T>() where T : new(), B => new T();"
                + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB<global::B>();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryIfTypeIsNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new B();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "#nullable disable\nclass B { public string StringValue { get; set; } }\n#nullable restore"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryAndCreateObjectIfNullIsReturned()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B? CreateB() => null;" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB() ?? new global::B();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryAndThrowIfNoParameterlessCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B? CreateB() => null;" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { private B() {} public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB() ?? throw new System.NullReferenceException("The object factory CreateB returned null");
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericObjectFactoryAndCreateObjectIfNullIsReturned()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T? Create<T>() where T : notnull => null;" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::B>() ?? new global::B();
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseGenericObjectFactoryAndThrowIfNoParameterlessCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T? Create<T>() where T : notnull => null;" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { private B() {} public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = Create<global::B>() ?? throw new System.NullReferenceException("The object factory Create returned null");
                target.StringValue = a.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void InvalidSignatureAsync()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] async Task<T> Create<T>() where T : new() => Task.FromResult(new T());" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB(int i, int j) => new B();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureVoid()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] void CreateB() => 0" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureMultipleTypeParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<T, T2>() where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureTypeParameterNotReturnType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB<T>() => new B();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureTooManyTypeParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T CreateB<S, T, T2>(S source) where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureTooManyTypeParametersSourceNotTypeParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T CreateB<S, T>(A source) where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureTooManyTypeParametersTargetNotTypeParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB<S, T>(S source) => new B();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureGenericSourceTargetSameTypeParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T CreateB<T>(T source) where T : new() => new T();" + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }
}
