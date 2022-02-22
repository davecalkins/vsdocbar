using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSDocBar
{
    // for IVsRunningDocTableEvents, v3 derives from v2 which derives from v1, so don't need to speify v1 or v2.  From v4 on
    // they do not derive from the earlier versions and so are all specified.
    internal class VSDocBarToolWindowControlViewModel : ViewModelBase, IVsRunningDocTableEvents3, IVsRunningDocTableEvents4, 
        IVsRunningDocTableEvents5, IVsRunningDocTableEvents6
    {
        private ObservableCollection<ObservableItemBase> _openDocList = new ObservableCollection<ObservableItemBase>();
        public ObservableCollection<ObservableItemBase> OpenDocList
        {
            get { return _openDocList; }
            set { SetProperty(ref _openDocList, value); }
        }

        private int _fontSize = 18;
        public int FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        private string _fontFamily = "Courier New";
        public string FontFamily
        {
            get { return _fontFamily; }
            set { SetProperty(ref _fontFamily, value); }
        }

        private IVsRunningDocumentTable _rdt;
        private IVsUIShellOpenDocument _sod;
        private DTE _dte;
        private bool _active;
        private const double _updateCheckIntervalMs = 500;

        #region initialization and shutdown

        public void Initialize(IVsRunningDocumentTable rdt, IVsUIShellOpenDocument sod, DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _rdt = rdt;
            _sod = sod;
            _dte = dte;

            _active = true;

            RegisterForRDTEvents();

            RequestUpdate();

            // timer is used to periodically update, batching update requests together
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(_updateCheckIntervalMs);
            _updateTimer.Tick += UpdateIfNeeded;
            _updateTimer.Start();
        }

        public void Shutdown()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _active = false;

            _updateTimer.Stop();

            UnregisterForRDTEvents();
        }

        #endregion

        #region click handling

        public void OnDocClicked(object listItemData)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (listItemData is ObservableOpenDoc openDoc)
            {
                var docWindowFrame = openDoc.DF.GetDocWindowFrame(_sod, out var isOpen);
                docWindowFrame?.Show();
            }
            else if (listItemData is ObservableProject proj)
            {
                if (_collapsedProjects.Contains(proj.ProjectName))
                    _collapsedProjects.Remove(proj.ProjectName);
                else
                    _collapsedProjects.Add(proj.ProjectName);

                // when expanding/collapsing a project by clicking it, force an immediate update
                // so it's more responsive
                RequestUpdate();
                UpdateIfNeeded(null, null);
            }
        }

        public void OnDocCloseBtnClicked(object listItemData)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var openDoc = listItemData as ObservableOpenDoc;
            if (openDoc == null)
                return;

            var docWindowFrame = openDoc.DF.GetDocWindowFrame(_sod, out var isOpen);
            docWindowFrame?.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_PromptSave);
        }

        #endregion

        #region event registration

        private uint _rdtCookie;

        private void RegisterForRDTEvents()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            RequestUpdate();

            _rdt.AdviseRunningDocTableEvents(this, out _rdtCookie);
        }

        private void UnregisterForRDTEvents()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            RequestUpdate();

            _rdt.UnadviseRunningDocTableEvents(_rdtCookie);
        }

        #endregion

        #region update

        private int _updateRequestedRev;
        private int _updateExecutedRev;
        private DispatcherTimer _updateTimer;

        private readonly HashSet<string> _collapsedProjects = new HashSet<string>();

        private const double _pointsToPixels = 96.0 / 72.0;

        private void UpdateFont()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // https://stackoverflow.com/questions/38399845/how-do-you-retrieve-the-font-size-of-the-text-editor-in-a-vs-extension

            var plist = _dte.get_Properties("FontsAndColors", "TextEditor");

            var prop = plist.Item("FontSize") as Property;
            if (prop != null)
            {
                var szPoints = (System.Int16)prop.Value;
                // need to convert from points to WPF pixels
                FontSize = (int)Math.Round((double)szPoints * _pointsToPixels);
            }

            prop = plist.Item("FontFamily") as Property;
            if (prop != null)
            {
                FontFamily = (string)prop.Value;
            }
        }

        private void RequestUpdate()
        {
            _updateRequestedRev++;
        }

        private void UpdateIfNeeded(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!_active)
                return;

            // only update if there have been update requests since the last update
            if (_updateExecutedRev == _updateRequestedRev)
                return;
            _updateExecutedRev = _updateRequestedRev;

            UpdateFont();

            // retrieve the set of open docs from VS, getting the doc fields
            // for each and organizing them by project
            var projectDocsMap = new Dictionary<string, List<DocFields>>();
            var docCookies = _rdt.GetDocs().ToList();
            foreach (var docCookie in docCookies)
            {
                var doc = _rdt.GetDocFields(docCookie);
                if (doc == null)
                    continue;

                // ignore project and solution files
                if ((doc.DocFlags & _VSRDTFLAGS.RDT_ProjSlnDocument) == _VSRDTFLAGS.RDT_ProjSlnDocument)
                    continue;

                // this check avoids adding in a ton of XAML files which get thrown in the running doc table
                // when you open a XAML file in the designer - 2019.02.13 - dsc
                var docWindowFrame = doc.GetDocWindowFrame(_sod, out var isOpen);
                if ((docWindowFrame == null) || !isOpen)
                    continue;

                List<DocFields> docsForProj;
                if (!projectDocsMap.TryGetValue(doc.ProjCaption, out docsForProj))
                {
                    docsForProj = new List<DocFields>();
                    projectDocsMap[doc.ProjCaption] = docsForProj;
                }
                docsForProj.Add(doc);
            }

            // get sorted list of project names (so the order is repeatable)
            var projectNames = projectDocsMap.Keys.ToList();
            projectNames.Sort();

            // build the "new" list contents which will be used to update the existing list
            var newOpenDocList = new List<ObservableItemBase>();
            foreach (var projectName in projectNames)
            {
                // add a project item
                newOpenDocList.Add(new ObservableProject { ProjectName = projectName });

                // only consider project files if the project is NOT collapsed
                if (_collapsedProjects.Contains(projectName))
                    continue;

                // sort files also (again, for repeatable order and easier visual lookup)
                var docFieldsList = projectDocsMap[projectName];
                docFieldsList.Sort((a, b) => string.Compare(a.DocCaption,b.DocCaption));

                // for all docs in this project
                foreach (var docFields in docFieldsList)
                {
                    // add a doc item
                    var doc = new ObservableOpenDoc
                    {
                        ProjectName = projectName,
                        FileName = docFields.DocCaption,
                        IsActive = (docFields.DocCookie == _activeDocCookie),
                        DF = docFields
                    };
                    newOpenDocList.Add(doc);
                }
            }

            // remove excessive items from the current list
            while (OpenDocList.Count > newOpenDocList.Count)
            {
                OpenDocList.RemoveAt(OpenDocList.Count-1);
            }

            for (int i = 0; i < newOpenDocList.Count; i++)
            {
                // insufficient # items in the current list, add this
                if (OpenDocList.Count <= i)
                {
                    OpenDocList.Add(newOpenDocList[i]);
                }
                // replace existing item, only if comparison fails (different type, different state)
                else if (OpenDocList[i].Compare(newOpenDocList[i]) != 0)
                {
                    OpenDocList[i] = newOpenDocList[i];
                }
            }
        }

        #endregion

        #region event callbacks

        #region IVsRunningDocTableEvents3

        private uint _activeDocCookie;

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, 
            uint dwEditLocksRemaining)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, 
            uint dwEditLocksRemaining)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnAfterSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _activeDocCookie = docCookie;
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, 
            string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents3.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, 
            uint dwEditLocksRemaining)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, 
            uint dwEditLocksRemaining)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnAfterSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _activeDocCookie = docCookie;
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, 
            string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents2.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, 
            uint dwEditLocksRemaining)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, 
            uint dwEditLocksRemaining)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _activeDocCookie = docCookie;
            RequestUpdate();
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsRunningDocTableEvents4

        public int OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        public int OnAfterSaveAll()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        public int OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument, int fClosedWithoutSaving)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsRunningDocTableEvents5

        public void OnAfterDocumentLockCountChanged(uint docCookie, uint dwRDTLockType, uint dwOldLockCount, uint dwNewLockCount)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
        }

        #endregion

        #region IVsRunningDocTableEvents6

        public void OnAfterDocDataChanged(uint cookie, IntPtr punkDocDataOld, IntPtr punkDocDataNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RequestUpdate();
        }

        #endregion

        #endregion
    }
}
