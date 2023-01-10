namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SDL2;
using ImGuiNET;
using System;
using System.Runtime.InteropServices;

public class ToolApp : Game
{
    public static ToolApp instance { get; private set; }

    public ImGuiRenderer imguiRenderer { get; private set; }

    private List<EditorWindow> _windows = new List<EditorWindow>();
    private List<EditorWindow> _windowDestroyQueue = new List<EditorWindow>();
    private object _dragDropPayload = null;
    private IntPtr _dummy;
    private EditorWindow _focusWindowQueue = null;

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

        _dummy = Marshal.AllocHGlobal(4);
    }

    public void SetDragDropPayload<T>(T data)
    {
        _dragDropPayload = data;
        ImGui.SetDragDropPayload(typeof(T).Name, _dummy, 4);
    }

    public unsafe bool AcceptDragDropPayload<T>(out T data)
    {
        var wtf = ImGui.AcceptDragDropPayload(typeof(T).Name);
        if (wtf.NativePtr != null)
        {
            data = (T)_dragDropPayload;
            return true;
        }

        data = default(T);
        return false;
    }

    public T GetWindow<T>() where T : EditorWindow, new()
    {
        foreach (var win in _windows)
        {
            if (win is T ret)
            {
                _focusWindowQueue = ret;
                return ret;
            }
        }

        T newWin = new T();
        _windows.Add(newWin);
        _focusWindowQueue = newWin;
        return newWin;
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        SDL.SDL_MaximizeWindow(Window.Handle);
        imguiRenderer = new ImGuiRenderer(this);
        imguiRenderer.RebuildFontAtlas();
        ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Marshal.FreeHGlobal(_dummy);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        imguiRenderer.UpdateInput();

        foreach (var window in _windows)
        {
            window.Update(gameTime);
        }
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
            if (_focusWindowQueue == win)
            {
                _focusWindowQueue = null;
                ImGui.SetNextWindowFocus();
            }

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