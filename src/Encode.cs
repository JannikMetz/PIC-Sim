namespace DefaultNamespace;

//Class Encode is Readign the Code and Extracting the operations
// and giving the operations to the Memory
public class Encode
{
    
using System.IO;

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
    
    
    
}
    
}