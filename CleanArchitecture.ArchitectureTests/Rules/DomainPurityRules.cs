using NetArchTest.Rules;

namespace CleanArchitecture.ArchitectureTests
{
    /// <summary>
    /// Guards domain purity: no EF Core, no file system, no HTTP in domain.
    /// </summary>
    public class DomainPurityRules
    {
        private static readonly System.Reflection.Assembly Domain = typeof(CleanArchitecture.Domain.NamespaceDoc).Assembly;

        [Fact]
        public void Domain_Should_Not_Reference_EFCore_Or_Data_Access()
        {
            var result = Types.InAssembly(Domain)
                .ShouldNot()
                .HaveDependencyOnAny(new[] {
                    "Microsoft.EntityFrameworkCore",
                    "System.Data",
                })
                .GetResult();

            Assert.True(result.IsSuccessful, 
                result.IsSuccessful ? null : 
                "Failing types: " + string.Join(", ", result.FailingTypeNames));
        }

        [Fact]
        public void Domain_Should_Not_Use_FileSystem_Or_Http()
        {
            var result = Types.InAssembly(Domain)
                .ShouldNot()
                .HaveDependencyOnAny(new[] {
                    "System.IO",
                    "System.Net.Http"
                })
                .GetResult();

            Assert.True(result.IsSuccessful, 
                result.IsSuccessful ? null : 
                "Failing types: " + string.Join(", ", result.FailingTypeNames));
        }
    }
}
