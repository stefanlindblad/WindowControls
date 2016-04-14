using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Fleck;
using Newtonsoft.Json;
using Stingray.WindowControls.Client;
using Stingray.WindowControls.Server;

namespace Stingray.WindowControls
{
    /// <summary>
    /// Contains the application entry point.
    /// </summary>
    internal class Program
    {
        // Default port for the websocket server if not supplied as a command-line argument.
        private const int DefaultPort = 9696;

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
                port = DefaultPort;

            // Set up a document window and a control panel to edit it.
            var documentWindow = new BrowserWindow(urlPrefix + "/document-window/document-window.html?port=" + port)
            {
                Text = "Document",
                Width = 1024,
                Height = 768,
                StartPosition = FormStartPosition.CenterScreen
            };

            _controlPanel = new BrowserWindow(urlPrefix + "/control-panel/control-panel.html?port=" + port)
            {
                Text = "Controls",
                Width = 300,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                ShowInTaskbar = false
            };

            // Defer showing the control panel window until the document window has been positioned.
            documentWindow.LocationChanged += DocumentWindowOnLocationChanged;

            // Run the web server until the document window is closed, at which point we exit the application.
            using (var server = new ExampleWebSocketServer(port))
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
            var contentDirectoryPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executablePath), "..", "..", "..", "Content"));
            return contentDirectoryPath;
        }
    }
}
