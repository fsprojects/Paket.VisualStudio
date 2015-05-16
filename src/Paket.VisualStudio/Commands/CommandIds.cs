using System.ComponentModel.Design;

namespace Paket.VisualStudio.Commands
{
    internal static class CommandIDs // see CommandTable.vsct for the command ids
    {
        public static readonly CommandID UpdatePackage = new CommandID(Guids.CommandSet, 0x0002);
        public static readonly CommandID RemovePackage = new CommandID(Guids.CommandSet, 0x000B);
        public static readonly CommandID CheckForUpdates = new CommandID(Guids.CommandSet, 0x0006);
        public static readonly CommandID AddPackage = new CommandID(Guids.CommandSet, 0x0014);
        public static readonly CommandID Update = new CommandID(Guids.CommandSet, 0x000A);
        public static readonly CommandID Install = new CommandID(Guids.CommandSet, 0x000C);
        public static readonly CommandID Restore = new CommandID(Guids.CommandSet, 0x000D);
        public static readonly CommandID RemovePackageFromProject = new CommandID(Guids.CommandSet, 0x000E);
        public static readonly CommandID Simplify = new CommandID(Guids.CommandSet, 0x000F);

        public static readonly CommandID ConvertFromNuget = new CommandID(Guids.CommandSet, 0x0010);
        public static readonly CommandID UpdateSolution = new CommandID(Guids.CommandSet, 0x0011);
        public static readonly CommandID InstallSolution = new CommandID(Guids.CommandSet, 0x0012);
        public static readonly CommandID RestoreSolution = new CommandID(Guids.CommandSet, 0x0013);
        public static readonly CommandID AddPackageToProject = new CommandID(Guids.CommandSet, 0x0015);
    };
}