using System;
using System.Linq;
using DiceFree.Items;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Native, reproducible fitting assets. The original character rigs remain unmodified on disk.
public static class PsionEvolutionGearAuthoring
{
    private const string Data="Assets/_DiceFree/Settings/PsionEvolutions/";
    private const string Gear="Assets/_DiceFree/Settings/Psion/Gear/";
    public static void Author(ItemDefinition[][] items)
    {
        foreach(var item in items.SelectMany(g=>g))
        {
            bool stone=item.slot==EquipmentSlot.OffHand,blade=item==items[0][0];
            item.icon=stone?StoneIcon(item.name):AssetDatabase.LoadAllAssetsAtPath(Gear+(blade?"Mirrorsteel Mindblade":"Mirrorleaf Codex")+" icon.asset").OfType<Sprite>().Single();
            item.modelTestingItem=false;EditorUtility.SetDirty(item);
            if(!stone)continue;
            var root=new GameObject(item.name);
            try
            {
                var silver=Load<Material>(Gear+"Mirrorsteel.mat");var glow=Load<Material>(Gear+"Charged amethyst.mat");
                Piece(root.transform,"Faceted thought",PrimitiveType.Cube,new Vector3(0,.09f,0),new Vector3(.13f,.24f,.13f),Quaternion.Euler(0,35,45),glow);
                for(int i=0;i<4;i++)
                {
                    float angle=i*Mathf.PI*.5f;var p=new Vector3(Mathf.Cos(angle)*.13f,.09f,Mathf.Sin(angle)*.13f);
                    Piece(root.transform,"Silver cage",PrimitiveType.Cube,p,new Vector3(.025f,.34f,.025f),Quaternion.Euler(0,0,i%2==0?12:-12),silver);
                }
                foreach(float y in new[]{-.08f,.26f})Piece(root.transform,"Silver crown",PrimitiveType.Sphere,new Vector3(0,y,0),new Vector3(.21f,.055f,.21f),Quaternion.identity,silver);
                PrefabUtility.SaveAsPrefabAsset(root,Data+item.name+".prefab");
            }
            finally{Object.DestroyImmediate(root);}
        }
    }
    private static void Piece(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Quaternion rotation,Material material)
    {
        var piece=GameObject.CreatePrimitive(type);piece.name=name;Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent,false);piece.transform.localPosition=p;piece.transform.localScale=scale;piece.transform.localRotation=rotation;piece.GetComponent<Renderer>().sharedMaterial=material;
    }
    public static EquipmentPresentation.Binding[] Fit(GameObject form,int n,ItemDefinition[][] items)
    {
        var all=form.GetComponentsInChildren<Transform>(true);
        var rest=all.Select(t=>(joint:t,position:t.localPosition,rotation:t.localRotation,scale:t.localScale)).ToArray();
        var animator=form.GetComponent<Animator>();animator.Rebind();animator.Play("Idle",0,0);animator.Update(0);
        form.GetComponent<PsionEvolutionHandPose>().PoseForFitting();
        GameObject Mount(ItemDefinition item,string joint,string prefab,Vector3 offset,Quaternion worldRotation)
        {
            var hand=all.First(b=>b.name==joint);var socket=new GameObject("Equipment socket — "+item.name).transform;
            socket.SetParent(hand,false);
            var s=hand.lossyScale;socket.localScale=new Vector3(1/Mathf.Abs(s.x),1/Mathf.Abs(s.y),1/Mathf.Abs(s.z));
            socket.position=form.GetComponent<PsionEvolutionHandPose>().PalmPoint(joint.Contains("-L-"))+form.transform.TransformVector(offset);socket.rotation=form.transform.rotation*worldRotation;
            var prop=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(prefab),socket);prop.transform.localScale=Vector3.one*(item.slot==EquipmentSlot.OffHand?.65f:n==1?.8f:1);prop.SetActive(false);return prop;
        }
        var main=items[n][0];var result=new System.Collections.Generic.List<EquipmentPresentation.Binding>();
        var mainVisual=Mount(main,"Bip001-R-Hand",Gear+(n==0?"Mirrorsteel Mindblade":"Mirrorleaf Codex")+".prefab",Vector3.zero,
            n==0?Quaternion.FromToRotation(Vector3.up,new Vector3(.28f,-.95f,.12f)):Quaternion.Euler(-15,-15,-10));
        result.Add(new EquipmentPresentation.Binding{slot=main.slot,definition=main,visuals=new[]{mainVisual}});
        foreach(var item in items.Select(g=>g[1]))
        {
            var prop=Mount(item,"Bip001-L-Hand",Data+item.name+".prefab",new Vector3(0,.10f,.025f),Quaternion.identity);
            result.Add(new EquipmentPresentation.Binding{slot=item.slot,definition=item,visuals=new[]{prop}});
        }
        foreach(var r in rest){r.joint.localPosition=r.position;r.joint.localRotation=r.rotation;r.joint.localScale=r.scale;}
        return result.ToArray();
    }
    public static Texture2D Glyph(string name,int kind)
    {
        string path=Data+name+" icon.asset";var old=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(old!=null)return old;
        const int size=96;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear};
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            var p=new Vector2((x+.5f)*2/size-1,(y+.5f)*2/size-1);float ink=0;
            float Line(Vector2 a,Vector2 b){var v=b-a;return Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude));}
            float Stroke(float d)=>1-Mathf.SmoothStep(.014f,.055f,d);
            if(kind==0)
            {
                var points=new[]{new Vector2(-.5f,.5f),new Vector2(.5f,.5f),new Vector2(.43f,-.1f),new Vector2(0,-.65f),new Vector2(-.43f,-.1f),new Vector2(-.5f,.5f)};
                for(int i=1;i<points.Length;i++)ink=Mathf.Max(ink,Stroke(Line(points[i-1],points[i])));
                ink=Mathf.Max(ink,Stroke(Mathf.Abs(p.magnitude-.22f)));
            }
            else if(kind==1)
            {
                ink=Stroke(Line(new Vector2(-.5f,-.55f),new Vector2(.48f,.58f)));
                ink=Mathf.Max(ink,Stroke(Line(new Vector2(-.5f,-.22f),new Vector2(-.16f,-.5f))));
                for(int i=0;i<3;i++)ink=Mathf.Max(ink,Stroke(Line(new Vector2(-.48f+i*.28f,.35f-i*.05f),new Vector2(-.4f+i*.28f,.68f-i*.05f))));
            }
            else
            {
                for(int i=0;i<3;i++)
                {float y0=(i-1)*.32f;ink=Mathf.Max(ink,Stroke(Line(new Vector2(-.5f,y0+.1f),new Vector2(.2f,y0+.1f))));ink=Mathf.Max(ink,Stroke(Line(new Vector2(.2f,y0+.1f),new Vector2(.5f,y0-.1f))));}
                ink=Mathf.Max(ink,Stroke(Mathf.Abs(p.magnitude-.76f))*.55f);
            }
            var purple=kind==2?new Color(.35f,.85f,1):new Color(.77f,.57f,1);
            var background=Color.Lerp(new Color(.025f,.03f,.06f),purple,Mathf.Exp(-3*p.sqrMagnitude)*.17f);
            texture.SetPixel(x,y,Color.Lerp(background,Color.Lerp(purple,Color.white,.4f),ink));
        }
        texture.Apply();AssetDatabase.CreateAsset(texture,path);return texture;
    }
    private static Sprite StoneIcon(string name)
    {
        string path=Data+name+" icon.asset";var old=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(old!=null)return old;
        const int size=96;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name};
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float u=(x+.5f)*2/size-1,v=(y+.5f)*2/size-1;var p=new Vector2(u,v);
            var color=Color.Lerp(new Color(.025f,.03f,.06f),new Color(.4f,.2f,.65f),Mathf.Exp(-4*p.sqrMagnitude));
            float diamond=Mathf.Abs(u)/.33f+Mathf.Abs(v)/.65f;
            if(diamond<1)color=u<0?new Color(.72f,.49f,1):new Color(.38f,.2f,.7f);
            if(Mathf.Abs(u)<.045f&&Mathf.Abs(v)<.61f)color=new Color(.87f,.76f,1);
            float cage=Mathf.Abs(u)/.49f+Mathf.Abs(v)/.8f;
            if(Mathf.Abs(cage-1)<.07f)color=new Color(.78f,.85f,.95f);
            if(Mathf.Abs(v)>.73f&&Mathf.Abs(v)<.83f&&Mathf.Abs(u)<.2f)color=new Color(.86f,.89f,1);
            texture.SetPixel(x,y,color);
        }
        texture.Apply();AssetDatabase.CreateAsset(texture,path);var sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f));sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
    }
    private static T Load<T>(string path) where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new Exception("Missing "+path);
}
