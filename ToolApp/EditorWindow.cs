namespace CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;

public class EditorWindow
{
    public string name = "Editor Window";

    public EditorWindow()
    {
    }

    public virtual ImGuiWindowFlags GetWindowFlags()
    {
        return ImGuiWindowFlags.None;
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void OnDrawWindow(ref bool open)
    {
        if (ImGui.Begin(name, ref open, GetWindowFlags()))
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