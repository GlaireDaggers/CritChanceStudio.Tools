namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SDL2;
using ImGuiNET;

public class ToolApp : Game
{
    public static ToolApp instance { get; private set; }

    public ImGuiRenderer imguiRenderer { get; private set; }

    private List<EditorWindow> _windows = new List<EditorWindow>();
    private List<EditorWindow> _windowDestroyQueue = new List<EditorWindow>();

    public ToolApp(string windowTitle)
    {
        GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);
        gdm.SynchronizeWithVerticalRetrace = true;
        gdm.PreferredBackBufferWidth = 1280;
        gdm.PreferredBackBufferHeight = 720;

        Window.Title = windowTitle;
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        IsFixedTimeStep = false;

        instance = this;
    }

    public T GetWindow<T>() where T : EditorWindow, new()
    {
        foreach (var win in _windows)
        {
            if (win is T ret)
            {
                return ret;
            }
        }

        T newWin = new T();
        _windows.Add(newWin);
        return newWin;
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        SDL.SDL_MaximizeWindow(Window.Handle);
        imguiRenderer = new ImGuiRenderer(this);
        imguiRenderer.RebuildFontAtlas();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        imguiRenderer.UpdateInput();
    }

    protected virtual void OnMenuBarGUI()
    {
    }

    protected virtual void OnGUI()
    {
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1f, 0);

        imguiRenderer.BeginLayout(gameTime);

        System.Numerics.Vector2 menuBarSize = System.Numerics.Vector2.Zero;
        if (ImGui.BeginMainMenuBar())
        {
            menuBarSize = ImGui.GetWindowSize();
            OnMenuBarGUI();
            ImGui.EndMainMenuBar();
        }

        ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, menuBarSize.Y));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight - menuBarSize.Y));
        if (ImGui.Begin("__dockmain", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus))
        {
            OnGUI();
            ImGui.End();
        }

        foreach (var win in _windows)
        {
            bool open = true;
            win.OnDrawWindow(ref open);

            if (!open)
            {
                _windowDestroyQueue.Add(win);
            }
        }

        foreach (var win in _windowDestroyQueue)
        {
            win.OnClosed();
            _windows.Remove(win);
        }
        _windowDestroyQueue.Clear();

        imguiRenderer.EndLayout();
    }
}