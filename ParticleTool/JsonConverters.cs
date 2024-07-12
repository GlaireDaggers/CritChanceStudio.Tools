using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;

namespace CritChanceStudio.Tools;

public class TextureJsonConverter : JsonConverter<Texture2D>
{
    public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var tool = (ParticleToolApp)ToolApp.instance;
        var relativePath = Path.GetDirectoryName(tool.currentPath);

        string path = (string)reader.Value;
        if (path != null)
        {
            path = Path.Combine(relativePath, path);
        }

        using var stream = File.OpenRead(path);
        Texture2D tex = Texture2D.FromStream(tool.GraphicsDevice, stream);
        tex.Name = path;
        return tex;
    }

    public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
    {
        var tool = (ParticleToolApp)ToolApp.instance;
        var relativePath = Path.GetDirectoryName(tool.currentPath);

        var path = value.Name;
        if (path != null)
        {
            path = Path.GetRelativePath(relativePath, path);
        }

        writer.WriteValue(path);
    }
}

public class RectangleJsonConverter : JsonConverter<Rectangle>
{
    public override Rectangle ReadJson(JsonReader reader, Type objectType, Rectangle existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        Rectangle r = existingValue;

        if (jo.ContainsKey("X"))
        {
            r.X = (int)jo["X"];
        }

        if (jo.ContainsKey("Y"))
        {
            r.Y = (int)jo["Y"];
        }

        if (jo.ContainsKey("Width"))
        {
            r.Width = (int)jo["Width"];
        }

        if (jo.ContainsKey("Height"))
        {
            r.Height = (int)jo["Height"];
        }

        return r;
    }

    public override void WriteJson(JsonWriter writer, Rectangle value, JsonSerializer serializer)
    {
        JObject jo = new JObject
        {
            { "X", value.X },
            { "Y", value.Y },
            { "Width", value.Width },
            { "Height", value.Height }
        };

        jo.WriteTo(writer);
    }
}

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        Vector2 v = existingValue;

        if (jo.ContainsKey("X"))
        {
            v.X = (int)jo["X"];
        }

        if (jo.ContainsKey("Y"))
        {
            v.Y = (int)jo["Y"];
        }

        return v;
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        JObject jo = new JObject
        {
            { "X", value.X },
            { "Y", value.Y }
        };

        jo.WriteTo(writer);
    }
}

public class ColorJsonConverter : JsonConverter<Color>
{
    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        Color v = existingValue;

        if (jo.ContainsKey("R"))
        {
            v.R = (byte)(int)jo["R"];
        }

        if (jo.ContainsKey("G"))
        {
            v.G = (byte)(int)jo["G"];
        }

        if (jo.ContainsKey("B"))
        {
            v.B = (byte)(int)jo["B"];
        }

        if (jo.ContainsKey("A"))
        {
            v.A = (byte)(int)jo["A"];
        }

        return v;
    }

    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        JObject jo = new JObject
        {
            { "R", value.R },
            { "G", value.G },
            { "B", value.B },
            { "A", value.A }
        };

        jo.WriteTo(writer);
    }
}