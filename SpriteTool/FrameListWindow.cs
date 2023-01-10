namespace CritChanceStudio.Tools;

using ImGuiNET;
using NativeFileDialogSharp;

using System;

public class FrameListWindow : EditorWindow
{
    public FrameListWindow() : base()
    {
        this.name = "Sprite Frames";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (ImGui.Button("Import Frame"))
        {
            var result = Dialog.FileOpen("png");
            if (result.IsOk)
            {
                Console.WriteLine("Opened file: " + result.Path);
            }
        }
        if (ImGui.Button("Import From TexturePacker"))
        {
            var result = Dialog.FileOpen("json");
            if (result.IsOk)
            {
                Console.WriteLine("Opened file: " + result.Path);
            }
        }
    }
}