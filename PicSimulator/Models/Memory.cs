using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PicSimulator.Models;

using System;



public class Memory : ObservableObject
{
    public event Action ProgramCounterChanged;
    public event Action MemoryArrayChanged;

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
            Console.WriteLine("Program Memory Set");
            OnPropertyChanged(nameof(ProgramMemory));
        }
    } // Program memory (ROM) - 2kB

    public Register[,] MemoryArray
    {
        get { return _memoryArray; }
        set
        {
            _memoryArray = value;
            MemoryArrayChanged?.Invoke();
        }
    }
    
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
            Console.WriteLine("Getting Program Counter:");
            var pcLath = GetPcLath();
            var pc = GetProgramCounter();
            pcLath = pcLath << 8;
            Console.WriteLine("Program Counter is: " + pc + " and pcLath is: " + pcLath);
            return (pc + pcLath); // combine pcl and pclath to get the full program counter
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
            Console.WriteLine("Program Counter set");  
            ProgramCounterChanged?.Invoke();
        }
    }
    
    
    // Constructor to initialize the memory with default values.
    public Memory()
    {
        InitializeMemory();
        CallStack = new Stack<int>();
    }

    public void ResetMemory()
    {
        Console.WriteLine("Resetting Memory");
        // Reset the memory to default values.
        for (int bank = 0; bank < 2; bank++)
        {
            for (int register = 0; register < 128; register++)
            {
                MemoryArray[bank, register] = new Register();
            }
        }
        
        Console.WriteLine("Resetting W-Register");
        WReg = 0; // Reset W register
    }

    public void InitializeMemory()
    {
        ResetMemory();
        Console.WriteLine("Initializing Memory");
        
        Console.WriteLine("Setting Registers to Reset Values");
        

        // DO NOT USE SetRegister() HERE BECAUSE IT ONLY SETS ADDRESSES ON CURRENT BANK
        
        // Set Status Bank 1 & 2 to 0001 1XXX
        MemoryArray[0,3].Value = 24;
        MemoryArray[1,3].Value = 24;
        
        // Set OPTION_REG to 1111 1111
        MemoryArray[1,1].Value = 255;
        
        // Set TRISA to ---1 1111 and TRISB to 1111 1111
        MemoryArray[1,5].Value = 31;
        MemoryArray[1,6].Value = 255;
        
    }

    public int GetBank()
    {
        Console.WriteLine("Getting the Bank Status");
        
        int bankBit = MemoryArray[0, 0x03].GetBitValue(5); // Bit 5 of the status register

        Console.WriteLine("Bank Status: " + bankBit);
        
        return bankBit;
    }
    
    public int GetRegister(int address)
    { 
        int bankBit = GetBank();
        Console.WriteLine($"Getting Memory in Bank {bankBit} at address {address}");
        return MemoryArray[bankBit, address].Value;
    }
    
    public void SetRegister(int address, int value)
    {
        value = value & 0xFF;
        int bankBit = GetBank();
        Console.WriteLine($"Setting Memory in Bank {bankBit} at address {address} to {value}");
        MemoryArray[bankBit, address].Value = value;

        // these addresses are mirrored in the other bank
        if (address == 0x02 || address == 0x03 || address == 0x04 || address == 0x0A || address == 0x0B)
        {
            // Update the other bank as well
            MemoryArray[1 - bankBit, address].Value = value;
        }
        
        MemoryArrayChanged?.Invoke();
    }

    public int GetBit(int address, int bitNumber)
    {
        int bankBit = GetBank();
        Console.WriteLine($"Getting Bit {bitNumber} in Bank {bankBit} at address {address}");
        int value = MemoryArray[bankBit, address].GetBitValue(bitNumber);
        return value;
    }
    
    public void SetBit(int address, int bitNumber, int value)
    {
        int bankBit = GetBank();
        Console.WriteLine($"Setting Bit {bitNumber} in Bank {bankBit} at address {address} to {value}");
        MemoryArray[bankBit, address].SetBitValue(bitNumber, value);
        // these addresses are mirrored in the other bank
        if (address == 0x02 || address == 0x03 || address == 0x04 || address == 0x0A || address == 0x0B)
        {
            // Update the other bank as well
            MemoryArray[1 - bankBit, address].SetBitValue(bitNumber, value);
        }
        
        MemoryArrayChanged?.Invoke();
    }
    
    public int GetProgramCounter()
    {
        // Get the program counter from the memory.
        return MemoryArray[0, 0x02].Value; 
    }
    
    public void SetProgramCounter(int value)
    {
        // Set the program counter in the memory.
        // TODO: nur 1 mal setten
        MemoryArray[0, 0x02].Value = value;
        MemoryArray[1, 0x02].Value = value;
        
        MemoryArrayChanged?.Invoke();
    }
    
    public void IncrementProgramCounter()
    {
        int pc = ProgramCounter;
        ProgramCounter = pc + 1;
        
        MemoryArrayChanged?.Invoke();
        // Maybe we need to revert this
        // int pc = GetProgramCounter();
        // if (pc == 0xFF)
        // {
        //     pc = 0;
        //     IncrementPcLath();
        // }
        // else
        // {
        //     pc++;
        // }
        // SetProgramCounter(pc);
    }
    
    public int GetPcLath()
    {
        // this value is the same on both banks
        return MemoryArray[0, 0x0A].Value; 
    }
    
    public void SetPcLath(int value)
    {
        // Set the program counter latch in the memory.
        MemoryArray[0, 0x0A].Value = value;
        MemoryArray[1, 0x0A].Value = value;
        
        MemoryArrayChanged?.Invoke();
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
        
        MemoryArrayChanged?.Invoke();
    }

    public void SetCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(0, 1); // Set the carry flag (bit 0 of the status register)
        MemoryArray[1, 0x03].SetBitValue(0, 1); 
        
        MemoryArrayChanged?.Invoke();
    }
    public void ClearCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(0, 0); // Clear the carry flag (bit 0 of the status register)
        MemoryArray[1, 0x03].SetBitValue(0, 0); 
        
        MemoryArrayChanged?.Invoke();
    }
    
    public void SetZeroFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(2, 1); // Set the zero flag (bit 2 of the status register)
        MemoryArray[1, 0x03].SetBitValue(2, 1); 
        
        MemoryArrayChanged?.Invoke();
    }
    public void ClearZeroFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(2, 0); // Clear the zero flag (bit 2 of the status register)
        MemoryArray[1, 0x03].SetBitValue(2, 0);
        
        MemoryArrayChanged?.Invoke();
    }
    
    public void SetDigitCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(1, 1); // Set the digit flag (bit 3 of the status register)
        MemoryArray[1, 0x03].SetBitValue(1, 1); 
        
        MemoryArrayChanged?.Invoke();
    }
    
    public void ClearDigitCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(1, 0); // Clear the digit flag (bit 3 of the status register)
        MemoryArray[1, 0x03].SetBitValue(1, 0); 
        
        MemoryArrayChanged?.Invoke();
    }
}

