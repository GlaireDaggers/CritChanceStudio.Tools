namespace CritChanceStudio.Tools;

using ImGuiNET;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using NativeFileDialogSharp;

using Newtonsoft.Json;

using RectpackSharp;

using SDL2;

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
            ConfirmChanges("Document has unsaved changes. Are you sure you want to create a new document?", New);
        }

        if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.O))
        {
            ConfirmChanges("Document has unsaved changes. Are you sure you want to open a different file?", Open);
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
                ConfirmChanges("Document has unsaved changes. Are you sure you want to create a new document?", New);
            }
            if (ImGui.MenuItem("Open", "CTRL+O"))
            {
                ConfirmChanges("Document has unsaved changes. Are you sure you want to open a different file?", Open);
            }
            if (ImGui.MenuItem("Save", "CTRL+S"))
            {
                Save();
            }
            if (ImGui.MenuItem("Save As"))
            {
                SaveAs();
            }
            if (ImGui.MenuItem("Export"))
            {
                Export();
            }
            if (ImGui.MenuItem("Quit"))
            {
                ConfirmChanges("Document has unsaved changes. Are you sure you want to quit?", Exit);
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

    private void Export()
    {
        // by default: start in the same folder as the sprite project
        string docPath = activeDocumentPath != null ? Path.GetDirectoryName(activeDocumentPath) : null;

        FileSave("json", defaultPath: docPath, onSave: (path) =>
        {
            // pack all frames into a texture sheet
            PackingRectangle[] rects = new PackingRectangle[activeDocument.frames.Count];
            Rectangle[] actualRects = new Rectangle[rects.Length];
            Dictionary<SpriteFrame, int> idMap = new Dictionary<SpriteFrame, int>();

            for (int i = 0; i < rects.Length; i++)
            {
                // add 1 pixel padding around each frame in atlas
                rects[i] = new PackingRectangle((uint)activeDocument.frames[i].srcRect.X,
                    (uint)activeDocument.frames[i].srcRect.Y,
                    (uint)activeDocument.frames[i].srcRect.Width + 2,
                    (uint)activeDocument.frames[i].srcRect.Height + 2, i);
            }

            RectanglePacker.Pack(rects, out PackingRectangle atlasBounds, PackingHints.FindBest);

            // create texture atlas
            Texture2D atlas = new Texture2D(GraphicsDevice, (int)atlasBounds.Width, (int)atlasBounds.Height, false, SurfaceFormat.Color);

            // clear atlas pixels
            Color[] clearPx = new Color[atlas.Width * atlas.Height];
            Array.Fill(clearPx, new Color(0, 0, 0, 0));
            atlas.SetData(clearPx);

            // copy each frame into atlas, map SpriteFrame -> rectangle in atlas
            for (int i = 0; i < rects.Length; i++)
            {
                // account for 1 pixel padding when copying data into atlas
                Rectangle actualRect = new Rectangle((int)rects[i].X + 1, (int)rects[i].Y + 1, (int)rects[i].Width - 2, (int)rects[i].Height - 2);
                actualRects[i] = actualRect;
                idMap.Add(activeDocument.frames[rects[i].Id], i);

                Texture2D srcTex = textureManager.GetTexture(activeDocument.frames[rects[i].Id].srcTexture);
                Color[] srcData = new Color[actualRects[i].Width * actualRects[i].Height];
                srcTex.GetData(0, activeDocument.frames[rects[i].Id].srcRect, srcData, 0, srcData.Length);
                atlas.SetData(0, actualRect, srcData, 0, srcData.Length);
            }

            string atlasPath = Path.ChangeExtension(path, ".png");
            using (var stream = File.OpenWrite(atlasPath))
                atlas.SaveAsPng(stream, atlas.Width, atlas.Height);

            atlas.Dispose();

            // convert to ExportDocument
            ExportDocument exportDoc = new ExportDocument();
            exportDoc.atlasPath = Path.GetFileName(atlasPath);
            exportDoc.frames = actualRects;
            exportDoc.animations = new ExportAnimation[activeDocument.animations.Count];

            for (int i = 0; i < exportDoc.animations.Length; i++)
            {
                var srcAnim = activeDocument.animations[i];
                exportDoc.animations[i].name = srcAnim.name;
                exportDoc.animations[i].looping = srcAnim.looping;
                exportDoc.animations[i].keyframes = new ExportKeyframe[srcAnim.keyframes.Count];

                for (int j = 0; j < srcAnim.keyframes.Count; j++)
                {
                    exportDoc.animations[i].keyframes[j] = new ExportKeyframe
                    {
                        frame = idMap[srcAnim.keyframes[j].frame],
                        duration = srcAnim.keyframes[j].duration,
                        offset = srcAnim.keyframes[j].offset,
                        motionDelta = srcAnim.keyframes[j].motionDelta,
                        hitboxes = srcAnim.keyframes[j].hitboxes.ToArray(),
                        sockets = srcAnim.keyframes[j].sockets.ToArray(),
                        tags = srcAnim.keyframes[j].tags.ToArray(),
                    };
                }
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new RectangleJsonConverter());
            settings.Converters.Add(new Vector2JsonConverter());

            string exportJson = JsonConvert.SerializeObject(exportDoc, settings);
            File.WriteAllText(path, exportJson);
        });
    }

    private void ConfirmChanges(string message, Action action)
    {
        if (unsavedChanges)
        {
            ShowDialog("Unsaved changes", message, new string[] { "Ok", "Cancel" }, (result) =>
            {
                if (result == 0)
                {
                    action?.Invoke();
                }
            });
        }
        else
        {
            action?.Invoke();
        }
    }

    private void New()
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

    private void Open()
    {
        FileOpen("spriteproj", onOpen: (path) =>
        {
            try
            {
                string filedata = File.ReadAllText(path);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new RectangleJsonConverter());
                settings.Converters.Add(new Vector2JsonConverter());
                DocumentState newDoc = JsonConvert.DeserializeObject<DocumentState>(filedata, settings);
                newDoc.MakePathsAbsolute(Path.GetDirectoryName(path));
                activeDocument = newDoc;
                activeAnimation = null;
                activeKeyframe = null;
                _undoStates.Clear();
                _redoStates.Clear();

                activeDocumentPath = path;
                UpdateTitle();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShowDialog("Error", "Failed to open file: " + e.Message, new string[] { "Ok" });
            }
        });
    }

    private void SaveAs()
    {
        FileSave("spriteproj", onSave: (path) =>
        {
            Save(path);
        });
    }

    private void Save()
    {
        if (activeDocumentPath == null)
        {
            SaveAs();
        }
        else
        {
            Save(activeDocumentPath);
        }
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
            ShowDialog("Error", "Failed to save file: " + e.Message, new string[] { "Ok" });
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