namespace DefaultNamespace;

//Class Encode is Readign the Code and Extracting the operations
// and giving the operations to the Memory
public class Encode
{
    
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FileReader
{
    public string ReadFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            throw new FileNotFoundException("Die Datei wurde nicht gefunden.", filePath);
        }
    }
    
    // This Function is AI-Generated, the Regex might not be correct
    public static int[] ExtractOpcodes(string input)
    {
        List<int> opcodes = new List<int>();
        string[] lines = input.Split('\n');
        
        foreach (string line in lines)
        {
            Match match = Regex.Match(line, "^\\s*\\d{4}\\s([0-9A-F]{4})\\b");
            if (match.Success)
            {
                opcodes.Add(Convert.ToInt32(match.Groups[1].Value, 16));
            }
        }
        
        return opcodes.ToArray();
    }
    
}
    
}