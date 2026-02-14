using NetArchTest.Rules;

namespace CleanArchitecture.ArchitectureTests.Rules
{
    /// <summary>
    /// Enforces Clean Architecture layering rules.
    /// </summary>
    public class LayeringRules
    {
        private static readonly System.Reflection.Assembly _domain = typeof(Domain.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly _application = typeof(Application.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly _infrastructureInMemory = typeof(Infrastructure.InMemory.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly _presentationWpf = typeof(Presentation.Wpf.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly _presentationBlazor = typeof(Presentation.BlazorWebApp.NamespaceDoc).Assembly;

        [Fact]
        public void Domain_Should_Not_Depend_On_Other_Layers()
        {
            var result = Types.InAssembly(_domain)
                .ShouldNot()
                .HaveDependencyOnAny([
                    "CleanArchitecture.Application",
                    "CleanArchitecture.Infrastructure",
                    "CleanArchitecture.Presentation"
                ])
                .GetResult();

            Assert.True(result.IsSuccessful,
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }

        [Fact]
        public void Application_Should_Not_Depend_On_Infrastructure_Or_Presentation()
        {
            var result = Types.InAssembly(_application)
                .ShouldNot()
                .HaveDependencyOnAny([
                    "CleanArchitecture.Infrastructure",
                    "CleanArchitecture.Presentation"
                ])
                .GetResult();

            Assert.True(result.IsSuccessful,
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }

        [Fact]
        public void PresentationShouldNotDependOnInfrastructure()
        {
            // Presentation may reference the Composition root for DI wiring,
            // but must not depend on concrete infrastructure implementations.
            var result = Types.InAssemblies([_presentationWpf, _presentationBlazor])
                .ShouldNot()
                .HaveDependencyOnAny([
                    "CleanArchitecture.Infrastructure.InMemory",
                    "CleanArchitecture.Infrastructure.EfCore"
                ])
                .GetResult();

            Assert.True(result.IsSuccessful,
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }

        [Fact]
        public void Infrastructure_Should_Not_Depend_On_Presentation()
        {
            var result = Types.InAssembly(_infrastructureInMemory)
                .ShouldNot()
                .HaveDependencyOnAny([
                    "CleanArchitecture.Presentation"
                ])
                .GetResult();

            Assert.True(result.IsSuccessful,
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }
    }
}
