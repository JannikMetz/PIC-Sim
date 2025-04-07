namespace PicSimulator.Models;

using System;
public class Memory
{
    // This class represents the memory of the PIC microcontroller.
    // It contains a 2D array to represent the memory banks.
    public int[,] MemoryArray { get; set; } = new int[2, 128]; // 2 banks of 128 bytes each
    
    // Status Register is 03h, Bit 5 is the Bit for Choosing the Bank
    
    public int wReg { get; set; } // W register
    
    
    // Constructor to initialize the memory with default values.
    public Memory()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                MemoryArray[i, j] = 0;
            }
        }
    }

    public bool isBank0()
    {
        Console.WriteLine("Getting the Bank Status");
        
        int bankBit = MemoryArray[0, 3] & 0x20; // Bit 5 of the status register
        
        if (bankBit == 0)
        {
            // Bank 0
            Console.WriteLine("Bank0 is selected");
            return true;
        }
        else
        {
            // Bank 1
            Console.WriteLine("Bank1 is selected");           
            return false;
        }   
        
        
        
    }
    
    public GetMemory(int address)
    {
        // Get the value from the specified memory bank and address.
        if (isBank0())
        {
            // Bank 0
            return MemoryArray[0, address];
        }
        else
        {
            // Bank 1
            return MemoryArray[1, address];
        }
    }
    
    public SetMemory(int address, int value)
    {
        // Set the value in the specified memory bank and address.
        if (isBank0())
        {
            // Bank 0
            MemoryArray[0, address] = value;
        }
        else
        {
            // Bank 1
            MemoryArray[1, address] = value;
        }
    }
}

