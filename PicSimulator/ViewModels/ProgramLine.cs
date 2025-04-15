namespace PicSimulator.ViewModels;

public class ProgramLine : ViewModelBase
{
    public int LineNumber { get; set; }
    public string Content { get; set; }
    private bool _isHighlighted;
    
    public ProgramLine(int lineNumber, string content, bool isHighlighted = false)
    {
        LineNumber = lineNumber;
        Content = content;
        _isHighlighted = isHighlighted;
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
}