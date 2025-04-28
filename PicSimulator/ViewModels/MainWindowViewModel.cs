using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PicSimulator.Models;
using PicSimulator.ViewModels;
using System.Collections.Generic;

using PicSimulator.Views;

namespace PicSimulator.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Fields
    
    private Encode _encode;
    private Memory _memory;
    private ALU _alu;
    private string _fileContent;
    private ObservableCollection<ProgramLine> _programLines;
    
    private MainWindow _mainWindow;

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
    
    public ObservableCollection<string> ColumnHeaders { get; set; } = new ObservableCollection<string>
    {
        "00", "01", "02", "03", "04", "05", "06", "07"
    };

    public ObservableCollection<string> RowHeaders { get; set; } = new ObservableCollection<string>
    {
        "00", "08", "10", "18", "20", "28", "30", "38", "40", "48", "50", "58", "60", "68", "70", "78", "80", "88", "90", "98", "A0", "A8", "B0", "B8", "C0", "C8", "D0", "D8", "E0", "E8", "F0", "F8"
    };

    public ObservableCollection<ObservableCollection<Register>> ObservableMemoryArray { get; private set; }
    
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
        _mainWindow = new MainWindow();
        _memory = new Memory();
        _memory.ProgramCounterChanged += OnProgramCounterChanged;
        _memory.MemoryArrayChanged += OnMemoryArrayChanged;

        // Initialisiere die ObservableCollection
        ObservableMemoryArray = new ObservableCollection<ObservableCollection<Register>>();
        InitializeObservableMemoryArray();
        _encode = new Encode(_memory);
        _alu = new ALU(_memory);
        LoadCommand = new RelayCommand(Load);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
        StartCommand = new RelayCommand(Start);
        TestCommand = new RelayCommand(Test);
    }
    
    private void InitializeObservableMemoryArray()
    {
        for (int i = 0; i < 2; i++)
        {
            var row = new ObservableCollection<Register>();
            for (int j = 0; j < 128; j++)
            {
                row.Add(_memory.MemoryArray[i, j]);
            }
            ObservableMemoryArray.Add(row);
        }
    }

    private void OnMemoryArrayChanged()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 128; j++)
            {
                ObservableMemoryArray[i][j] = _memory.MemoryArray[i, j];
            }
        }
    }

    private async void Load(object parameter)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select a file",
            AllowMultiple = false,
            Filters =
            [
                new FileDialogFilter
                {
                    Name = "LST Files",
                    Extensions = ["lst"]
                }
            ]
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
            _mainWindow.ErrorMessageBox(0);
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