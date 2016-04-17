using System.Windows.Forms;
using CefSharp;
using Stingray.WindowControls.Server;

namespace Stingray.WindowControls.Client
{
    /// <summary>
    /// Callbacks for in-browser keyboard events that were not handled by JavaScript.
    /// </summary>
    internal class KeyboardHandler : IKeyboardHandler
    {
        public bool OnKeyEvent(IWebBrowser browserControl, KeyType type, int windowsKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            // Handle CTRL-<key> keyboard shortcuts from browser window.
            if (type == KeyType.RawKeyDown && modifiers == CefEventFlags.ControlDown)
            {
                switch ((Keys)windowsKeyCode)
                {
                    // Ctrl+D opens the Chrome dev tools in a separate window.
                    case Keys.D:
                        browserControl.ShowDevTools();
                        return true;

                    // Ctrl+R reloads the page.
                    case Keys.R:
                        browserControl.Reload(true);
                        return true;

                    // Ctrl+Z undos the last action
                    case Keys.Z:
                        WebSocketServer.Undo();
                        return true;

                    // Ctrl+Y redos the last action
                    case Keys.Y:
                        WebSocketServer.Redo();
                        return true;
                }
            }
            // The key event was not handled by us.
            return false;
        }

        public bool OnPreKeyEvent(IWebBrowser browserControl, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            // We don't need any pre-filtering of keys.
            return false;
        }
    }
}