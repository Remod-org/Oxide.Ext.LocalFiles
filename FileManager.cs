using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oxide.Ext.Files;

namespace Oxide.Plugins
{
    [Info("Files DLL Exercise", "RFC1920", "1.0.1")]
    [Description("do stuff")]
    internal class FileManager : RustPlugin
    {
        void OnServerInitialized()
        {
            OxideFilesExtension.ScanDir();
        }
    }
}
