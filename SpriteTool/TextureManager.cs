namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

public class TextureManager
{
    private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
    private Dictionary<Texture2D, IntPtr> _imguiMap = new Dictionary<Texture2D, IntPtr>();

    private GraphicsDevice _graphicsDevice;
    private ImGuiRenderer _imguiRenderer;

    private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
    private Queue<string> _textureRefreshQueue = new Queue<string>();

    public TextureManager(ToolApp app)
    {
        _graphicsDevice = app.GraphicsDevice;
        _imguiRenderer = app.imguiRenderer;
    }

    public void Update()
    {
        lock (_textureRefreshQueue)
        {
            while (_textureRefreshQueue.Count > 0)
            {
                string path = _textureRefreshQueue.Dequeue();

                Console.WriteLine("Modified: " + path);

                // retrieve existing texture from cache
                Texture2D oldTex = _cache[path];
                _cache.Remove(path);

                // try and reload texture
                // if it fails, just put the old texture back
                try
                {
                    Texture2D newTex = GetTexture(path);
                    _imguiMap.Remove(oldTex);
                }
                catch
                {
                    _cache.Add(path, oldTex);
                }
            }
        }
    }

    public Texture2D GetTexture(string path)
    {
        path = Path.GetFullPath(path);

        if (_cache.ContainsKey(path))
        {
            return _cache[path];
        }

        using (var stream = File.OpenRead(path))
        {
            Texture2D tex = Texture2D.FromStream(_graphicsDevice, stream);
            _cache.Add(path, tex);

            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            watcher.Changed += OnFileChanged;
            _watchers.Add(watcher);

            return tex;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // if a texture changes, we need to queue a reload
        lock (_textureRefreshQueue)
        {
            _textureRefreshQueue.Enqueue(e.FullPath);
        }
    }

    public IntPtr GetImGuiHandle(string path)
    {
        Texture2D tex = GetTexture(path);
        
        if (_imguiMap.ContainsKey(tex))
        {
            return _imguiMap[tex];
        }

        IntPtr handle = _imguiRenderer.BindTexture(tex);
        _imguiMap[tex] = handle;
        return handle;
    }
}