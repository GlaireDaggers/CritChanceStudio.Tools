namespace CritChanceStudio.Tools;

using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class TestWindow : ViewportWindow2D
{
    private Texture2D _testSprite;

    public TestWindow() : base()
    {
        this.name = "Test Window";
        _testSprite = ToolApp.instance.Content.Load<Texture2D>("content/leigh_sheet.png");
    }

    public override void OnGUI()
    {
        base.OnGUI();
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