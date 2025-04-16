using System;
using System.Linq;
using PicSimulator.Models;

namespace PicSimulator.ViewModels;

public class Breakpoint : ViewModelBase
{
    public int LineNumber { get; set; }
    private int PcIndex { get; set; }
    private bool _isActive;
    private bool _isOpcode;
    private ALU _alu;

    
    public Breakpoint(int lineNumber, Memory memory, int pcIndex = 1025, bool isOpcode = false, bool isActive = false)
    {
        LineNumber = lineNumber;
        PcIndex = pcIndex;
        _isActive = isActive;
        _isOpcode = isOpcode;
        _alu = new ALU(memory);

        
    }
    
    
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            if (_isActive != value)
            {
                // _alu checks if PcIndex is or over 1025 and does not set the breakpoint if it is
                _alu.UpdateBreakpoints(PcIndex, value);
                _isActive = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsOpcode
    {
        get { return _isOpcode; }
        set
        {
            if (_isOpcode != value)
            {
                _isOpcode = value;
                OnPropertyChanged();
            }
        }
    }
    
}