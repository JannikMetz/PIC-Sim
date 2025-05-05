
using System;
using Avalonia.Controls;
using Avalonia.Input;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Text.RegularExpressions;
using Avalonia.Interactivity;

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

    private void HexTextBox_TextInput(object? sender, TextInputEventArgs e)
    {
        if (!Regex.IsMatch(e.Text, "^[0-9a-fA-F]+$"))
        {
            e.Handled = true;
        }
    }
    
    private void HexTextBox_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }


    private void HexTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string input = textBox.Text?.Trim() ?? "";

            if (!Regex.IsMatch(input, "^[0-9a-fA-F]{1,2}$"))
            {
                textBox.Text = "00";
            }
            else
            {
                textBox.Text = input.PadLeft(2, '0').ToUpperInvariant();
            }
        }
    }
    private void HexTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox tb)
        {
            FocusManager.ClearFocus();
            e.Handled = true;
        }
    }
}