namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public struct SpriteFrame
{
    public string srcTexture;
    public Rectangle srcRect;
}

public class Animation : ICloneable
{
    public string name;

    public object Clone()
    {
        Animation clone = new Animation();
        clone.name = name;
        return clone;
    }
}

public class DocumentState : ICloneable
{
    public List<SpriteFrame> frames = new List<SpriteFrame>();
    public List<Animation> animations = new List<Animation>();

    public object Clone()
    {
        DocumentState clone = new DocumentState();
        clone.frames = Clone(frames);
        clone.animations = DeepClone(animations);
        return clone;
    }

    private List<T> Clone<T>(List<T> src) where T : struct
    {
        List<T> newList = new List<T>();
        newList.AddRange(src);

        return newList;
    }

    private List<T> DeepClone<T>(List<T> src) where T : ICloneable
    {
        List<T> newList = new List<T>(src.Count);

        for(int i = 0; i < src.Count; i++)
        {
            newList.Add((T)src[i].Clone());
        }

        return newList;
    }
}