namespace CritChanceStudio.Tools;

using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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