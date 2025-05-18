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
    
    public int StackPointer
    {
        get { return _memory.CallStack.Count; }
    }
    
    
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

    #region StatusReg
    
    public bool StatusBit0
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(0) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(0, 1);
                _memory.MemoryArray[1,3].SetBitValue(0, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(0, 0);
                _memory.MemoryArray[1,3].SetBitValue(0, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit1
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(1) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(1, 1);
                _memory.MemoryArray[1,3].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(1, 0);
                _memory.MemoryArray[1,3].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit2
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(2) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(2, 1);
                _memory.MemoryArray[1,3].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(2, 0);
                _memory.MemoryArray[1,3].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit3
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(3) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(3, 1);
                _memory.MemoryArray[1,3].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(3, 0);
                _memory.MemoryArray[1,3].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit4
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(4) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(4, 1);
                _memory.MemoryArray[1,3].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(4, 0);
                _memory.MemoryArray[1,3].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit5
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(5) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(5, 1);
                _memory.MemoryArray[1,3].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(5, 0);
                _memory.MemoryArray[1,3].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit6
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(6) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(6, 1);
                _memory.MemoryArray[1,3].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(6, 0);
                _memory.MemoryArray[1,3].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool StatusBit7
    {
        get { return _memory.MemoryArray[0,3].GetBitValue(7) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,3].SetBitValue(7, 1);
                _memory.MemoryArray[1,3].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[0,3].SetBitValue(7, 0);
                _memory.MemoryArray[1,3].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region OptionsReg
    
    public bool OptionBit0
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(0) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(0, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(0, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit1
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(1) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit2
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(2) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit3
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(3) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit4
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(4) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit5
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(5) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit6
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(6) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool OptionBit7
    {
        get { return _memory.MemoryArray[1,1].GetBitValue(7) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[1,1].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[1,1].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region Intcon

    public bool IntconBit0
    {
        get { return _memory.MemoryArray[1,0x0B].GetBitValue(0) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,0x0B].SetBitValue(0, 1);
                _memory.MemoryArray[1,0x0B].SetBitValue(0, 1);
            }
            else
            {
                _memory.MemoryArray[0,0x0B].SetBitValue(0, 0);
                _memory.MemoryArray[1,0x0B].SetBitValue(0, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool IntconBit1
    {
        get { return _memory.MemoryArray[1,0x0B].GetBitValue(1) == 1; }
        set 
        {
            if (value)
            {
                _memory.MemoryArray[0,0x0B].SetBitValue(1, 1);
                _memory.MemoryArray[1,0x0B].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[0,0x0B].SetBitValue(1, 0);
                _memory.MemoryArray[1,0x0B].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }

    public bool IntconBit2
    {
        get { return _memory.MemoryArray[1, 0x0B].GetBitValue(2) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(2, 1);
                _memory.MemoryArray[1, 0x0B].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(2, 0);
                _memory.MemoryArray[1, 0x0B].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }

    public bool IntconBit3
    {
        get { return _memory.MemoryArray[1, 0x0B].GetBitValue(3) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(3, 1);
                _memory.MemoryArray[1, 0x0B].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(3, 0);
                _memory.MemoryArray[1, 0x0B].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }

    public bool IntconBit4
    {
        get { return _memory.MemoryArray[1, 0x0B].GetBitValue(4) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(4, 1);
                _memory.MemoryArray[1, 0x0B].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(4, 0);
                _memory.MemoryArray[1, 0x0B].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }

    public bool IntconBit5
    {
        get { return _memory.MemoryArray[1, 0x0B].GetBitValue(5) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(5, 1);
                _memory.MemoryArray[1, 0x0B].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(5, 0);
                _memory.MemoryArray[1, 0x0B].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }

    public bool IntconBit6
    {
        get { return _memory.MemoryArray[1, 0x0B].GetBitValue(6) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(6, 1);
                _memory.MemoryArray[1, 0x0B].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(6, 0);
                _memory.MemoryArray[1, 0x0B].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }

    public bool IntconBit7
    {
        get { return _memory.MemoryArray[1, 0x0B].GetBitValue(7) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(7, 1);
                _memory.MemoryArray[1, 0x0B].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[0, 0x0B].SetBitValue(7, 0);
                _memory.MemoryArray[1, 0x0B].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region TrisA

    public bool TrisAPin0
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(0) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(0, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(0, 0);
            }
            OnPropertyChanged();
        }
    }
    public bool TrisAPin1
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(1) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }
    public bool TrisAPin2
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(2) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }
    public bool TrisAPin3
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(3) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }
    public bool TrisAPin4
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(4) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisAPin5
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(5) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisAPin6
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(6) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisAPin7
    {
        get { return _memory.MemoryArray[1,5].GetBitValue(7) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,5].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[1,5].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region PortA
    
    public bool PortAPin0
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(0) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(0, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(0, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin1
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(1) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin2
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(2) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin3
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(3) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin4
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(4) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin5
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(5) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin6
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(6) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortAPin7
    {
        get { return _memory.MemoryArray[0,5].GetBitValue(7) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,5].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[0,5].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    
    
    #endregion
    
    #region TrisB
    
    public bool TrisBPin0
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(0) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(0, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(0, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin1
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(1) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin2
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(2) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin3
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(3) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin4
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(4) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin5
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(5) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin6
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(6) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool TrisBPin7
    {
        get { return _memory.MemoryArray[1,6].GetBitValue(7) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[1,6].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[1,6].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region PortB
    
    private bool _portBPin0;
    public bool PortBPin0
    {
        get
        {
            _portBPin0 = _memory.MemoryArray[0,6].GetBitValue(0) == 1;
            return _portBPin0;
        }
        set
        {
            if (value != _portBPin0)
            {
                IntInterrupt(value);
                if (value)
                {
                    _memory.MemoryArray[0, 6].SetBitValue(0, 1);
                }
                else
                {
                    _memory.MemoryArray[0, 6].SetBitValue(0, 0);
                }

                OnPropertyChanged();
            }
        }
    }
    
    public bool PortBPin1
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(1) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(1, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(1, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortBPin2
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(2) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(2, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(2, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortBPin3
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(3) == 1; }
        set
        {
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(3, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(3, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortBPin4
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(4) == 1; }
        set
        {
            PortBInterrupt();
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(4, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(4, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortBPin5
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(5) == 1; }
        set
        {
            PortBInterrupt();
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(5, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(5, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortBPin6
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(6) == 1; }
        set
        {
            PortBInterrupt();
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(6, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(6, 0);
            }
            OnPropertyChanged();
        }
    }
    
    public bool PortBPin7
    {
        get { return _memory.MemoryArray[0,6].GetBitValue(7) == 1; }
        set
        {
            PortBInterrupt();
            if (value)
            {
                _memory.MemoryArray[0,6].SetBitValue(7, 1);
            }
            else
            {
                _memory.MemoryArray[0,6].SetBitValue(7, 0);
            }
            OnPropertyChanged();
        }
    }
    
    #endregion

    #endregion
    

    public MainWindowViewModel()
    {
        _mainWindow = new MainWindow();
        _memory = new Memory();
        _memory.ResetedMemory += OnResetedMemory;
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
                OnPropertyChanged(nameof(ObservableMemoryArray));
                
                // Status bits
                OnPropertyChanged(nameof(StatusBit0));
                OnPropertyChanged(nameof(StatusBit1));
                OnPropertyChanged(nameof(StatusBit2));
                OnPropertyChanged(nameof(StatusBit3));
                OnPropertyChanged(nameof(StatusBit4));
                OnPropertyChanged(nameof(StatusBit5));
                OnPropertyChanged(nameof(StatusBit6));
                OnPropertyChanged(nameof(StatusBit7));
                
                // Option bits
                OnPropertyChanged(nameof(OptionBit0));
                OnPropertyChanged(nameof(OptionBit1));
                OnPropertyChanged(nameof(OptionBit2));
                OnPropertyChanged(nameof(OptionBit3));
                OnPropertyChanged(nameof(OptionBit4));
                OnPropertyChanged(nameof(OptionBit5));
                OnPropertyChanged(nameof(OptionBit6));
                OnPropertyChanged(nameof(OptionBit7));
                
                // Intcon bits
                OnPropertyChanged(nameof(IntconBit0));
                OnPropertyChanged(nameof(IntconBit1));
                OnPropertyChanged(nameof(IntconBit2));
                OnPropertyChanged(nameof(IntconBit3));
                OnPropertyChanged(nameof(IntconBit4));
                OnPropertyChanged(nameof(IntconBit5));
                OnPropertyChanged(nameof(IntconBit6));
                OnPropertyChanged(nameof(IntconBit7));
                
                // TrisA bits
                OnPropertyChanged(nameof(TrisAPin0));
                OnPropertyChanged(nameof(TrisAPin1));
                OnPropertyChanged(nameof(TrisAPin2));
                OnPropertyChanged(nameof(TrisAPin3));
                OnPropertyChanged(nameof(TrisAPin4));
                OnPropertyChanged(nameof(TrisAPin5));
                OnPropertyChanged(nameof(TrisAPin6));
                OnPropertyChanged(nameof(TrisAPin7));
                
                // PortA bits
                OnPropertyChanged(nameof(PortAPin0));
                OnPropertyChanged(nameof(PortAPin1));
                OnPropertyChanged(nameof(PortAPin2));
                OnPropertyChanged(nameof(PortAPin3));
                OnPropertyChanged(nameof(PortAPin4));
                OnPropertyChanged(nameof(PortAPin5));
                OnPropertyChanged(nameof(PortAPin6));
                OnPropertyChanged(nameof(PortAPin7));
                
                // TrisB bits
                OnPropertyChanged(nameof(TrisBPin0));
                OnPropertyChanged(nameof(TrisBPin1));
                OnPropertyChanged(nameof(TrisBPin2));
                OnPropertyChanged(nameof(TrisBPin3));
                OnPropertyChanged(nameof(TrisBPin4));
                OnPropertyChanged(nameof(TrisBPin5));
                OnPropertyChanged(nameof(TrisBPin6));
                OnPropertyChanged(nameof(TrisBPin7));
                
                // PortB bits
                OnPropertyChanged(nameof(PortBPin0));
                OnPropertyChanged(nameof(PortBPin1));
                OnPropertyChanged(nameof(PortBPin2));
                OnPropertyChanged(nameof(PortBPin3));
                OnPropertyChanged(nameof(PortBPin4));
                OnPropertyChanged(nameof(PortBPin5));
                OnPropertyChanged(nameof(PortBPin6));
                OnPropertyChanged(nameof(PortBPin7));
            }

            if (e.PropertyName == nameof(_memory.CallStack))
            {
                UpdateStackItems();
                OnPropertyChanged(nameof(StackPointer));
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
        OnPropertyChanged(nameof(StackItems));
        OnPropertyChanged(nameof(StackPointer));
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
    
    public int SetExecutionSpeed
    {
        get => _alu.ExecutionSpeed;
        set
        {
            if (_alu.ExecutionSpeed != value)
            {
                _alu.ExecutionSpeed = value;
                OnPropertyChanged();
            }
        }
    }
    private void IntInterrupt(bool value)
    {
        bool INTEDG = Convert.ToBoolean(_memory.MemoryArray[1, 1].GetBitValue(6));
        if (INTEDG == value)
        {
            _memory.MemoryArray[0, 0xB].SetBitValue(1, 1);
            _memory.MemoryArray[1, 0xB].SetBitValue(1, 1);
        }
    }

    private void PortBInterrupt()
    {
        _memory.MemoryArray[0, 0xB].SetBitValue(0, 1);
        _memory.MemoryArray[1, 0xB].SetBitValue(0, 1);
    }
}