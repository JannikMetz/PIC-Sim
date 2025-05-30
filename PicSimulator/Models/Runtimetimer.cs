using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PicSimulator.Models;

public class Runtimetimer : INotifyPropertyChanged
{
    public Runtimetimer(int frequency)
    {
        Frequency = frequency;
    }
    
    private int _timer;
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
    
    public int Timer
    {
        get { return (int)(1.0 / (_frequency / 4.0)) * _timer; }
    }
    
    public void IncrementTimer()
    {
        _timer++;
        OnPropertyChanged(nameof(Timer));
    }
    public void Reset()
    {
        _timer = 0;
        OnPropertyChanged(nameof(Timer));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}