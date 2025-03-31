namespace DefaultNamespace;

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
                return ANDWF(operation); // ANDWF
            case 0x0900:
                return COMF(operation); // COMF
            case 0x0300:
                return DECF(operation); // DECF
            case 0x0B00:
                return DECFSZ(operation); // DECFSZ
            case 0x0A00:
                return INCF(operation); // INCF
            case 0x0F00:
                return INCFSZ(operation); // INCFSZ
            case 0x0400:
                return IORWF(operation); // IORWF
            case 0x0800:
                return MOVF(operation); // MOVF
            case 0x0D00:
                return RLF(operation); // RLF
            case 0x0C00:
                return RRF(operation); // RRF
            case 0x0200:
                return SUBWF(operation); // SUBWF
            case 0x0E00:
                return SWAPF(operation); // SWAPF
            case 0x0600:
                return XORWF(operation); // XORWF
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
        
    }
    
    
    public bool AddWF(int f)
    {
        int mask = 0x0080;
        int destinationBit = f & mask;
        
        // get f from registers with address
        
        // Calculate the result
        
        // Set Flags
        
        // write back to register f or W
        if (destinationBit == 0)
        {
            // write to register W
        }
        else
        {
            // write to register f
        }
        
        return true;
        
    }
    

}

