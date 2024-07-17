namespace CritChanceStudio.Tools;

using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

public class SpriteToolViewport : ViewportWindow2D
{
    public int FrameIndex
    {
        get => _frame;
        set {
            _frame = value;
            _frameTimer = 0f;
            _isPlaying = false;
            _posPreview = Vector2.Zero;
        }
    }

    private int _frame = 0;
    private float _frameTimer = 0f;
    private bool _isPlaying = false;
    private Vector2 _posPreview = Vector2.Zero;

    public SpriteToolViewport() : base()
    {
        this.name = "Viewport";
    }

    public override void OnGUI()
    {
        base.OnGUI();

        var tool = (SpriteToolApp)ToolApp.instance;

        // draw origin marker
        DrawCircle(new Num.Vector2(_posPreview.X, _posPreview.Y), 2f, Color.White);

        // draw hitboxes & sockets
        if (tool.activeAnimation != null && tool.activeAnimation.keyframes.Count > 0)
        {
            foreach (var hitbox in tool.activeAnimation.keyframes[_frame].hitboxes)
            {
                Rectangle r = hitbox.rect;
                r.X += (int)_posPreview.X;
                r.Y += (int)_posPreview.Y;

                DrawRect(r, Color.Yellow, hitbox.name);
            }

            foreach (var socket in tool.activeAnimation.keyframes[_frame].sockets)
            {
                Vector2 pos = socket.position + _posPreview;
                DrawCircle(new Num.Vector2(pos.X, pos.Y), 2f, Color.White, socket.name);
            }
        }

        if (ImGui.Button("Play/Pause"))
        {
            _isPlaying = !_isPlaying;
        }

        ImGui.SameLine();

        if (ImGui.Button("Stop"))
        {
            _isPlaying = false;
            _frame = 0;
            _frameTimer = 0f;
            _posPreview = Vector2.Zero;
        }

        // draw tag list
        if (tool.activeAnimation != null && tool.activeAnimation.keyframes.Count > 0)
        {
            foreach (var tag in tool.activeAnimation.keyframes[_frame].tags)
            {
                ImGui.Text(tag);
            }
        }
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

    private void DrawRect(Rectangle rect, Color color, string label = null)
    {
        var drawList = ImGui.GetWindowDrawList();
        Num.Vector2 contentMin = ImGui.GetWindowContentRegionMin() + ImGui.GetWindowPos();
        Num.Vector2 contentMax = ImGui.GetWindowContentRegionMax() + ImGui.GetWindowPos();
        Num.Vector2 contentCenter = (contentMax + contentMin) / 2;

        Num.Vector2 offset = contentCenter - new Num.Vector2(cameraPos.X, cameraPos.Y);
        Num.Vector2 topLeft = offset + new Num.Vector2(rect.Left, rect.Top) * cameraZoom;
        Num.Vector2 bottomRight = offset + new Num.Vector2(rect.Right, rect.Bottom) * cameraZoom;
        topLeft.X = (int)topLeft.X;
        topLeft.Y = (int)topLeft.Y;
        bottomRight.X = (int)bottomRight.X;
        bottomRight.Y = (int)bottomRight.Y;
        drawList.AddRect(topLeft, bottomRight, color.PackedValue);
        drawList.AddRectFilled(topLeft, bottomRight, new Color(color.R / 255f, color.G / 255f, color.B / 255f, (color.A / 255f) * 0.1f).PackedValue);

        if (label != null)
        {
            drawList.AddText(topLeft, color.PackedValue, label);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var tool = (SpriteToolApp)ToolApp.instance;
        if (tool.activeAnimation == null || tool.activeAnimation.keyframes.Count == 0) return;

        if (_frame >= tool.activeAnimation.keyframes.Count)
        {
            _frame = 0;
        }

        Keyframe keyframe = tool.activeAnimation.keyframes[_frame];

        if (_isPlaying)
        {
            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _posPreview += (float)gameTime.ElapsedGameTime.TotalSeconds * keyframe.motionDelta;

            if (_frameTimer >= (keyframe.duration / 1000.0f))
            {
                _frameTimer = 0f;
                _frame++;

                if (_frame >= tool.activeAnimation.keyframes.Count)
                {
                    _frame = 0;
                    _posPreview = Vector2.Zero;
                }
            }
        }
    }

    protected override void Render(RenderTarget2D target)
    {
        base.Render(target);

        var tool = (SpriteToolApp)ToolApp.instance;
        if (tool.activeAnimation == null || tool.activeAnimation.keyframes.Count == 0) return;

        if (_frame >= tool.activeAnimation.keyframes.Count)
        {
            _frame = 0;
            _frameTimer = 0f;
            _posPreview = Vector2.Zero;
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None,
            RasterizerState.CullNone, null, CameraMatrix);
        {
            Keyframe keyframe = tool.activeAnimation.keyframes[_frame];
            var frame = tool.activeDocument.frames[keyframe.frameIdx];

            Texture2D tex = frame.GetTexture(tool.textureManager);

            SpriteEffects flip = SpriteEffects.None;
            Vector2 frameOffset = frame.offset;

            // note: we also need to "mirror" frame offset when sprite is flipped

            if (keyframe.mirrorX)
            {
                flip |= SpriteEffects.FlipHorizontally;
                frameOffset.X = frame.size.X - frame.offset.X - frame.srcRect.Width;
            }

            if (keyframe.mirrorY)
            {
                flip |= SpriteEffects.FlipVertically;
                frameOffset.Y = frame.size.Y - frame.offset.Y - frame.srcRect.Height;
            }

            spriteBatch.Draw(tex, keyframe.offset + frameOffset + _posPreview, frame.srcRect, Color.White, 0.0f,
                Vector2.Zero, 1.0f, flip, 0.0f);
        }
        spriteBatch.End();
    }
}