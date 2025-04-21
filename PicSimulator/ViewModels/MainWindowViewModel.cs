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
using System.Linq;
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
    public ObservableCollection<Breakpoint> _breakpoints;
    
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
    
    public ObservableCollection<Breakpoint> Breakpoints
    {
        get => _breakpoints;
        set
        {
            if (_breakpoints != value)
            {
                _breakpoints = value;
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
    
    public ICommand ResetCommand { get; }
    
    public ICommand PauseCommand { get; }

    #endregion


    public MainWindowViewModel()
    {
        _mainWindow = new MainWindow();
        _memory = new Memory();
        _memory.ProgramCounterChanged += OnProgramCounterChanged;
        _encode = new Encode(_memory);
        _alu = new ALU(_memory);
        LoadCommand = new RelayCommand(Load);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
        StartCommand = new RelayCommand(Start);
        TestCommand = new RelayCommand(Test);
        ResetCommand = new RelayCommand(Reset);
        PauseCommand = new RelayCommand(Pause);

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
                Breakpoints = _encode.CreateBreakpoints(_fileContent); 
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
            _alu.IsActive = true;
            _alu.Start(); 
        });

        aluThread.IsBackground = true;
        aluThread.Start();
    }
    
    private void Pause(object parameter)
    {
        Console.WriteLine("Pause command executed");
        _alu.IsActive = false;
    }
    
    
    private void Reset(object parameter)
    {
        Console.WriteLine("Reset command executed");
        _alu.IsActive = false;
        _alu.BreakpointSecs = 0;
        _memory.ResetMemory();
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