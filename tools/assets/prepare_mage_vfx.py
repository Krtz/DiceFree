"""Selected cached VFX dependency plan/extraction; excludes demos and publisher code."""
import argparse,collections,hashlib,json,re,struct,tarfile
from pathlib import Path
choices={
"Free Game VFX CollectionURP":{"EmberProjectile":"Prefab/FX_Fireball.prefab"},
"Magic Effects FREE":{
"EmberImpact":"Hits and explosions/Explosion.prefab",
"FrostImpact":"Hits and explosions/Snow hit.prefab",
"FrostCircle":"Magic circles/Freeze circle.prefab",
"LightningImpact":"Hits and explosions/Electro hit.prefab",
"SandBurst":"Smoke effects/Dust ground.prefab",
"SandLoop":"Smoke effects/Dust loop.prefab",
"MathShield":"Magic shields/Magic shield pink.prefab",
"RadiationAura":"Character auras/Debuff.prefab",
"RadiationImpact":"Hits and explosions/Green hit.prefab"}}
def metadata(path):
    with path.open("rb") as f:
        header=f.read(10)
        if header[:3]!=b"\x1f\x8b\x08" or not header[3]&4:return {}
        extra=f.read(struct.unpack("<H",f.read(2))[0]);pos=0
        while pos+4<=len(extra):
            size=struct.unpack_from("<H",extra,pos+2)[0];value=extra[pos+4:pos+4+size];pos+=4+size
            if value.startswith(b"{"):return json.loads(value)
    return {}
def main():
    p=argparse.ArgumentParser();p.add_argument("--extract",action="store_true");a=p.parse_args()
    catalog=json.loads(Path("T:/TEMP/DiceFree-AssetCatalog/packages.json").read_text(encoding="utf-8-sig"))
    output=[]
    for pack in catalog:
        if pack["name"] not in choices:continue
        package=Path(pack["path"]);tag="Eric" if pack["name"].startswith("Free Game") else "Hovl"
        base=Path("Assets/_DiceFree/Art/MageVfxVendor")/tag
        with tarfile.open(package,"r:gz") as tar:
            members={m.name:m for m in tar};paths={}
            for name,m in members.items():
                if name.endswith("/pathname"):
                    path=tar.extractfile(m).read().decode("utf-8-sig","replace").splitlines()[0].strip("\0 \r\n").replace("\\","/")
                    if path.startswith("Assets/") and ".." not in Path(path).parts:paths[name.split("/")[0]]=path
            roots={}
            for key,suffix in choices[pack["name"]].items():
                hits=[g for g,path in paths.items() if path.endswith(suffix)]
                if len(hits)!=1:raise ValueError((key,hits))
                roots[key]=hits[0]
            pending=list(roots.values());closure=set()
            while pending:
                guid=pending.pop()
                if guid in closure or guid not in paths:continue
                path=paths[guid]
                if Path(path).suffix.lower() in (".cs",".dll",".asmdef",".unity"):continue
                closure.add(guid);member=members.get(guid+"/asset")
                if member and member.size<8*1024*1024 and Path(path).suffix.lower() in (".prefab",".mat",".asset",".shader",".shadergraph",".shadersubgraph",".anim",".controller"):
                    pending.extend(d.decode() for d in re.findall(rb"(?<![a-fA-F0-9])([a-fA-F0-9]{32})(?![a-fA-F0-9])",tar.extractfile(member).read()) if d.decode() in paths)
            receipt=dict(package=pack["name"],packageSha256=hashlib.file_digest(package.open("rb"),"sha256").hexdigest(),
                header=metadata(package),roots={k:str(base/paths[g][7:]).replace("\\","/") for k,g in roots.items()},files=[])
            for guid in sorted(closure):
                member=members.get(guid+"/asset")
                if not member:continue
                dest=base/paths[guid][7:];content=tar.extractfile(member).read()
                receipt["files"].append(dict(source=paths[guid],destination=str(dest).replace("\\","/"),guid=guid,bytes=len(content),sha256=hashlib.sha256(content).hexdigest()))
                if a.extract:
                    dest.parent.mkdir(parents=True,exist_ok=True)
                    if dest.exists() and dest.read_bytes()!=content:raise ValueError("Different asset already exists: "+str(dest))
                    if not dest.exists():dest.write_bytes(content)
                    meta=members.get(guid+"/asset.meta")
                    if meta and not Path(str(dest)+".meta").exists():Path(str(dest)+".meta").write_bytes(tar.extractfile(meta).read())
            output.append(receipt)
            print(json.dumps(dict(package=pack["name"],roots=receipt["roots"],files=len(receipt["files"]),bytes=sum(f["bytes"] for f in receipt["files"]),metadata=receipt["header"])),flush=True)
    target=Path("SourceArt/Vfx/Mage") if a.extract else Path("Library")
    target.mkdir(parents=True,exist_ok=True)
    (target/"selected-pack-sources.json").write_text(json.dumps(dict(mode="extracted" if a.extract else "plan",canonicalAdmission=False,packages=output),indent=2),encoding="utf8")
if __name__=="__main__":main()
