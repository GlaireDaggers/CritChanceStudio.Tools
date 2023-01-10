namespace CritChanceStudio.Tools;

using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

public class SpriteToolViewport : ViewportWindow2D
{
    private Texture2D _testSprite;

    public SpriteToolViewport() : base()
    {
        this.name = "Viewport";
        _testSprite = ToolApp.instance.Content.Load<Texture2D>("content/leigh_sheet.png");
    }

    public override void OnGUI()
    {
        base.OnGUI();

        ImGui.Button("Play/Pause");
        ImGui.SameLine();
        ImGui.Button("Stop");

        bool looping = true;
        ImGui.Checkbox("Looping", ref looping);

        // draw hitbox overlay
        DrawRect(new Rectangle(0, 0, 64, 64), Color.Yellow);
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        var drawList = ImGui.GetWindowDrawList();
        Num.Vector2 contentMin = ImGui.GetWindowContentRegionMin() + ImGui.GetWindowPos();
        Num.Vector2 contentMax = ImGui.GetWindowContentRegionMax() + ImGui.GetWindowPos();
        Num.Vector2 contentCenter = (contentMax + contentMin) / 2;

        Num.Vector2 offset = contentCenter - new Num.Vector2(cameraPos.X, cameraPos.Y);
        Num.Vector2 topLeft = offset + new Num.Vector2(rect.Left, rect.Top) * cameraZoom;
        Num.Vector2 bottomRight = offset + new Num.Vector2(rect.Right, rect.Bottom) * cameraZoom;
        drawList.AddRect(topLeft, bottomRight, color.PackedValue);
        drawList.AddRectFilled(topLeft, bottomRight, new Color(color.R / 255f, color.G / 255f, color.B / 255f, (color.A / 255f) * 0.1f).PackedValue);
    }

    protected override void Render(RenderTarget2D target)
    {
        base.Render(target);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None,
            RasterizerState.CullNone, null, CameraMatrix);
        {
            spriteBatch.Draw(_testSprite, Vector2.Zero, Color.White);
        }
        spriteBatch.End();
    }
}