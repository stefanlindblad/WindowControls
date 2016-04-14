using System;
using System.Windows.Forms;
using CefSharp;

namespace Stingray.WindowControls.Client
{
    /// <summary>
    /// Callbacks for JavaScript-triggered dialogs.
    /// </summary>
    internal class JsDialogHandler : IJsDialogHandler
    {
        public bool OnJSAlert(IWebBrowser browser, string url, string message)
        {
            MessageBox.Show(message, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        public bool OnJSConfirm(IWebBrowser browser, string url, string message, out bool retval)
        {
            retval = MessageBox.Show(message, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
            return true;
        }

        public bool OnJSPrompt(IWebBrowser browser, string url, string message, string defaultValue, out bool retval, out string result)
        {
            // Use the default prompt implementation.
            // Note that calling prompt() before the window has appeared can cause an access violation.
            retval = true;
            result = String.Empty;
            return false;
        }

        public bool OnJSBeforeUnload(IWebBrowser browser, string message, bool isReload, out bool allowUnload)
        {
            // Disallow leaving the page unless this is a reload operation.
            allowUnload = isReload;
            return true;
        }
    }
}
