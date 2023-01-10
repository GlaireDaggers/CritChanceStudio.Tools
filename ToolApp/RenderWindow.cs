namespace CritChanceStudio.Tools;

using ImGuiNET;
using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class RenderWindow : EditorWindow
{
    protected RenderTarget2D _renderTarget;
    private System.IntPtr _renderTargetHandle;

    public RenderWindow() : base()
    {
    }

    public override void OnDrawWindow(ref bool open)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0f, 0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, new Num.Vector2(0f, 0f));
        base.OnDrawWindow(ref open);
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
    }

    public override void OnGUI()
    {
        base.OnGUI();
        Num.Vector2 contentMin = ImGui.GetWindowContentRegionMin();
        Num.Vector2 contentMax = ImGui.GetWindowContentRegionMax();
        Num.Vector2 windowPos = ImGui.GetWindowPos();
        contentMin += windowPos;
        contentMax += windowPos;

        // recreate render target as needed
        Num.Vector2 contentSize = contentMax - contentMin;
        if (_renderTarget == null || (int)contentSize.X != _renderTarget.Width || (int)contentSize.Y != _renderTarget.Height)
        {
            if (_renderTarget != null)
            {
                ToolApp.instance.imguiRenderer.UnbindTexture(_renderTargetHandle);
                _renderTarget.Dispose();
            }

            _renderTarget = new RenderTarget2D(ToolApp.instance.GraphicsDevice, (int)contentSize.X, (int)contentSize.Y, false, SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
            _renderTargetHandle = ToolApp.instance.imguiRenderer.BindTexture(_renderTarget);
        }

        ToolApp.instance.GraphicsDevice.SetRenderTarget(_renderTarget);
        Render(_renderTarget);
        ToolApp.instance.GraphicsDevice.SetRenderTarget(null);

        ImGui.GetWindowDrawList().AddImage(_renderTargetHandle, contentMin, contentMax);
    }

    protected virtual void Render(RenderTarget2D target)
    {
        ToolApp.instance.GraphicsDevice.Clear(new Color(0f, 0f, 0f, 0f));
    }

    public override void OnClosed()
    {
        base.OnClosed();
    }
}