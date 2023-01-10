namespace CritChanceStudio.Tools;

using ImGuiNET;
using Num = System.Numerics;
using Microsoft.Xna.Framework;

public class HitboxListWindow : EditorWindow
{
    public HitboxListWindow() : base()
    {
        this.name = "Hitbox List";
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
            if (ImGui.Button("Add Hitbox"))
            {
                tool.RegisterUndo("Add hitbox");
                tool.activeKeyframe.hitboxes.Add(new Hitbox
                {
                    name = "Hitbox",
                    rect = new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32)
                });
            }

            if (ImGui.BeginChild("_hitbox_list"))
            {
                for (int i = 0; i < tool.activeKeyframe.hitboxes.Count; i++)
                {
                    var hitbox = tool.activeKeyframe.hitboxes[i];

                    string hitboxName = hitbox.name;
                    if (ImGui.InputText("##hitbox_" + i + "_name", ref hitboxName, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        tool.RegisterUndo("Rename Hitbox");
                        hitbox.name = hitboxName;
                        tool.activeKeyframe.hitboxes[i] = hitbox;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Delete"))
                    {
                        tool.RegisterUndo("Delete Hitbox");
                        tool.activeKeyframe.hitboxes.RemoveAt(i--);
                    }
                    else
                    {
                        Num.Vector2 rectXY = new Num.Vector2(hitbox.rect.X, hitbox.rect.Y);
                        Num.Vector2 rectWH = new Num.Vector2(hitbox.rect.Width, hitbox.rect.Height);

                        if (ImGui.InputFloat2("Position ##hitbox_" + i, ref rectXY, null, ImGuiInputTextFlags.EnterReturnsTrue))
                        {
                            tool.RegisterUndo("Modify hitbox");
                            hitbox.rect.X = (int)rectXY.X;
                            hitbox.rect.Y = (int)rectXY.Y;
                            tool.activeKeyframe.hitboxes[i] = hitbox;
                        }
                        if (ImGui.InputFloat2("Size ##hitbox_" + i, ref rectWH, null, ImGuiInputTextFlags.EnterReturnsTrue))
                        {
                            tool.RegisterUndo("Modify hitbox");
                            hitbox.rect.Width = (int)rectWH.X;
                            hitbox.rect.Height = (int)rectWH.Y;
                            tool.activeKeyframe.hitboxes[i] = hitbox;
                        }
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}