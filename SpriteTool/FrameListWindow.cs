namespace CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NativeFileDialogSharp;
using System;
using Num = System.Numerics;

public class FrameListWindow : EditorWindow
{
    public FrameListWindow() : base()
    {
        this.name = "Sprite Frames";
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
                    try
                    {
                        Texture2D texture = tool.textureManager.GetTexture(infile);

                        tool.RegisterUndo("Import Frames");
                        tool.activeDocument.frames.Add(new SpriteFrame
                        {
                            srcTexture = infile,
                            srcRect = new Rectangle(0, 0, texture.Width, texture.Height),
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        tool.ShowDialog("Error", "Failed importing frames: " + e.Message, new string[] { "Ok" });
                    }
                }
            });
        }

        if (ImGui.BeginChild("_frame_list"))
        {
            for (int i = 0; i < tool.activeDocument.frames.Count; i++)
            {
                var frame = tool.activeDocument.frames[i];
                Texture2D tex = tool.textureManager.GetTexture(frame.srcTexture);
                Num.Vector2 uvMin = new Num.Vector2(frame.srcRect.Left / (float)tex.Width, frame.srcRect.Top / (float)tex.Height);
                Num.Vector2 uvMax = new Num.Vector2(frame.srcRect.Right / (float)tex.Width, frame.srcRect.Bottom / (float)tex.Height);

                ImGui.ImageButton("_frame_" + i, tool.textureManager.GetImGuiHandle(frame.srcTexture), new Num.Vector2(frame.srcRect.Width, frame.srcRect.Height),
                    uvMin, uvMax);

                if (ImGui.BeginDragDropSource())
                {
                    // begin drag-drop operation with this SpriteFrame
                    ToolApp.instance.SetDragDropPayload(frame);
                    ImGui.Image(tool.textureManager.GetImGuiHandle(frame.srcTexture), new Num.Vector2(frame.srcRect.Width, frame.srcRect.Height));
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
}