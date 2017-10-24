using System;
using System.Diagnostics;
using System.IO;

namespace Paket.VisualStudio.Utils
{

    public static class PaketLauncher
    {
        private static void LaunchProcess(string WorkingDirectory, string ProcessStart, string PaketSubCommand, DataReceivedEventHandler PaketDataReceivedHandler)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = ProcessStart;
                process.StartInfo.Arguments = PaketSubCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = WorkingDirectory;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.OutputDataReceived += PaketDataReceivedHandler;
                process.ErrorDataReceived += PaketDataReceivedHandler;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Exit code '{process.ExitCode}' returned");
                }
            }
            catch (Exception e)
            {
                throw new PaketRuntimeException($"Error running: {ProcessStart} {PaketSubCommand}\n{e.Message}", e);
            }

        }

        public static void LaunchPaket(string SolutionDirectory, string PaketSubCommand, DataReceivedEventHandler PaketDataReceivedHandler)
        {
            //No error handling, let errors from LaunchProcess bubble!
            var PaketLocation = Path.Combine(SolutionDirectory, ".paket", "paket.exe");
            var PaketBootstrapLocation = Path.Combine(SolutionDirectory, ".paket", "paket.bootstrapper.exe");

            if (!File.Exists(PaketLocation))
            {
                if (!File.Exists(PaketBootstrapLocation))
                {
                    //Don't have .paket\paket.exe or paket.bootstrapper.exe
                    throw new FileNotFoundException(
                        $@"Could not locate .paket\paket.exe and .paket\paket.bootstrapper.exe under the folder '{SolutionDirectory}'"
                        + $"\nTo download the binaries, visit https://github.com/fsprojects/Paket/releases"
                        + $"\nTo know more about Paket, visit https://fsprojects.github.io/Paket/getting-started.html\n");
                }

                //Try and get the .paket\paket.exe using paket.bootstrapper.exe
                //If something went wrong with paket.bootstrapper.exe e.g. couldn't download paket.exe due to proxy auth error
                //then we should not execute the command that was originally issued for paket.exe.
                LaunchProcess(SolutionDirectory, PaketBootstrapLocation, "", PaketDataReceivedHandler);
            }
            //At this point, all is well .paket\paket.exe exists or .paket\paket.bootstrapper.exe downloaded it successfully.
            //Now issue the original command to .paket\paket.exe
            LaunchProcess(SolutionDirectory, PaketLocation, PaketSubCommand, PaketDataReceivedHandler);
        }
    }

    public class PaketRuntimeException : Exception
    {
        public PaketRuntimeException()
            : base()
        {
        }

        public PaketRuntimeException(string message)
            : base(message)
        {
        }

        public PaketRuntimeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
