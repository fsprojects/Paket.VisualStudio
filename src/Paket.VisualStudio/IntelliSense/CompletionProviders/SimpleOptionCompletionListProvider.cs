using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense.CompletionProviders
{
    internal class SimpleOptionCompletionListProvider : ICompletionListProvider
    {
        private readonly string[] validValues;
        private readonly CompletionContextType contextType;

        public SimpleOptionCompletionListProvider(CompletionContextType contextType, params string[] validValues)
        {
            if (validValues == null)
                throw new ArgumentNullException("validValues");

            this.contextType = contextType;
            this.validValues = validValues;
        }

        public CompletionContextType ContextType
        {
            get { return contextType; }
        }

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            return validValues.Select(value => new Completion(value, value, null, null, "iconAutomationText"));
        }
    }
}