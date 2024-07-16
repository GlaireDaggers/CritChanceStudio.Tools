namespace CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NativeFileDialogSharp;
using System;
using Num = System.Numerics;

using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using System.IO;

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
            Texture2D texture = tool.textureManager.GetImageTexture(path);
            Rectangle srcRect = TrimTexture(texture);

            tool.RegisterUndo("Import Frames");
            tool.activeDocument.frames.Add(new SpriteFrame
            {
                source = SpriteFrameSource.Image,
                srcPath = path,
                srcRect = srcRect,
                offset = new Vector2(srcRect.X, srcRect.Y)
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

    public bool TryImportAseprite(string path)
    {
        SpriteToolApp tool = ToolApp.instance as SpriteToolApp;

        try
        {
            Texture2D[] frames = tool.textureManager.GetAsepriteFrames(path);
            tool.RegisterUndo("Import Frames");

            int frameBase = tool.activeDocument.frames.Count;

            for (int i = 0; i < frames.Length; i++)
            {
                Texture2D texture = frames[i];
                Rectangle srcRect = TrimTexture(texture);

                tool.activeDocument.frames.Add(new SpriteFrame
                {
                    source = SpriteFrameSource.AsepriteProject,
                    srcPath = path,
                    srcIndex = i,
                    srcRect = srcRect,
                    offset = new Vector2(srcRect.X, srcRect.Y)
                });
            }

            tool.ShowDialog("Import Animations", "Import tagged ranges from file as animations?", new string[] { "Yes", "No" }, (choice) =>
            {
                if (choice == 0)
                {
                    tool.RegisterUndo("Import Animations");

                    string asepritePath = Path.GetFullPath(path);
                    using (var stream = File.OpenRead(asepritePath))
                    {
                        AsepriteFile asepriteFile = AsepriteFileLoader.FromStream(path, stream, true, false);

                        // set up an animation for each tag
                        foreach (var tag in asepriteFile.Tags)
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
                                    duration = (int)asepriteFile.Frames[i].Duration.TotalMilliseconds,
                                    offset = new Vector2(-asepriteFile.CanvasWidth / 2, -asepriteFile.CanvasHeight / 2),
                                    frame = tool.activeDocument.frames[frameBase + i],
                                });
                            }

                            tool.activeDocument.animations.Add(anim);
                        }
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
                    ToolApp.instance.SetDragDropPayload(frame);
                    ImGui.Image(frame.GetImGuiHandle(tool.textureManager), new Num.Vector2(frame.srcRect.Width, frame.srcRect.Height));
                    ImGui.EndDragDropSource();
                }

                ImGui.SameLine();
                
                if (ImGui.Button("Delete"))
                {
                    tool.RegisterUndo("Delete frame");
                    tool.activeDocument.frames.RemoveAt(i--);
                }
            }

            ImGui.EndChild();
        }
    }

    private Rectangle TrimTexture(Texture2D texture)
    {
        // find tight-fitting src rect

        Color[] c = new Color[texture.Width * texture.Height];
        texture.GetData(c);

        int minJ = 0;
        for (int j = 0; j < texture.Height; j++)
        {
            for (int i = 0; i < texture.Width; i++)
            {
                if (c[i + (j * texture.Width)].A > 0)
                {
                    minJ = j;
                    j = texture.Height;
                    break;
                }
            }
        }

        int minI = texture.Width;
        int maxI = 0;

        int maxJ = texture.Height;
        for (int j = minJ; j < texture.Height; j++)
        {
            for (int i = 0; i < texture.Width; i++)
            {
                if (c[i + (j * texture.Width)].A > 0)
                {
                    minI = Math.Min(minI, i);
                    maxI = Math.Max(maxI, i);
                    maxJ = j;
                }
            }
        }

        return new Rectangle(minI, minJ, (maxI - minI) + 1, (maxJ - minJ) + 1);
    }
}