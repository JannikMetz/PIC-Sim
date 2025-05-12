using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace PicSimulator.Models;

public class Register : INotifyPropertyChanged
{
    private int _value;
    
    public int Value
    {
        get { return _value; }
        set
        {
            if (value >= 0x00 && value <= 0xFF)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }
    
    public void WriteValueFromUiThread(int value)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Value = value;
        });
    }
    
    
    public int GetBitValue(int bitNumber)
    {
        if (bitNumber < 0 || bitNumber > 7)
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 0 and 7.");
        return (Value >> bitNumber) & 1; 
    }
    
    public void SetBitValue(int bitNumber, int value)
    {
        if (bitNumber < 0 || bitNumber > 7)
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 0 and 7.");
        if (value != 0)
            WriteValueFromUiThread(Value | (1 << bitNumber));
        else
            WriteValueFromUiThread(Value & ~(1 << bitNumber));
    }

    public Register()
    {
        Value = 0;
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}