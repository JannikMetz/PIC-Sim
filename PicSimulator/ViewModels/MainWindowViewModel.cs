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
using System.Threading.Tasks;
using PicSimulator.Views;
using Avalonia.Threading;

namespace PicSimulator.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Fields
    
    private Encode _encode;
    private Memory _memory;
    private ALU _alu;
    private string _fileContent;
    private ObservableCollection<ProgramLine> _programLines;
    private ObservableCollection<Breakpoint> _breakpoints;
    private ObservableCollection<IOPin> _IOPins;
    private MainWindow _mainWindow;
    private Watchdog _watchdog;
    private Timer0 _timer;

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
                UpdateCombinedList();
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
                UpdateCombinedList();
            }
        }
    }
    
    public ObservableCollection<ProgramLineWithBreakpoint> CombinedList { get; } = new();

    
    
    
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
    
    public ObservableCollection<string> PinNumbers{ get; set; } = new ObservableCollection<string>
    {
        "0", "1", "2", "3", "4", "5", "6", "7"
    };
    
    private ObservableCollection<Register>_observableMemoryArray;
    
    public ObservableCollection<Register> ObservableMemoryArray
    {
        get => _observableMemoryArray;
        set
        {
            _observableMemoryArray = value;
            OnPropertyChanged(nameof(ObservableMemoryArray));
        }
    }
    
    public ObservableCollection<IOPin> IOPins { get; set; } = new ObservableCollection<IOPin>();
    
    #endregion

    #region Commands

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand StartCommand { get; }
    
    public ICommand ResetCommand { get; }
    
    public ICommand PauseCommand { get; }
    
    public ICommand StepCommand { get; }
    
    public ICommand SkipCommand { get; }


    #endregion

    #region SpecialFunctionRegisters
    
    public ObservableCollection<int> StackItems { get; set; } = new ObservableCollection<int>();
    
    public int WReg
    {
        get { return _memory.WReg; }
    }
    public int ProgramCounter
    {
        get { return _memory.ProgramCounter2; }
    }
    
    public int FSR
    {
        get { return _memory.MemoryArray[0,4].Value; }
    }
    
    public int PCL
    {
        get { return _memory.MemoryArray[0,2].Value; }
    }
    
    public int PCLATH
    {
        get { return _memory.MemoryArray[0,0x0A].Value; }
    }
    
    public int StatusReg
    {
        get { return _memory.MemoryArray[0,3].Value; }
    }
    
    // TODO: Implement the stack pointer
    public int StackPointer
    {
        get { return 0; }
    }
    
    
    // TODO: wich prescaler is used?
    public int Prescaler
    {
        get
        {
            if (_memory.MemoryArray[1,1].GetBitValue(3) == 1)
            {
                // prescaler watchdog
                return Convert.ToInt32(Math.Pow(2,_memory.MemoryArray[1, 1].Value & 0x07)); // Get the prescaler value from the register
            }
            else
            {
                // prescaler timer0
                return Convert.ToInt32(Math.Pow(2,(_memory.MemoryArray[1, 1].Value & 0x07) + 1));
            }
        }
    }

    // TODO: Implement the WDT enable/disable
    public bool IsWDTEnabled
    {
        get { return _watchdog.WatchdogEnabled; }
        set
        {
            if (_watchdog.WatchdogEnabled != value)
            {
                _watchdog.WatchdogEnabled = value;
                OnPropertyChanged(nameof(IsWDTEnabled));
            }
        }
    }
    
    public int WDT
    {
        get { return _watchdog.WatchdogTimerValue; }
    }
    
    #endregion

    public MainWindowViewModel()
    {
        _mainWindow = new MainWindow();
        _memory = new Memory();
        _memory.ResetedMemory += OnResetedMemory;
        _memory.StackChanged += UpdateStackItems;
        _memory.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_memory.WReg))
            {
                OnPropertyChanged(nameof(WReg));
            }
            
            if (e.PropertyName == nameof(_memory.ProgramCounter2))
            {
                OnPropertyChanged(nameof(ProgramCounter));
                HighlightCurrentLine();
            }

            if (e.PropertyName == nameof(_memory.MemoryArray))
            {
                OnPropertyChanged(nameof(StatusReg));
                OnPropertyChanged(nameof(FSR));
                OnPropertyChanged(nameof(PCL));
                OnPropertyChanged(nameof(PCLATH));
                OnPropertyChanged(nameof(Prescaler));
            }
        };

        
        
        UpdateStackItems();
        InitializeObservableMemoryArray();
        _watchdog = new Watchdog(_memory);
        _watchdog.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_watchdog.WatchdogTimerValue))
            {
                OnPropertyChanged(nameof(WDT));
            }
            
            if (e.PropertyName == nameof(_watchdog.WatchdogEnabled))
            {
                OnPropertyChanged(nameof(IsWDTEnabled));
            }
        };
        _timer = new Timer0(_memory);
        _alu = new ALU(_memory, _watchdog, _timer);
        _encode = new Encode(_memory, _alu);
        InitializeIOPins();
        
        // Loading and Saving File Commands
        LoadCommand = new RelayCommand(Load);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
        
        // Execution Commands
        StartCommand = new RelayCommand(Start);
        StepCommand = new RelayCommand(Step);
        SkipCommand = new RelayCommand(Skip);
        ResetCommand = new RelayCommand(Reset);
        PauseCommand = new RelayCommand(Pause);
    }
    
    private void InitializeIOPins()
    {
        // Initialize the I/O pins here
        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < 8; i++)
            {
                IOPins.Add(new IOPin( _memory, i, j));
            }
        }
    }
    
    private void InitializeObservableMemoryArray()
    {
        ObservableMemoryArray = new ObservableCollection<Register>();

        for (int bank = 0; bank < 2; bank++)
        {
            for (int addr = 0; addr < 128; addr++)
            {
                ObservableMemoryArray.Add(_memory.MemoryArray[bank, addr]);
            }
        }
    }

    private async void Load(object parameter)
    {
        _memory.ResetMemory();
        _memory.PowerOnReset();
        
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
    
    private void UpdateCombinedList()
    {
        CombinedList.Clear();
        
        int count = Math.Min(ProgramLines?.Count ?? 0, Breakpoints?.Count ?? 0);
        for (int i = 0; i < count; i++)
        {
            CombinedList.Add(new ProgramLineWithBreakpoint
            {
                Line = ProgramLines[i],
                Breakpoint = Breakpoints[i]
            });
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
        Task.Run(() =>
        {
            IsExecuting = true;
            _alu.Start();
        });
    }
    
    private void Step(object parameter)
    {
        if (_fileContent == null || _fileContent == string.Empty)
        {
            _mainWindow.ErrorMessageBox(0);
            Console.WriteLine("No file loaded");
            return;
        }
        Console.WriteLine("Step command executed");
        _alu.Step();
    }
    
    private void Skip(object parameter)
    {
        if (_fileContent == null || _fileContent == string.Empty)
        {
            _mainWindow.ErrorMessageBox(0);
            Console.WriteLine("No file loaded");
            return;
        }
        Console.WriteLine("Skip command executed");
        _alu.Skip();
    }
    
    private void Pause(object parameter)
    {
        Console.WriteLine("Pause command executed");
        IsExecuting = false;
        OnPropertyChanged();
    }
    
    private void Reset(object parameter)
    {
        Console.WriteLine("Reset command executed");
        IsExecuting = false;
        _alu.BreakpointSecs = 0;
        _memory.PowerOnReset();
    }

    private void OnResetedMemory()
    {
        InitializeObservableMemoryArray();
    }
    
    private void UpdateStackItems()
    {
        StackItems.Clear();
        foreach (var item in _memory.CallStack)
        {
            StackItems.Add(item);
        }

        while (StackItems.Count < 8)
        {
            StackItems.Add(0);
        }
    }
    
    
    private void HighlightCurrentLine()
    {
        if (ProgramLines == null) return;
        
        foreach (var line in ProgramLines)
        {
            line.IsHighlighted = (line.LineNumber == _encode.OpcodeLines[_memory.ProgramCounter2]);
        }
    }
    
    public bool IsExecuting
    {
        get => _alu.IsActive;
        set
        {
            if (_alu.IsActive != value)
            {
                _alu.IsActive = value;
                OnPropertyChanged();
            }
        }
    }
    
    
}