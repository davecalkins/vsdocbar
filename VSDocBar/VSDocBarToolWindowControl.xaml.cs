using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSDocBar
{
    /// <summary>
    /// Interaction logic for VSDocBarToolWindowControl.
    /// </summary>
    public partial class VSDocBarToolWindowControl : UserControl
    {
        private readonly VSDocBarToolWindowControlViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSDocBarToolWindowControl"/> class.
        /// </summary>
        public VSDocBarToolWindowControl()
        {
            this.InitializeComponent();

            // establish the view model for data binding
            this.DataContext = _viewModel = new VSDocBarToolWindowControlViewModel();
        }

        public void Initialize(IVsRunningDocumentTable rdt, IVsUIShellOpenDocument sod, DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // initialization pass-through to the view model
            _viewModel.Initialize(rdt, sod, dte);
        }

        public void Shutdown()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // shutdown pass-through to the view model
            _viewModel.Shutdown();
        }

        private void OpenDocItemClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // handle a click on an item in the list (could be project or doc)
            _viewModel.OnDocClicked((sender as ContentControl)?.DataContext);
        }

        private void OpenDocCloseBtnClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // handle a click on the close button for a document
            _viewModel.OnDocCloseBtnClicked((sender as ContentControl)?.DataContext);
        }
    }
}
