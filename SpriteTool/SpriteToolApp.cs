namespace CritChanceStudio.Tools;

using ImGuiNET;
using NativeFileDialogSharp;

using System.Collections.Generic;

public class SpriteToolApp : ToolApp
{
    private struct UndoState
    {
        public string title;
        public DocumentState state;
    }

    public TextureManager textureManager;
    public DocumentState activeDocument = new DocumentState();

    private Stack<UndoState> _undoStates = new Stack<UndoState>();
    private Stack<UndoState> _redoStates = new Stack<UndoState>();

    public SpriteToolApp() : base("Sprite Tool")
    {
    }

    public void RegisterUndo(string title)
    {
        _redoStates.Clear();
        _undoStates.Push(new UndoState
        {
            title = title,
            state = (DocumentState)activeDocument.Clone()
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
            var undoState = _undoStates.Pop();
            _redoStates.Push(new UndoState
            {
                title = undoState.title,
                state = (DocumentState)activeDocument.Clone()
            });
            activeDocument = undoState.state;
        }
    }

    public void Redo()
    {
        if (_redoStates.Count > 0)
        {
            var redoState = _redoStates.Pop();
            _undoStates.Push(new UndoState
            {
                title = redoState.title,
                state = (DocumentState)activeDocument.Clone()
            });
            activeDocument = redoState.state;
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
            if (ImGui.MenuItem("New"))
            {
                activeDocument = new DocumentState();
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
            ImGui.EndMenu();
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }
}