namespace CritChanceStudio.Tools;

using ImGuiNET;

using System;

public class AnimationListWindow : EditorWindow
{
    public AnimationListWindow() : base()
    {
        this.name = "Animation List";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        SpriteToolApp tool = ToolApp.instance as SpriteToolApp;

        if (ImGui.Button("New Animation"))
        {
            tool.RegisterUndo("New Animation");
            tool.activeDocument.animations.Add(new Animation()
            {
                name = "New Animation"
            });
        }

        if (ImGui.BeginChild("_anim_list"))
        {
            for (int i = 0; i < tool.activeDocument.animations.Count; i++)
            {
                var anim = tool.activeDocument.animations[i];

                var availableRect = ImGui.GetContentRegionAvail();

                string animName = anim.name;
                if (ImGui.InputText("##anim_" + i + "_name", ref animName, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    tool.RegisterUndo("Rename Animation");
                    anim.name = animName;
                }

                var rectMin = ImGui.GetItemRectMin();
                var rectMax = ImGui.GetItemRectMax();
                rectMax.X = rectMin.X + availableRect.X;

                if (anim == tool.activeAnimation)
                {
                    ImGui.GetWindowDrawList().AddRect(rectMin, rectMax, new Microsoft.Xna.Framework.Color(1f, 1f, 0f).PackedValue);
                }

                ImGui.SameLine();
                if (ImGui.Button("Edit##anim_" + i))
                {
                    tool.activeAnimation = anim;
                }
                ImGui.SameLine();
                if (ImGui.Button("Delete##anim_" + i))
                {
                    tool.RegisterUndo("Delete Animation");
                    if (anim == tool.activeAnimation)
                    {
                        tool.activeAnimation = null;
                        tool.activeKeyframe = null;
                    }
                    tool.activeDocument.animations.RemoveAt(i--);
                }
                ImGui.SameLine();
                if (ImGui.Button("Clone##anim_" + i))
                {
                    tool.RegisterUndo("Clone Animation");
                    var newAnim = anim.Clone();
                    tool.activeDocument.animations.Add(newAnim);
                    tool.activeAnimation = newAnim;
                }
            }

            ImGui.EndChild();
        }
    }
}