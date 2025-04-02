namespace PicSimulator.Models;

public class Register
{
    public int Address { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    
    public Register(int length, int address, string name, int value)
    {
        this.Address = address;
        this.Name = name;
        this.Value = value;
    }
}