using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSDocBar
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("9b88e713-a0ee-449e-aaa6-32dd3b614d48")]
    public class VSDocBarToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSDocBarToolWindow"/> class.
        /// </summary>
        public VSDocBarToolWindow() : base(null)
        {
            this.Caption = "VSDocBarToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new VSDocBarToolWindowControl();
        }

        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // get needed services
            var rdt = GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            var sod = GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            var dte = GetService(typeof(SDTE)) as DTE;

            // initialize the control, passing along the needed services
            (Content as VSDocBarToolWindowControl)?.Initialize(rdt, sod, dte);
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // notify the control of shutdown
            (Content as VSDocBarToolWindowControl)?.Shutdown();
        }
    }
}
