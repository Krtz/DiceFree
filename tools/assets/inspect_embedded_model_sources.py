"""Read glTF asset attribution/source metadata without importing models."""
import argparse,io,json,mmap,re,struct,zipfile
from pathlib import Path
from datetime import datetime,timezone

def asset(stream):
    header=stream.read(20)
    if len(header)!=20 or header[:4]!=b"glTF":return None
    count=struct.unpack_from("<I",header,12)[0]
    pending=b""
    while count>0:
        chunk=stream.read(min(count,65536));count-=len(chunk)
        if not chunk:break
        pending=(pending+chunk)[-131072:]
        match=re.search(rb'"asset"\s*:\s*',pending)
        if match:
            try:return json.JSONDecoder().raw_decode(pending[match.end():].decode("utf8","replace").lstrip())[0]
            except json.JSONDecodeError:pass
    return None

def zipped(archive,prefix="",depth=0):
    for member in archive.infolist():
        if member.is_dir():continue
        suffix=Path(member.filename).suffix.lower()
        if suffix==".glb":
            with archive.open(member) as stream:
                value=asset(stream)
                if value:yield dict(path=prefix+member.filename,asset=value)
        elif suffix==".zip" and depth<3 and member.file_size<=768*1024*1024:
            with zipfile.ZipFile(io.BytesIO(archive.read(member))) as nested:
                yield from zipped(nested,prefix+member.filename+"!/",depth+1)

def main():
    parser=argparse.ArgumentParser();parser.add_argument("--inventory",required=True);parser.add_argument("--output",required=True);args=parser.parse_args()
    inv=json.loads(Path(args.inventory).read_text(encoding="utf8"));rows=[]
    for row in inv["files"]:
        path=Path(inv["source"])/row["file"];values=[]
        if path.suffix.lower()==".zip":
            with zipfile.ZipFile(path) as archive:values=list(zipped(archive))
        elif path.suffix.lower()==".glb":
            with path.open("rb") as stream:
                with mmap.mmap(stream.fileno(),0,access=mmap.ACCESS_READ) as mapping:
                    count=struct.unpack_from("<I",mapping,12)[0]
                    match=re.search(rb'"asset"\s*:\s*',mapping[20:20+min(count,1048576)])
                    start=20+match.end() if match else -1
                    if start<0:
                        start=mapping.find(b'"asset"',20,20+count)
                        if start>=0:start=mapping.find(b":",start,start+40)+1
                    if start>=0:
                        value,_=json.JSONDecoder().raw_decode(mapping[start:start+131072].decode("utf8","replace").lstrip())
                        values=[dict(path=path.name,asset=value)]
        if values:
            rows.append(dict(file=row["file"],sha256=row["sha256"],metadata=values))
            print(json.dumps(dict(file=row["file"],metadata=values)),flush=True)
    output=dict(inspectedUtc=datetime.now(timezone.utc).isoformat(),method="Read-only glTF asset metadata; no engine import",files=rows)
    Path(args.output).write_text(json.dumps(output,indent=2,ensure_ascii=False),encoding="utf8")

if __name__=="__main__":main()
