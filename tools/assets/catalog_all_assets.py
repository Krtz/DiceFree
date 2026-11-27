"""Read-only full Unity/ART_ASSETS catalog. Never extracts or executes archive content."""
from pathlib import Path
import os,sys,json,csv,hashlib,tarfile,zipfile,gzip,io,collections,concurrent.futures,datetime,importlib.util,time
PROJECT=Path(sys.argv[1] if len(sys.argv)>1 else r'T:\TEMP\DiceFree-Options')
ART=Path(sys.argv[2] if len(sys.argv)>2 else r'T:\TEMP\ART_ASSETS')
OUT=Path(sys.argv[3] if len(sys.argv)>3 else r'T:\TEMP\DiceFree-AssetCatalog\2026-11-27');OUT.mkdir(parents=True,exist_ok=True)
CACHE=Path(os.environ.get('APPDATA',r'C:\Users\Axel\AppData\Roaming'))/'Unity'/'Asset Store-5.x'
GROUPS={'Model':{'.fbx','.obj','.blend','.gltf','.glb','.dae','.stl'},'Prefab':{'.prefab'},'Animation':{'.anim','.controller','.overridecontroller','.mask'},'Texture':{'.png','.jpg','.jpeg','.tga','.psd','.dds','.exr','.hdr','.tif'},'Material/shader':{'.mat','.shader','.shadergraph','.hlsl','.cginc','.shadersubgraph'},'Audio':{'.wav','.mp3','.ogg','.flac','.aif'},'VFX Graph':{'.vfx','.vfxgraph'},'Code':{'.cs','.asmdef','.asmref','.dll'},'Scene':{'.unity'},'Documentation':{'.md','.txt','.pdf','.html'},'Archive':{'.zip','.rar','.7z','.unitypackage'}}
def kind(p):
 e=Path(p).suffix.lower()
 return next((k for k,v in GROUPS.items() if e in v),'Other')
def sha(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
 return h.hexdigest()
def write(name,data): (OUT/name).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
def csvout(name,rows,fields):
 with (OUT/name).open('w',newline='',encoding='utf-8-sig') as f:
  w=csv.DictWriter(f,fieldnames=fields,extrasaction='ignore');w.writeheader();w.writerows(rows)
project=[]
for p in sorted((PROJECT/'Assets').rglob('*')):
 if not p.is_file() or p.suffix.lower()=='.meta':continue
 rel=p.relative_to(PROJECT).as_posix();s=p.stat();meta=Path(str(p)+'.meta');guid=''
 if meta.exists():
  for line in meta.read_text(encoding='utf8',errors='replace').splitlines()[:6]:
   if line.startswith('guid:'):guid=line[5:].strip();break
 project.append(dict(path=rel,kind=kind(p),bytes=s.st_size,modifiedUtc=datetime.datetime.fromtimestamp(s.st_mtime,datetime.timezone.utc).isoformat(),guid=guid,root=rel.split('/')[1]))
write('unity-project-files.json',project);csvout('unity-project-files.csv',project,['path','kind','bytes','guid','root','modifiedUtc']);present={r['path'] for r in project}
print('PROJECT_INDEX',len(project),flush=True)
if '--project-only' in sys.argv:
 summary=json.loads((OUT/'summary.json').read_text(encoding='utf8')) if (OUT/'summary.json').exists() else {}
 summary.update(projectAssets=len(project),projectKinds=dict(collections.Counter(r['kind'] for r in project)),scannedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat())
 write('summary.json',summary);print('PROJECT_INDEX_COMPLETE',len(project),flush=True);sys.exit(0)
previous_packages={r['path']:r for r in json.loads((OUT/'unity-packages.json').read_text(encoding='utf8'))} if (OUT/'unity-packages.json').exists() else {}
def package(p):
 s=p.stat();paths=[];errors=[]
 try:
  with gzip.open(p,'rb') as inflated, tarfile.open(fileobj=inflated,mode='r|') as archive:
   for m in archive:
    if m.isfile() and m.name.endswith('/pathname'):
     with archive.extractfile(m) as f: paths.append(f.read(65536).split(b'\0')[0].decode('utf8','replace').splitlines()[0].strip().replace('\\','/'))
 except Exception as e:errors.append(type(e).__name__+': '+str(e))
 paths=sorted(set(paths));matches=[x for x in paths if x in present];parts=p.relative_to(CACHE).parts
 previous=previous_packages.get(str(p),{});modified=datetime.datetime.fromtimestamp(s.st_mtime,datetime.timezone.utc).isoformat();digest=previous.get('sha256') if previous.get('bytes')==s.st_size and previous.get('modifiedUtc')==modified else None
 return dict(source='Unity Asset Store cache',name=p.stem,path=str(p),publisher=parts[0],bytes=s.st_size,sha256=digest or sha(p),modifiedUtc=modified,paths=paths,counts=dict(collections.Counter(kind(x) for x in paths)),total=len(paths),installedPathMatches=len(matches),installedPaths=matches,scan='complete' if not errors else 'archive-error',errors=errors)
packages=[];allpacks=sorted(CACHE.rglob('*.unitypackage'))
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
 for row in pool.map(package,allpacks):
  packages.append(row)
  if len(packages)%10==0: print('UNITY_PACKAGES',len(packages),'of',len(allpacks),flush=True);write('progress.json',dict(stage='unity-packages',done=len(packages),total=len(allpacks)))
write('unity-packages.json',packages);csvout('unity-packages.csv',packages,['name','publisher','bytes','sha256','total','installedPathMatches','scan','path'])
if '--packages-only' in sys.argv:
 summary=json.loads((OUT/'summary.json').read_text(encoding='utf8')) if (OUT/'summary.json').exists() else {}
 summary.update(unityPackages=len(packages),unityPackagesComplete=sum(r['scan']=='complete' for r in packages),unityPackageAssets=sum(r['total'] for r in packages),packagesWithInstalledPaths=sum(bool(r['installedPathMatches']) for r in packages))
 write('summary.json',summary);write('progress.json',dict(stage='complete',**summary));print('UNITY_PACKAGES_COMPLETE',json.dumps(summary),flush=True);sys.exit(0)
known={}
for old in (PROJECT/'docs/assets').glob('*.json'):
 try:
  data=json.loads(old.read_text(encoding='utf-8-sig'))
  for r in data.get('files',[]) if isinstance(data,dict) else []:
   if r.get('sha256') and r.get('models'):known[r['sha256']]=r
 except Exception:pass
spec=importlib.util.spec_from_file_location('model_inspect',str(PROJECT/'tools/assets/inspect_downloaded_assets.py'));inspect=importlib.util.module_from_spec(spec);spec.loader.exec_module(inspect)
def raw(p):
 s=p.stat();row=dict(source='ART_ASSETS',name=p.name,path=str(p),relativePath=p.relative_to(ART).as_posix(),bytes=s.st_size,sha256=sha(p),kind=kind(p),members=[],models=[],errors=[])
 previous=known.get(row['sha256']);cached={m['path']:m for m in previous.get('models',[])} if previous else {}
 def archive(z,prefix='',depth=0):
  for m in z.infolist():
   if m.is_dir():continue
   name=prefix+m.filename;ext=Path(m.filename).suffix.lower();row['members'].append(dict(path=name,kind=kind(name),bytes=m.file_size))
   try:
    if name in cached:row['models'].append(dict(cached[name],inspection='previous technical inspection, current archive SHA256 verified'));continue
    if ext in ('.glb','.gltf','.fbx','.obj'):
     model=dict(path=name,format=ext[1:].upper())
     if ext=='.glb':
      with z.open(m) as f:model.update(inspect.glb(f))
     elif ext=='.fbx' and m.file_size<=128*1024*1024:model.update(inspect.fbx(z.read(m)))
     elif ext=='.fbx':model['inspection']='Header/topology inspection deferred: member larger than128MiB; member is cataloged'
     elif ext=='.gltf' and m.file_size<16*1024*1024:
      d=json.loads(z.read(m));model.update(skins=len(d.get('skins',[])),animations=len(d.get('animations',[])),animationNames=[a.get('name','') for a in d.get('animations',[])],materials=len(d.get('materials',[])))
     elif ext=='.obj':model.update(skins=0,animations=0,inspection='OBJ format has no skeletal rig')
     row['models'].append(model)
    elif ext=='.zip' and depth<3 and m.file_size<256*1024*1024:
     with zipfile.ZipFile(io.BytesIO(z.read(m))) as child:archive(child,name+'!/',depth+1)
    elif ext in ('.rar','.7z','.zip'):row['errors'].append(dict(path=name,note='Nested archive cataloged; content expansion deferred'))
   except Exception as e:row['errors'].append(dict(path=name,error=type(e).__name__+': '+str(e)[:220]))
 try:
  if p.suffix.lower()=='.zip':
   with zipfile.ZipFile(p) as z:archive(z)
  elif p.suffix.lower()=='.glb':
   with p.open('rb') as f:row['models']=[dict(path=p.name,**inspect.glb(f))]
 except Exception as e:row['errors'].append(dict(error=str(e)))
 row['counts']=dict(collections.Counter(m['kind'] for m in row['members']));return row
rawfiles=sorted(p for p in ART.rglob('*') if p.is_file());rawrows=[]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
 for row in pool.map(raw,rawfiles):
  rawrows.append(row)
  if len(rawrows)%20==0:print('ART_FILES',len(rawrows),'of',len(rawfiles),flush=True)
write('art-files.json',rawrows);csvout('art-files.csv',rawrows,['relativePath','kind','bytes','sha256','path'])
modelrows=[dict(archive=r['relativePath'],**m) for r in rawrows for m in r['models']];write('art-model-details.json',modelrows)
csvout('art-models.csv',modelrows,['archive','path','format','triangles','vertices','skins','animations','materials','inspection'])
csvout('art-archive-members.csv',[dict(archive=r['relativePath'],**m) for r in rawrows for m in r['members']],['archive','path','kind','bytes'])
byhash=collections.defaultdict(list)
for r in packages+rawrows:byhash[r['sha256']].append(r['path'])
write('exact-duplicates.json',[dict(sha256=h,paths=v) for h,v in byhash.items() if len(v)>1])
summary=dict(scannedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),project=str(PROJECT),rawRoot=str(ART),unityCache=str(CACHE),projectAssets=len(project),projectKinds=dict(collections.Counter(r['kind'] for r in project)),unityPackages=len(packages),unityPackagesComplete=sum(r['scan']=='complete' for r in packages),unityPackageAssets=sum(r['total'] for r in packages),packagesWithInstalledPaths=sum(bool(r['installedPathMatches']) for r in packages),rawFiles=len(rawrows),rawArchives=sum(r['kind']=='Archive' for r in rawrows),rawModels=len(modelrows),rawMembers=sum(len(r['members']) for r in rawrows),duplicateGroups=sum(len(v)>1 for v in byhash.values()),rawInspectionLimitations=sum(bool(r['errors']) for r in rawrows))
write('summary.json',summary);write('progress.json',dict(stage='complete',**summary));print('CATALOG_COMPLETE',json.dumps(summary),flush=True)