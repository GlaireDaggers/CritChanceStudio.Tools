namespace CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NativeFileDialogSharp;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;

public class SpriteToolApp : ToolApp
{
    private struct UndoState
    {
        public string title;
        public DocumentState state;
    }

    public TextureManager textureManager;
    public DocumentState activeDocument = new DocumentState();
    public Animation activeAnimation = null;
    public Keyframe activeKeyframe = null;
    public string activeDocumentPath = null;
    public bool unsavedChanges = false;

    public object clipboard;

    private Stack<UndoState> _undoStates = new Stack<UndoState>();
    private Stack<UndoState> _redoStates = new Stack<UndoState>();

    private KeyboardState _prevKb = new KeyboardState();
    private KeyboardState _curKb = new KeyboardState();

    public SpriteToolApp() : base("Sprite Tool")
    {
        UpdateTitle();
    }

    public void RegisterUndo(string title)
    {
        unsavedChanges = true;
        UpdateTitle();

        _redoStates.Clear();
        _undoStates.Push(new UndoState
        {
            title = title,
            state = activeDocument.Clone()
        });
    }

    public bool HasUndo(out string title)
    {
        if (_undoStates.Count > 0)
        {
            title = _undoStates.Peek().title;
            return true;
        }

        title = "";
        return false;
    }

    public bool HasRedo(out string title)
    {
        if (_redoStates.Count > 0)
        {
            title = _redoStates.Peek().title;
            return true;
        }

        title = "";
        return false;
    }

    public void Undo()
    {
        if (_undoStates.Count > 0)
        {
            unsavedChanges = true;
            UpdateTitle();

            var undoState = _undoStates.Pop();
            _redoStates.Push(new UndoState
            {
                title = undoState.title,
                state = activeDocument.Clone()
            });
            activeDocument = undoState.state;
            activeAnimation = null;
            activeKeyframe = null;
        }
    }

    public void Redo()
    {
        if (_redoStates.Count > 0)
        {
            unsavedChanges = true;
            UpdateTitle();

            var redoState = _redoStates.Pop();
            _undoStates.Push(new UndoState
            {
                title = redoState.title,
                state = activeDocument.Clone()
            });
            activeDocument = redoState.state;
            activeAnimation = null;
            activeKeyframe = null;
        }
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        textureManager = new TextureManager(this);

        GetWindow<SpriteToolViewport>();
        GetWindow<FrameListWindow>();
        GetWindow<AnimationListWindow>();
        GetWindow<KeyframeListWindow>();
        GetWindow<HitboxListWindow>();
        GetWindow<SocketListWindow>();
        GetWindow<TagListWindow>();
        GetWindow<KeyframeDetailsWindow>();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _prevKb = _curKb;
        _curKb = Keyboard.GetState();

        if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.Z))
        {
            Undo();
        }

        if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.Y))
        {
            Redo();
        }

        if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.N))
        {
            activeDocument = new DocumentState();
            activeAnimation = null;
            activeKeyframe = null;
            _undoStates.Clear();
            _redoStates.Clear();
            activeDocumentPath = null;
            unsavedChanges = false;
            UpdateTitle();
        }

        if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.O))
        {
            Open();
        }

        if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.S))
        {
            Save();
        }
    }

    private bool IsKeyPressed(Keys key)
    {
        return _curKb.IsKeyDown(key) && !_prevKb.IsKeyDown(key);
    }

    private bool IsKeyDown(Keys key)
    {
        return _curKb.IsKeyDown(key);
    }

    protected override void OnMenuBarGUI()
    {
        base.OnMenuBarGUI();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("New", "CTRL+N"))
            {
                activeDocument = new DocumentState();
                activeAnimation = null;
                activeKeyframe = null;
                _undoStates.Clear();
                _redoStates.Clear();
                activeDocumentPath = null;
                unsavedChanges = false;
                UpdateTitle();
            }
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

        if (ImGui.BeginMenu("Edit"))
        {
            if (HasUndo(out var undoTitle))
            {
                if (ImGui.MenuItem("Undo " + undoTitle, "CTRL+Z"))
                {
                    Undo();
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.MenuItem("Undo", "CTRL+Z");
                ImGui.EndDisabled();
            }

            if (HasRedo(out var redoTitle))
            {
                if (ImGui.MenuItem("Redo " + redoTitle, "CTRL+Y"))
                {
                    Redo();
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.MenuItem("Redo", "CTRL+Y");
                ImGui.EndDisabled();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Window"))
        {
            if (ImGui.MenuItem("Viewport"))
            {
                GetWindow<SpriteToolViewport>();
            }
            if (ImGui.MenuItem("Frame List"))
            {
                GetWindow<FrameListWindow>();
            }
            if (ImGui.MenuItem("Animation List"))
            {
                GetWindow<AnimationListWindow>();
            }
            if (ImGui.MenuItem("Keyframe List"))
            {
                GetWindow<KeyframeListWindow>();
            }
            if (ImGui.MenuItem("Hitbox List"))
            {
                GetWindow<HitboxListWindow>();
            }
            if (ImGui.MenuItem("Socket List"))
            {
                GetWindow<SocketListWindow>();
            }
            if (ImGui.MenuItem("Tag List"))
            {
                GetWindow<TagListWindow>();
            }
            if (ImGui.MenuItem("Keyframe Details"))
            {
                GetWindow<KeyframeDetailsWindow>();
            }
            ImGui.EndMenu();
        }
    }

    private void Open()
    {
        var result = Dialog.FileOpen("json");
        if (result.IsOk)
        {
            try
            {
                string filedata = File.ReadAllText(result.Path);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new RectangleJsonConverter());
                settings.Converters.Add(new Vector2JsonConverter());
                DocumentState newDoc = JsonConvert.DeserializeObject<DocumentState>(filedata, settings);
                newDoc.MakePathsAbsolute(Path.GetDirectoryName(result.Path));
                activeDocument = newDoc;
                activeAnimation = null;
                activeKeyframe = null;
                _undoStates.Clear();
                _redoStates.Clear();

                activeDocumentPath = result.Path;
                UpdateTitle();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void SaveAs()
    {
        var result = Dialog.FileSave("json");
        if (result.IsOk)
        {
            Save(result.Path);
        }
    }

    private void Save()
    {
        if (activeDocumentPath == null)
        {
            SaveAs();
        }

        Save(activeDocumentPath);
    }

    private void Save(string path)
    {
        try
        {
            DocumentState serializeDoc = activeDocument.Clone();
            serializeDoc.MakePathsRelative(Path.GetDirectoryName(path));
            serializeDoc.NormalizePaths();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new RectangleJsonConverter());
            settings.Converters.Add(new Vector2JsonConverter());
            string filedata = JsonConvert.SerializeObject(serializeDoc, settings);
            File.WriteAllText(path, filedata);

            activeDocumentPath = path;
            unsavedChanges = false;
            UpdateTitle();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void UpdateTitle()
    {
        if (activeDocumentPath == null)
        {
            Window.Title = "Sprite Tool - Untitled" + (unsavedChanges ? " *" : "");
        }
        else
        {
            Window.Title = "Sprite Tool - " + activeDocumentPath + (unsavedChanges ? " *" : "");
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }
}