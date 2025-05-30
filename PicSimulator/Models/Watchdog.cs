using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PicSimulator.Models;

public class Watchdog : INotifyPropertyChanged
{
    private Memory _memory;
    private int _watchdogValue; // this is only used for the prescaler
    private int _watchdogTimerValue; // this is the actual watchdog timer value
    private bool _aluIsSleeping = false;
    private bool _watchdogEnabled = false;
    private int _frequency = 4;
    public int Frequency
    {
        get { return _frequency; }
        set
        {
            if (_frequency != value)
            {
                _frequency = value;
            }
        }
    }

    public int WatchdogTimerValue
    {
        get { return (int)(1.0 / (_frequency / 4.0)) * _watchdogTimerValue; }
    }
    
    public bool WatchdogEnabled
    {
        get { return _watchdogEnabled; }
        set
        {
            if (_watchdogEnabled != value)
            {
                _watchdogEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool AluIsSleeping
    {
        get { return _aluIsSleeping; }
        set
        {
            if (_aluIsSleeping != value)
            {
                _aluIsSleeping = value;
                OnPropertyChanged();
            }
        }
    }

    public Watchdog(Memory memory)
    {
        _memory = memory;
        _watchdogValue = 0;
        _watchdogTimerValue = 0;
    }
    
    public void Reset()
    {
        _watchdogValue = 0;
        _watchdogTimerValue = 0;
        OnPropertyChanged(nameof(WatchdogTimerValue));
    }

    public void Increment()
    {
        if (!_watchdogEnabled)
        {
            return;
        }
        
        int preScaler = 1;
        if (_memory.MemoryArray[1,1].GetBitValue(3) == 1)
        {
            preScaler = Convert.ToInt32(Math.Pow(2,_memory.MemoryArray[1, 1].Value & 0x07)); // Get the prescaler value from the register
        }

        _watchdogValue++;
        if (_watchdogValue >= preScaler)
        {
            if (_watchdogTimerValue >= 18000)
            {
                _watchdogTimerValue = 0;
                if (AluIsSleeping)
                {
                    _memory.WakeUpFromSleepReset(false);
                    AluIsSleeping = false;
                }
                else
                {
                    _memory.MLCRReset(2); 
                }
            }
            else
            {
                _watchdogTimerValue++;
            }
            _watchdogValue = 0;
            OnPropertyChanged(nameof(WatchdogTimerValue));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}