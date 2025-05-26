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
    private ALU _alu;
    private MainWindow _mainWindow;

    private static int[] _opcodeLines = new int[1024]; // Program memory (ROM) - 2kB

    public int[] OpcodeLines
    {
        get { return _opcodeLines; }
        set { _opcodeLines = value; }
    }

    public Encode(Memory memory, ALU alu)
    {
        _memory = memory;
        _alu = alu;
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

    public ObservableCollection<ProgramLine> ExtractOpcodes(string input)
    {
        ObservableCollection<ProgramLine> programLines = new ObservableCollection<ProgramLine>();
        int[] opcodes = new int[1024];
        string[] lines = input.Split('\n');
        int lineIndex = 0;
        Console.WriteLine("<----------- Extracting Opcodes ----------->");

        foreach (string line in lines)
        {
            // Create a new ProgramLine object and add it to the collection
            ProgramLine programLine = new ProgramLine(lineIndex, line);
            programLines.Add(programLine);
            lineIndex++;

            //check if line starts with whitespace or number
            if (!string.IsNullOrWhiteSpace(line) && line[0] != ' ')
            {
                string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                // Extract the opcode from the line
                int code = int.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                int programCounter = int.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
                Console.WriteLine($"Opcode found at line index {Array.IndexOf(lines, line) + 1}: {code}");
                opcodes[programCounter] = code;

                // Save the opcode to the opcodeLines array
                _opcodeLines[programCounter] = Array.IndexOf(lines, line);
            }
        }

        Console.WriteLine("<----------- End of Opcodes ----------->");

        // Write the opcodes to the program memory
        _memory.ProgramMemory = opcodes;
        return programLines;
    }


    public ObservableCollection<Breakpoint> CreateBreakpoints(string input)
    {
        ObservableCollection<Breakpoint> breakpoints = new ObservableCollection<Breakpoint>();
        string[] lines = input.Split('\n');
        int lineIndex = 0;
        int ProgramCounterLineIndex = 0;

        foreach (string line in lines)
        {
            Breakpoint breakpoint;
            //check if line starts with whitespace or number
            if (!string.IsNullOrWhiteSpace(line) && line[0] != ' ')
            {
                breakpoint = new Breakpoint(lineIndex, _memory, _alu, ProgramCounterLineIndex, true);
                ProgramCounterLineIndex++;
            }
            else
            {
                breakpoint = new Breakpoint(lineIndex, _memory, _alu);
            }

            breakpoints.Add(breakpoint);
            lineIndex++;
        }

        return breakpoints;
    }


}