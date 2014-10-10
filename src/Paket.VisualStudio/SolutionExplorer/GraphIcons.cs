using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class GraphIcons
    {
        private const string Prefix = "Paket.VisualStudio";
        private const string PrefixDot = Prefix + ".";

        public const string Packages = PrefixDot + "Packages";
        public const string Package = PrefixDot + "Package";
        public const string Github = PrefixDot + "Github";
        public const string PackagesConfig = PrefixDot + "PackagesConfig";
        public const string PackageUpdate = PrefixDot + "PackageUpdate";
        public const string PackPrefix = "pack://application:,,,/" + Prefix + ";component/Resources/";

        private readonly IServiceProvider serviceProvider;

        public GraphIcons(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            RegisterIcon(Packages, "nuget.png");
            RegisterIcon(Package, "nuget.png");
            RegisterIcon(Github, "github.png");
            RegisterIcon(PackagesConfig, "config.ico");
            RegisterIcon(PackageUpdate, "update.ico");
        }

        private void RegisterIcon(string imageName, string resourceName)
        {
            var imageService = serviceProvider.GetService<SVsImageService, IVsImageService>();
            imageService.Add(imageName, new LazyImage(() => LoadWpfImage(resourceName)));
        }

        private static ImageSource LoadWpfImage(string resourceName)
        {
            string fullResourceName = PackPrefix + resourceName;

            return new BitmapImage(new Uri(fullResourceName));
        }
    }

    internal class LazyImage : IVsUIObject
    {
        private readonly Lazy<IVsUIObject> instance;

        public LazyImage(Func<ImageSource> imageCreator)
        {
            instance = new Lazy<IVsUIObject>(() => WpfPropertyValue.CreateIconObject(imageCreator()));
        }

        public int Equals(IVsUIObject pOtherObject, out bool pfAreEqual)
        {
            return instance.Value.Equals(pOtherObject, out pfAreEqual);
        }

        public int get_Data(out object pVar)
        {
            return instance.Value.get_Data(out pVar);
        }

        public int get_Format(out uint pdwDataFormat)
        {
            return instance.Value.get_Format(out pdwDataFormat);
        }

        public int get_Type(out string pTypeName)
        {
            return instance.Value.get_Type(out pTypeName);
        }
    }

}