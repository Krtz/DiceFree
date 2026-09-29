namespace DiceFree.Persistence
{
    public sealed class SaveFileDiagnostics
    {
        public string path, integrity, writtenUtc;
        public bool exists, valid;
        public int schema;
        public long revision;
    }
}
