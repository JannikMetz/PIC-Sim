using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Tmds.DBus.Protocol;

namespace PicSimulator.Models;

using System;



public class Memory : ObservableObject
{
    public event Action ResetedMemory;
    
    public event Action TimerWritten;
    
    private EEPROMService _eepromService;

    // Constructor to initialize the memory with default values.
    public Memory()
    {
        ResetMemory();
        PowerOnReset();
        CallStack = new int[8];
        StackPointer = 0;
        
        string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, "..", "..", ".."));
        string eepromPath = Path.Combine(projectRoot, "EEPROM.txt");
        // Initialize the EEPROM service.
        _eepromService = new EEPROMService(eepromPath, this);
    }

    // This class represents the memory of the PIC microcontroller.
    // It contains a 2D array to represent the memory banks.
    private Register[,] _memoryArray = new Register[2, 128]; // 2 banks of 128 bytes each

    private int[] _programMemory = new int[1024]; // Program memory (ROM) - 2kB
    
    public bool OnReset { get; set; } = false;

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
            OnPropertyChanged();
        }
    }

    public int Timer { get; set; } // Timer in microseconds

    int _wReg; // W register (accumulator)

    public int WReg // W register
    {
        get { return _wReg & 0xFF; } // W register (only lower 8 bits)
        set
        {
            _wReg = value & 0xFF; // Set W register (only lower 8 bits)
            OnPropertyChanged();
        }

    }

    private int[] _callStack;

    public int[] CallStack
    {
        get { return _callStack; }
        set
        {
            _callStack = value;
            OnPropertyChanged(nameof(CallStack));
        }
    }
    
    private int _stackPointer;
    
    public int StackPointer
    {
        get { return _stackPointer; }
        set
        {
            _stackPointer = value;
            OnPropertyChanged();
        }
    }

    private int _programCounter2;

    public int ProgramCounter2
    {
        get { return _programCounter2; }
        set
        {
            _programCounter2 = value;
            OnPropertyChanged();
        }
    }
    
    public void ClearEeprom()
    {
        _eepromService.ClearEeprom();
    }

    public void IncrementProgramCounter()
    {
        int pc = ProgramCounter2;
        if (pc == 0x3FF)
        {
            pc = 0;
        }
        else
        {
            pc++;
        }

        SetRegister(0x02, pc & 0xFF); // Only lower 8 bits are represented in the register
        ProgramCounter2 = pc;
    }

    public void SetProgramCounterForJump(int address)
    {
        int pcLath = GetRegister(0x0A);

        // mask for bit 3 and 4
        pcLath = pcLath & 0x18;

        // add the upper 2 Bits of pcLath to the address
        int pc = address + (pcLath << 8);

        SetRegister(0x02, pc & 0xFF); // Only lower 8 bits are represented in the register
        ProgramCounter2 = pc;
    }

    public void SetProgramCounterForReturn(int value)
    {
        SetRegister(0x02, value & 0xFF); // Only lower 8 bits are represented in the register
        ProgramCounter2 = value;
    }

    public void SetProgramCounterAfterManipulation()
    {
        int pc = GetRegister(0x02);
        int pcLath = GetRegister(0x0A);

        ProgramCounter2 = pc + ((pcLath & 0x1F) << 8);
    }

    // set everything to 0
    public void ResetMemory()
    {
        OnReset = true;
        Console.WriteLine("Resetting Memory");
        // Reset the memory to default values.
        for (int bank = 0; bank < 2; bank++)
        {
            for (int register = 0; register < 128; register++)
            {
                Register reg = new Register();

                // MemoryArray changed when the value of the register changes
                reg.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Register.Value))
                    {
                        OnPropertyChanged(nameof(MemoryArray));
                    }
                };

                MemoryArray[bank, register] = reg;
            }
        }

        Console.WriteLine("Resetting W-Register");
        WReg = 0; // Reset W register
        ProgramCounter2 = 0;
        OnReset = false;

        ResetedMemory?.Invoke();
    }

    public void PowerOnReset()
    {
        OnReset = true;
        Console.WriteLine("Initializing Memory");

        Console.WriteLine("Resetting Program Counter");

        ProgramCounter2 = 0;

        Console.WriteLine("Setting Registers to Reset Values");


        // DO NOT USE SetRegister() HERE BECAUSE IT ONLY SETS ADDRESSES ON CURRENT BANK

        // Set Status Bank 1 & 2 to 0001 1XXX
        MemoryArray[0, 3].Value = 0x18;
        MemoryArray[1, 3].Value = 0x18;

        // Set OPTION_REG to 1111 1111
        MemoryArray[1, 1].Value = 0xFF;

        // Set TRISA to ---1 1111 and TRISB to 1111 1111
        MemoryArray[1, 5].Value = 0x1F;
        MemoryArray[1, 6].Value = 0xFF;
        OnReset = false;
    }

    public void MLCRReset(int status)
    {
        OnReset = true;
        SetProgramCounterForReturn(0);

        // manipulate the status register
        int value;
        switch (status)
        {
            // MLCR during normal operation
            case 0:
                value = MemoryArray[0, 0x03].Value & 0x1F;
                MemoryArray[0, 0x03].Value = value;
                MemoryArray[1, 0x03].Value = value;
                break;
            // MLCR during sleep
            case 1:
                value = MemoryArray[0, 0x03].Value & 0x07;
                MemoryArray[0, 0x03].Value = value + 0x10;
                MemoryArray[1, 0x03].Value = value + 0x10;
                break;
            // WDT during normal operation
            case 2:
                value = MemoryArray[0, 0x03].Value & 0x07;
                MemoryArray[0, 0x03].Value = value + 0x08;
                MemoryArray[1, 0x03].Value = value + 0x08;
                break;
            default:
                throw new Exception("Unknown memory status");
        }

        // Clear PcLath
        MemoryArray[0, 0x0A].Value = 0x00;
        MemoryArray[1, 0x0A].Value = 0x00;

        // INTCON
        MemoryArray[0, 0x0B].Value = _memoryArray[0, 0x0B].Value & 0x01;
        MemoryArray[1, 0x0B].Value = _memoryArray[1, 0x0B].Value & 0x01;

        // OPTION_REG
        MemoryArray[1, 0x01].Value = 0xFF;

        // TRISA
        MemoryArray[1, 0x05].Value = 0x1F;
        // TRISB
        MemoryArray[1, 0x06].Value = 0xFF;

        // EECON (there is a q)
        MemoryArray[1, 0x08].Value = 0x00;
        
        OnReset = false;

    }

    public void WakeUpFromSleepReset(bool isInterrupt)
    {
        OnReset = true;
        IncrementProgramCounter();

        if (isInterrupt)
        {
            MemoryArray[0, 0x03].SetBitValue(3, 0);
            MemoryArray[1, 0x03].SetBitValue(3, 0);

            MemoryArray[0, 0x03].SetBitValue(4, 1);
            MemoryArray[1, 0x03].SetBitValue(4, 1);
        }
        else
        {
            MemoryArray[0, 0x03].SetBitValue(3, 0);
            MemoryArray[1, 0x03].SetBitValue(3, 0);

            MemoryArray[0, 0x03].SetBitValue(4, 0);
            MemoryArray[1, 0x03].SetBitValue(4, 0);
        }

        MemoryArray[1, 0x08].SetBitValue(4, 0);
        OnReset = false;
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
        // indirect addressing
        if (address == 0)
        {
            // FSR register strores the address of the register to be accessed
            address = GetRegister(0x04);
            bankBit = address & 0x80;
            address = address & 0x7F;
        }

        Console.WriteLine($"Getting Memory in Bank {bankBit} at address {address}");
        return MemoryArray[bankBit, address].Value;
    }

    public void SetRegister(int address, int value)
    {
        value = value & 0xFF;
        int bankBit = GetBank();
        
        if (address == 0)
        {
            // FSR register strores the address of the register to be accessed
            address = GetRegister(0x04);
            bankBit = address & 0x80;
            address = address & 0x7F;
        }
        
        Console.WriteLine($"Setting Memory in Bank {bankBit} at address {address} to {value}");
        MemoryArray[bankBit, address].WriteValueFromUiThread(value);

        // these addresses are mirrored in the other bank
        if (address == 0x02 || address == 0x03 || address == 0x04 || address == 0x0A || address == 0x0B)
        {
            // Update the other bank as well
            MemoryArray[1 - bankBit, address].WriteValueFromUiThread(value);
        }

        // update the program counter
        if (address == 0x02)
        {
            SetProgramCounterAfterManipulation();
        }

        if (address == 0x01 && bankBit == 0)
        {
            TimerWritten?.Invoke();
        }
        if (address == 0x08 && bankBit == 1)
        {
            // EEPROM
            if ((value & 0x01) == 1)
            {
                // Read from EEPROM
                _eepromService.ReadByte();
            }
            
            if ((value & 0x02) == 1)
            {
                // Write to EEPROM
                _eepromService.WriteByteAsync();
            }
        }
        
        if(address == 0x09 && bankBit == 1)
        {
            // EECON2
            _eepromService.WriteToEECON2();
        }
    }

    public int GetBit(int address, int bitNumber)
    {
        int bankBit = GetBank();
        
        if (address == 0)
        {
            // FSR register strores the address of the register to be accessed
            address = GetRegister(0x04);
            bankBit = address & 0x80;
            address = address & 0x7F;
        }
        
        Console.WriteLine($"Getting Bit {bitNumber} in Bank {bankBit} at address {address}");
        int value = MemoryArray[bankBit, address].GetBitValue(bitNumber);
        return value;
    }

    public void SetBit(int address, int bitNumber, int value)
    {
        int bankBit = GetBank();
        
        if (address == 0)
        {
            // FSR register strores the address of the register to be accessed
            address = GetRegister(0x04);
            bankBit = address & 0x80;
            address = address & 0x7F;
        }
        
        Console.WriteLine($"Setting Bit {bitNumber} in Bank {bankBit} at address {address} to {value}");
        MemoryArray[bankBit, address].SetBitValue(bitNumber, value);
        // these addresses are mirrored in the other bank
        if (address == 0x02 || address == 0x03 || address == 0x04 || address == 0x0A || address == 0x0B)
        {
            // Update the other bank as well
            MemoryArray[1 - bankBit, address].SetBitValue(bitNumber, value);
        }
        
        // update the program counter
        if (address == 0x02)
        {
            SetProgramCounterAfterManipulation();
        }

        if (address == 0x01 && bankBit == 0)
        {
            TimerWritten?.Invoke();
        }
        
        if (address == 0x08 && bankBit == 1)
        {
            // EEPROM
            if (bitNumber == 0)
            {
                // Read from EEPROM
                _eepromService.ReadByte();
            }

            if (bitNumber == 1)
            {
                // Write to EEPROM
                _eepromService.WriteByteAsync();
            }
        }
        
        if(address == 0x09 && bankBit == 1)
        {
            // EECON2
            _eepromService.WriteToEECON2();
        }
    }

    public void SetCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(0, 1); // Set the carry flag (bit 0 of the status register)
        MemoryArray[1, 0x03].SetBitValue(0, 1);
    }

    public void ClearCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(0, 0); // Clear the carry flag (bit 0 of the status register)
        MemoryArray[1, 0x03].SetBitValue(0, 0);
    }

    public void SetZeroFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(2, 1); // Set the zero flag (bit 2 of the status register)
        MemoryArray[1, 0x03].SetBitValue(2, 1);
    }

    public void ClearZeroFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(2, 0); // Clear the zero flag (bit 2 of the status register)
        MemoryArray[1, 0x03].SetBitValue(2, 0);
    }

    public void SetDigitCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(1, 1); // Set the digit flag (bit 3 of the status register)
        MemoryArray[1, 0x03].SetBitValue(1, 1);
    }

    public void ClearDigitCarryFlag()
    {
        MemoryArray[0, 0x03].SetBitValue(1, 0); // Clear the digit flag (bit 3 of the status register)
        MemoryArray[1, 0x03].SetBitValue(1, 0);
    }

    public void PushToCallStack(int value)
    {
        CallStack[StackPointer] = value;
        StackPointer++;
        if (StackPointer > 7)
        {
            StackPointer = 0;
        }
        OnPropertyChanged(nameof(CallStack));
    }

    public int PopFromCallStack()
    {
        StackPointer--;
        if (StackPointer < 0)
        {
            StackPointer = 7;
        }
        int value = CallStack[StackPointer];
        return value;
    }
}

