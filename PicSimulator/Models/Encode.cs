using System.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PicSimulator.ViewModels;
using PicSimulator.Views;

namespace PicSimulator.Models;

public class Encode
{

    private Memory _memory;
    private MainWindow _mainWindow;
    
    private int[] opcodeLines = new int[1024]; // Program memory (ROM) - 2kB

    public int[] OpcodeLines
    {
        get { return opcodeLines; }
        set { opcodeLines = value; }
    }
    public Encode(Memory memory)
    {
        _memory = memory;
        _mainWindow = new MainWindow();
    }
    
    public string ReadFile(string filePath)
    {
        
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
        else
        {
            _mainWindow.ErrorMessageBox(1);
            throw new FileNotFoundException("Die Datei wurde nicht gefunden.", filePath);
        }
    }
    
    // This Function is AI-Generated, the Regex might not be correct
    // Do we need this?
    public ObservableCollection<ProgramLine> ExtractOpcodes(string input)
    {
        ObservableCollection<ProgramLine> programLines = new ObservableCollection<ProgramLine>();
        List<int> opcodes = new List<int>();
        string[] lines = input.Split('\n');
        int ProgramCounterLineIndex = 0;
        int lineIndex = 0;
        Console.WriteLine("<----------- Extracting Opcodes ----------->");
            
        foreach (string line in lines)
        {
            // Create a new ProgramLine object and add it to the collection
            ProgramLine programLine = new ProgramLine(lineIndex, line);
            lineIndex++;
            programLines.Add(programLine);
            
            //check if line starts with whitespace or number
            if (!string.IsNullOrWhiteSpace(line) && line[0] != ' ')
            {
                string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                // Extract the opcode from the line
                string code =  parts[1];
                Console.WriteLine($"Opcode found at line index {Array.IndexOf(lines, line)+1}: {code}");
                opcodes.Add(int.Parse(code, System.Globalization.NumberStyles.HexNumber));
                
                // Save the opcode to the opcodeLines array
                opcodeLines[ProgramCounterLineIndex] = Array.IndexOf(lines, line);
                
                ProgramCounterLineIndex++;
            }
        }
        
        Console.WriteLine("<----------- End of Opcodes ----------->");
        
        // Write the opcodes to the program memory
        _memory.ProgramMemory = opcodes.ToArray(); 
        return programLines;
    }
}
    
