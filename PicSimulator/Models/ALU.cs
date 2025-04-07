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

        int Mask4BitOperation = 0x3B00;
        result = Opcode & Mask4BitOperation;
        switch (result)
        {
            case 0x1000:
                return BCF(Opcode); // BCF
            case 0x1400:
                return BSF(Opcode); // BSF
        }
        
        // if this happened we have an error:
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

