namespace CleanArchitecture.Presentation.Wpf;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddTransient<Views.TodosView>();
        services.AddTransient<ViewModels.TodosViewModel>();
        services.AddSingleton<MainWindow>();
        return services;
    }
}
