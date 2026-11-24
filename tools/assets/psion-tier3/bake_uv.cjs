const fs=require('fs'),path=require('path');
const root=process.argv[2];if(!root)throw Error('Provide the extracted art workspace path.');
for(const name of ['PsionicKnight','MirageMage']){
 const file=path.join(root,name,name+'.glb'),raw=fs.readFileSync(file);let at=12,json,bin;
 while(at<raw.length){const len=raw.readUInt32LE(at),type=raw.readUInt32LE(at+4),data=raw.subarray(at+8,at+8+len);if(type===0x4e4f534a)json=JSON.parse(data.toString());if(type===0x004e4942)bin=Buffer.from(data);at+=8+len;}
 let parts=[bin],length=bin.length,baked=0;
 function infos(mat){const out=[];function visit(o){if(!o||typeof o!=='object')return;if(o.extensions?.KHR_texture_transform)out.push(o);for(const [k,v] of Object.entries(o))if(k!=='extensions')visit(v);}visit(mat);return out;}
 for(const mesh of json.meshes)for(const primitive of mesh.primitives){
  const mat=json.materials[primitive.material],textures=infos(mat);if(!textures.length)continue;
  const tr=textures[0].extensions.KHR_texture_transform;const signature=JSON.stringify(tr);
  if(textures.some(t=>JSON.stringify(t.extensions.KHR_texture_transform)!==signature))throw Error('Different material UV transforms need independent UV sets: '+mat.name);
  const uvIndex=tr.texCoord??textures[0].texCoord??0,key='TEXCOORD_'+uvIndex,accessor=json.accessors[primitive.attributes[key]],view=json.bufferViews[accessor.bufferView];
  if(accessor.componentType!==5126||accessor.type!=='VEC2')throw Error('Unexpected UV storage');
  const result=Buffer.alloc(accessor.count*8),offset=tr.offset??[0,0],scale=tr.scale??[1,1],r=tr.rotation??0;
  for(let i=0;i<accessor.count;i++){
   const p=(view.byteOffset??0)+(accessor.byteOffset??0)+i*(view.byteStride??8),u=bin.readFloatLE(p)*scale[0],v=bin.readFloatLE(p+4)*scale[1];
   result.writeFloatLE(offset[0]+Math.cos(r)*u-Math.sin(r)*v,i*8);result.writeFloatLE(offset[1]+Math.sin(r)*u+Math.cos(r)*v,i*8+4);
  }
  const pad=(4-length%4)%4;if(pad){parts.push(Buffer.alloc(pad));length+=pad;}
  const vi=json.bufferViews.length;json.bufferViews.push({buffer:0,byteOffset:length,byteLength:result.length,target:34962});parts.push(result);length+=result.length;
  primitive.attributes[key]=json.accessors.length;json.accessors.push({bufferView:vi,componentType:5126,count:accessor.count,type:'VEC2'});delete primitive.attributes.TANGENT;baked++;
 }
 for(const mat of json.materials)for(const info of infos(mat)){delete info.extensions.KHR_texture_transform;if(!Object.keys(info.extensions).length)delete info.extensions;}
 json.extensionsRequired=(json.extensionsRequired??[]).filter(x=>x!=='KHR_texture_transform');json.extensionsUsed=(json.extensionsUsed??[]).filter(x=>x!=='KHR_texture_transform');json.buffers[0].byteLength=length;
 let j=Buffer.from(JSON.stringify(json)),padding=(4-j.length%4)%4;j=Buffer.concat([j,Buffer.alloc(padding,32)]);let b=Buffer.concat(parts);b=Buffer.concat([b,Buffer.alloc((4-b.length%4)%4)]);
 const header=Buffer.alloc(12);header.writeUInt32LE(0x46546c67);header.writeUInt32LE(2,4);header.writeUInt32LE(28+j.length+b.length,8);
 const jc=Buffer.alloc(8);jc.writeUInt32LE(j.length);jc.writeUInt32LE(0x4e4f534a,4);const bc=Buffer.alloc(8);bc.writeUInt32LE(b.length);bc.writeUInt32LE(0x004e4942,4);
 const output=path.join(root,name,name+'-baked.glb');fs.writeFileSync(output,Buffer.concat([header,jc,j,bc,b]));console.log(name,'UV transforms baked into',baked,'primitives; rigs and skin weights retained.');
}
