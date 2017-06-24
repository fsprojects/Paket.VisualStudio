using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Paket.VisualStudio.Restore
{
    public class PaketRestorer : IPackageRestorer
    {
        public void Restore(Dependencies dependencies, IEnumerable<RestoringProject> project)
        {
            if (project == null)
            {
                dependencies.Restore();
            }
            else
            {
                dependencies.Restore(
                    FSharpOption<string>.None,
                    ListModule.OfSeq(project.Select(p => p.ReferenceFile)));
            }
        }
    }
}