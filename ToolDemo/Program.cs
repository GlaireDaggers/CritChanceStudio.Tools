using System;
using CritChanceStudio.Tools;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using (var tool = new DemoApp())
        {
            tool.Run();
        }
    }
}