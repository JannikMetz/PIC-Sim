using System;
using System.Linq;
using PicSimulator.Models;

namespace PicSimulator.ViewModels;

public class ProgramLine : ViewModelBase
{
    public int LineNumber { get; set; }
    public string Content { get; set; }
    private bool _isHighlighted;
    private bool _isBreakpoint;
    
    public ProgramLine(int lineNumber, string content, bool isHighlighted = false, bool isBreakpoint = false)
    {
        LineNumber = lineNumber;
        Content = content;
        _isHighlighted = isHighlighted;
        _isBreakpoint = isBreakpoint;
    }

    public bool IsHighlighted
    {
        get { return _isHighlighted; }
        set
        {
            if (_isHighlighted != value)
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool IsBreakpoint
    {
        get { return _isBreakpoint; }
        set
        {
            if (_isBreakpoint != value)
            {
                _isBreakpoint = value;
                OnPropertyChanged();
            }
        }
    }
}
