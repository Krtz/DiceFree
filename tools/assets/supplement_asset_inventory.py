"""Bounded OBJ/header/PNG review and outer-ZIP CRC checks for a catalog delta."""
import argparse, io, json, struct, zipfile
from pathlib import Path
from datetime import datetime, timezone

def entries(archive,prefix="",depth=0):
    for member in archive.infolist():
        if member.is_dir():continue
        name=prefix+member.filename
        yield archive,member,name
        if Path(member.filename).suffix.lower()==".zip" and depth<3 and member.file_size<=768*1024*1024:
            with zipfile.ZipFile(io.BytesIO(archive.read(member))) as nested:
                yield from entries(nested,name+"!/",depth+1)

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--inventory",required=True);p.add_argument("--baseline",required=True)
    a=p.parse_args();path=Path(a.inventory);data=json.loads(path.read_text(encoding="utf8"))
    previous={r["file"]:r for r in json.loads(Path(a.baseline).read_text(encoding="utf8"))["files"]}
    for row in data["files"]:
        if row["file"] in previous:
            row["unchangedFromBaseline"]=row["sha256"]==previous[row["file"]]["sha256"]
            continue
        source=Path(data["source"])/row["file"]
        if source.suffix.lower()!=".zip":continue
        images=[];formats=[]
        row["models"]=[m for m in row["models"] if m.get("format")!="OBJ"]
        with zipfile.ZipFile(source) as archive:
            bad=archive.testzip()
            row["outerZipCrcCheck"]=dict(status="passed" if bad is None else "failed",badMember=bad)
            for container,member,name in entries(archive):
                ext=Path(member.filename).suffix.lower()
                if ext==".png":
                    with container.open(member) as stream:header=stream.read(24)
                    if header[:8]==b"\x89PNG\r\n\x1a\n" and len(header)==24:
                        w,h=struct.unpack(">II",header[16:24]);images.append(dict(path=name,width=w,height=h))
                elif ext==".obj" and member.file_size<=64*1024*1024:
                    vertices=faces=triangles=0;names=[];materials=set()
                    with container.open(member) as stream:
                        for binary in stream:
                            if len(binary)>1024*1024:raise ValueError("OBJ line exceeds 1 MiB")
                            line=binary.decode("utf8","replace").strip()
                            if line.startswith("v "):vertices+=1
                            elif line.startswith("f "):
                                n=len(line.split())-1;faces+=1;triangles+=max(0,n-2)
                            elif line.startswith(("o ","g ")) and len(names)<30:names.append(line[2:])
                            elif line.startswith("usemtl "):materials.add(line[7:])
                    row["models"].append(dict(path=name,format="OBJ",vertices=vertices,polygons=faces,triangles=triangles,
                        materials=len(materials),objectNames=names,skins=0,animations=0,
                        note="OBJ polygon fan estimate; format supplies no skeletal rig or animation"))
                elif ext==".blend":
                    with container.open(member) as stream:header=stream.read(12)
                    record=dict(path=name,bytes=member.file_size,headerHex=header.hex())
                    if header[:4]==bytes.fromhex("28b52ffd") and member.file_size<8*1024*1024:
                        try:
                            from compression import zstd
                            header=zstd.ZstdDecompressor().decompress(container.read(member),max_length=12)
                            record["compression"]="Zstandard"
                        except Exception as error:record["headerError"]=str(error)
                    record["decodedHeader"]=header.decode("ascii","replace")
                    record["inspection"]="Blender unavailable; scene geometry, rig and animation not parsed"
                    formats.append(record)
        row["pngHeaders"]=images;row["additionalFormats"]=formats
        row["textureSummary"]=dict(pngCount=len(images),largestPngSide=max((max(i["width"],i["height"]) for i in images),default=0),
            note="ZIP/nested-ZIP PNG header dimensions only; RAR images/JPEG dimensions, GPU format and full decoding not measured")
        print(json.dumps(dict(file=row["file"],crc=row["outerZipCrcCheck"]["status"],png=row["textureSummary"],formats=formats)),flush=True)
    data["supplementedUtc"]=datetime.now(timezone.utc).isoformat()
    limit="New ZIPs: outer archive CRC verified; PNG header dimensions; OBJ fan counts; Blender scene not parsed"
    data["limits"]=[item for item in data["limits"] if not item.startswith("New ZIPs:")]+[limit]
    path.write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding="utf8")

if __name__=="__main__":main()
