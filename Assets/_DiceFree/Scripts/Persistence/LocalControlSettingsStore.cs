using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Persistence
{
    public sealed class LocalControlSettingsStore
    {
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        public string Path { get; }
        public bool ReadOnly { get; private set; }
        public string Feedback { get; private set; }
        public LocalControlSettingsStore(string directory) => Path=System.IO.Path.Combine(directory,"user-controls-v1.json");
        private static string Hash(string text)
        {
            using var hash=SHA256.Create();
            return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        private static ControlSettings Read(string path)
        {
            var envelope=JsonUtility.FromJson<Envelope>(File.ReadAllText(path));
            if(envelope!=null && envelope.version>1) throw new NotSupportedException("Newer controls settings version.");
            if(envelope==null || envelope.version!=1 || envelope.payload==null || envelope.checksum!=Hash(envelope.payload))
                throw new InvalidDataException("Controls settings checksum/header failed.");
            var settings=JsonUtility.FromJson<ControlSettings>(envelope.payload);
            if(settings==null || settings.version!=1 || settings.bindings==null) throw new InvalidDataException("Invalid controls settings model.");
            return settings;
        }
        public ControlSettings Load()
        {
            ReadOnly=false;
            Feedback="No controls settings found; defaults used.";
            ControlBindings.Load(new ControlSettings());
            foreach(var candidate in new[] { Path, Path+".bak" })
            {
                if(!File.Exists(candidate)) continue;
                try
                {
                    var settings=Read(candidate);
                    if(!ControlBindings.Load(settings)) throw new InvalidDataException("Invalid/conflicting controls settings.");
                    Feedback=candidate==Path ? "Controls loaded." : "Recovered controls from backup.";
                    return settings;
                }
                catch(NotSupportedException) { ReadOnly=true; Feedback="Newer settings version preserved; changes are session-only."; return null; }
                catch(Exception error) when(error is IOException || error is InvalidDataException || error is ArgumentException || error is UnauthorizedAccessException)
                { Feedback="Unreadable controls settings; safe defaults used. " + error.Message; }
            }
            return null;
        }
        public void Save(ControlSettings settings)
        {
            if(ReadOnly) return;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            using var writeLock=new FileStream(Path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            bool valid=false;
            if(File.Exists(Path))
                try { Read(Path); valid=true; }
                catch(NotSupportedException) { ReadOnly=true; Feedback="Newer settings version preserved; changes are session-only."; return; }
                catch(Exception error) when(error is IOException || error is ArgumentException) { }
            var payload=JsonUtility.ToJson(settings);
            var bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Envelope { version=1,payload=payload,checksum=Hash(payload) },true));
            using(var stream=new FileStream(Path+".pending",FileMode.Create,FileAccess.Write,FileShare.None))
            { stream.Write(bytes,0,bytes.Length); stream.Flush(true); }
            Read(Path+".pending");
            if(File.Exists(Path)) File.Replace(Path+".pending",Path,valid ? Path+".bak" : Path+".corrupt-"+Guid.NewGuid().ToString("N"));
            else File.Move(Path+".pending",Path);
            Feedback="Controls saved.";
        }
    }
}
