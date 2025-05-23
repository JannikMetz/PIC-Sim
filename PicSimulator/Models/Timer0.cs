using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PicSimulator.Models;

public class Timer0 : INotifyPropertyChanged
{
    private Memory _memory;
    private int _timer0Value; // this is only used for the prescaler
    private Runtimetimer _runtimetimer;

    public Runtimetimer RuntimeTimer
    {
        get { return _runtimetimer; }
        set
        {
            if (_runtimetimer != value)
            {
                _runtimetimer = value;
                OnPropertyChanged(nameof(RuntimeTimer));
            }
        }
    }
    public int Timer
    {
        get { return _runtimetimer.Timer; }
    }
    
    public int Frequency
    {
        get {return _runtimetimer.Frequency;}
        set
        {
            if (_runtimetimer.Frequency != value)
            {
                _runtimetimer.Frequency = value;
            }
        }
    }
    
    public Timer0(Memory memory)
    {
        _memory = memory;
        _timer0Value = 0;
        _memory.TimerWritten += OnTimerWritten;
        _runtimetimer = new Runtimetimer(4);
        _runtimetimer.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(Runtimetimer.Timer))
            {
                OnPropertyChanged(nameof(Timer));
            }
        };
    }
    
    public void OnTimerWritten()
    {
        _timer0Value = 0;
    }
    
    public void Reset()
    {
        _timer0Value = 0;
        _memory.MemoryArray[0,1].Value = 0x00; // Reset Timer0 register
        _memory.MemoryArray[1,0x0B].SetBitValue(2, 0); // Clear Timer0 overflow flag
        _memory.MemoryArray[0,0x0B].SetBitValue(2, 0); 
    }

    public void IncrementTimer()
    {
        int preScaler = 1;
        if (_memory.MemoryArray[1,1].GetBitValue(3) == 0)
        {
            preScaler = Convert.ToInt32(Math.Pow(2,(_memory.MemoryArray[1, 1].Value & 0x07) + 1)); // Get the prescaler value from the register
        }
        
        _timer0Value++;
        if (_timer0Value >= preScaler)
        {
            int timer = _memory.MemoryArray[0,1].Value;
            if (timer == 0xFF)
            {
                timer = 0;
                _memory.MemoryArray[0,1].Value = timer;
                _memory.MemoryArray[1,0x0B].SetBitValue(2, 1); // Set the Timer0 overflow flag
                _memory.MemoryArray[0,0x0B].SetBitValue(2, 1); 
            }
            else
            {
                _memory.MemoryArray[0,1].Value = timer + 1;
            }
            _timer0Value = 0;
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}