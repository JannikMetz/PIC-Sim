using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PicSimulator.Models;

public class Encode
{
    
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
    public static int[] ExtractOpcodes(string input)
    {
        List<int> opcodes = new List<int>();
        string[] lines = input.Split('\n');
        
        foreach (string line in lines)
        {
            //check if line starts with whitespace or number
            if (line[0] != ' ')
            {
                // Extract the opcode from the line
                string code =  line.Substring(0, 3);
                opcodes.Add(int.Parse(code, System.Globalization.NumberStyles.HexNumber));
            }
            
        }
        
        return opcodes.ToArray();
    }
    
}
    
