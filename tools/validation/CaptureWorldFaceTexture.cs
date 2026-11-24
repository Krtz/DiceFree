// Run with the Unity runtime eval_file command after the hero has entered the face to photograph.
// Writes into the nearest enclosing Unity project; rebuild the player to use the refreshed texture.
Type FindType(string name) => AppDomain.CurrentDomain.GetAssemblies()
    .Select(assembly => assembly.GetType(name)).First(type => type != null);
var traversalType=FindType("DiceFree.Characters.TraversalInput");
var hero=(Component)UnityEngine.Object.FindObjectsByType(traversalType).First();
var region=FindType("DiceFree.World.AshfallReach");
var origin=(Vector3)region.GetField("Origin").GetValue(null);
var bounds=(Rect)region.GetField("Bounds").GetValue(null);
bool second=bounds.Contains(new Vector2(hero.transform.position.x,hero.transform.position.z));
float width=(float)region.GetField("FaceWidth").GetRawConstantValue(),height=(float)region.GetField("FaceHeight").GetRawConstantValue();
var face=new Rect(-85,-53,width,height);
if(second)
{
    face=(Rect)region.GetField("LocalBounds").GetValue(null);
}
var project=System.IO.Directory.GetParent(Application.dataPath);
while(project!=null&&!System.IO.Directory.Exists(System.IO.Path.Combine(project.FullName,"Assets/_DiceFree/Scenes")))
    project=project.Parent;
if(project==null)throw new InvalidOperationException("Run from Play Mode or a player built inside its Unity project.");
string output=System.IO.Path.Combine(project.FullName,"Assets/_DiceFree/Resources/WorldFaceCrossing",second?"World 2.png":"World 1.png");
System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
var root=new GameObject("Runtime world-face photograph");
var camera=root.AddComponent<Camera>();
camera.enabled=false;camera.orthographic=true;camera.orthographicSize=face.height*.5f;
camera.aspect=face.width/face.height;
camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.26f,.17f);
camera.nearClipPlane=.3f;camera.farClipPlane=1500;camera.cullingMask=~((1<<31)|(1<<29)|(1<<30));
camera.transform.SetPositionAndRotation((second?origin:Vector3.zero)+new Vector3(face.center.x,650,face.center.y),Quaternion.Euler(90,0,0));
var frame=new RenderTexture(1536,1536,24,RenderTextureFormat.ARGB32);frame.Create();
var pixels=new Texture2D(1536,1536,TextureFormat.RGB24,false);
var previous=RenderTexture.active;bool fog=RenderSettings.fog;
var faders=UnityEngine.Object.FindObjectsByType(FindType("DiceFree.Core.CameraOcclusionFader")).Cast<Behaviour>().Where(f=>f.enabled).ToArray();
try
{
    RenderSettings.fog=false;camera.targetTexture=frame;
    foreach(var fader in faders)fader.enabled=false;
    var requestType=FindType("UnityEngine.Rendering.Universal.UniversalRenderPipeline+SingleCameraRequest");
    var request=Activator.CreateInstance(requestType);requestType.GetField("destination").SetValue(request,frame);
    typeof(UnityEngine.Rendering.RenderPipeline).GetMethods().Single(method=>method.Name=="SubmitRenderRequest")
        .MakeGenericMethod(requestType).Invoke(null,new object[]{camera,request});
    RenderTexture.active=frame;pixels.ReadPixels(new Rect(0,0,1536,1536),0,0);pixels.Apply();
    System.IO.File.WriteAllBytes(output,pixels.EncodeToPNG());
}
finally
{
    camera.targetTexture=null;RenderTexture.active=previous;RenderSettings.fog=fog;
    foreach(var fader in faders)if(fader!=null)fader.enabled=true;
    UnityEngine.Object.Destroy(root);frame.Release();UnityEngine.Object.Destroy(frame);UnityEngine.Object.Destroy(pixels);
}
return "Captured "+output;
