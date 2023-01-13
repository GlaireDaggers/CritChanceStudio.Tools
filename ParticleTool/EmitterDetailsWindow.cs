namespace CritChanceStudio.Tools;

using CritChanceStudio.ParticleEngine;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NativeFileDialogSharp;

using System;
using System.IO;

using Num = System.Numerics;

public class EmitterDetailsWindow : EditorWindow
{
    public EmitterDetailsWindow() : base()
    {
        name = "Emitter Details";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        var tool = (ParticleToolApp)ToolApp.instance;
        var activeEmitter = tool.activeEmitter as ParticleSpriteRenderer;

        if (activeEmitter == null)
        {
            ImGui.Text("Select an emitter to edit");
        }
        else
        {
            if (activeEmitter.texture == tool.blank)
            {
                ImGui.Text("Texture: None");
            }
            else
            {
                ImGui.Text("Texture: " + activeEmitter.texture.Name);
            }

            ImGui.SameLine();
            if (ImGui.Button("Browse"))
            {
                DialogResult result = Dialog.FileOpen("png");
                if (result.IsOk)
                {
                    try
                    {
                        using var stream = File.OpenRead(result.Path);
                        Texture2D tex = Texture2D.FromStream(tool.GraphicsDevice, stream);
                        tex.Name = result.Path;

                        if (activeEmitter.texture != tool.blank)
                        {
                            activeEmitter.texture.Dispose();
                        }

                        activeEmitter.texture = tex;
                    }
                    catch(System.Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            InputInt("Texture Rows", ref activeEmitter.rows, 1);
            InputInt("Texture Columns", ref activeEmitter.columns, 1);
            InputInt("Texture Cycles", ref activeEmitter.cycles, 1);
            InputFloat("Start Scale", ref activeEmitter.startScale, 0f);
            InputFloat("End Scale", ref activeEmitter.endScale, 0f);
            ColorField("Start Color", ref activeEmitter.startColor);
            ColorField("End Color", ref activeEmitter.endColor);
            BlendModeDropdown("Blend Mode", ref activeEmitter.blendMode);

            InputFloat("Start Delay", ref activeEmitter.emitter.startDelay, 0f);
            InputInt("Max Particle Count", ref activeEmitter.emitter.maxParticles, 1);
            InputInt("Emit Particles Per Burst", ref activeEmitter.emitter.emitParticleCountPerBurst, 1);
            InputInt("Number of Bursts", ref activeEmitter.emitter.emitParticleMaxBurstCount, 0);
            InputFloat("Burst Interval", ref activeEmitter.emitter.emitParticleBurstInterval, 0f);
            InputFloat("Min Lifetime", ref activeEmitter.emitter.minLifetime, 0f);
            InputFloat("Max Lifetime", ref activeEmitter.emitter.maxLifetime, 0f);
            DragVector2("Min Position", ref activeEmitter.emitter.minPosition);
            DragVector2("Max Position", ref activeEmitter.emitter.maxPosition);
            ImGui.InputFloat("Min Rotation", ref activeEmitter.emitter.minRotation);
            ImGui.InputFloat("Max Rotation", ref activeEmitter.emitter.maxRotation);
            DragVector2("Min Scale", ref activeEmitter.emitter.minScale);
            DragVector2("Max Scale", ref activeEmitter.emitter.maxScale);
            RandomRangeTypeDropdown("Scale Random Mode", ref activeEmitter.emitter.scaleRangeType);
            DragVector2("Min Velocity", ref activeEmitter.emitter.minVelocity);
            DragVector2("Max Velocity", ref activeEmitter.emitter.maxVelocity);
            RandomRangeTypeDropdown("Velocity Random Mode", ref activeEmitter.emitter.velocityRangeType);
            ImGui.InputFloat("Min Angular Velocity", ref activeEmitter.emitter.minAngularVelocity);
            ImGui.InputFloat("Max Angular Velocity", ref activeEmitter.emitter.maxAngularVelocity);
            DragVector2("Min Linear Force", ref activeEmitter.emitter.minLinearForce);
            DragVector2("Max Linear Force", ref activeEmitter.emitter.maxLinearForce);
            RandomRangeTypeDropdown("Linear Force Random Mode", ref activeEmitter.emitter.linearForceRangeType);
            ImGui.InputFloat("Min Angular Force", ref activeEmitter.emitter.minAngularForce);
            ImGui.InputFloat("Max Angular Force", ref activeEmitter.emitter.maxAngularForce);
            ColorField("Min Color", ref activeEmitter.emitter.minColor);
            ColorField("Max Color", ref activeEmitter.emitter.maxColor);
            RandomRangeTypeDropdown("Color Random Mode", ref activeEmitter.emitter.colorRangeType);
            ImGui.InputFloat("Drag", ref activeEmitter.emitter.drag);
            ImGui.InputFloat("Angular Drag", ref activeEmitter.emitter.angularDrag);
        }
    }

    string[] rangeTypeLabels = new string[] { "Non-uniform", "Uniform" };
    private bool RandomRangeTypeDropdown(string label, ref ParticleEmitter.RandomRangeType rangeType)
    {
        int idx = (int)rangeType;
        bool ret = ImGui.Combo(label, ref idx, rangeTypeLabels, rangeTypeLabels.Length);
        rangeType = (ParticleEmitter.RandomRangeType)idx;
        return ret;
    }

    string[] blendStateLabels = new string[] { "Alpha Blend", "Premultiplied", "Additive", "Opaque" };
    private bool BlendModeDropdown(string label, ref BlendMode blendMode)
    {
        int idx = (int)blendMode;
        bool ret = ImGui.Combo(label, ref idx, blendStateLabels, blendStateLabels.Length);
        blendMode = (BlendMode)idx;
        return ret;
    }

    private bool InputInt(string label, ref int value, int min)
    {
        bool ret = ImGui.InputInt(label, ref value);
        if (value < min) value = min;
        return ret;
    }

    private bool InputFloat(string label, ref float value, float min)
    {
        bool ret = ImGui.InputFloat(label, ref value);
        if (value < min) value = min;
        return ret;
    }

    private bool DragVector2(string label, ref Vector2 vector)
    {
        Num.Vector2 v = new Num.Vector2(vector.X, vector.Y);
        bool ret = ImGui.DragFloat2(label, ref v);
        vector = new Vector2(v.X, v.Y);
        return ret;
    }

    private bool ColorField(string label, ref Color color)
    {
        Vector4 v = color.ToVector4();
        Num.Vector4 v2 = new Num.Vector4(v.X, v.Y, v.Z, v.W);
        bool ret = ImGui.ColorEdit4(label, ref v2);
        color = new Color(new Vector4(v2.X, v2.Y, v2.Z, v2.W));
        return ret;
    }
}