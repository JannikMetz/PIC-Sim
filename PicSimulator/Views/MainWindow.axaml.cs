
using System;
using Avalonia.Controls;

namespace PicSimulator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void OnOpenWebsiteClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {           
        const string url = "https://github.com/JannikMetz/PIC-Sim/wiki"; // Ersetzen Sie dies durch Ihre Dokumentations-URL
        var launcher = TopLevel.GetTopLevel(this)?.Launcher;
        
        // Null propagation: If launcher is null, the method will not be called
            launcher?.LaunchUriAsync(new Uri(url));
    }
}