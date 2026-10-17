"""Read-only ZIP/nested ZIP, glTF and binary-FBX inventory. Never executes archive code or imports into Unity.
python tools/assets/inspect_downloaded_assets.py --source T:\\TEMP\\ART_ASSETS --output docs/assets/downloaded-assets-2026-10-15.json
Counts describe packaged data, not engine renderer costs or a license grant.
"""
import argparse, collections, hashlib, io, json, re, struct, zipfile, zlib, subprocess, tempfile, shutil, threading
from pathlib import Path
from datetime import datetime, timezone

MAX_MEMBER=768*1024*1024

def glb(stream):
    header=stream.read(12)
    if len(header)!=12 or header[:4]!=b'glTF': raise ValueError('Invalid GLB header')
    magic,version,size=struct.unpack('<4sII',header)
    length,kind=struct.unpack('<II',stream.read(8))
    if version!=2 or kind!=0x4e4f534a: raise ValueError('Unsupported GLB JSON chunk')
    if length>64*1024*1024:
        return dict(format='GLB',version=version,declaredBytes=size,jsonBytes=length,inspectionSkipped='Valid GLB header; JSON chunk exceeds 64 MiB bounded inspection limit')
    d=json.loads(stream.read(length));access=d.get('accessors',[])
    vertices=triangles=0;other=0
    for mesh in d.get('meshes',[]):
        for p in mesh.get('primitives',[]):
            n=access[p['attributes']['POSITION']]['count'];vertices+=n
            count=access[p['indices']]['count'] if 'indices' in p else n
            mode=p.get('mode',4)
            if mode==4:triangles+=count//3
            elif mode in (5,6):triangles+=max(0,count-2)
            else:other+=1
    return dict(format='GLB',version=version,declaredBytes=size,meshes=len(d.get('meshes',[])),
        nodes=len(d.get('nodes',[])),positionVertices=vertices,triangles=triangles,
        skins=len(d.get('skins',[])),joints=sum(len(s.get('joints',[])) for s in d.get('skins',[])),
        animations=len(d.get('animations',[])),animationNames=[a.get('name','unnamed') for a in d.get('animations',[])],
        materials=len(d.get('materials',[])),images=len(d.get('images',[])),nonTrianglePrimitives=other,
        extensionsRequired=d.get('extensionsRequired',[]))

def fbx(data):
    if not data.startswith(b'Kaydara FBX Binary  \x00\x1a\x00'):
        return dict(format='FBX',binary=False,note='ASCII FBX: geometry/rig not parsed')
    version=struct.unpack_from('<I',data,23)[0];wide=version>=7500
    fmt='<QQQB' if wide else '<IIIB';header=25 if wide else 13
    counts=collections.Counter();geometries=[];stackNames=[];warnings=[]
    sizes={'Y':('h',2),'C':('?',1),'I':('i',4),'F':('f',4),'D':('d',8),'L':('q',8)}
    def prop(pos):
        t=chr(data[pos]);pos+=1
        if t in sizes:
            f,n=sizes[t];return struct.unpack_from('<'+f,data,pos)[0],pos+n
        if t in ('S','R'):
            n=struct.unpack_from('<I',data,pos)[0];pos+=4
            value=data[pos:pos+n]
            return (value.decode('utf8','replace') if t=='S' else None),pos+n
        if t in 'fdlibc':
            count,encoding,n=struct.unpack_from('<III',data,pos);pos+=12
            return dict(array=t,count=count,encoding=encoding,start=pos,bytes=n),pos+n
        raise ValueError('Unsupported FBX property '+repr(t))
    def node(pos,parent=None,geo=None):
        end,num,plen,nlen=struct.unpack_from(fmt,data,pos)
        if end==0:return pos+header
        if end>len(data) or end<=pos:raise ValueError('Invalid FBX node span')
        pos+=header;name=data[pos:pos+nlen].decode('ascii','replace');pos+=nlen
        props=[]
        for _ in range(num):v,pos=prop(pos);props.append(v)
        counts[name]+=1
        if name=='Geometry' and props and props[-1]=='Mesh':
            geo=dict(name=str(props[1]).split('\x00')[0],vertices=0,polygons=0,triangles=0);geometries.append(geo)
        if name=='Vertices' and geo is not None and props:geo['vertices']=props[0]['count']//3
        if name=='PolygonVertexIndex' and geo is not None and props:
            a=props[0];raw=data[a['start']:a['start']+a['bytes']]
            if a['encoding']==1:raw=zlib.decompress(raw)
            elif a['encoding']!=0:raise ValueError('Unknown FBX array encoding')
            if len(raw)!=a['count']*4:raise ValueError('Invalid polygon array length')
            face=0
            for (index,) in struct.iter_unpack('<i',raw):
                face+=1
                if index<0:geo['polygons']+=1;geo['triangles']+=max(0,face-2);face=0
        if name=='Deformer' and props and isinstance(props[-1],str):counts['Deformer:'+props[-1]]+=1
        if name=='NodeAttribute' and props and isinstance(props[-1],str):counts['NodeAttribute:'+props[-1]]+=1
        if name=='AnimationStack' and len(props)>1:stackNames.append(str(props[1]).split('\x00')[0])
        while pos<end-header:pos=node(pos,name,geo)
        return end
    pos=27
    while pos+header<len(data):
        if not any(data[pos:pos+header]):break
        pos=node(pos)
    return dict(format='FBX',binary=True,version=version,meshGeometries=len(geometries),
        vertices=sum(g['vertices'] for g in geometries),triangles=sum(g['triangles'] for g in geometries),
        polygons=sum(g['polygons'] for g in geometries),skins=counts['Deformer:Skin'],
        clusters=counts['Deformer:Cluster'],skeletonAttributes=counts['NodeAttribute:LimbNode'],
        animationStacks=counts['AnimationStack'],animationCurves=counts['AnimationCurve'],animationNames=stackNames,
        materials=counts['Material'],geometry=geometries)

def rar(payload,prefix):
    tool=shutil.which('tar')
    if not tool:return [dict(path=prefix,warning='RAR requires libarchive tar; not available')]
    with tempfile.TemporaryDirectory(prefix='DiceFree-RarInspection-') as temp:
        path=Path(temp)/'wrapper.rar';path.write_bytes(payload)
        listing=subprocess.run([tool,'-tf',str(path)],capture_output=True,timeout=30,check=True)
        names=listing.stdout.decode('utf8','replace').splitlines()
        if len(names)>5000:raise ValueError('RAR member count exceeds inspection limit')
        result=[]
        for name in names:
            entry=dict(path=prefix+name)
            if Path(name).suffix.lower()=='.fbx':
                process=subprocess.Popen([tool,'-xOf',str(path),'--',name],stdout=subprocess.PIPE,stderr=subprocess.DEVNULL)
                timer=threading.Timer(30,process.kill)
                timer.start()
                try:
                    payload=process.stdout.read(MAX_MEMBER+1)
                    if len(payload)>MAX_MEMBER:
                        process.kill();entry['warning']='RAR FBX exceeds inspection limit'
                    else:
                        if process.wait(timeout=30)!=0:raise ValueError('RAR FBX read failed')
                        entry['bytes']=len(payload);entry['model']=fbx(payload)
                finally:
                    timer.cancel()
                    process.stdout.close()
                    if process.poll() is None:process.kill()
                    process.wait()
            result.append(entry)
        return result

def inspect_zip(z,prefix='',depth=0):
    result=[]
    for m in z.infolist():
        if m.is_dir():continue
        ext=Path(m.filename).suffix.lower();path=prefix+m.filename
        entry=dict(path=path,bytes=m.file_size,compressedBytes=m.compress_size,crc32=f'{m.CRC:08x}')
        if m.flag_bits&1:entry['warning']='Encrypted member';result.append(entry);continue
        try:
            if ext=='.glb':
                with z.open(m) as stream:entry['model']=glb(stream)
            elif ext in ('.fbx','.zip','.rar'):
                if m.file_size>MAX_MEMBER:entry['warning']='Member exceeds bounded inspection limit'
                elif ext=='.fbx':entry['model']=fbx(z.read(m))
                elif ext=='.rar':entry['members']=rar(z.read(m),path+'!/')
                elif depth<3:
                    with zipfile.ZipFile(io.BytesIO(z.read(m))) as nested:entry['members']=inspect_zip(nested,path+'!/',depth+1)
                else:entry['warning']='Nested ZIP depth limit'
            elif re.search(r'license|licence|readme|credit|copyright|attribution',Path(m.filename).name,re.I):
                entry['documentation']=True
                if ext in ('.txt','.md','.json','.html') and m.file_size<128*1024:
                    entry['text']=z.read(m).decode('utf8','replace')[:16000]
        except Exception as e:entry['error']=str(e)
        result.append(entry)
    return result

def flatten(members):
    for m in members:
        yield m
        yield from flatten(m.get('members',[]))

def main():
    p=argparse.ArgumentParser();p.add_argument('--source',required=True);p.add_argument('--output',required=True);a=p.parse_args()
    source=Path(a.source);rows=[]
    for path in sorted(source.iterdir(),key=lambda p:p.name.lower()):
        if not path.is_file() or path.suffix.lower() not in ('.zip','.glb','.crdownload'):continue
        row=dict(file=path.name,bytes=path.stat().st_size,modifiedUtc=datetime.fromtimestamp(path.stat().st_mtime,timezone.utc).isoformat(),sha256=hashlib.file_digest(path.open('rb'),'sha256').hexdigest())
        try:
            if path.suffix.lower()=='.zip':
                with zipfile.ZipFile(path) as z:row['members']=inspect_zip(z)
            elif path.suffix.lower()=='.glb':
                with path.open('rb') as stream:row['model']=glb(stream)
            else:row['excluded']='Incomplete browser download'
        except Exception as e:row['error']=str(e)
        flat=list(flatten(row.get('members',[])))
        row['extensions']=dict(collections.Counter(Path(m['path']).suffix.lower() for m in flat if 'members' not in m))
        row['models']=[dict(path=m['path'],**m['model']) for m in flat if 'model' in m]
        if 'model' in row:row['models'].append(dict(path=path.name,**row['model']))
        row['errors']=[dict(path=m['path'],error=m['error']) for m in flat if 'error' in m]
        row['documents']=[m['path'] for m in flat if m.get('documentation')]
        rows.append(row);print(json.dumps(dict(file=path.name,models=row['models'],documents=row['documents'],errors=row['errors'])),flush=True)
    output=Path(a.output);output.parent.mkdir(parents=True,exist_ok=True)
    output.write_text(json.dumps(dict(inspectedUtc=datetime.now(timezone.utc).isoformat(),source=str(source),method='Read-only archive directories, glTF JSON accessors and binary FBX node/array inspection; no Unity/Blender import, code execution or source mutation',limits=['FBX triangles are polygon fan estimates, not evaluated modifiers/instances','No textures decoded, no visual/engine/rig deformation validation','No source-page license inferred from filenames','768 MiB member and three nested ZIP levels'],files=rows),indent=2,ensure_ascii=False),encoding='utf8')
    print('INSPECTION_COMPLETE',len(rows),str(output),flush=True)
if __name__=='__main__':main()
