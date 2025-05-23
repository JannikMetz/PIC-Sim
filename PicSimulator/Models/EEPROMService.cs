using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PicSimulator.Models;

public class EEPROMService
{
    private readonly string eepromFilePath;
    private const int EepromSize = 64;
    private Memory _memory;
    private Queue<int> writeSequence = new Queue<int>(2);

    private int EEADR
    {
        get { return _memory.MemoryArray[0, 0x09].Value;}
    }
    
    private int EEDATA
    {
        get { return _memory.MemoryArray[0, 0x08].Value;}
        set
        {
            _memory.MemoryArray[0, 0x08].Value = value;
        }
    }
    
    private int EECON1
    {
        get { return _memory.MemoryArray[1, 0x08].Value;}
        set
        {
            _memory.MemoryArray[1, 0x08].Value = value;
        }
    }
    
    private int EECON2
    {
        get { return _memory.MemoryArray[1, 0x09].Value;}
        set
        {
            _memory.MemoryArray[1, 0x09].Value = value;
        }
    }
    

    public EEPROMService(string path, Memory memory)
    {
        _memory = memory;
        eepromFilePath = path;

        if (!File.Exists(eepromFilePath))
        {
            File.Create(eepromFilePath).Close();
            var emptyEeprom = new string[EepromSize];
            for (int i = 0; i < EepromSize; i++)
                emptyEeprom[i] = "FF"; 

            File.WriteAllLines(eepromFilePath, emptyEeprom);
        }
    }
    
    public void ClearEeprom()
    {
        var emptyEeprom = new string[EepromSize];
        for (int i = 0; i < EepromSize; i++)
            emptyEeprom[i] = "FF"; 

        File.WriteAllLines(eepromFilePath, emptyEeprom);
    }
    
    

    public void ReadByte()
    {
        int address = EEADR;
        if (address < 0 || address >= EepromSize)
            throw new ArgumentOutOfRangeException(nameof(address));

        var lines = File.ReadAllLines(eepromFilePath);
        EEDATA = Convert.ToInt32(lines[address], 16);
    }

    public async Task WriteByteAsync()
    {
        if (_memory.RuntimeTimer == null)
        {
            throw new InvalidOperationException("Runtime timer is not set.");
        }
        int timer = _memory.RuntimeTimer.Timer;
        while (_memory.RuntimeTimer.Timer < timer + 1000)
        {
            await Task.Delay(1);
        }
        
        // check if the WREN is set
        if ((EECON1 & 0x04) == 0)
        {
            return;
        }
        
        var sequence = writeSequence.ToArray();
        if (sequence.Length == 2 && sequence[0] == 0x55 && sequence[1] == 0xAA)
        {
            writeSequence.Clear();
            int address = EEADR;
            int value = EEDATA;
            if (address < 0 || address >= EepromSize)
                throw new ArgumentOutOfRangeException(nameof(address));

            var lines = new List<string>(File.ReadAllLines(eepromFilePath));
            lines[address] = value.ToString("X2");
            File.WriteAllLines(eepromFilePath, lines);
            // set Interrupt flag after writing
            _memory.MemoryArray[1, 0x08].SetBitValue(4, 1);
            // clear WR bit
            _memory.MemoryArray[1, 0x08].SetBitValue(1, 0);
        }
    }
    
    public void WriteToEECON2()
    {
        if (writeSequence.Count == 2)
            writeSequence.Dequeue();
        writeSequence.Enqueue(EECON2);
    }
}