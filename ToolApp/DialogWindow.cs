namespace CritChanceStudio.Tools;

using System;

public class DialogWindow
{
    public bool open = false;
    public string title;
    public string message;
    public string[] buttons;
    public Action<int> onClicked;
}