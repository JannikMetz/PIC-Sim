using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PicSimulator.Models;
using PicSimulator.ViewModels;

namespace PicSimulator.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Fields
    
    private Encode _encode;
    private Memory _memory;
    private ALU _alu;
    private string _fileContent;
    private ObservableCollection<ProgramLine> _programLines;
    
    #endregion

    #region Properties

    public ObservableCollection<ProgramLine> ProgramLines
    {
        get => _programLines;
        set
        {
            if (_programLines != value)
            {
                _programLines = value;
                OnPropertyChanged();
            }
        }
    }
    
    public int WReg
    {
        get { return _memory.WReg; }
        set
        {
            if (_memory.WReg != value)
            {
                _memory.WReg = value;
                OnPropertyChanged();
            }
        }
    }
    public int ProgramCounter
    {
        get { return _memory.ProgramCounter; }
        set
        {
            if (_memory.ProgramCounter != value)
            {
                _memory.ProgramCounter = value;
                OnPropertyChanged();
            }
        }
    }
    
    // This is the content of the file as a string
    public string FileContent
    {
        get { return _fileContent; }
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
    public ICommand StartCommand { get; }
    public ICommand TestCommand { get; }

    #endregion


    public MainWindowViewModel()
    {
        
        _memory = new Memory();
        _memory.ProgramCounterChanged += OnProgramCounterChanged;
        _encode = new Encode(_memory);
        _alu = new ALU(_memory);
        LoadCommand = new RelayCommand(Load);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
        StartCommand = new RelayCommand(Start);
        TestCommand = new RelayCommand(Test);
    }

    private async void Load(object parameter)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select a file",
            AllowMultiple = false
        };

        var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;
        if (mainWindow != null)
        {
            var result = await openFileDialog.ShowAsync(mainWindow);

            if (result != null && result.Length > 0)
            {
                FileContent = _encode.ReadFile(result[0]);
                ProgramLines= _encode.ExtractOpcodes(_fileContent);
            }
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
    private void Start(object parameter)
    {
        if (_fileContent == null || _fileContent == string.Empty)
        {
            Console.WriteLine("No file loaded");
            return;
        }
        Console.WriteLine("Start command executed");
        // Start the Simulator in a new thread
        Thread aluThread = new Thread(() =>
        {
            _alu.Start(); 
        });

        aluThread.IsBackground = true;
        aluThread.Start();
    }
    

    private void Test(object parameter)
    {
        _encode.ExtractOpcodes(_fileContent);
    }
    
    private void OnProgramCounterChanged()
    {
        HighlightCurrentLine();
    }
    
    private void HighlightCurrentLine()
    {
        foreach (var line in ProgramLines)
        {
            line.IsHighlighted = (line.LineNumber == _encode.OpcodeLines[_memory.ProgramCounter]);
        }
    }
}