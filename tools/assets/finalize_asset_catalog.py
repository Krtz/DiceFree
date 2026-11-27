from pathlib import Path
import json,csv,collections,struct
OUT=Path(r'T:\TEMP\DiceFree-AssetCatalog\2026-11-27')
def load(n):return json.loads((OUT/n).read_text(encoding='utf-8-sig'))
def write(n,v):(OUT/n).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf8')
def kind(p):
 e=Path(p).suffix.lower()
 groups={'Model':{'.fbx','.obj','.blend','.gltf','.glb','.dae','.stl'},'Prefab':{'.prefab'},'Animation':{'.anim','.controller','.overridecontroller','.mask'},'Texture':{'.png','.jpg','.jpeg','.tga','.psd','.dds','.exr','.hdr','.tif'},'Material/shader':{'.mat','.shader','.shadergraph','.hlsl','.cginc','.shadersubgraph'},'Audio':{'.wav','.mp3','.ogg','.flac','.aif'},'VFX Graph':{'.vfx','.vfxgraph'},'Code':{'.cs','.asmdef','.asmref','.dll'},'Scene':{'.unity'},'Documentation':{'.md','.txt','.pdf','.html'},'Archive':{'.zip','.rar','.7z','.unitypackage'}}
 return next((k for k,v in groups.items() if e in v),'Other')
project=load('unity-project-files.json');present={r['path'] for r in project};packages=load('unity-packages.json')
for p in packages:
 p['paths']=sorted(set(x.split('\0')[0].splitlines()[0].strip().replace('\\','/') for x in p['paths']))
 p['counts']=dict(collections.Counter(kind(x) for x in p['paths']));p['installedPaths']=[x for x in p['paths'] if x in present];p['installedPathMatches']=len(p['installedPaths']);p['fileEntries']=sum(bool(Path(x).suffix) for x in p['paths'])
 try:
  with open(p['path'],'rb') as f:
   header=f.read(10)
   if header[3]&4:
    length=struct.unpack('<H',f.read(2))[0];extra=f.read(length);meta=json.loads(extra[4:]);p['packageMetadata']={k:meta[k] for k in ('title','version','unity_version','description','category','publisher','author','link') if k in meta}
 except Exception:pass
write('unity-packages.json',packages)
with (OUT/'unity-packages.csv').open('w',newline='',encoding='utf-8-sig') as f:
 w=csv.DictWriter(f,fieldnames=['name','publisher','bytes','sha256','total','fileEntries','installedPathMatches','scan','path'],extrasaction='ignore');w.writeheader();w.writerows(packages)
summary=load('summary.json');summary.update(packagesWithInstalledPaths=sum(bool(p['installedPathMatches']) for p in packages),unityPackageFileEntries=sum(p['fileEntries'] for p in packages),scannedUtc=__import__('datetime').datetime.now(__import__('datetime').timezone.utc).isoformat());write('summary.json',summary)
print('Corrected path padding; installed packs:',summary['packagesWithInstalledPaths'],'file entries:',summary['unityPackageFileEntries'])
print('Animation packs:',[(p['name'],p['counts'],p['installedPathMatches']) for p in packages if ('RPG' in p['name'] and 'Anim' in p['name'])])