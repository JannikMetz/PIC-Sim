using System;
using System.Threading;
using PicSimulator.ViewModels;

namespace PicSimulator.Models;

public class ALU
{
    private Memory _memory;
    private Watchdog _watchdog;
    private Timer0 _timer;
    public ALU(Memory memory, Watchdog watchdog, Timer0 timer)
    {
        _memory = memory;
        _watchdog = watchdog;
        _timer = timer;
    }
    
    public int BreakpointSecs = 0;
    
    private static bool[] _breakpoints = new bool[1024]; // Max of 1024 Opcodes
    
    public bool IsActive = false;

    public int ExecutionSpeed = 100;
    
    public void Start()
    {

        while (IsActive)  // false if Execution is stopped by Reset or Pausing
        {
            
            // Check if Breakpoints are active
            if(_breakpoints[_memory.ProgramCounter2])
            {
                Console.WriteLine("Breakpoint active at: " + _memory.ProgramCounter2.ToString("X4") +  " for " + BreakpointSecs + " Secs");
                BreakpointSecs++;
            }
            else
            {
                BreakpointSecs = 0;
                // Read the opcode from the program memory
                int opcode = _memory.ProgramMemory[_memory.ProgramCounter2];

                // Execute the operation
                Console.WriteLine("Executing Opcode: " + opcode.ToString("X4") + " at PC: " +
                                  _memory.ProgramCounter2.ToString("X4"));
                _watchdog.Increment(); // increment watchdog timer
                GetOperation(opcode);
            }
            Thread.Sleep(ExecutionSpeed);
        }
        // Stopped 
        Console.WriteLine("Execution Stopped");
    }
    
    public void Step()
    {
        // Read the opcode from the program memory
        int opcode = _memory.ProgramMemory[_memory.ProgramCounter2];

        // Execute the operation
        Console.WriteLine("Executing Opcode: " + opcode.ToString("X4") + " at PC: " +
                          _memory.ProgramCounter2.ToString("X4"));
        _watchdog.Increment(); // increment watchdog timer
        GetOperation(opcode);
    }
    
    public void Skip()
    {
        // Read the opcode from the program memory
        int opcode = _memory.ProgramMemory[_memory.ProgramCounter2];

        // Execute the operation
        Console.WriteLine("Skipping Opcode: " + opcode.ToString("X4") + " at PC: " +
                          _memory.ProgramCounter2.ToString("X4"));
        _watchdog.Increment(); // increment watchdog timer
        _memory.IncrementProgramCounter();
    }
    
    
    public void UpdateBreakpoints(int ProgramCounterIndex, bool active)
    {
        if (ProgramCounterIndex <= 1024 && ProgramCounterIndex >= 0)
        {
            Console.WriteLine("Updating Breakpoint at Program counter: " + ProgramCounterIndex.ToString("X4"));
            _breakpoints[ProgramCounterIndex] = active;
        }
        else
        {
            Console.WriteLine("Breakpoint is not part of Opcodes");
        }
    }
    
    public bool GetOperation(int Opcode)
    {
        if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
        {
            // an Operation takes 1 microsecond at 4MHz but sometimes an
            _timer.IncrementTimer(); // Operation takes 2 microseconds then we increment again in the Operation
        }

        int Mask6BitOperant = 0x3F00;
        int result = Opcode & Mask6BitOperant;
        switch (result)
        {
            case 0x0700:
                return ADDWF(Opcode); // ADDWF
            case 0x0500:
                return ANDWF(Opcode); // AndWF
            case 0x0900:
                return COMF(Opcode); // COMF
            case 0x0300:
                return DECF(Opcode); // DECF
            case 0x0B00:
                return DECFSZ(Opcode); // DECFSZ
            case 0x0A00:
                return INCF(Opcode); // INCF
            case 0x0F00:
                return INCFSZ(Opcode); // INCFSZ
            case 0x0400:
                return IORWF(Opcode); // IORWF
            case 0x0800:
                return MOVF(Opcode); // MOVF
            case 0x0D00:
                return RLF(Opcode); // RLF
            case 0x0C00:
                return RRF(Opcode); // RRF
            case 0x0200:
                return SUBWF(Opcode); // SUBWF
            case 0x0E00:
                return SWAPF(Opcode); // SWAPF
            case 0x0600:
                return XORWF(Opcode); // XORWF
            case 0x3900:
                return ANDLW(Opcode); // ANDLW
            case 0x3800:
                return IORLW(Opcode); // IORLW
            case 0x3A00:
                return XORLW(Opcode); // XORLW
    }
        
        int Mask7BitOperation = 0x3F80; //CLRF AND CLRW AND MOVEWF
        result = Opcode & Mask7BitOperation;
        switch (result)
        {
            case 0x0180:
                return CLRF(Opcode); // CLRF
            case 0x0100:
                return CLRW(Opcode); // CLRW
            case 0x0080:
                return MOVWF(Opcode); // MOVWF
        }
        
        int MaskNopOperation = 0x3F9F; //  NOP 
        result = Opcode & MaskNopOperation;
        switch (result)
        {
            case 0x0000:
                return NOP(); // NOP
        }

        int Mask4BitOperation = 0x3C00;
        result = Opcode & Mask4BitOperation;
        switch (result)
        {
            case 0x1000:
                return BCF(Opcode); // BCF
            case 0x1400:
                return BSF(Opcode); // BSF
            case 0x1800:
                return BTFSC(Opcode); // BTFSC
            case 0x1C00:
                return BTFSS(Opcode); // BTFSS
            case 0x3000:
                return MOVLW(Opcode); // MOVLW
            case 0x3400:
                return RETLW(Opcode); // RETLW
        }
        
        int Mask5BitOperation = 0x3E00;
        result = Opcode & Mask5BitOperation;
        switch (result)
        {
            case 0x3E00:
                return ADDLW(Opcode); // ADDLW
            case 0x3C00:
                return SUBLW(Opcode); // SUBLW
        }
        
        int Mask3BitOperation = 0x3F00;
        result = Opcode & Mask3BitOperation;
        switch (result)
        {
            case 0x2000:
                return CALL(Opcode);
            case 0x2800:
                return GOTO(Opcode);
        }
        
        int Mask14BitOperation = 0x3FFF;
        result = Opcode & Mask14BitOperation;
        switch (result)
        {
            case 0x0064:
                return CLRWDT();
            case 0x0009:
                return RETFIE();
            case 0x0008:
                return RETURN();
            case 0x0063:
                return SLEEP();
        }
        
        // if this happened we have an error:
        Console.WriteLine("Unbekannter Opcode: " + Opcode.ToString("X4"));
        return false;
    }

    private bool ANDLW(int opcode)
    {
        // mask with relevant bits
        int value = opcode & 0x00FF;
        // AND with WReg
        int result = _memory.WReg & value;
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        // result into WReg
        _memory.WReg = result;
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool IORLW(int opcode)
    {
        // mask with relevant bits
        int value = opcode & 0x00FF;
        // IOR with WReg
        int result = _memory.WReg | value;
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        // result into WReg
        _memory.WReg = result;
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool XORLW(int opcode)
    {
        // mask with relevant bits
        int value = opcode & 0x00FF;
        // IOR with WReg
        int result = _memory.WReg ^ value;
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        // result into WReg
        _memory.WReg = result;
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool CLRF(int opcode)
    {
        int address = opcode & 0x007F;
        _memory.SetRegister(address, 0x00);
       
        // Set Zero Flag
        _memory.SetZeroFlag();

        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool CLRW(int opcode)
    {
        _memory.WReg = 0x00;
        
        // Set Zero Flag
        _memory.SetZeroFlag();
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool MOVWF(int opcode) 
    {
        int address = opcode & 0x007F;
        
        // write WReg to register
        _memory.SetRegister(address, _memory.WReg);
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool NOP()
    {
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool BCF(int opcode)
    {
        int address = opcode & 0x007F;
        
        // get bit number from opcode
        int bitNumber = (opcode & 0x0380) >> 7;
        
        // clear the bit in the register
        _memory.SetBit(address, bitNumber, 0);
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool BSF(int opcode)
    {
        int address = opcode & 0x007F;
        
        // get bit number from opcode
        int bitNumber = (opcode & 0x0380) >> 7;
        
        // set the bit in the register
        _memory.SetBit(address, bitNumber, 1);
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool BTFSC(int opcode)
    {
        int address = opcode & 0x007F;
        
        // get bit number from opcode
        int bitNumber = (opcode & 0x0380) >> 7;
        
        // check the bit in the register
        int bitValue = _memory.GetBit(address, bitNumber);
        if (bitValue == 0)
        {
            // skip next instruction
            _memory.IncrementProgramCounter();
            
            // only when we skip this instruction takes 2 microseconds
            if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
            {
                _timer.IncrementTimer();
            }
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        return true;
    }

    private bool BTFSS(int opcode)
    {
        int address = opcode & 0x007F;
        
        // get bit number from opcode
        int bitNumber = (opcode & 0x0380) >> 7;
        
        // check the bit in the register
        int bitValue = _memory.GetBit(address, bitNumber);
        if (bitValue == 1)
        {
            // skip next instruction
            _memory.IncrementProgramCounter();
            
            // only when we skip this instruction takes 2 microseconds
            if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
            {
                _timer.IncrementTimer();
            }
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        return true;
    }

    private bool MOVLW(int opcode)
    {
        int value = opcode & 0x00FF;
        
        // move value into WReg
        _memory.WReg = value;
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool RETLW(int opcode)
    {
        int value = opcode & 0x00FF;
        
        // move value into WReg
        _memory.WReg = value;

        // get the last program counter from the top of the call stack
        int pc = _memory.PopFromCallStackAsync().Result;
        
        // set program counter
        _memory.SetProgramCounterForReturn(pc);
        
        // this instruction takes 2 microseconds
        if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
        {
            _timer.IncrementTimer();
        }
        
        // program counter is not incremented here because we did it in the CALL instruction
        
        return true;
    }

    private bool ADDLW(int opcode)
    {
        // mask with relevant bits
        int value = opcode & 0x00FF;
        // ADD to WReg
        int result = _memory.WReg + value;
        _memory.WReg = result;
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool SUBLW(int opcode)
    {
        // mask with relevant bits
        int value = opcode & 0x00FF;
        
        // SUB to WReg
        int result =  value - _memory.WReg;
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        // this is a mistake on the PIC hardware, but we implement it as it is
        if (result >= 0)
        {
            // Set Carry Flag
            _memory.SetCarryFlag();
        }
        else
        {
            // Clear Carry Flag
            _memory.ClearCarryFlag();
        }
        
        int digitCarry = result & 0x000F;
        
        if (digitCarry >= 0)
        {
            // Set Digit Carry Flag
            _memory.SetDigitCarryFlag();
        }
        else
        {
            // Clear Digit Carry Flag
            _memory.ClearDigitCarryFlag();
        }
        
        // result into WReg
        _memory.WReg = result & 0x00FF; // ensure result is 8 bits
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    private bool CALL(int opcode)
    {
        int pc = opcode & 0x07FF;
        
        // push the program counter, incremented by 1, onto the call stack
        Console.WriteLine("Calling Stack");
        _memory.PushToCallStack(_memory.ProgramCounter2 + 1);
        Console.WriteLine("Calling Stack done");
        _memory.SetProgramCounterForJump(pc);
        
        if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
        {
            _timer.IncrementTimer();
        }
        return true;
    }

    private bool GOTO(int opcode)
    {
        int pc = opcode & 0x07FF; 
        
        // set the program counter
        _memory.SetProgramCounterForJump(pc);
        
        if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
        {
            _timer.IncrementTimer();
        }
        return true;
    }

    private bool SLEEP()
    {
        _watchdog.Reset();
        
        // set PD to 0 on both banks 
        _memory.MemoryArray[0,3].SetBitValue(3, 0);
        _memory.MemoryArray[1,3].SetBitValue(3, 0);
        
        // set TO to 1 on both banks
        _memory.MemoryArray[0,3].SetBitValue(4, 1);
        _memory.MemoryArray[1,3].SetBitValue(4, 1);
        
        _watchdog.AluIsSleeping = true;
        
        while (_watchdog.AluIsSleeping)
        {
            // wait for the watchdog to wake up
            Thread.Sleep(1000);
        }
        
        return true;
    }

    private bool RETURN()
    {
        // get the last program counter from the top of the call stack
        int pc = _memory.PopFromCallStackAsync().Result;
        
        // set program counter
        _memory.SetProgramCounterForReturn(pc);
        
        // this instruction takes 2 microseconds
        if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
        {
            _timer.IncrementTimer();
        }
        
        // prgramm counter is not incremented here because we did it in the CALL instruction
        
        return true;
    }

    private bool RETFIE()
    {
        // TODO: implement RETFIE (return from interrupt)
        if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
        {
            _timer.IncrementTimer();
        }
        return true;
    }

    private bool CLRWDT()
    {
        _watchdog.Reset();
        
        _memory.MemoryArray[0,3].SetBitValue(3, 1);
        _memory.MemoryArray[1,3].SetBitValue(3, 1);
        _memory.MemoryArray[0,3].SetBitValue(4, 1);
        _memory.MemoryArray[1,3].SetBitValue(4, 1);
        
        return true;
    }
    
    public bool ADDWF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.WReg + _memory.GetRegister(address);
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        if (result > 0xFF)
        {
            // Set Carry Flag
            _memory.SetCarryFlag();
        }
        else
        {
            // Clear Carry Flag
            _memory.ClearCarryFlag();
        }
        
        int digitCarry = result & 0x000F;
        
        if (digitCarry > 0x0F)
        {
            // Set Digit Carry Flag
            _memory.SetDigitCarryFlag();
        }
        else
        {
            // Clear Digit Carry Flag
            _memory.ClearDigitCarryFlag();
        }
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }
    
    public bool ANDWF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.WReg & _memory.GetRegister(address);
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    public bool COMF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = ~_memory.GetRegister(address);
        // ensure result is 8 bits
        result = result & 0x00FF; 
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }

        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    public bool DECF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        result--;
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }

        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }

    public bool DECFSZ(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        result--;
        
        
        if (result == 0)
        {
            // Skip: increment program counter twice
            _memory.IncrementProgramCounter();
            
            // only when we skip this instruction takes 2 microseconds
            if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
            {
                _timer.IncrementTimer();
            }
        }
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        _memory.IncrementProgramCounter();
 
        return true;
    }
    
    public bool INCF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        result++;
        
        if (result == 0x100)
        {
            // Set Zero Flag if result is more than 8 bits
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }

        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }
    public bool INCFSZ(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        result++;

        if (result == 0x100)
        {
            // Set Zero Flag if result is more than 8 bits
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        {
            // Skip: increment program counter twice
            _memory.IncrementProgramCounter();
            
            // only when we skip this instruction takes 2 microseconds
            if (_memory.MemoryArray[1, 1].GetBitValue(5) == 0)
            {
                _timer.IncrementTimer();
            }
        }
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        _memory.IncrementProgramCounter();
 
        return true;
    }
    public bool IORWF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        result = _memory.WReg | result;
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        _memory.IncrementProgramCounter();
        
        return true;
    }
    public bool MOVF(int f)
    {
        int address = f & 0x7F;
        int destinationBit = f & 0x80;
        
        // get the value from the register
        int result = _memory.GetRegister(address);
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }
    public bool RLF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        
        // get the carry bit from the status register
        int carryBit = _memory.GetBit(0x03, 0);
        // the bit that is shifted out
        int newCarryBit = result & 0x80;
        _memory.SetBit(0x03, 0, newCarryBit);
        
        // shift left through carry
        result = (result << 1) + carryBit;
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();

        return true;
    }
    public bool RRF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address);
        
        // get the carry bit from the status register
        int carryBit = _memory.GetBit(0x03, 0);
        // the bit that is shifted out
        int newCarryBit = result & 0x01;
        _memory.SetBit(0x03, 0, newCarryBit);
        
        // shift right through carry
        result = (result >> 1) + (carryBit << 7);
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();

        return true;
    }
    public bool SUBWF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.GetRegister(address) - _memory.WReg;
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        if (result >= 0x00)
        {
            // Set Carry Flag
            _memory.SetCarryFlag();
        }
        else
        {
            // Clear Carry Flag
            _memory.ClearCarryFlag();
        }
        
        int digitCarry = result & 0x000F;
        
        if (digitCarry >= 0)
        {
            // Set Digit Carry Flag
            _memory.SetDigitCarryFlag();
        }
        else
        {
            // Clear Digit Carry Flag
            _memory.ClearDigitCarryFlag();
        }
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        result = result & 0x00FF; // ensure result is 8 bits
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }
    public bool SWAPF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int upperNibble = _memory.GetRegister(address) & 0xF0;
        int lowerNibble = _memory.GetRegister(address) & 0x0F;
        
        int result = (lowerNibble << 4) + (upperNibble >> 4);
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }
    public bool XORWF(int f)
    {
        int address = f & 0x007F;
        int destinationBit = f & 0x0080;
        int result = _memory.WReg ^ _memory.GetRegister(address);
        
        if (result == 0)
        {
            // Set Zero Flag
            _memory.SetZeroFlag();
        }
        else
        {
            // Clear Zero Flag
            _memory.ClearZeroFlag();
        }
        
        
        if (destinationBit == 0)
        {
            // write to register W
            _memory.WReg = result;
        }
        else
        {
            // write to register f
            _memory.SetRegister(address, result);
        }
        
        // increment program counter
        _memory.IncrementProgramCounter();
        
        return true;
    }
    
}

