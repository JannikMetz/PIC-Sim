using System;

namespace PicSimulator.Models;

public class Register
{
    private int _value;

    public int GetValue()
    {
        return _value;
    }

    public void SetValue(int value)
    {
        _value = value & 0xFF; // Nur die unteren 8 Bits verwenden
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
}