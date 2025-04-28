using PicSimulator.Models;

namespace PicSimulator.ViewModels;

public class IOPin : ViewModelBase
{
    private Memory _memory;
    private Register _register;
    private int _index;
    private bool _isPORTA; // true = TRISA, false = TRISB
    private bool _isInput; // else is Output
    private bool _isHigh; // true = high, false = low


    public IOPin(Memory memory, int index, bool isPORTA)
    {
        
        _memory = memory;
        _index = index;
        _isPORTA = isPORTA;
        _isHigh = false;
        
        if (_isPORTA)
        {
            _register = memory.MemoryArray[1, 5];
        }
        else
        {
            _register = memory.MemoryArray[1, 6];
        }
        
        if (_register.GetBitValue(index) == 1)
        {
            _isInput = true;
        }
        else
        {
            _isInput = false;
        };

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
        get
        {
            return _isInput;

        }
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
        get { return _isPORTA; }
        set
        {
            if (_isPORTA != value)
            {
                _isPORTA = value;
                OnPropertyChanged();
            }
        }
    }
    
    // public bool isHigh
    // {
    //     get
    //     {
    //         if (_isPORTA)
    //         {
    //             return _memory.MemoryArray[0, 5].GetBitValue(_index) != _register.GetBitValue(_index);
    //         }
    //         else
    //         {
    //             return _memory.MemoryArray[0, 6].GetBitValue(_index) != _register.GetBitValue(_index);
    //         }
    //     }
    // }
    //
}
