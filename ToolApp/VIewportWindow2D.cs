namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

using ImGuiNET;

public class ViewportWindow2D : RenderWindow
{
    public Num.Vector2 cameraPos = Num.Vector2.Zero;
    public float cameraZoom = 1.0f;

    public Matrix CameraMatrix => Matrix.CreateScale(cameraZoom) * Matrix.CreateTranslation(-cameraPos.X + (_renderTarget.Width / 2), -cameraPos.Y + (_renderTarget.Height / 2), 0f);

    public readonly SpriteBatch spriteBatch = new SpriteBatch(ToolApp.instance.GraphicsDevice);

    private bool _dragging = false;

    public ViewportWindow2D() : base()
    {
    }

    public override void OnClosed()
    {
        base.OnClosed();
        spriteBatch.Dispose();
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (ImGui.Button("-"))
        {
            cameraZoom /= 2;
            cameraZoom = (int)cameraZoom;
            if (cameraZoom < 1f)
            {
                cameraZoom = 1f;
            }
        }
        ImGui.SameLine();
        ImGui.Text($"Zoom: {(int)(cameraZoom * 100)}%%");
        ImGui.SameLine();
        if (ImGui.Button("+"))
        {
            cameraZoom *= 2;
            cameraZoom = (int)cameraZoom;
            if (cameraZoom > 16f)
            {
                cameraZoom = 16f;
            }
        }
    }

    protected override void Render(RenderTarget2D target)
    {
        // draw grid
        var drawList = ImGui.GetWindowDrawList();

        Num.Vector2 contentMin = ImGui.GetWindowContentRegionMin();
        Num.Vector2 contentMax = ImGui.GetWindowContentRegionMax();
        Num.Vector2 windowPos = ImGui.GetWindowPos();
        contentMin += windowPos;
        contentMax += windowPos;
        Num.Vector2 contentSize = contentMax - contentMin;

        ImGui.InvisibleButton(name + "__drag", contentSize, ImGuiButtonFlags.MouseButtonRight);
        ImGui.SetItemAllowOverlap();
        if (ImGui.IsItemActivated())
        {
            _dragging = true;
        }

        if (_dragging)
        {
            cameraPos -= ImGui.GetIO().MouseDelta;
            _dragging = ImGui.IsMouseDown(ImGuiMouseButton.Right);
        }

        ImGui.SetCursorScreenPos(contentMin);

        int cellSize = (int)cameraZoom * 32;

        Num.Vector2 centerPos = (contentMax + contentMin) * 0.5f;
        centerPos.X -= cameraPos.X % cellSize;
        centerPos.Y -= cameraPos.Y % cellSize;

        int columns = (int)contentSize.X / cellSize;
        int rows = (int)contentSize.Y / cellSize;

        for (int i = -columns / 2; i <= columns / 2; i++)
        {
            float x = centerPos.X + (i * cellSize);
            drawList.AddLine(new Num.Vector2(x, contentMin.Y), new Num.Vector2(x, contentMax.Y), new Color(1f, 1f, 1f, 0.1f).PackedValue);
        }

        for (int i = -rows / 2; i <= rows / 2; i++)
        {
            float y = centerPos.Y + (i * cellSize);
            drawList.AddLine(new Num.Vector2(contentMin.X, y), new Num.Vector2(contentMax.X, y), new Color(1f, 1f, 1f, 0.1f).PackedValue);
        }

        base.Render(target);
    }
}