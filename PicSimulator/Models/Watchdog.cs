using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PicSimulator.Models;

public class Watchdog : INotifyPropertyChanged
{
    private Memory _memory;
    private int _watchdogValue;
    private int _watchdogTimerValue;
    
    public int WatchdogTimerValue
    {
        get { return _watchdogTimerValue; }
        set
        {
            if (_watchdogTimerValue != value)
            {
                _watchdogTimerValue = value;
                OnPropertyChanged();
            }
        }
    }

    public Watchdog(Memory memory)
    {
        _memory = memory;
        _watchdogValue = 0;
    }
    
    public void Reset()
    {
        _watchdogValue = 0;
    }

    public void Increment()
    {
        int preScaler = 1;
        if (_memory.MemoryArray[1,1].GetBitValue(3) == 0)
        {
            preScaler = ((_memory.MemoryArray[1, 1].Value & 0x07) + 1); // Get the prescaler value from the register
        }

        _watchdogValue++;
        if (_watchdogValue >= preScaler)
        {
            if (WatchdogTimerValue >= 18000)
            {
                _watchdogTimerValue = 0;
                _memory.ProgramCounter = 0; // Reset the program counter
            }
            else
            {
                WatchdogTimerValue++;
            }
            _watchdogValue = 0;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}