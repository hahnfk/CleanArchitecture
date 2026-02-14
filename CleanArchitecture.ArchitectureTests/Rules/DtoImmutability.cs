using System.Reflection;

namespace CleanArchitecture.ArchitectureTests
{
    /// <summary>
    /// Example "fitness function" without external libs:
    /// DTOs must be immutable (get-only properties).
    /// </summary>
    public class DtoImmutability
    {
        private static readonly Assembly Application = typeof(CleanArchitecture.Application.NamespaceDoc).Assembly;

        [Fact]
        public void Dtos_Should_Be_Immutable_When_Named_Dto()
        {
            var types = Application
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace != null && t.Name.EndsWith("Dto"));

            var offenders = types.Where(t =>
                t.GetProperties().Any(p => p.SetMethod != null && p.SetMethod.IsPublic
                    && !p.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                        .Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit")));

            Assert.False(offenders.Any(), "Mutable DTOs detected: " + string.Join(", ", offenders.Select(o => o.FullName)));
        }
    }
}
