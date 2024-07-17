namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

    [JsonIgnore]
    public Rectangle srcRect;

    [JsonIgnore]
    public Vector2 offset;

    [JsonIgnore]
    public Vector2 size;

    public bool Equals(SpriteFrame other)
    {
        return source == other.source
            && srcIndex == other.srcIndex
            && srcPath == other.srcPath;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is SpriteFrame && Equals((SpriteFrame)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(source, srcPath, srcIndex);
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

    public void CalcMetrics(TextureManager texManager)
    {
        Texture2D texture = GetTexture(texManager);
        Rectangle srcRect = TrimTexture(texture);

        this.srcRect = srcRect;
        offset = new Vector2(srcRect.X, srcRect.Y);
        size = new Vector2(texture.Width, texture.Height);
    }

    private static Rectangle TrimTexture(Texture2D texture)
    {
        // find tight-fitting src rect

        Color[] c = new Color[texture.Width * texture.Height];
        texture.GetData(c);

        int minJ = 0;
        for (int j = 0; j < texture.Height; j++)
        {
            for (int i = 0; i < texture.Width; i++)
            {
                if (c[i + (j * texture.Width)].A > 0)
                {
                    minJ = j;
                    j = texture.Height;
                    break;
                }
            }
        }

        int minI = texture.Width;
        int maxI = 0;

        int maxJ = texture.Height;
        for (int j = minJ; j < texture.Height; j++)
        {
            for (int i = 0; i < texture.Width; i++)
            {
                if (c[i + (j * texture.Width)].A > 0)
                {
                    minI = Math.Min(minI, i);
                    maxI = Math.Max(maxI, i);
                    maxJ = j;
                }
            }
        }

        return new Rectangle(minI, minJ, (maxI - minI) + 1, (maxJ - minJ) + 1);
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
    public bool mirrorX = false;
    public bool mirrorY = false;
    public Vector2 offset = Vector2.Zero;
    public Vector2 motionDelta = Vector2.Zero;
    public int frameIdx;
    public List<Hitbox> hitboxes = new List<Hitbox>();
    public List<Socket> sockets = new List<Socket>();
    public List<string> tags = new List<string>();

    #region Version migration (v0.1)

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public SpriteFrame? frame = null;

    #endregion

    public Keyframe Clone()
    {
        Keyframe clone = new Keyframe();
        clone.duration = duration;
        clone.mirrorX = mirrorX;
        clone.mirrorY = mirrorY;
        clone.offset = offset;
        clone.motionDelta = motionDelta;
        clone.frameIdx = frameIdx;
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
    public static string FORMAT_VERSION = "v0.2";

    public string version;
    public List<SpriteFrame> frames = new List<SpriteFrame>();
    public List<Animation> animations = new List<Animation>();

    public void MigrateVersion()
    {
        // v0.1
        if (version == null)
        {
            foreach (var anim in animations)
            {
                for (int i = 0; i < anim.keyframes.Count; i++)
                {
                    int frIdx = frames.IndexOf(anim.keyframes[i].frame.Value);
                    if (frIdx != -1)
                    {
                        anim.keyframes[i].frameIdx = frIdx;
                    }

                    anim.keyframes[i].frame = null;
                }
            }
        }
        else if (version != FORMAT_VERSION)
        {
            throw new FormatException($"Document has invalid/unknown format version (was it saved with a newer version of the tool?)\nDocument Version: {version}");
        }

        version = FORMAT_VERSION;
    }

    public void RecalcMetrics(TextureManager textureManager)
    {
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.CalcMetrics(textureManager);
            frames[i] = frame;
        }
    }

    public DocumentState Clone()
    {
        DocumentState clone = new DocumentState()
        {
            version = FORMAT_VERSION
        };
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
    }

    public void MakePathsRelative(string rootDirectory)
    {
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            frame.srcPath = Path.GetRelativePath(rootDirectory, frame.srcPath);
            frames[i] = frame;
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