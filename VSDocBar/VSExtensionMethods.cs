using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace VSDocBar
{
    /// <summary>
    /// Holds fields returned from VS for a given open document
    /// </summary>
    public class DocFields
    {
        public uint DocCookie { get; private set; }
        public string DocPath { get; private set; }
        public string DocCaption { get; private set; }
        public _VSRDTFLAGS DocFlags { get; private set; }
        public string DocMoniker { get; private set; }
        public uint DocItemID { get; private set; }
        public string ProjCaption { get; private set; }

        public IVsHierarchy DocHier { get; private set; }

        public DocFields(uint docCookie, string docPath, string docCaption, _VSRDTFLAGS docFlags, string docMoniker, uint docItemID, string projCaption, IVsHierarchy docHier)
        {
            DocCookie = docCookie;
            DocPath = docPath;
            DocCaption = docCaption;
            DocFlags = docFlags;
            DocMoniker = docMoniker;
            DocItemID = docItemID;
            ProjCaption = projCaption;
            DocHier = docHier;
        }

        public IVsWindowFrame GetDocWindowFrame(IVsUIShellOpenDocument sod, out bool isOpen)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            isOpen = false;

            Guid logicalView = VSConstants.LOGVIEWID_Primary;
            var openIDs = new uint[1];
            sod.IsDocumentOpen(DocHier as IVsUIHierarchy, DocItemID, DocMoniker,
                ref logicalView, 0, out var openHier, openIDs, out var docWindowFrame, out var isOpenInt);

            isOpen = isOpenInt != 0;

            return docWindowFrame;
        }
    }

    /// <summary>
    /// Helpers used to get open docs from VS along with the related doc fields for each one
    /// </summary>
    public static class VSExtensionMethods
    {
        public static DocFields GetDocFields(this IVsRunningDocumentTable rdt, uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            uint rdtFlags;
            uint numReadLocks;
            uint numEditLocks;
            IVsHierarchy hier;
            uint itemID;
            IntPtr pDocData;

            rdt.GetDocumentInfo(docCookie,
                out rdtFlags,
                out numReadLocks,
                out numEditLocks,
                out var docPath,
                out hier,
                out itemID,
                out pDocData);

            object propVal = null;

            if (hier == null)
                return null;

            hier.GetProperty(itemID, (int)__VSHPROPID.VSHPROPID_Caption, out propVal);
            var docCaption = (propVal as string) ?? "";

            var docFlags = (_VSRDTFLAGS) rdtFlags;

            var docMoniker = (rdt as IVsRunningDocumentTable4)?.GetDocumentMoniker(docCookie) ?? "";

            hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Caption, out propVal);
            var projCaption = (propVal as string) ?? "";

            return new DocFields(docCookie, docPath, docCaption, docFlags, docMoniker, itemID, projCaption, hier);
        }

        public static IEnumerable<uint> GetDocs(this IVsRunningDocumentTable rdt)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = new List<uint>();

            IEnumRunningDocuments rde;
            if (rdt.GetRunningDocumentsEnum(out rde) != VSConstants.S_OK)
                return result;

            var docCookies = new uint[1];
            while (true)
            {
                if (rde.Next(1, docCookies, out var numFetched) != VSConstants.S_OK)
                    break;
                if (numFetched != 1)
                    break;
                result.Add(docCookies[0]);
            }

            return result;
        }

        public static string FormatRDTFlags(this uint val)
        {
            return ((_VSRDTFLAGS) val).FormatRDTFlags();
        }

        public static string FormatRDTFlags(this _VSRDTFLAGS flags)
        {
            return String.Join("|", flags.GetIndividualFlags());
        }

        public static string FormatRDTAttrib(this uint val)
        {
            return ((__VSRDTATTRIB) val).FormatRDTAttrib();
        }

        public static string FormatRDTAttrib(this __VSRDTATTRIB attrib)
        {
            return String.Join("|", attrib.GetIndividualFlags());
        }

        public static string FormatItemID(this uint itemID)
        {
            string res = itemID.ToString();

            switch (itemID)
            {
                case VSConstants.VSITEMID_NIL:
                    res = "VSITEMID_NIL";
                    break;
                case VSConstants.VSITEMID_ROOT:
                    res = "VSITEMID_ROOT";
                    break;
                case VSConstants.VSITEMID_SELECTION:
                    res = "VSITEMID_SELECTION";
                    break;
            }

            return res;
        }

        public static void Test()
        {
            _VSRDTFLAGS f1 = _VSRDTFLAGS.RDT_ReadLock;
            Debug.WriteLine(f1.FormatRDTFlags());

            _VSRDTFLAGS f2 = _VSRDTFLAGS.RDT_ReadLock | _VSRDTFLAGS.RDT_EditLock;
            Debug.WriteLine(f2.FormatRDTFlags());

            __VSRDTATTRIB a1 = __VSRDTATTRIB.RDTA_Hierarchy;
            Debug.WriteLine(a1.FormatRDTAttrib());

            __VSRDTATTRIB a2 = __VSRDTATTRIB.RDTA_Hierarchy | __VSRDTATTRIB.RDTA_ItemID;
            Debug.WriteLine(a2.FormatRDTAttrib());
        }
    }
}
