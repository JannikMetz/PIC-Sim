namespace PicSimulator.Models;

public class Timer0
{
    private Memory _memory;
    private int _timer0Value;
    
    public Timer0(Memory memory)
    {
        _memory = memory;
        _timer0Value = 0;
    }

    public void IncrementTimer()
    {
        int preScaler = 1;
        if (_memory.MemoryArray[1,1].GetBitValue(3) == 0)
        {
            preScaler = ((_memory.MemoryArray[1, 1].Value & 0x07) + 1) << 1; // Get the prescaler value from the register
        }
        
        _timer0Value++;
        if (_timer0Value >= preScaler)
        {
            int timer = _memory.MemoryArray[0,1].Value;
            if (timer == 0xFF)
            {
                timer = 0;
                _memory.MemoryArray[0,1].Value = timer;
                _memory.MemoryArray[1,1].SetBitValue(2, 1); // Set the Timer0 overflow flag
            }
            else
            {
                _memory.MemoryArray[0,1].Value = timer + 1;
            }
            _timer0Value = 0;
        }
    }
}