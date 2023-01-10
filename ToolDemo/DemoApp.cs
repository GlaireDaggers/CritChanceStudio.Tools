using CritChanceStudio.Tools;

using ImGuiNET;

public class DemoApp : ToolApp
{
    public DemoApp() : base("Demo App")
    {
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }

    protected override void OnMenuBarGUI()
    {
        base.OnMenuBarGUI();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Quit"))
            {
                Exit();
            }
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Window"))
        {
            if (ImGui.MenuItem("Test Window"))
            {
                GetWindow<TestWindow>();
            }
            ImGui.EndMenu();
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }
}