using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Paket.VisualStudio.Utils
{
    internal static class DteHelper
    {
        public static DTE2 DTE = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;

        public static void ExecuteCommand(string commandName)
        {
            var command = DTE.Commands.Item(commandName);
            if (command.IsAvailable)
            {
                DTE.ExecuteCommand(command.Name);
            }
        }

        public static bool IsSupportedFile(string allowedName)
        {
            var doc = DTE.ActiveDocument;

            if (doc == null || string.IsNullOrEmpty(doc.FullName) || Path.GetFileName(doc.FullName) != allowedName)
                return false;

            return true;
        }

        public static string DownloadText(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch (Exception)
            { /* Can't download. Just ignore */ }

            return null;
        }

        public static void AnimateWindowSize(this UserControl target, double oldHeight, double newHeight)
        {
            target.Height = oldHeight;
            target.BeginAnimation(UserControl.HeightProperty, null);

            Storyboard sb = new Storyboard();

            var aniHeight = new DoubleAnimationUsingKeyFrames();
            aniHeight.Duration = new Duration(new TimeSpan(0, 0, 0, 2));
            aniHeight.KeyFrames.Add(new EasingDoubleKeyFrame(target.Height, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 1))));
            aniHeight.KeyFrames.Add(new EasingDoubleKeyFrame(newHeight, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 1, 200))));

            Storyboard.SetTarget(aniHeight, target);
            Storyboard.SetTargetProperty(aniHeight, new PropertyPath(UserControl.HeightProperty));

            sb.Children.Add(aniHeight);

            sb.Begin();
        }

        public static string GetFileName(this IPropertyOwner owner)
        {
            IVsTextBuffer bufferAdapter;

            if (!owner.Properties.TryGetProperty(typeof(IVsTextBuffer), out bufferAdapter))
                return null;

            var persistFileFormat = bufferAdapter as IPersistFileFormat;
            string ppzsFilename = null;
            uint pnFormatIndex;
            int returnCode = -1;

            if (persistFileFormat != null)
            {
                try
                {
                    returnCode = persistFileFormat.GetCurFile(out ppzsFilename, out pnFormatIndex);
                }
                catch (NotImplementedException)
                {
                    return null;
                }
            }

            if (returnCode != VSConstants.S_OK)
                return null;

            return ppzsFilename;
        }

        public static void RunProcess(string arguments, string directory, string statusbar, Action callback = null)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (callback != null)
                        callback();
                }
                catch { /* Ignore any failure */ }
            });
        }

        public static void SaveDocument()
        {
            var doc = DTE.ActiveDocument;

            if (doc != null)
                doc.Save();
        }
    }
}
