using System.ComponentModel.Design;

namespace Paket.VisualStudio.Commands
{
    internal static class CommandIDs // see CommandTable.vsct for the command ids
    {
        public static readonly CommandID UpdatePackage = new CommandID(Guids.CommandSet, 0x0002);
        public static readonly CommandID RemovePackage = new CommandID(Guids.CommandSet, 0x000B);
    };
}