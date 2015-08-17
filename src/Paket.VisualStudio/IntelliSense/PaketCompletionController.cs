using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Paket.VisualStudio.IntelliSense.Classifier;

namespace Paket.VisualStudio.IntelliSense
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(PaketDependenciesFileContentType.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class PaketDependenciesCompletionController : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdaptersFactory;

        [Import]
        internal ICompletionBroker CompletionBroker;

        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);
            Debug.Assert(view != null);

            ITextDocument document;
            if (!TextDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            if (!PaketDependenciesClassifierProvider.IsPaketDependenciesFile(document.FilePath))
                return;

            CommandFilter filter = new CommandFilter(view, CompletionBroker);

            IOleCommandTarget next;
            ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(filter, out next));
            filter.Next = next;
        }
    }

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(PaketReferencesFileContentType.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class PaketReferencesCompletionController : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdaptersFactory;

        [Import]
        internal ICompletionBroker CompletionBroker;

        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);
            Debug.Assert(view != null);

            ITextDocument document;
            if (!TextDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            if (!PaketDependenciesClassifierProvider.IsPaketReferencesFile(document.FilePath))
                return;

            CommandFilter filter = new CommandFilter(view, CompletionBroker);

            IOleCommandTarget next;
            ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(filter, out next));
            filter.Next = next;
        }
    }


    internal sealed class CommandFilter : IOleCommandTarget
    {
        private ICompletionSession currentSession;

        public CommandFilter(IWpfTextView textView, ICompletionBroker broker)
        {
            currentSession = null;

            TextView = textView;
            Broker = broker;
        }

        public IWpfTextView TextView { get; private set; }
        public ICompletionBroker Broker { get; private set; }
        public IOleCommandTarget Next { get; set; }

        private static char GetTypeChar(IntPtr pvaIn)
        {
            return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            bool handled = false;
            int hresult = VSConstants.S_OK;

            // 1. Pre-process
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        handled = StartSession();
                        break;
                    case VSConstants.VSStd2KCmdID.RETURN:
                        handled = Complete(false);
                        break;
                    case VSConstants.VSStd2KCmdID.TAB:
                        handled = Complete(true);
                        break;
                    case VSConstants.VSStd2KCmdID.CANCEL:
                        handled = Cancel();
                        break;
                }
            }

            if (!handled)
                hresult = Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (ErrorHandler.Succeeded(hresult))
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID)
                    {
                        case VSConstants.VSStd2KCmdID.TYPECHAR:
                            char ch = GetTypeChar(pvaIn);
                            if (ch == ':')
                                Cancel();
                            else if (!char.IsPunctuation(ch) && !char.IsControl(ch))
                                StartSession();
                            else if (currentSession != null)
                                Filter();
                            break;
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                            if (currentSession == null)
                                StartSession();

                            Filter();
                            break;
                    }
                }
            }

            return hresult;
        }

        private void Filter()
        {
            if (currentSession == null)
                return;

            currentSession.SelectedCompletionSet.SelectBestMatch();
            currentSession.SelectedCompletionSet.Recalculate();
        }

        private bool Cancel()
        {
            if (currentSession == null)
                return false;

            currentSession.Dismiss();

            return true;
        }

        private bool Complete(bool force)
        {
            if (currentSession == null)
                return false;

            if (!currentSession.SelectedCompletionSet.SelectionStatus.IsSelected && !force)
            {
                currentSession.Dismiss();
                return false;
            }
            else
            {
                currentSession.Commit();
                return true;
            }
        }

        private bool StartSession()
        {
            if (currentSession != null)
                return false;

            SnapshotPoint caret = TextView.Caret.Position.BufferPosition;
            ITextSnapshot snapshot = caret.Snapshot;

            if (!Broker.IsCompletionActive(TextView))
            {
                currentSession = Broker.CreateCompletionSession(TextView,
                    snapshot.CreateTrackingPoint(caret, PointTrackingMode.Positive), true);
            }
            else
            {
                currentSession = Broker.GetSessions(TextView)[0];
            }
            currentSession.Dismissed += (sender, args) => currentSession = null;

            if (!currentSession.IsStarted)
                currentSession.Start();

            return true;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)prgCmds[0].cmdID)
                {
                    case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                        return VSConstants.S_OK;
                }
            }
            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }

}