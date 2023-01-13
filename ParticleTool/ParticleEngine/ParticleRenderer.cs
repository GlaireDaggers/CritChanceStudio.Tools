namespace CritChanceStudio.ParticleEngine;

using Microsoft.Xna.Framework;

using System;

public abstract class ParticleRenderer : IDisposable
{
    public ParticleEmitter emitter;

    public abstract void Render(Matrix viewModel);
    
    public virtual void Dispose()
    {
    }
}