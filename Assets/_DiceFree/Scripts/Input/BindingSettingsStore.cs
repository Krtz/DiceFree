using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiceFree.Input
{
    /// <summary>Install/user settings only; never reads or writes an Echo save.</summary>
    public sealed class BindingSettingsStore
    {
        public string Path { get; }
        public string BackupPath => Path + ".bak";

        public BindingSettingsStore(string directory) =>
            Path = System.IO.Path.Combine(directory, "bindings.json");

        public IEnumerable<string> CandidatePaths()
        {
            if (File.Exists(Path)) yield return Path;
            if (File.Exists(BackupPath)) yield return BackupPath;
        }

        public string Read(string path) => File.ReadAllText(path);

        public void Write(string json)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            using var writeLock = new FileStream(Path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string pending = Path + ".pending";
            try
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                using (var stream = new FileStream(pending, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(Path)) File.Replace(pending, Path, BackupPath);
                else File.Move(pending, Path);
            }
            finally
            {
                if (File.Exists(pending)) File.Delete(pending);
            }
        }
    }
}
