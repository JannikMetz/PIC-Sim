using System;
using System.ComponentModel;

namespace PicSimulator.Models;

public class Register : INotifyPropertyChanged
{
    private int _value;
    
    public int Value
    {
        get { return _value; }
        set
        {
            _value = value & 0xFF;
            OnPropertyChanged(nameof(Value));
        } // Nur die unteren 8 Bits verwenden
    }
    
    
    public int GetBitValue(int bitNumber)
    {
        if (bitNumber < 0 || bitNumber > 7)
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 0 and 7.");
        return (_value >> bitNumber) & 1; 
    }
    
    public void SetBitValue(int bitNumber, int value)
    {
        if (bitNumber < 0 || bitNumber > 7)
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 0 and 7.");
        if (value != 0)
            _value |= (1 << bitNumber);
        else
            _value &= ~(1 << bitNumber);
    }

    public Register()
    {
        _value = 0;
    }
    
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}