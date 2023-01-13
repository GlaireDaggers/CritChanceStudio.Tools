using CritChanceStudio.ParticleEngine;
using CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NativeFileDialogSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.IO;

public class ParticleToolApp : ToolApp
{
    public ParticleSystem particleSystem = new ParticleSystem();
    public ParticleRenderer activeEmitter = null;
    public string currentPath = null;

    public Texture2D blank;

    public ParticleToolApp() : base("Particle Tool")
    {
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        blank = new Texture2D(GraphicsDevice, 2, 2);
        blank.SetData(new Color[] { Color.White, Color.White, Color.White, Color.White });

        GetWindow<ParticleToolViewport>();
        GetWindow<EmitterListWindow>();
        GetWindow<EmitterDetailsWindow>();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }

    protected override void OnMenuBarGUI()
    {
        base.OnMenuBarGUI();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Open", "CTRL+O"))
            {
                Open();
            }
            if (ImGui.MenuItem("Save", "CTRL+S"))
            {
                Save();
            }
            if (ImGui.MenuItem("Save As"))
            {
                SaveAs();
            }
            if (ImGui.MenuItem("Quit"))
            {
                Exit();
            }
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Window"))
        {
            if (ImGui.MenuItem("Viewport"))
            {
                GetWindow<ParticleToolViewport>();
            }
            if (ImGui.MenuItem("Emitter List"))
            {
                GetWindow<EmitterListWindow>();
            }
            if (ImGui.MenuItem("Emitter Details"))
            {
                GetWindow<EmitterDetailsWindow>();
            }
            ImGui.EndMenu();
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }

    protected void Open()
    {
        DialogResult result = Dialog.FileOpen("json");
        if (result.IsOk)
        {
            var oldPath = currentPath;
            try
            {
                string jsonData = File.ReadAllText(result.Path);
                currentPath = result.Path;

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new TextureJsonConverter());
                settings.Converters.Add(new Vector2JsonConverter());
                settings.Converters.Add(new ColorJsonConverter());
                settings.Converters.Add(new StringEnumConverter());
                settings.Formatting = Formatting.Indented;

                ParticleSystem system = JsonConvert.DeserializeObject<ParticleSystem>(jsonData, settings);

                foreach (var emitter in system.emitters)
                {
                    emitter.Init(GraphicsDevice);
                }

                particleSystem = system;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                currentPath = oldPath;
            }
        }
    }

    protected void SaveAs()
    {
        DialogResult result = Dialog.FileSave("json");
        if (result.IsOk)
        {
            Save(result.Path);
        }
    }

    protected void Save()
    {
        if (currentPath == null)
        {
            SaveAs();
        }
        else
        {
            Save(currentPath);
        }
    }

    protected void Save(string path)
    {
        var oldPath = currentPath;
        currentPath = path;

        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new TextureJsonConverter());
            settings.Converters.Add(new Vector2JsonConverter());
            settings.Converters.Add(new ColorJsonConverter());
            settings.Converters.Add(new StringEnumConverter());
            settings.Formatting = Formatting.Indented;

            string jsonData = JsonConvert.SerializeObject(particleSystem, settings);
            File.WriteAllText(path, jsonData);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            currentPath = oldPath;
        }
    }
}