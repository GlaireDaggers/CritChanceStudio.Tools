namespace CritChanceStudio.Tools;

using ImGuiNET;
using Num = System.Numerics;
using Microsoft.Xna.Framework;

public class TagListWindow : EditorWindow
{
    public TagListWindow() : base()
    {
        this.name = "Tag List";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        var tool = (SpriteToolApp)ToolApp.instance;

        if (tool.activeKeyframe == null)
        {
            ImGui.Text("Select a keyframe to edit");
        }
        else
        {
            if (ImGui.Button("Add Tag"))
            {
                tool.RegisterUndo("Add tag");
                tool.activeKeyframe.tags.Add("New tag");
            }

            if (ImGui.BeginChild("_tag_list"))
            {
                for (int i = 0; i < tool.activeKeyframe.tags.Count; i++)
                {
                    string tagName = tool.activeKeyframe.tags[i];
                    if (ImGui.InputText("##tag_" + i + "_name", ref tagName, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        tool.RegisterUndo("Rename Tag");
                        tool.activeKeyframe.tags[i] = tagName;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Delete##tag_" + i))
                    {
                        tool.RegisterUndo("Delete Tag");
                        tool.activeKeyframe.tags.RemoveAt(i--);
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}