﻿namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public interface ICloneable<T>
{
    T Clone();
}

public struct SpriteFrame
{
    public string srcTexture;
    public Rectangle srcRect;
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
    public List<Keyframe> keyframes = new List<Keyframe>();

    public Animation Clone()
    {
        Animation clone = new Animation();
        clone.name = name;
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