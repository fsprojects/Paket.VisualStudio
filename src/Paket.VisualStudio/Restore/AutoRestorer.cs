using System;
using System.Collections.Generic;

namespace Paket.VisualStudio.Restore
{
    public class AutoRestorer : IPackageRestorer
    {
        private readonly IPackageRestorer restorer;
        private readonly PaketSettings settings;

        public AutoRestorer(IPackageRestorer restorer, PaketSettings settings)
        {
            if (restorer == null)
                throw new ArgumentNullException("restorer");
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.restorer = restorer;
            this.settings = settings;
        }

        public void Restore(Dependencies dependencies, IEnumerable<RestoringProject> projects)
        {
            if (!settings.AutoRestore)
                return;
            restorer.Restore(dependencies, projects);
        }
    }
}
