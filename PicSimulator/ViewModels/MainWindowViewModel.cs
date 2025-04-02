using System;
using System.Windows.Input;
using Avalonia.Input;
namespace PicSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }

    public MainWindowViewModel()
    {
        LoadCommand = new RelayCommand(Load);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
    }

    private void Load(object parameter)
    {
        Console.WriteLine("Load command executed");
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