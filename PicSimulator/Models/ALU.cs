namespace PicSimulator.Models;

public class ALU
{
    
    public bool GetOperation(int operation)
    {
        int maskByteOriented = 0x3F00;
        int result = operation & maskByteOriented;
        switch (result)
        {
            case 0x0700:
                return AddWF(operation); // ADDWF
            case 0x0500:
                return AndWF(operation); // AndWF
            case 0x0900:
                return ComF(operation); // COMF
            case 0x0300:
                return DecF(operation); // DECF
            case 0x0B00:
                return DecFSZ(operation); // DECFSZ
            case 0x0A00:
                return IncF(operation); // INCF
            case 0x0F00:
                return IncFSZ(operation); // INCFSZ
            case 0x0400:
                return IOrWF(operation); // IORWF
            case 0x0800:
                return MovF(operation); // MOVF
            case 0x0D00:
                return RLF(operation); // RLF
            case 0x0C00:
                return RRF(operation); // RRF
            case 0x0200:
                return SubWF(operation); // SUBWF
            case 0x0E00:
                return SwapF(operation); // SWAPF
            case 0x0600:
                return XOrWF(operation); // XORWF
        }
        
        int maskBitOrientedSpecial = 0x3F80; //CLRF AND CLRW AND MOVEWF
        result = operation & maskBitOrientedSpecial;
        switch (result)
        {
            case 0x0180:
                return true; // CLRF
            case 0x0100:
                return true; // CLRW
            case 0x0080:
                return true; // MOVWF
        }


        
        
        maskBitOrientedSpecial = 0x00FF; //  NOP 
        result = operation & maskBitOrientedSpecial;
        switch (result)
        {
            case 0x0000:
                return true; // NOP
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

