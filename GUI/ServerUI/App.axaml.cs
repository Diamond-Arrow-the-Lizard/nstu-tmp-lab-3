using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Lab3.Interfaces;
using Lab3.Interfaces.Handlers;
using Lab3.Models.Handlers;
using Lab3.Models.Services;
using ServerUI.ViewModels;
using ServerUI.Views;
using Microsoft.Extensions.DependencyInjection;
using ServerUI.Models;

namespace ServerUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection = ProvideServices();

        var services = collection.BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new ServerView
            {
                DataContext = services.GetRequiredService<ServerViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private static ServiceCollection ProvideServices()
    {
        var services = new ServiceCollection();
        
        int defaultPort = 5000;
        
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IDiskService, DiskService>();
        services.AddSingleton<ServerHandler>(_ => new ServerHandler(defaultPort));
        services.AddSingleton<ServerViewModel>();
        return services;
    }
}