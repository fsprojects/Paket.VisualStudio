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
            // Note: Performance optimization because this command is optimized in paket itself
            dependencies.Restore();
        }
    }
}
