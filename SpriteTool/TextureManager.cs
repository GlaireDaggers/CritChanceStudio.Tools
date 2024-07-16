namespace CritChanceStudio.Tools;

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using AsepriteDotNet.Common;
using AsepriteDotNet.Aseprite.Types;
using Microsoft.Xna.Framework;

public class TextureManager
{
    private struct AsepriteProject
    {
        public Texture2D[] frames;
    }

    private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
    private Dictionary<string, AsepriteProject> _asepriteCache = new Dictionary<string, AsepriteProject>();
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
                    Texture2D newTex = GetImageTexture(path);
                    _imguiMap.Remove(oldTex);
                }
                catch
                {
                    _cache.Add(path, oldTex);
                }
            }
        }
    }

    public Texture2D[] GetAsepriteFrames(string path)
    {
        path = Path.GetFullPath(path);

        if (_asepriteCache.ContainsKey(path))
        {
            return _asepriteCache[path].frames;
        }

        using (var stream = File.OpenRead(path))
        {
            AsepriteFile asepriteFile = AsepriteFileLoader.FromStream(path, stream, true, false);
            Texture2D[] frames = new Texture2D[asepriteFile.FrameCount];

            for (int i = 0; i < asepriteFile.FrameCount; i++)
            {
                Rgba32[] pixels = asepriteFile.Frames[i].FlattenFrame();
                frames[i] = new Texture2D(_graphicsDevice, asepriteFile.CanvasWidth, asepriteFile.CanvasHeight, false, SurfaceFormat.Color);
                frames[i].SetData(pixels);
            }

            _asepriteCache[path] = new AsepriteProject
            {
                frames = frames
            };

            return frames;
        }
    }

    public Texture2D GetImageTexture(string path)
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

    public IntPtr GetImGuiHandle(Texture2D tex)
    {   
        if (_imguiMap.ContainsKey(tex))
        {
            return _imguiMap[tex];
        }

        IntPtr handle = _imguiRenderer.BindTexture(tex);
        _imguiMap[tex] = handle;
        return handle;
    }
}