using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace CritChanceStudio.ParticleEngine;

public class ParticleSystem
{
    public List<ParticleSpriteRenderer> emitters = new List<ParticleSpriteRenderer>();

    public void Update(float deltaTime)
    {
        for (int i = 0; i < emitters.Count; i++)
        {
            emitters[i].emitter.Update(deltaTime);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < emitters.Count; i++)
        {
            emitters[i].emitter.Reset();
        }
    }

    public void Render(Matrix viewModel)
    {
        for (int i = 0; i < emitters.Count; i++)
        {
            emitters[i].Render(viewModel);
        }
    }
}