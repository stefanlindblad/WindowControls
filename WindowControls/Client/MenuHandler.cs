using CefSharp;

namespace Stingray.WindowControls.Client
{
    /// <summary>
    /// Callbacks for in-browser menu requests.
    /// </summary>
    internal class MenuHandler : IMenuHandler
    {
        public bool OnBeforeContextMenu(IWebBrowser browser, IContextMenuParams parameters)
        {
            // Disable default right-click context menu.
            return false;
        }
    }
}
