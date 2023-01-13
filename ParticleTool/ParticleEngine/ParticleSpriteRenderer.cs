using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json;

namespace CritChanceStudio.ParticleEngine;

public enum BlendMode
{
    AlphaBlend,
    Premultiplied,
    Additive,
    Opaque,
}

public class ParticleSpriteRenderer : ParticleRenderer
{
    public Texture2D texture;
    public int rows = 1;
    public int columns = 1;
    public int cycles = 1;

    public Color startColor = Color.White;
    public Color endColor = Color.White;

    public float startScale = 1f;
    public float endScale = 1f;

    public BlendMode blendMode = BlendMode.AlphaBlend;

    [JsonIgnore]
    public SamplerState samplerState = SamplerState.PointWrap;

    private SpriteBatch _spriteBatch;

    public void Init(GraphicsDevice graphicsDevice)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public override void Render(Matrix viewModel)
    {
        if (texture == null) return;

        BlendState blendState = BlendState.NonPremultiplied;

        switch (blendMode)
        {
            case BlendMode.AlphaBlend:
                blendState = BlendState.NonPremultiplied;
                break;
            case BlendMode.Premultiplied:
                blendState = BlendState.AlphaBlend;
                break;
            case BlendMode.Additive:
                blendState = BlendState.Additive;
                break;
            case BlendMode.Opaque:
                blendState = BlendState.Opaque;
                break;
        }

        _spriteBatch.Begin(SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.None, RasterizerState.CullNone, null, viewModel);
        {
            Rectangle cellRect = new Rectangle(0, 0, texture.Width / columns, texture.Height / columns);
            Vector2 particleOrigin = new Vector2(cellRect.Width / 2, cellRect.Height / 2);

            int totalFrames = rows * columns;

            ParticleEmitter.Particle[] particles = emitter.Particles;
            for (int i = 0; i < emitter.ParticleCount; i++)
            {
                float normalizedAge = particles[i].lifetime / particles[i].maxLifetime;
                float scale = MathHelper.Lerp(startScale, endScale, normalizedAge);
                Color tint = Color.Lerp(startColor, endColor, normalizedAge);
                int frameNum = (int)(normalizedAge * totalFrames * cycles);
                int frameColumn = frameNum % columns;
                int frameRow = frameNum / columns;

                Vector4 particleCol = tint.ToVector4() * particles[i].tint.ToVector4();

                _spriteBatch.Draw(
                    texture,
                    particles[i].position,
                    new Rectangle(cellRect.Width * frameColumn, cellRect.Height * frameRow, cellRect.Width, cellRect.Height),
                    new Color(particleCol),
                    MathHelper.ToRadians(particles[i].rotation),
                    particleOrigin,
                    scale,
                    SpriteEffects.None,
                    0f);
            }
        }
        _spriteBatch.End();
    }
}