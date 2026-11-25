// Unity CLI eval_file after reaching World Two. Presentation overview only;
// gameplay camera and service verification are recorded separately.
Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
var clock=UnityEngine.Object.FindObjectsByType(T("DiceFree.World.WorldDayNightCycle")).First();clock.GetType().GetMethod("SetHour").Invoke(clock,new object[]{15f,false});
var root=new GameObject("Anvilreach overview camera");var camera=root.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=90;camera.aspect=1.6f;camera.nearClipPlane=.3f;camera.farClipPlane=1500;camera.cullingMask=~((1<<31)|(1<<29)|(1<<30));
camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.15f,.16f);var rotation=Quaternion.Euler(65,25,0);camera.transform.SetPositionAndRotation(new Vector3(64100,10,63791)-rotation*Vector3.forward*230,rotation);
var target=new RenderTexture(1600,1000,24);var previous=RenderTexture.active;bool fog=RenderSettings.fog;var texture=new Texture2D(1600,1000,TextureFormat.RGB24,false);
string path=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA","anvilreach-overview.png");
try{RenderSettings.fog=false;camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1600,1000),0,0);texture.Apply();System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());}
finally{camera.targetTexture=null;RenderTexture.active=previous;RenderSettings.fog=fog;UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(texture);}
return path;
