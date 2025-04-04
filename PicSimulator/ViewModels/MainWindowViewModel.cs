using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using PicSimulator.Models;

namespace PicSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Fields
    
    private string _fileContent;
    private FileReader _fileReader;
    
    #endregion
    
    #region Properties
    
    public string FileContent
    {
        get {return _fileContent;}
        set
        {
            if (_fileContent != value)
            {
                _fileContent = value;
                OnPropertyChanged();
            }
        }
    }
    
    #endregion
    
    #region Commands

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    
    #endregion
    

    public MainWindowViewModel()
    {
        _fileReader = new FileReader();
        LoadCommand = new RelayCommand(Load);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
    }

    private async void Load(object parameter)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select a file",
            AllowMultiple = false
        };

        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var result = await openFileDialog.ShowAsync(mainWindow);

        if (result != null && result.Length > 0)
        {
            FileContent = _fileReader.ReadFile(result[0]);
        }
    }

    private void Save(object parameter)
    {
        Console.WriteLine("Save command executed");
    }

    private void SaveAs(object parameter)
    {
        Console.WriteLine("Save as command executed");
    }
}