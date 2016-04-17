using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Stingray.WindowControls.Client;
using Stingray.WindowControls.Server;

namespace Stingray.WindowControls
{
    /// <summary>
    /// Contains the application entry point.
    /// </summary>
    internal class Program
    {
        // We keep the control panel as state, since it needs to be accessible inside DocumentWindowOnLocationChanged.
        private static Form _controlPanel;

        [STAThread]
        private static void Main(string[] commandLineArgs)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var urlPrefix = "file:///" + GetContentDirectoryPath().Replace("\\", "/");
            int port;

            // Parse port number from command line, if supplied.
            if (!Int32.TryParse(commandLineArgs.FirstOrDefault(), out port))
                port = WindowControls.Settings.Default._defaultPort;

            // Set up a document window and a control panel to edit it.
            var mainWindowName = WindowControls.Settings.Default._mainWindowName;
            var documentWindow = new BrowserWindow(urlPrefix + "/" + mainWindowName + "/" + mainWindowName + ".html?port=" + port)
            {
                Text = WindowControls.Settings.Default._mainWindowTitle,
                Width = WindowControls.Settings.Default._mainWindowWidth,
                Height = WindowControls.Settings.Default._mainWindowHeight,
                StartPosition = FormStartPosition.CenterScreen
            };

            var ctrlWindowName = WindowControls.Settings.Default._ctrlWindowName;
            _controlPanel = new BrowserWindow(urlPrefix + "/" + ctrlWindowName + "/" + ctrlWindowName + ".html?port=" + port)
            {
                Text = WindowControls.Settings.Default._ctrlWindowTitle,
                Width = WindowControls.Settings.Default._ctrlWindowWidth,
                Height = WindowControls.Settings.Default._ctrlWindowHeight,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                ShowInTaskbar = false
            };

            // Defer showing the control panel window until the document window has been positioned.
            documentWindow.LocationChanged += DocumentWindowOnLocationChanged;

            // Run the web server until the document window is closed, at which point we exit the application.
            using (var server = new WebSocketServer(port))
                Application.Run(documentWindow);

            // Dispose of the control panel. The document window will be disposed by Application.Run.
            _controlPanel.Dispose();
        }

        private static void DocumentWindowOnLocationChanged(object sender, EventArgs args)
        {
            // At this point we know the bounds of the document window.
            // Show the control panel overlaid on the lower right corner.
            Debug.Assert(_controlPanel != null);
            var documentWindow = (Form)sender;
            documentWindow.LocationChanged -= DocumentWindowOnLocationChanged;
            var controlPanelX = documentWindow.Bounds.Right - _controlPanel.Bounds.Width / 2;
            var controlPanelY = documentWindow.Bounds.Bottom - _controlPanel.Bounds.Height / 2;
            _controlPanel.Location = new Point(controlPanelX, controlPanelY);
            _controlPanel.Show(documentWindow);
            _controlPanel.BringToFront();
        }

        private static string GetContentDirectoryPath()
        {
            // Assumes the Content directory is located relative to the built executable, so content can easily be reloaded during development.
            var executablePath = Assembly.GetEntryAssembly().Location;
            var contentDirectoryPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executablePath), WindowControls.Settings.Default._contentPath));
            return contentDirectoryPath;
        }
    }
}
