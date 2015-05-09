using System;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense
{
    public class CompletionEntry : Completion2, IComparable<CompletionEntry>, IComparable, ICustomCommit
    {
        private readonly Action<CompletionEntry> commitAction;

        public CompletionEntry(string displayText, string insertionText, string description, ImageSource iconSource, string iconAutomationText = "iconAutomationText", Action<CompletionEntry> commitAction = null)
            : base(displayText, insertionText, description, iconSource, iconAutomationText)
        {
            this.commitAction = commitAction;
        }

        public int SortingPriority { get; set; }

        public int CompareTo(object other)
        {
            return InternalCompareTo(other as CompletionEntry);
        }

        public int CompareTo(CompletionEntry other)
        {
            return InternalCompareTo(other);
        }

        protected virtual int InternalCompareTo(CompletionEntry other)
        {
            int value = -1;
            if (other != null)
            {
                value = other.SortingPriority.CompareTo(SortingPriority);
                if (value == 0)
                {
                    value = string.Compare(DisplayText, other.DisplayText, StringComparison.OrdinalIgnoreCase);
                }
                return value;
            }

            return value;
        }

        public virtual bool UpdateDisplayText(string typedText)
        {
            bool textUpdated = false;
            string str = !string.IsNullOrEmpty(typedText) ? string.Format("Search remote NuGet packages for '{0}'", typedText) : "Search remote NuGet packages";
            if (str != DisplayText)
            {
                DisplayText = str;
                textUpdated = true;
            }
            return textUpdated;
        }

        public virtual void Commit()
        {
            if (commitAction != null)
            {
                commitAction(this);
            }
        }
    }
}