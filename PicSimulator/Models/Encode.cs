using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PicSimulator.Models;

public class Encode
{

    private Memory _memory;

    public Encode(Memory memory)
    {
        _memory = memory;
    }
    
    public string ReadFile(string filePath)
    {
        
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
        else
        {
            throw new FileNotFoundException("Die Datei wurde nicht gefunden.", filePath);
        }
    }
    
    // This Function is AI-Generated, the Regex might not be correct
    // Do we need this?
    public void ExtractOpcodes(string input)
    {
        List<int> opcodes = new List<int>();
        string[] lines = input.Split('\n');
        
        Console.WriteLine("<----------- Extracting Opcodes ----------->");
        
        foreach (string line in lines)
        {
            //check if line starts with whitespace or number
            if (!string.IsNullOrWhiteSpace(line) && line[0] != ' ')
            {
                string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                // Extract the opcode from the line
                string code =  parts[1];
                Console.WriteLine($"Opcode found at index {Array.IndexOf(lines, line)+1}: {code}");                
                opcodes.Add(int.Parse(code, System.Globalization.NumberStyles.HexNumber));
            }
            
        }
        
        Console.WriteLine("<----------- End of Opcodes ----------->");
        
        _memory.ProgramMemory = opcodes.ToArray();
        
        
    }
    
}
    
