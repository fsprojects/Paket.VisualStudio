using System.Collections.Generic;

namespace Paket.VisualStudio.Restore
{
    public interface IPackageRestorer
    {
        void Restore(Dependencies dependencies, IEnumerable<RestoringProject> projects);
    }
}