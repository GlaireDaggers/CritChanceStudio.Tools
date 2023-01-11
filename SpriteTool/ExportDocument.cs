using Microsoft.Xna.Framework;

namespace CritChanceStudio.Tools;

public struct ExportKeyframe
{
    public int duration;
    public int frame;
    public Vector2 offset;
    public Vector2 motionDelta;
    public Hitbox[] hitboxes;
    public Socket[] sockets;
    public string[] tags;
}

public struct ExportAnimation
{
    public string name;
    public ExportKeyframe[] keyframes;
}

public class ExportDocument
{
    public string atlasPath;
    public Rectangle[] frames;
    public ExportAnimation[] animations;
}