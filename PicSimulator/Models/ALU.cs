using System;

namespace PicSimulator.Models;

public class ALU
{
    
    public bool GetOperation(int Opcode)
    {
        int Mask6BitOperant = 0x3F00;
        int result = Opcode & Mask6BitOperant;
        switch (result)
        {
            case 0x0700:
                return AddWF(Opcode); // ADDWF
            case 0x0500:
                return AndWF(Opcode); // AndWF
            case 0x0900:
                return ComF(Opcode); // COMF
            case 0x0300:
                return DecF(Opcode); // DECF
            case 0x0B00:
                return DecFSZ(Opcode); // DECFSZ
            case 0x0A00:
                return IncF(Opcode); // INCF
            case 0x0F00:
                return IncFSZ(Opcode); // INCFSZ
            case 0x0400:
                return IOrWF(Opcode); // IORWF
            case 0x0800:
                return MovF(Opcode); // MOVF
            case 0x0D00:
                return RLF(Opcode); // RLF
            case 0x0C00:
                return RRF(Opcode); // RRF
            case 0x0200:
                return SubWF(Opcode); // SUBWF
            case 0x0E00:
                return SwapF(Opcode); // SWAPF
            case 0x0600:
                return XOrWF(Opcode); // XORWF
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
                return NOP(Opcode); // NOP
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
    
    
    public bool AddWF(int f)
    {
        int mask = 0x0080;
        int destinationBit = f & mask;
        
        // get f from registers with address
        
        // Calculate the result
        
        // Set Flags in Status Register
        
        // write back to register f or W
        if (destinationBit == 0)
        {
            // write to register W
        }
        else
        {
            // write to register f
        }
        
        //Update the program counter
        
        return true;
    }
    
    public bool AndWF(int f)
    {
        int mask = 0x0080;
        int destinationBit = f & mask;
        
        // get f from registers with address
        
        // Calculate the result
        
        // Set Flags in Status Register
        
        // write back to register f or W
        if (destinationBit == 0)
        {
            // write to register W
        }
        else
        {
            // write to register f
        }
        
        //Update the program counter
        
        return true;
    }

    public bool ComF(int f)
    {
        return true;
    }

    public bool DecF(int f)
    {
        return true;
    }

    public bool DecFSZ(int f)
    {
        return true;
    }
    
    public bool IncF(int f)
    {
        return true;
    }
    public bool IncFSZ(int f)
    {
        return true;
    }
    public bool IOrWF(int f)
    {
        return true;
    }
    public bool MovF(int f)
    {
        return true;
    }
    public bool RLF(int f)
    {
        return true;
    }
    public bool RRF(int f)
    {
        return true;
    }
    public bool SubWF(int f)
    {
        return true;
    }
    public bool SwapF(int f)
    {
        return true;
    }
    public bool XOrWF(int f)
    {
        return true;
    }
    
}

