
using System;
using Avalonia.Controls;

using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

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
    
    public async void ErrorMessageBox(int errorcode)
    {

        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Errorcode is:" + errorcode.ToString(), 
            ButtonEnum.Ok);
        
        switch (errorcode)
        {
            case 0: box = MessageBoxManager.GetMessageBoxStandard("Error", "No File Loaded. Please load a file first.", 
                        ButtonEnum.Ok); break;
            case 1: box = MessageBoxManager.GetMessageBoxStandard("Error", "File not found. Please load a file first.",
                        ButtonEnum.Ok); break;
        }
        // Show the message box
        var result = await box.ShowAsync();

    }
}