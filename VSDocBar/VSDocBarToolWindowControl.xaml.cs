using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void OpenDocItemMouseDown(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(e is MouseButtonEventArgs mbe))
                return;

            // in order to get middle button events, need to handle mouse down, check
            // for middle button but then otherwise allow it to be handled elsewhere
            // by the button's normal click.  experimented with this somewhat and it
            // seemed the only way to get both middle and left clicks on a button

            if (mbe.ChangedButton == MouseButton.Middle)
            {
                // treat middle button on item the same as clicking the close button
                _viewModel.OnDocCloseBtnClicked((sender as ContentControl)?.DataContext);

                // we've already handled this
                e.Handled = true;
            }
            else
                // otherwise allow normal handling
                e.Handled = false;
        }

        private void OpenDocItemClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // left click selects the item (project or open doc)
            _viewModel.OnDocClicked((sender as ContentControl)?.DataContext);
        }

        private void OpenDocCloseBtnClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // click on the close button results in closing the doc
            _viewModel.OnDocCloseBtnClicked((sender as ContentControl)?.DataContext);
        }
    }
}
 