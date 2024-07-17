namespace CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NativeFileDialogSharp;
using System;
using Num = System.Numerics;

public struct SpriteFramePayload
{
    public int idx;
}

public class FrameListWindow : EditorWindow
{
    public FrameListWindow() : base()
    {
        this.name = "Sprite Frames";
    }

    public bool TryImportFrame(string path)
    {
        SpriteToolApp tool = ToolApp.instance as SpriteToolApp;

        try
        {
            tool.RegisterUndo("Import Frames");

            var frame = new SpriteFrame
            {
                source = SpriteFrameSource.Image,
                srcPath = path,
            };
            frame.CalcMetrics(tool.textureManager);

            tool.activeDocument.frames.Add(frame);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            tool.ShowDialog("Error", "Failed importing frame: " + e.Message, new string[] { "Ok" });
            return false;
        }

        return true;
    }

    public bool TryImportAseprite(string path)
    {
        SpriteToolApp tool = ToolApp.instance as SpriteToolApp;

        var asepriteProject = tool.textureManager.GetAsepriteFile(path);

        try
        {
            tool.RegisterUndo("Import Frames");

            int frameBase = tool.activeDocument.frames.Count;

            for (int i = 0; i < asepriteProject.FrameCount; i++)
            {
                var frame = new SpriteFrame
                {
                    source = SpriteFrameSource.AsepriteProject,
                    srcPath = path,
                    srcIndex = i
                };
                frame.CalcMetrics(tool.textureManager);

                tool.activeDocument.frames.Add(frame);
            }

            tool.ShowDialog("Import Animations", "Import tagged ranges from file as animations?", new string[] { "Yes", "No" }, (choice) =>
            {
                if (choice == 0)
                {
                    tool.RegisterUndo("Import Animations");

                    // set up an animation for each tag
                    foreach (var tag in asepriteProject.Tags)
                    {
                        Animation anim = new Animation
                        {
                            name = tag.Name,
                            looping = tag.Repeat == 0,
                        };

                        for (int i = tag.From; i <= tag.To; i++)
                        {
                            anim.keyframes.Add(new Keyframe
                            {
                                duration = (int)asepriteProject.Frames[i].Duration.TotalMilliseconds,
                                offset = new Vector2(-asepriteProject.CanvasWidth / 2, -asepriteProject.CanvasHeight / 2),
                                frameIdx = frameBase + i,
                            });
                        }

                        tool.activeDocument.animations.Add(anim);
                    }
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            tool.ShowDialog("Error", "Failed importing frame: " + e.Message, new string[] { "Ok" });
            return false;
        }

        return true;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        SpriteToolApp tool = ToolApp.instance as SpriteToolApp;

        if (ImGui.Button("Import Frame"))
        {
            tool.FileOpenMultiple("png", onOpen: (paths) =>
            {
                foreach (var infile in paths)
                {
                    Console.WriteLine("Opened file: " + infile);
                    TryImportFrame(infile);
                }
            });
        }

        if (ImGui.BeginChild("_frame_list"))
        {
            for (int i = 0; i < tool.activeDocument.frames.Count; i++)
            {
                var frame = tool.activeDocument.frames[i];
                Texture2D tex = frame.GetTexture(tool.textureManager);
                Num.Vector2 uvMin = new Num.Vector2(frame.srcRect.Left / (float)tex.Width, frame.srcRect.Top / (float)tex.Height);
                Num.Vector2 uvMax = new Num.Vector2(frame.srcRect.Right / (float)tex.Width, frame.srcRect.Bottom / (float)tex.Height);

                ImGui.ImageButton("_frame_" + i, frame.GetImGuiHandle(tool.textureManager), new Num.Vector2(frame.srcRect.Width, frame.srcRect.Height),
                    uvMin, uvMax);

                if (ImGui.IsItemHovered())
                {
                    if (frame.source == SpriteFrameSource.AsepriteProject)
                    {
                        ImGui.SetTooltip($"{frame.srcPath} [{frame.srcIndex}]");
                    }
                    else
                    {
                        ImGui.SetTooltip(frame.srcPath);
                    }
                }

                if (ImGui.BeginDragDropSource())
                {
                    // begin drag-drop operation with this SpriteFrame
                    ToolApp.instance.SetDragDropPayload(new SpriteFramePayload {
                        idx = i
                    });
                    ImGui.Image(frame.GetImGuiHandle(tool.textureManager), new Num.Vector2(frame.srcRect.Width, frame.srcRect.Height));
                    ImGui.EndDragDropSource();
                }

                ImGui.SameLine();
                
                if (ImGui.Button("Delete"))
                {
                    // check if any animations still refer to this frame
                    if (FrameUsed(i, tool.activeDocument))
                    {
                        int frameIdx = i;
                        tool.ShowDialog("Delete used frame",
                            "One or more animations still refer to this frame. Are you sure you want to delete it? (keyframes referencing this frame will be deleted)",
                            new string[] { "Yes", "No" }, (choice) =>
                        {
                            if (choice == 0)
                            {
                                tool.RegisterUndo("Delete frame");
                                DeleteFrame(tool.activeDocument, frameIdx);
                            }
                        });
                    }
                    else
                    {
                        tool.RegisterUndo("Delete frame");
                        DeleteFrame(tool.activeDocument, i--);
                    }
                }
            }

            ImGui.EndChild();
        }
    }

    private void DeleteFrame(DocumentState doc, int idx)
    {
        // fix up frame IDs

        foreach (var anim in doc.animations)
        {
            for (int j = 0; j < anim.keyframes.Count; j++)
            {
                if (anim.keyframes[j].frameIdx > idx) {
                    anim.keyframes[j].frameIdx--;
                }
                else if (anim.keyframes[j].frameIdx == idx) {
                    anim.keyframes.RemoveAt(j);
                }
            }
        }

        doc.frames.RemoveAt(idx);
    }

    private bool FrameUsed(int idx, DocumentState doc)
    {
        foreach (var anim in doc.animations)
        {
            for (int j = 0; j < anim.keyframes.Count; j++)
            {
                if (anim.keyframes[j].frameIdx == idx)
                {
                    return true;
                }
            }
        }

        return false;
    }
}