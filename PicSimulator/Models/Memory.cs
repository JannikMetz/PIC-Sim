using System.Collections.Generic;
using System.Reflection.Emit;

namespace PicSimulator.Models;

using System;



public class Memory
{
    // This class represents the memory of the PIC microcontroller.
    // It contains a 2D array to represent the memory banks.
    private Register[,] _memoryArray = new Register[2, 128]; // 2 banks of 128 bytes each

    private int [] _programMemory = new int[1024]; // Program memory (ROM) - 2kB
    public int[] ProgramMemory
    {
        get { return _programMemory; }

        set
        {
           // value.CopyTo(_programMemory, 0); Could also work 
            _programMemory = value;

        }
    } // Program memory (ROM) - 2kB
    

    
    public int Timer { get; set; } // Timer in microseconds

    int _wReg; // W register (accumulator)
    public int WReg // W register
    {
        get
        {
            return _wReg & 0xFF;
        } // W register (only lower 8 bits)
        set
        {
            _wReg = value & 0xFF; // Set W register (only lower 8 bits)
        }
        
    } 
    
    public Stack<int> CallStack { get; set; } // Call stack for function calls 
    
    /// <summary>
    /// This property is used to push the program counter to the call stack.
    /// </summary>
    public int ProgramCounter {
        get
        {
            var pcLath = GetPcLath();
            var pc = GetProgramCounter();
            pcLath = pcLath << 8;
            
            return pc + pcLath; // combine pcl and pclath to get the full program counter
        }
        set
        {
            int pc = value & 0xFF; // Lower 8 bits
            int pcLath = (value >> 8) & 0x07; // Upper 3 bits
            
            // add the upper 2 Bits of pcLath to the value
            pcLath = pcLath + (GetPcLath() & 0x18);
            
            // write the values back to the memory
            SetProgramCounter(pc);
            SetPcLath(pcLath);
        }
    }
    
    
    // Constructor to initialize the memory with default values.
    public Memory()
    {
        ResetMemory();
    }

    public void ResetMemory()
    {
        Console.WriteLine("Resetting Memory");
        // Reset the memory to default values.
        for (int bank = 0; bank < 2; bank++)
        {
            for (int register = 0; register < 128; register++)
            {
                _memoryArray[bank, register] = new Register();
            }
        }
        
        Console.WriteLine("Resetting W-Register");
        WReg = 0; // Reset W register
    }

    public int GetBank()
    {
        Console.WriteLine("Getting the Bank Status");
        
        int bankBit = _memoryArray[0, 0x03].GetBitValue(5); // Bit 5 of the status register

        Console.WriteLine("Bank Status: " + bankBit);
        
        return bankBit;
    }
    
    public int GetRegister(int address)
    { 
        int bankBit = GetBank();
        Console.WriteLine($"Getting Memory in Bank {bankBit} at address {address}");
        return _memoryArray[bankBit, address].GetValue();
    }
    
    public void SetRegister(int address, int value)
    {
        value = value & 0xFF;
        int bankBit = GetBank();
        Console.WriteLine($"Setting Memory in Bank {bankBit} at address {address} to {value}");
        _memoryArray[bankBit, address].SetValue(value);

        // these addresses are mirrored in the other bank
        if (address == 0x02 || address == 0x03 || address == 0x04 || address == 0x0A || address == 0x0B)
        {
            // Update the other bank as well
            _memoryArray[1 - bankBit, address].SetValue(value);
        }
    }

    public int GetBit(int address, int bitNumber)
    {
        int bankBit = GetBank();
        Console.WriteLine($"Getting Bit {bitNumber} in Bank {bankBit} at address {address}");
        int value = _memoryArray[bankBit, address].GetBitValue(bitNumber);
        return value;
    }
    
    public void SetBit(int address, int bitNumber, int value)
    {
        int bankBit = GetBank();
        Console.WriteLine($"Setting Bit {bitNumber} in Bank {bankBit} at address {address} to {value}");
        _memoryArray[bankBit, address].SetBitValue(bitNumber, value);
        // these addresses are mirrored in the other bank
        if (address == 0x02 || address == 0x03 || address == 0x04 || address == 0x0A || address == 0x0B)
        {
            // Update the other bank as well
            _memoryArray[1 - bankBit, address].SetBitValue(bitNumber, value);
        }
    }
    
    public int GetProgramCounter()
    {
        // Get the program counter from the memory.
        return _memoryArray[0, 0x02].GetValue(); 
    }
    
    public void SetProgramCounter(int value)
    {
        // Set the program counter in the memory.
        // TODO: nur 1 mal setten
        _memoryArray[0, 0x02].SetValue(value);
        _memoryArray[1, 0x02].SetValue(value);
    }
    
    public void IncrementProgramCounter()
    {
        int pc = GetProgramCounter();
        if (pc == 0xFF)
        {
            pc = 0;
            IncrementPcLath();
        }
        else
        {
            pc++;
        }
        SetProgramCounter(pc);
    }
    
    public int GetPcLath()
    {
        // this value is the same on both banks
        return _memoryArray[0, 0x0A].GetValue(); 
    }
    
    public void SetPcLath(int value)
    {
        // Set the program counter latch in the memory.
        _memoryArray[0, 0x0A].SetValue(value);
        _memoryArray[1, 0x0A].SetValue(value);
    }

    public void IncrementPcLath()
    {
        int pcLath = GetPcLath();
        if (pcLath == 0x07)
        {
            pcLath = 0;
        }
        else
        {
            pcLath++;
        }
        SetPcLath(pcLath);
    }

    public void SetCarryFlag()
    {
        _memoryArray[0, 0x03].SetBitValue(0, 1); // Set the carry flag (bit 0 of the status register)
        _memoryArray[1, 0x03].SetBitValue(0, 1); 
    }
    public void ClearCarryFlag()
    {
        _memoryArray[0, 0x03].SetBitValue(0, 0); // Clear the carry flag (bit 0 of the status register)
        _memoryArray[1, 0x03].SetBitValue(0, 0); 
    }
    
    public void SetZeroFlag()
    {
        _memoryArray[0, 0x03].SetBitValue(2, 1); // Set the zero flag (bit 2 of the status register)
        _memoryArray[1, 0x03].SetBitValue(2, 1); 
    }
    public void ClearZeroFlag()
    {
        _memoryArray[0, 0x03].SetBitValue(2, 0); // Clear the zero flag (bit 2 of the status register)
        _memoryArray[1, 0x03].SetBitValue(2, 0);
    }
    
    public void SetDigitCarryFlag()
    {
        _memoryArray[0, 0x03].SetBitValue(1, 1); // Set the digit flag (bit 3 of the status register)
        _memoryArray[1, 0x03].SetBitValue(1, 1); 
    }
    
    public void ClearDigitCarryFlag()
    {
        _memoryArray[0, 0x03].SetBitValue(1, 0); // Clear the digit flag (bit 3 of the status register)
        _memoryArray[1, 0x03].SetBitValue(1, 0); 
    }
}

