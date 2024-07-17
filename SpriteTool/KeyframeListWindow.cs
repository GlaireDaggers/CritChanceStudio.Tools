namespace CritChanceStudio.Tools;

using ImGuiNET;

using System;
using Num = System.Numerics;

public class KeyframeListWindow : EditorWindow
{
    public KeyframeListWindow() : base()
    {
        this.name = "Keyframe List";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        SpriteToolApp tool = (SpriteToolApp)ToolApp.instance;

        if (tool.activeAnimation == null)
        {
            ImGui.Text("Select an animation to edit");
        }
        else
        {
            bool looping = tool.activeAnimation.looping;
            if (ImGui.Checkbox("Looping", ref looping))
            {
                tool.RegisterUndo("Modify animation");
                tool.activeAnimation.looping = looping;
            }

            // keyframe list
            if (ImGui.BeginChild("_keyframe_timeline", new Num.Vector2(ImGui.GetContentRegionAvail().X, 64), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar))
            {
                for (int i = 0; i < tool.activeAnimation.keyframes.Count; i++)
                {
                    var keyframe = tool.activeAnimation.keyframes[i];

                    Num.Vector2 cursorPos = ImGui.GetCursorScreenPos();
                    ImGui.InvisibleButton("__keyframe_drop_insert_" + i, new Num.Vector2(4, ImGui.GetContentRegionAvail().Y));
                    if (ImGui.BeginDragDropTarget())
                    {
                        if (ToolApp.instance.AcceptDragDropPayload(out SpriteFramePayload frame))
                        {
                            // insert new keyframe here
                            tool.RegisterUndo("Add new keyframe");
                            tool.activeAnimation.keyframes.Insert(i, new Keyframe()
                            {
                                frameIdx = frame.idx
                            });
                        }
                        ImGui.EndDragDropTarget();
                    }
                    ImGui.SetCursorScreenPos(cursorPos);
                    if (ImGui.Button("Keyframe " + i, new Num.Vector2(keyframe.duration, 32)))
                    {
                        tool.activeKeyframe = keyframe;
                        tool.GetWindow<SpriteToolViewport>().FrameIndex = i;
                    }
                    var rectMin = ImGui.GetItemRectMin();
                    var rectMax = ImGui.GetItemRectMax();
                    if (keyframe == tool.activeKeyframe)
                    {
                        ImGui.GetWindowDrawList().AddRect(rectMin, rectMax, new Microsoft.Xna.Framework.Color(1f, 1f, 0f).PackedValue);
                    }
                    ImGui.SameLine();
                }

                ImGui.InvisibleButton("__keyframe_drop_append", ImGui.GetContentRegionAvail());
                if (ImGui.BeginDragDropTarget())
                {
                    if (ToolApp.instance.AcceptDragDropPayload(out SpriteFramePayload frame))
                    {
                        // append new keyframe to end
                        tool.RegisterUndo("Add new keyframe");
                        tool.activeAnimation.keyframes.Add(new Keyframe()
                        {
                            frameIdx = frame.idx
                        });
                    }
                    ImGui.EndDragDropTarget();
                }

                ImGui.EndChild();
            }
        }
    }
}