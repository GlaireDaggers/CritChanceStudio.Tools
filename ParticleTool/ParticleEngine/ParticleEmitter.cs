namespace CritChanceStudio.ParticleEngine;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;

using System;

public class ParticleEmitter
{
    public struct Particle
    {
        public int id;
        public float lifetime;
        public float maxLifetime;
        public Color tint;
        public Vector2 position;
        public float rotation;
        public Vector2 scale;
        public Vector2 velocity;
        public float angularVelocity;
        public Vector2 force;
        public float angularForce;
    }

    public enum RandomRangeType
    {
        NonUniform,
        Uniform
    }

    [JsonIgnore]
    public int ParticleCount => _particleCount;

    [JsonIgnore]
    public int MaxParticleCount => _maxParticleCount;

    [JsonIgnore]
    public Particle[] Particles => _particles;

    public string name = "Emitter";

    public float startDelay = 0f;
    public int maxParticles = 1024;

    public int emitParticleCountPerBurst = 1;
    public int emitParticleMaxBurstCount = 0;
    public float emitParticleBurstInterval = 0.1f;

    public float minLifetime = 1f;
    public float maxLifetime = 1f;

    public Vector2 minPosition = Vector2.Zero;
    public Vector2 maxPosition = Vector2.Zero;

    public float positionMinRadius = 0f;
    public float positionMaxRadius = 0f;

    public float minRotation = 0f;
    public float maxRotation = 0f;

    public Vector2 minScale = Vector2.One;
    public Vector2 maxScale = Vector2.One;
    public RandomRangeType scaleRangeType = RandomRangeType.NonUniform;

    public Vector2 minVelocity = Vector2.Zero;
    public Vector2 maxVelocity = Vector2.Zero;
    public RandomRangeType velocityRangeType = RandomRangeType.NonUniform;

    public float minAngularVelocity = 0f;
    public float maxAngularVelocity = 0f;

    public Vector2 minLinearForce = Vector2.Zero;
    public Vector2 maxLinearForce = Vector2.Zero;
    public RandomRangeType linearForceRangeType = RandomRangeType.NonUniform;

    public float minAngularForce = 0f;
    public float maxAngularForce = 0f;

    public Color minColor = Color.White;
    public Color maxColor = Color.White;
    public RandomRangeType colorRangeType = RandomRangeType.NonUniform;

    public float drag = 0f;
    public float angularDrag = 0f;

    public Vector2 radialImpulseOrigin = Vector2.Zero;
    public float radialImpulseMin = 0f;
    public float radialImpulseMax = 0f;

    public Vector2 radialForceOrigin = Vector2.Zero;
    public float radialForce = 0f;

    private Particle[] _particles;
    private int _particleCount;
    private int _maxParticleCount;

    private float _delayTimer = 0f;

    private int _bursts = 0;
    private float _burstTimer = 0f;

    private Random _rng = new Random();
    private int _nextId = 0;

    public void Reset()
    {
        _particleCount = 0;
        _maxParticleCount = 0;
        _bursts = 0;
        _burstTimer = 0f;
        _delayTimer = 0f;
    }

    public void Update(float deltaTime)
    {
        if (_particles == null)
        {
            _particles = new Particle[maxParticles];
        }
        else if (_particles.Length != maxParticles)
        {
            Array.Resize(ref _particles, maxParticles);
        }

        if (_particleCount > _particles.Length)
        {
            _particleCount = _particles.Length;
        }

        if (_delayTimer < startDelay)
        {
            _delayTimer += deltaTime;
            return;
        }

        if (_particleCount > _maxParticleCount)
        {
            _maxParticleCount = _particleCount;
        }

        // emit new particles
        if (_bursts < emitParticleMaxBurstCount || emitParticleMaxBurstCount == 0)
        {
            _burstTimer -= deltaTime;

            if (_burstTimer <= 0f)
            {
                // emit a new burst
                _burstTimer = emitParticleBurstInterval;
                _bursts++;
                for (int i = 0; i < emitParticleCountPerBurst; i++)
                {
                    if (_particleCount == maxParticles)
                    {
                        // remove oldest particle to make room
                        RemoveParticle(GetOldestParticle());
                    }

                    Particle p = new Particle();
                    p.id = _nextId++;
                    p.lifetime = 0f;
                    p.maxLifetime = RandomRange(minLifetime, maxLifetime);
                    p.tint = RandomRange(minColor, maxColor, colorRangeType);
                    p.position = RandomRange(minPosition, maxPosition, RandomRangeType.NonUniform);
                    p.rotation = RandomRange(minRotation, maxRotation);
                    p.scale = RandomRange(minScale, maxScale, scaleRangeType);
                    p.velocity = RandomRange(minVelocity, maxVelocity, velocityRangeType);
                    p.angularVelocity = RandomRange(minAngularVelocity, maxAngularVelocity);
                    p.force = RandomRange(minLinearForce, maxLinearForce, linearForceRangeType);
                    p.angularForce = RandomRange(minAngularForce, maxAngularForce);

                    Vector2 randPos = Vector2.Transform(Vector2.UnitX, Matrix.CreateRotationZ(RandomRange(0f, MathF.Tau)));
                    randPos *= RandomRange(positionMinRadius, positionMaxRadius);
                    p.position += randPos;

                    Vector2 radialImpulseDir = p.position - radialImpulseOrigin;
                    if (radialImpulseDir.LengthSquared() > float.Epsilon)
                    {
                        p.velocity += (radialImpulseDir / radialImpulseDir.Length()) * RandomRange(radialImpulseMin, radialImpulseMax);
                    }

                    _particles[_particleCount++] = p;
                }
            }
        }

        // simulate existing particles
        for (int i = 0; i < _particleCount; i++)
        {
            Particle p = _particles[i];
            p.lifetime += deltaTime;
            p.position += p.velocity * deltaTime;
            p.rotation += p.angularVelocity * deltaTime;
            p.velocity += p.force * deltaTime;
            p.angularVelocity += p.angularForce * deltaTime;

            p.velocity -= (p.velocity * drag * deltaTime);
            p.angularVelocity -= (p.angularVelocity * angularDrag * deltaTime);

            Vector2 radialForceDir = p.position - radialForceOrigin;
            if (radialForceDir.LengthSquared() > float.Epsilon)
            {
                p.velocity += (radialForceDir / radialForceDir.Length()) * radialForce * deltaTime;
            }

            _particles[i] = p;

            if (p.lifetime >= p.maxLifetime)
            {
                RemoveParticle(i--);
            }
        }
    }

    private Vector2 RandomRange(Vector2 min, Vector2 max, RandomRangeType rangeType)
    {
        if (rangeType == RandomRangeType.Uniform)
        {
            return Vector2.Lerp(min, max, (float)_rng.NextDouble());
        }

        return new Vector2(
            RandomRange(min.X, max.X),
            RandomRange(min.Y, max.Y)
            );
    }

    private Color RandomRange(Color min, Color max, RandomRangeType rangeType)
    {
        if (rangeType == RandomRangeType.Uniform)
        {
            return Color.Lerp(min, max, (float)_rng.NextDouble());
        }

        Vector4 lhs = min.ToVector4();
        Vector4 rhs = max.ToVector4();

        return new Color(
            RandomRange(lhs.X, rhs.X),
            RandomRange(lhs.Y, rhs.Y),
            RandomRange(lhs.Z, rhs.Z),
            RandomRange(lhs.W, rhs.W)
            );
    }

    private float RandomRange(float min, float max)
    {
        return MathHelper.Lerp(min, max, (float)_rng.NextDouble());
    }

    private void RemoveParticle(int index)
    {
        // swap last particle into this position & decrement particle count
        _particles[index] = _particles[--_particleCount];
    }

    private int GetOldestParticle()
    {
        int oldest = -1;
        float oldestLifetime = 0f;

        for (int i = 0; i < _particleCount; i++)
        {
            if (_particles[i].lifetime >= oldestLifetime)
            {
                oldest = i;
                oldestLifetime = _particles[i].lifetime;
            }
        }

        return oldest;
    }
}