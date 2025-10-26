using NetArchTest.Rules;

namespace CleanArchitecture.ArchitectureTests
{
    /// <summary>
    /// Enforces Clean Architecture layering rules.
    /// </summary>
    public class LayeringRules
    {
        private static readonly System.Reflection.Assembly Domain = typeof(CleanArchitecture.Domain.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly Application = typeof(CleanArchitecture.Application.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly InfrastructureInMemory = typeof(CleanArchitecture.Infrastructure.InMemory.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly PresentationWpf = typeof(CleanArchitecture.Presentation.Wpf.NamespaceDoc).Assembly;
        private static readonly System.Reflection.Assembly PresentationBlazor = typeof(CleanArchitecture.Presentation.BlazorWebApp.NamespaceDoc).Assembly;

        [Fact]
        public void Domain_Should_Not_Depend_On_Other_Layers()
        {
            var result = Types.InAssembly(Domain)
                .ShouldNot()
                .HaveDependencyOnAny(new[] {
                    "CleanArchitecture.Application",
                    "CleanArchitecture.Infrastructure",
                    "CleanArchitecture.Presentation"
                })
                .GetResult();

            Assert.True(result.IsSuccessful, 
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }

        [Fact]
        public void Application_Should_Not_Depend_On_Infrastructure_Or_Presentation()
        {
            var result = Types.InAssembly(Application)
                .ShouldNot()
                .HaveDependencyOnAny(new[] {
                    "CleanArchitecture.Infrastructure",
                    "CleanArchitecture.Presentation"
                })
                .GetResult();

            Assert.True(result.IsSuccessful, 
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }

        [Fact]
        public void Presentation_Should_Not_Depend_On_Infrastructure()
        {
            var result = Types.InAssemblies(new[] { PresentationWpf, PresentationBlazor })
                .ShouldNot()
                .HaveDependencyOn("CleanArchitecture.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, 
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }

        [Fact]
        public void Infrastructure_Should_Not_Depend_On_Presentation()
        {
            var result = Types.InAssembly(InfrastructureInMemory)
                .ShouldNot()
                .HaveDependencyOnAny(new[] {
                    "CleanArchitecture.Presentation"
                })
                .GetResult();

            Assert.True(result.IsSuccessful, 
                result.FailingTypeNames != null && result.FailingTypeNames.Any()
                    ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}"
                    : null);
        }
    }
}
