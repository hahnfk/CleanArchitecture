using CleanArchitecture.Presentation.Wpf.ViewModels;
using CleanArchitecture.Presentation.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace CleanArchitecture.Presentation.Wpf;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(s => s.AddCleanArchitectureApp())
            .Build();

        var sp = _host.Services;

        var window = sp.GetService<MainWindow>() ?? new MainWindow();

        var view = sp.GetRequiredService<TodosView>();
        var vm = sp.GetRequiredService<TodosViewModel>();

        view.DataContext = vm;
        window.Content = view;

        Current.MainWindow = window;
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
