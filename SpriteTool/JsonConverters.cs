using Microsoft.Xna.Framework;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;

namespace CritChanceStudio.Tools;

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