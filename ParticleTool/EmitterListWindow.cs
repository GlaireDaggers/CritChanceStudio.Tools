namespace CritChanceStudio.Tools;

using CritChanceStudio.ParticleEngine;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class EmitterListWindow : EditorWindow
{
    public EmitterListWindow() : base()
    {
        name = "Emitter List";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        var tool = (ParticleToolApp)ToolApp.instance;

        if (ImGui.Button("New Emitter"))
        {
            var emitter = new ParticleSpriteRenderer
            {
                texture = tool.blank,
                blendMode = BlendMode.AlphaBlend,
                emitter = new ParticleEmitter()
                {
                    name = "Emitter",
                    minPosition = new Vector2(-64, -64),
                    maxPosition = new Vector2(64, 64),
                    emitParticleBurstInterval = 0.1f,
                },
            };
            emitter.Init(tool.GraphicsDevice);

            tool.particleSystem.emitters.Add(emitter);
        }

        for (int i = 0; i < tool.particleSystem.emitters.Count; i++)
        {
            var emitter = tool.particleSystem.emitters[i];

            ImGui.InputText("##emitter_" + i, ref emitter.emitter.name, 1024, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.SameLine();
            if (ImGui.Button("Edit##emitter_" + i))
            {
                tool.activeEmitter = emitter;
            }
            ImGui.SameLine();
            if (ImGui.Button("Delete##emitter_" + i))
            {
                ParticleSpriteRenderer renderer = (ParticleSpriteRenderer)tool.particleSystem.emitters[i];
                if (renderer.texture != tool.blank)
                {
                    renderer.texture.Dispose();
                }

                tool.particleSystem.emitters.RemoveAt(i--);
            }
        }
    }
}