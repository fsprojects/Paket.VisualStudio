using System;

namespace Paket.VisualStudio.Restore
{
    public class RestoringProject
    {
        private readonly string projectName;
        private readonly string referenceFile;

        public RestoringProject(string projectName, string referenceFile)
        {
            if (projectName == null)
                throw new ArgumentNullException("projectName");
            if (referenceFile == null)
                throw new ArgumentNullException("referenceFile");

            this.projectName = projectName;
            this.referenceFile = referenceFile;
        }

        public string ProjectName
        {
            get { return projectName; }
        }

        public string ReferenceFile
        {
            get { return referenceFile; }
        }
    }
}