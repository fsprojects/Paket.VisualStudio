using System.Collections.Generic;

namespace Paket.VisualStudio.Restore
{
    public interface IPackageRestorer
    {
        void Restore(IEnumerable<RestoringProject> projects);
    }
}