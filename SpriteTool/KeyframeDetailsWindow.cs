namespace CritChanceStudio.Tools;

using ImGuiNET;

using System;
using Num = System.Numerics;
using Microsoft.Xna.Framework.Graphics;

public class KeyframeDetailsWindow : EditorWindow
{
    public KeyframeDetailsWindow() : base()
    {
        this.name = "Keyframe Details";
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
            if (ImGui.Button("Delete Keyframe"))
            {
                tool.RegisterUndo("Delete keyframe");
                tool.activeAnimation.keyframes.Remove(tool.activeKeyframe);
                tool.activeKeyframe = null;
            }
            else
            {
                // draw sprite frame
                var frame = tool.activeKeyframe.frame;
                Texture2D tex = frame.GetTexture(tool.textureManager);
                Num.Vector2 uvMin = new Num.Vector2(frame.srcRect.Left / (float)tex.Width, frame.srcRect.Top / (float)tex.Height);
                Num.Vector2 uvMax = new Num.Vector2(frame.srcRect.Right / (float)tex.Width, frame.srcRect.Bottom / (float)tex.Height);

                ImGui.ImageButton("_keyframe_details_frame", frame.GetImGuiHandle(tool.textureManager), new Num.Vector2(frame.srcRect.Width, frame.srcRect.Height),
                    uvMin, uvMax);

                // allow sprite frames to be dragged here to swap out sprites for this keyframe
                if (ImGui.BeginDragDropTarget())
                {
                    if (tool.AcceptDragDropPayload(out SpriteFrame newFrame))
                    {
                        tool.RegisterUndo("Modify keyframe");
                        tool.activeKeyframe.frame = newFrame;
                    }
                    ImGui.EndDragDropTarget();
                }

                // draw keyframe details (duration, offset, motion delta)
                int duration = tool.activeKeyframe.duration;
                if (ImGui.InputInt("Duration (ms)", ref duration, 1, 1, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (duration < 16)
                    {
                        duration = 16;
                    }

                    tool.RegisterUndo("Modify keyframe");
                    tool.activeKeyframe.duration = duration;
                }

                Num.Vector2 offset = new Num.Vector2(tool.activeKeyframe.offset.X, tool.activeKeyframe.offset.Y);
                if (ImGui.InputFloat2("Offset (px)", ref offset, null, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    tool.RegisterUndo("Modify keyframe");
                    tool.activeKeyframe.offset = new Microsoft.Xna.Framework.Vector2((int)offset.X, (int)offset.Y);
                }

                if (ImGui.Button("Copy##offset"))
                {
                    tool.clipboard = offset;
                }
                ImGui.SameLine();
                if (ImGui.Button("Paste##offset"))
                {
                    if (tool.clipboard is Num.Vector2 newOffset)
                    {
                        tool.RegisterUndo("Modify keyframe");
                        tool.activeKeyframe.offset = new Microsoft.Xna.Framework.Vector2((int)newOffset.X, (int)newOffset.Y);
                    }
                }

                Num.Vector2 motionDelta = new Num.Vector2(tool.activeKeyframe.motionDelta.X, tool.activeKeyframe.motionDelta.Y);
                if (ImGui.InputFloat2("Motion Delta (px)", ref motionDelta, null, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    tool.RegisterUndo("Modify keyframe");
                    tool.activeKeyframe.motionDelta = new Microsoft.Xna.Framework.Vector2(motionDelta.X, motionDelta.Y);
                }

                if (ImGui.Button("Copy##motiondelta"))
                {
                    tool.clipboard = motionDelta;
                }
                ImGui.SameLine();
                if (ImGui.Button("Paste##motiondelta"))
                {
                    if (tool.clipboard is Num.Vector2 newMotionDelta)
                    {
                        tool.RegisterUndo("Modify keyframe");
                        tool.activeKeyframe.motionDelta = new Microsoft.Xna.Framework.Vector2((int)newMotionDelta.X, (int)newMotionDelta.Y);
                    }
                }
            }
        }
    }
}