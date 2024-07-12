namespace CritChanceStudio.Tools;

using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Num = System.Numerics;

public class ParticleToolViewport : ViewportWindow2D
{
    public ParticleToolViewport() : base()
    {
        this.name = "Viewport";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        var tool = (ParticleToolApp)ToolApp.instance;

        if (ImGui.Button("Reset"))
        {
            tool.particleSystem.Reset();
        }

        foreach (var emitter in tool.particleSystem.emitters)
        {
            ImGui.Text($"{emitter.emitter.name} - {emitter.emitter.MaxParticleCount} max particles ({emitter.emitter.ParticleCount} current)");
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var tool = (ParticleToolApp)ToolApp.instance;
        tool.particleSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    protected override void Render(RenderTarget2D target)
    {
        base.Render(target);

        var tool = (ParticleToolApp)ToolApp.instance;

        // draw origin circle
        DrawCircle(Num.Vector2.Zero, 2f, Color.White);

        tool.particleSystem.Render(CameraMatrix);
    }

    private void DrawCircle(Num.Vector2 pos, float radius, Color color, string label = null)
    {
        var drawList = ImGui.GetWindowDrawList();
        Num.Vector2 contentMin = ImGui.GetWindowContentRegionMin() + ImGui.GetWindowPos();
        Num.Vector2 contentMax = ImGui.GetWindowContentRegionMax() + ImGui.GetWindowPos();
        Num.Vector2 contentCenter = (contentMax + contentMin) / 2;
        Num.Vector2 offset = contentCenter - new Num.Vector2(cameraPos.X, cameraPos.Y);

        drawList.AddCircleFilled(offset + (pos * cameraZoom), radius, color.PackedValue);

        if (label != null)
        {
            drawList.AddText(offset + (pos * cameraZoom), color.PackedValue, label);
        }
    }
}