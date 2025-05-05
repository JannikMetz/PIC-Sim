using System;
using PicSimulator.Models;

namespace PicSimulator.ViewModels;

public class IOPin : ViewModelBase
{
    private Memory _memory;
    private Register _register;
    private Register _trisRegister;
    private int _index;
    private int _registerValue; // 0 = PORTA, 1 = TRISA; 2 = PORTB, 3 = TRISB
    private bool _isInput; // else is Output
    private bool _isSet; // true = high, false = low
    private int _bank; // 0 = bank0, 1 = bank1
    private int _address; // 5 = A, 6 = B


    public IOPin(Memory memory, int index, int registerValue)
    {
        
        _memory = memory;
        _index = index;
        _isSet = false;
        _registerValue = registerValue;


        if (registerValue == 0)
        {
            _bank = 0;
            _address = 5;
        }
        else if (registerValue == 1)
        {
            _bank = 1;
            _address = 5;
        }
        else if (registerValue == 2)
        {
            _bank = 0;
            _address = 6;
        }
        else 
        {
            _bank = 1;
            _address = 6;
        }
        
        _register = _memory.MemoryArray[_bank, _address];
        _trisRegister = _memory.MemoryArray[1, _address];


        if (_register.GetBitValue(_index) == 1)
        {
            _isSet = true;
        }
        else
        {
            _isSet = false;
        }
        
        if (_trisRegister.GetBitValue(_index) == 1)
        {
            _isInput = true;
        }
        else
        {
            _isInput = false;
        }
        
    }
    
    public int Index
    {
        get { return _index; }
        set
        {
            if (_index != value)
            {
                _index = value;
                OnPropertyChanged();
            }
        }
    }


    public bool IsInput
    {
        get { return _isInput; }
        set
        {
            if (_isInput != value)
            {
                _isInput = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsPORTA
    {
        get
        {
            if (_registerValue == 0)
            {
                return true;
            }
                return false;
        }
    }
    
    public bool IsTRISA
    {
        get
        {
            if (_registerValue == 1)
            {
                return true;
            }
                return false;
        }
    }
    
    public bool IsPORTB
    {
        get
        {
            if (_registerValue == 2)
            {
                return true;
            }
                return false;
        }
    }
    
    public bool IsTRISB
    {
        get
        {
            if (_registerValue == 3)
            {
                return true;
            }
                return false;
        }
    }
    public bool IsSet
    {
        get { return _isSet; }
        set
        {
            if (_isSet != value)
            {
                if (value)
                {
                    _memory.MemoryArray[_bank, _address].SetBitValue(_index, 1);
                }
                else
                {
                    _memory.MemoryArray[_bank, _address].SetBitValue(_index, 0);
                }

                _isSet = value;
                OnPropertyChanged();
            }
        }
    }
}

