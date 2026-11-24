Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
object Read(object o,string name)=>o?.GetType().GetField(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(o)??o?.GetType().GetProperty(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(o);
object Call(object o,string name,params object[] args)=>o.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(o,args);
var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA","ArmRepair");System.IO.Directory.CreateDirectory(qa);
int checks=0;var errors=new System.Collections.Generic.List<string>();string failure=null;
string Quote(string s)=>s==null?"null":"\""+s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"";
void Need(bool v,string label){checks++;if(!v)throw new Exception(label);}
void Log(string text,string trace,LogType type){if(type is LogType.Error or LogType.Exception)errors.Add(text);}
async System.Threading.Tasks.Task Run()
{
 var objects=new System.Collections.Generic.List<GameObject>();var models=new System.Collections.Generic.List<(string id,GameObject root,Animator animator)>();
 RenderTexture rt=null;Camera camera=null;
 try{
  var menu=UnityEngine.Object.FindObjectsByType(T("DiceFree.Advancement.StartMenuController")).Cast<Component>().First();var profile=Read(menu,"profile");
  var states=((System.Collections.IEnumerable)Call(menu,"ManifestationEntries",profile)).Cast<object>().Select(row=>Read(row,"Item1")).ToArray();
  var stage=(Component)Read(menu,"PortraitStage");var catalog=Resources.Load("MenuPresentationCatalog");
  foreach(var id in new[]{"class.psionic-knight","class.mirage-mage"}){Call(stage,"Get",id,states.Single(v=>(string)Read(v,"classId")==id),catalog);}
  await System.Threading.Tasks.Task.Delay(250);
  foreach(var id in new[]{"class.psionic-knight","class.mirage-mage"}){
   var p=((System.Collections.IEnumerable)Read(stage,"Portraits")).Cast<object>().Single(v=>(string)Read(v,"classId")==id);
   var root=UnityEngine.Object.Instantiate((GameObject)Read(p,"root"));objects.Add(root);root.transform.position=new Vector3(0,-3500,0);root.transform.rotation=Quaternion.identity;
   foreach(var c in root.GetComponentsInChildren<Camera>(true))c.enabled=false;foreach(var l in root.GetComponentsInChildren<Light>(true))l.enabled=false;
   foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
   var inv=root.GetComponentsInChildren(T("DiceFree.Items.CarriedInventory"),true).Cast<Component>().Single();var equipment=root.GetComponentsInChildren(T("DiceFree.Items.Equipment"),true).Cast<Component>().Single();var saved=states.Single(v=>(string)Read(v,"classId")==id);Call(inv,"Restore",Read(saved,"inventory"));Call(equipment,"Restore",Read(saved,"equipment"));Need(((Array)Read(equipment,"Slots")).Length==2,"Restored both equipped hands");
foreach(var m in root.GetComponentsInChildren<MonoBehaviour>(true))if(!new[]{"EquipmentPresentation","Equipment","CarriedInventory","ActorStats"}.Contains(m.GetType().Name))m.enabled=false;
   var a=root.GetComponentInChildren<Animator>();a.enabled=true;a.Rebind();a.Play("Idle",0,.2f);a.Update(0);a.enabled=false;
   models.Add((id,root,a));
  }
  var lamp=new GameObject("Arm review light").AddComponent<Light>();objects.Add(lamp.gameObject);lamp.type=LightType.Directional;lamp.cullingMask=1<<30;lamp.intensity=1.25f;lamp.transform.rotation=Quaternion.Euler(25,150,0);
  camera=new GameObject("Arm review camera").AddComponent<Camera>();objects.Add(camera.gameObject);camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.05f;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.085f,.11f);camera.nearClipPlane=.05f;camera.farClipPlane=20;
  rt=new RenderTexture(1280,960,24);rt.Create();camera.targetTexture=rt;
  void Pose(Animator a){foreach(var m in a.GetComponentsInChildren<MonoBehaviour>(true))if(T("DiceFree.UI.IPortraitBonePose").IsInstanceOfType(m))Call(m,"ApplyPortraitPose");}
  async System.Threading.Tasks.Task Capture(string filename,float angle,Vector3 center,float size){camera.orthographicSize=size;camera.transform.position=center+Quaternion.Euler(0,angle,0)*Vector3.forward*5;camera.transform.LookAt(center);await System.Threading.Tasks.Task.Delay(30);camera.Render();RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();RenderTexture.active=null;System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,filename+".png"),tex.EncodeToPNG());UnityEngine.Object.Destroy(tex);}
  foreach(var model in models){
   foreach(var other in models)other.root.SetActive(other.root==model.root);
   var a=model.animator;Need(a.isHuman&&a.avatar.isValid,"Valid humanoid "+model.id);
   Need(a.GetComponent(T("DiceFree.Visuals.PsionEvolutionHandPose"))!=null,"Independent hand pose installed");
   var u=a.GetBoneTransform(HumanBodyBones.LeftUpperArm);var e=a.GetBoneTransform(HumanBodyBones.LeftLowerArm);var h=a.GetBoneTransform(HumanBodyBones.LeftHand);
   float length1=Vector3.Distance(u.position,e.position),length2=Vector3.Distance(e.position,h.position);
   foreach(var state in new[]{"Idle","Locomotion","Cast","Death"})foreach(float time in new[]{.1f,.45f,.8f}){
    a.enabled=true;a.Play(state,0,time);a.Update(0);a.enabled=false;
    var before=e.rotation;Pose(a);
    Need(Mathf.Abs(Vector3.Distance(u.position,e.position)-length1)<.01f,"Upper arm length stable "+state);
    Need(Mathf.Abs(Vector3.Distance(e.position,h.position)-length2)<.01f,"Forearm length stable "+state);
    if(state is "Idle" or "Locomotion" or "Cast"){
     var rh=a.GetBoneTransform(HumanBodyBones.RightHand);var ru=a.GetBoneTransform(HumanBodyBones.RightUpperArm);var t=a.transform;
     Need(t.InverseTransformPoint(h.position).x<t.InverseTransformPoint(u.position).x-.04f,"Left focus hand clears torso "+state);
     Need(t.InverseTransformPoint(rh.position).x>t.InverseTransformPoint(ru.position).x+.04f,"Right weapon hand clears torso "+state);
    }else Need(Quaternion.Angle(before,e.rotation)<.1f,"Presentation does not override "+state);
    if(time==.45f)await Capture(model.id+"-"+state,0,model.root.transform.position+Vector3.up*.95f,1.05f);
   }
   a.enabled=true;a.Play("Idle",0,.2f);a.Update(0);a.enabled=false;Pose(a);
   await Capture(model.id+"-equipped-front",0,model.root.transform.position+Vector3.up*.95f,1.05f);
   await Capture(model.id+"-equipped-side",55,model.root.transform.position+Vector3.up*.95f,1.05f);
  }
  for(int i=0;i<models.Count;i++){models[i].root.SetActive(true);models[i].root.transform.position=new Vector3(i==0?-.8f:.8f,-3500,0);}
  await Capture("equipped-arm-comparison",0,new Vector3(0,-3499.05f,0),1.2f);
  Need(errors.Count==0,"No runtime errors");
 }catch(Exception e){failure=e.ToString();Debug.LogWarning(failure);}
 finally{foreach(var o in objects)if(o!=null)UnityEngine.Object.Destroy(o);if(rt!=null){rt.Release();UnityEngine.Object.Destroy(rt);}Application.logMessageReceived-=Log;System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"pose-review.json"),"{\"passed\":"+(failure==null?"true":"false")+",\"checks\":"+checks+",\"failure\":"+Quote(failure)+",\"runtimeErrors\":"+errors.Count+"}");}
}
Application.logMessageReceived+=Log;_ = Run();return "Equipped idle, movement, casting and death review scheduled.";



