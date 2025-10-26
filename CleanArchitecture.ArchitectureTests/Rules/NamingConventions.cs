using NetArchTest.Rules;
using Xunit;

namespace CleanArchitecture.ArchitectureTests
{
    /// <summary>
    /// Simple naming convention rules (adjust as needed).
    /// </summary>
    public class NamingConventions
    {
        private static readonly System.Reflection.Assembly Application = typeof(CleanArchitecture.Application.NamespaceDoc).Assembly;

        [Fact]
        public void UseCases_Should_End_With_UseCase()
        {
            var result = Types.InAssembly(Application)
                .That().HaveNameEndingWith("UseCase")
                .Or().HaveNameEndingWith("Query")
                .Or().HaveNameEndingWith("Command")
                .Should().ResideInNamespace("CleanArchitecture.Application")
                .GetResult();

            Assert.True(result.IsSuccessful, result.FailingTypeNames != null && result.FailingTypeNames.Any() ? $"Failing types: {string.Join(", ", result.FailingTypeNames)}" : null);
        }
    }
}
