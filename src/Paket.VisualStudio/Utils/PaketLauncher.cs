using System;
using System.Diagnostics;
using System.IO;

namespace Paket.VisualStudio.Utils
{

    public static class PaketLauncher
    {
        const string PAKET_EXE = ".paket\\paket.exe";
        const string PAKET_BOOTSTRAPPER_EXE = ".paket\\paket.bootstrapper.exe";
        const string PAKET_RELEASE_URL = "https://github.com/fsprojects/Paket/releases";
        const string PAKET_GETTING_STARTED_URL = "https://fsprojects.github.io/Paket/getting-started.html";

        private static int LaunchProcess(string SolutionDirectory, string FileName, string PaketSubCommand, DataReceivedEventHandler PaketDataReceivedHandler)
        {
            Process process = new Process();
            process.StartInfo.FileName = SolutionDirectory + FileName;
            process.StartInfo.Arguments = PaketSubCommand;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = SolutionDirectory;
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
            return process.ExitCode;
        }

        public static void LaunchPaket(string SolutionDirectory, string PaketSubCommand, DataReceivedEventHandler PaketDataReceivedHandler)
        {
            var paketLocation = Path.Combine(SolutionDirectory, PAKET_EXE);
            var paketBootstrapLocation = Path.Combine(SolutionDirectory, PAKET_BOOTSTRAPPER_EXE);

            if (!File.Exists(paketLocation))
            {
                //If .paket\paket.exe is not found under the solution dir, try launching paket.bootstrapper.exe
                if (File.Exists(paketBootstrapLocation))
                {
                    int ExitCode = LaunchProcess(SolutionDirectory, PAKET_BOOTSTRAPPER_EXE, "", PaketDataReceivedHandler);
                    if (ExitCode != 0)
                        /* If something went wrong with paket.bootstrapper.exe e.g. couldn't download paket.exe due to proxy auth error
                         * then we should not execute the command that was originally issued for paket.exe.
                         */
                        throw new PaketRuntimeException("paket.bootstrapper.exe terminated abnormally.");
                }
                else
                    throw new FileNotFoundException(
                        @"Could not locate .paket\paket.exe and .paket\paket.bootstrapper.exe under the solution " + SolutionDirectory
                        + "\nTo download the binaries, visit " + PAKET_RELEASE_URL
                        + "\nTo know more about Paket, visit " + PAKET_GETTING_STARTED_URL + "\n");
            }
            /* At this point, all is well .paket\paket.exe exists or .paket\paket.bootstrapper.exe downloaded it successfully.
             * Now issue the original command to .paket\paket.exe
             */
            if (LaunchProcess(SolutionDirectory, PAKET_EXE, PaketSubCommand, PaketDataReceivedHandler) != 0)
                throw new PaketRuntimeException($"{PAKET_EXE} {PaketSubCommand} failed");
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
