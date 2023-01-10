namespace CritChanceStudio.Tools;

using ImGuiNET;

public class EditorWindow
{
    public string name = "Editor Window";

    public EditorWindow()
    {
    }

    public virtual void OnDrawWindow(ref bool open)
    {
        if (ImGui.Begin(name, ref open))
        {
            OnGUI();
            ImGui.End();
        }
    }

    public virtual void OnGUI()
    {
    }

    public virtual void OnClosed()
    {
    }
}