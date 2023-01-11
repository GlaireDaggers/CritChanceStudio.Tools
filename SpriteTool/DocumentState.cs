namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

public interface ICloneable<T>
{
    T Clone();
}

public struct SpriteFrame : IEquatable<SpriteFrame>
{
    public string srcTexture;
    public Rectangle srcRect;

    public bool Equals(SpriteFrame other)
    {
        return srcTexture == other.srcTexture && srcRect == other.srcRect;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is SpriteFrame && Equals((SpriteFrame)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(srcTexture, srcRect);
    }
}

public struct Hitbox
{
    public string name;
    public Rectangle rect;
}

public struct Socket
{
    public string name;
    public Vector2 position;
}

public class Keyframe : ICloneable<Keyframe>
{
    public int duration = 100;
    public Vector2 offset = Vector2.Zero;
    public Vector2 motionDelta = Vector2.Zero;
    public SpriteFrame frame;
    public List<Hitbox> hitboxes = new List<Hitbox>();
    public List<Socket> sockets = new List<Socket>();
    public List<string> tags = new List<string>();

    public Keyframe Clone()
    {
        Keyframe clone = new Keyframe();
        clone.duration = duration;
        clone.offset = offset;
        clone.motionDelta = motionDelta;
        clone.frame = frame;
        clone.hitboxes = DocumentState.Clone(hitboxes);
        clone.sockets = DocumentState.Clone(sockets);
        clone.tags = DocumentState.Clone(tags);

        return clone;
    }
}

public class Animation : ICloneable<Animation>
{
    public string name;
    public bool looping;
    public List<Keyframe> keyframes = new List<Keyframe>();

    public Animation Clone()
    {
        Animation clone = new Animation();
        clone.name = name;
        clone.looping = looping;
        clone.keyframes = DocumentState.DeepClone(keyframes);
        return clone;
    }
}

public class DocumentState : ICloneable<DocumentState>
{
    public List<SpriteFrame> frames = new List<SpriteFrame>();
    public List<Animation> animations = new List<Animation>();

    public DocumentState Clone()
    {
        DocumentState clone = new DocumentState();
        clone.frames = Clone(frames);
        clone.animations = DeepClone(animations);
        return clone;
    }

    public void NormalizePaths()
    {
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.srcTexture = frame.srcTexture.Replace('\\', '/');
            frames[i] = frame;
        }

        foreach (var anim in animations)
        {
            foreach (var keyframe in anim.keyframes)
            {
                keyframe.frame.srcTexture = keyframe.frame.srcTexture.Replace('\\', '/');
            }
        }
    }

    public void MakePathsRelative(string rootDirectory)
    {
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.srcTexture = Path.GetRelativePath(rootDirectory, frame.srcTexture);
            frames[i] = frame;
        }

        foreach (var anim in animations)
        {
            foreach (var keyframe in anim.keyframes)
            {
                keyframe.frame.srcTexture = Path.GetRelativePath(rootDirectory, keyframe.frame.srcTexture);
            }
        }
    }

    public void MakePathsAbsolute(string rootDirectory)
    {
        rootDirectory = Path.GetFullPath(rootDirectory);

        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.srcTexture = Path.Combine(rootDirectory, frame.srcTexture);
            frames[i] = frame;
        }

        foreach (var anim in animations)
        {
            foreach (var keyframe in anim.keyframes)
            {
                keyframe.frame.srcTexture = Path.Combine(rootDirectory, keyframe.frame.srcTexture);
            }
        }
    }

    public static List<T> Clone<T>(List<T> src)
    {
        List<T> newList = new List<T>();
        newList.AddRange(src);

        return newList;
    }

    public static List<T> DeepClone<T>(List<T> src) where T : ICloneable<T>
    {
        List<T> newList = new List<T>(src.Count);

        for(int i = 0; i < src.Count; i++)
        {
            newList.Add((T)src[i].Clone());
        }

        return newList;
    }
}