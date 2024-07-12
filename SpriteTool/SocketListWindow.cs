namespace CritChanceStudio.Tools;

using ImGuiNET;
using Num = System.Numerics;
using Microsoft.Xna.Framework;

public class SocketListWindow : EditorWindow
{
    public SocketListWindow() : base()
    {
        this.name = "Socket List";
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
            if (ImGui.Button("Add Socket"))
            {
                tool.RegisterUndo("Add socket");
                tool.activeKeyframe.sockets.Add(new Socket
                {
                    name = "Socket",
                    position = Vector2.Zero,
                });
            }

            if (ImGui.BeginChild("_socket_list"))
            {
                for (int i = 0; i < tool.activeKeyframe.sockets.Count; i++)
                {
                    var socket = tool.activeKeyframe.sockets[i];

                    string socketName = socket.name;
                    if (ImGui.InputText("##socket_" + i + "_name", ref socketName, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        tool.RegisterUndo("Rename Socket");
                        socket.name = socketName;
                        tool.activeKeyframe.sockets[i] = socket;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Delete##socket_" + i))
                    {
                        tool.RegisterUndo("Delete Socket");
                        tool.activeKeyframe.sockets.RemoveAt(i--);
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Copy##socket_" + i))
                        {
                            tool.clipboard = socket;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Paste##socket_" + i))
                        {
                            if (tool.clipboard is Socket paste)
                            {
                                tool.RegisterUndo("Modify socket");
                                socket = paste;
                                tool.activeKeyframe.sockets[i] = socket;
                            }
                        }

                        Num.Vector2 posXY = new Num.Vector2(socket.position.X, socket.position.Y);
                        
                        if (ImGui.InputFloat2("Position ##socket_" + i, ref posXY, null, ImGuiInputTextFlags.EnterReturnsTrue))
                        {
                            tool.RegisterUndo("Modify socket");
                            socket.position.X = (int)posXY.X;
                            socket.position.Y = (int)posXY.Y;
                            tool.activeKeyframe.sockets[i] = socket;
                        }
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}