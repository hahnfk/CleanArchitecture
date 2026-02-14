using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Presentation.Wpf.ViewModels;
using CleanArchitecture.Presentation.Wpf.Views;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace CleanArchitecture.Presentation.Wpf;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddInfrastructure(ctx.Configuration);
                services.AddApplication();
                services.AddPresentation();
            })
            .Build();

        var sp = _host.Services;

        var window = sp.GetService<MainWindow>() ?? new MainWindow();
        var view = sp.GetRequiredService<MainView>();
        var vm = sp.GetRequiredService<MainViewModel>();

        view.DataContext = vm;
        window.Content = view;
        Current.MainWindow = window;
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync().ConfigureAwait(false);
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
