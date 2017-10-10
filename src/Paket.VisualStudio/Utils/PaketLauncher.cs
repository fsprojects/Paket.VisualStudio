using System.Diagnostics;

namespace Paket.VisualStudio.Utils
{
    public static class PaketLauncher
    {
        public static void LaunchPaket(string SolutionDirectory, string PaketSubCommand, DataReceivedEventHandler PaketDataReceivedHandler)
        {
                Process process = new Process();
                process.StartInfo.FileName = SolutionDirectory + ".paket\\paket.exe";
                process.StartInfo.Arguments = PaketSubCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = SolutionDirectory;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += PaketDataReceivedHandler;
                process.ErrorDataReceived += PaketDataReceivedHandler;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
        }
    }
    
}
