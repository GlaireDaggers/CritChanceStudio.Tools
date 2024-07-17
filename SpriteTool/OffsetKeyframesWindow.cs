namespace CritChanceStudio.Tools;

using ImGuiNET;

using Num = System.Numerics;
using Microsoft.Xna.Framework;

public class OffsetKeyframesWindow : EditorWindow
{
    private Num.Vector2 _offset = Num.Vector2.Zero;

    public OffsetKeyframesWindow() : base()
    {
        this.name = "Offset Keyframes";
    }

    public override ImGuiWindowFlags GetWindowFlags()
    {
        return base.GetWindowFlags() | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.Modal;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        var tool = (SpriteToolApp)ToolApp.instance;

        if (tool.activeAnimation == null)
        {
            ImGui.Text("Select an animation to edit");
        }
        else
        {
            ImGui.InputFloat2("Offset (px)", ref _offset);

            if (ImGui.Button("Copy"))
            {
                tool.clipboard = _offset;
            }
            ImGui.SameLine();
            if (ImGui.Button("Paste"))
            {
                if (tool.clipboard is Num.Vector2 newOffset)
                {
                    _offset = newOffset;
                }
            }

            if (ImGui.Button("Apply"))
            {
                tool.RegisterUndo("Offset keyframes");
                for (int i = 0; i < tool.activeAnimation.keyframes.Count; i++)
                {
                    tool.activeAnimation.keyframes[i].offset += new Vector2(_offset.X, _offset.Y);
                }

                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
        }
    }
}