namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

public interface ICloneable<T>
{
    T Clone();
}

public enum SpriteFrameSource
{
    Image,
    AsepriteProject
}

public struct SpriteFrame : IEquatable<SpriteFrame>
{
    public SpriteFrameSource source;
    public string srcPath;
    public int srcIndex;
    public Rectangle srcRect;
    public Vector2 offset;

    public bool Equals(SpriteFrame other)
    {
        return source == other.source && srcIndex == other.srcIndex && srcPath == other.srcPath && srcRect == other.srcRect && offset == other.offset;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is SpriteFrame && Equals((SpriteFrame)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(source, srcPath, srcIndex, srcRect, offset);
    }

    public Texture2D GetTexture(TextureManager texManager)
    {
        // FIXME:
        // if you import an aseprite project and then remove frames, this will crash
        // need to figure out how to handle that case... some kind of dummy "error" placeholder?

        switch (source)
        {
            case SpriteFrameSource.Image: return texManager.GetImageTexture(srcPath);
            case SpriteFrameSource.AsepriteProject: return texManager.GetAsepriteFrames(srcPath)[srcIndex];
        }

        return null;
    }

    public nint GetImGuiHandle(TextureManager texManager)
    {
        return texManager.GetImGuiHandle(GetTexture(texManager));
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
            frame.srcPath = frame.srcPath.Replace('\\', '/');
            frames[i] = frame;
        }

        foreach (var anim in animations)
        {
            foreach (var keyframe in anim.keyframes)
            {
                keyframe.frame.srcPath = keyframe.frame.srcPath.Replace('\\', '/');
            }
        }
    }

    public void MakePathsRelative(string rootDirectory)
    {
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.srcPath = Path.GetRelativePath(rootDirectory, frame.srcPath);
            frames[i] = frame;
        }

        foreach (var anim in animations)
        {
            foreach (var keyframe in anim.keyframes)
            {
                keyframe.frame.srcPath = Path.GetRelativePath(rootDirectory, keyframe.frame.srcPath);
            }
        }
    }

    public void MakePathsAbsolute(string rootDirectory)
    {
        rootDirectory = Path.GetFullPath(rootDirectory);

        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.srcPath = Path.Combine(rootDirectory, frame.srcPath);
            frames[i] = frame;
        }

        foreach (var anim in animations)
        {
            foreach (var keyframe in anim.keyframes)
            {
                keyframe.frame.srcPath = Path.Combine(rootDirectory, keyframe.frame.srcPath);
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