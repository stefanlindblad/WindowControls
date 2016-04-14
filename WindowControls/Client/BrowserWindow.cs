using System;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace Stingray.WindowControls.Client
{
    /// <summary>
    /// A window with a Chromium-based web browser control filling the entire window bounds.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    internal class BrowserWindow : Form
    {
        // State.
        private bool _shouldShowInTaskbar;

        /// <summary>
        /// Contructs a new BrowserWindow that displays the specified web address.
        /// </summary>
        /// <param name="address">Address to load into the web browser.</param>
        public BrowserWindow(string address)
        {
            WindowState = FormWindowState.Minimized;
            
            var browser = new ChromiumWebBrowser(address)
            {
                Dock = DockStyle.Fill,
                KeyboardHandler = new KeyboardHandler(),
                MenuHandler = new MenuHandler(),
                JsDialogHandler = new JsDialogHandler()
            };

            browser.NavStateChanged += OnBrowserOnNavStateChanged;
            Controls.Add(browser);
        }

        protected override void OnLoad(EventArgs args)
        {
            // Before showing the window, store the ShowInTaskbar setting, then hide until the web page has loaded.
            base.OnLoad(args);
            _shouldShowInTaskbar = ShowInTaskbar;
            ShowInTaskbar = false;
        }

        private void OnBrowserOnNavStateChanged(object sender, NavStateChangedEventArgs args)
        {
            // Wait until we've finished loading until showing the window.
            if (args.IsLoading)
                return;

            var browser = (ChromiumWebBrowser)sender;
            browser.NavStateChanged -= OnBrowserOnNavStateChanged;
            BeginInvoke((Action)ShowWindow);
        }

        private void ShowWindow()
        {
            ShowInTaskbar = _shouldShowInTaskbar;
            WindowState = FormWindowState.Normal;
        }
    }
}
