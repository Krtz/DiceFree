using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Items;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>Original sculpted wolf loot. Only EquipmentPresentation decides what is equipped.</summary>
    [DisallowMultipleComponent]
    public sealed class WolfRewardEquipmentVisuals : MonoBehaviour
    {
        public const string Prefix = "item.wolf-survival.";
        readonly HashSet<Transform> prepared = new();
        readonly List<Mesh> meshes = new();
        Material[] palette;
        Texture2D furTexture,leatherTexture;
        const int Silver=0, Dark=1, Ivory=2, Leather=3, Fur=4, FurLight=5, Teal=6, Blue=7, Violet=8, Black=9;

        public void PrepareVisual(Transform root, EquipmentPresentation presentation, CarriedInventory inventory)
        {
            if(root==null||presentation==null||inventory==null||prepared.Contains(root))return;
            var animator=ClassPresentationSwitcher.FindVisualAnimator(root);
            if(animator==null||!animator.isHuman)return;
            var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var chest=animator.GetBoneTransform(HumanBodyBones.Chest)??animator.GetBoneTransform(HumanBodyBones.Spine);
            if(hand==null||chest==null)return;
            prepared.Add(root);
            EnsurePalette();
            var head=animator.GetBoneTransform(HumanBodyBones.Head);
            var foot=animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            float scale=head!=null&&foot!=null?Mathf.Clamp(Vector3.Distance(head.position,foot.position)/1.55f,.65f,1.5f):1;
            var bindings=presentation.Bindings.ToList();
            foreach(var item in inventory.Definitions.Where(d=>d!=null&&d.stableId!=null&&d.stableId.StartsWith(Prefix,StringComparison.Ordinal)))
            {
                var binding=bindings.FirstOrDefault(b=>b.definition==item);
                if(binding!=null&&binding.visuals.Any(v=>v!=null&&v.transform.IsChildOf(root)))continue;
                var mesh=CreateMesh(item.stableId.Substring(Prefix.Length));
                if(mesh==null)continue;
                meshes.Add(mesh);
                var group=new GameObject("Wolf reward - "+item.displayName);
                group.SetActive(false);
                group.layer=2;
                group.transform.SetParent(root,false);
                group.AddComponent<MeshFilter>().sharedMesh=mesh;
                group.AddComponent<MeshRenderer>().sharedMaterials=palette;
                Transform bone=item.slot==EquipmentSlot.Chest?chest:item.slot==EquipmentSlot.Ring?
                    animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal)??hand:hand;
                group.transform.SetPositionAndRotation(bone.position,root.rotation);
                // World units, independent of imported rig scale.
                group.transform.localScale=Vector3.Scale(Vector3.one*scale,new Vector3(1/root.lossyScale.x,1/root.lossyScale.y,1/root.lossyScale.z));
                if(item.slot==EquipmentSlot.MainHand)
                {
                    var pose=group.AddComponent<WolfRewardWeaponPose>();
                    pose.Configure(root,animator,item.handsRequired==2);
                }
                else
                {
                    if(item.slot==EquipmentSlot.Chest)group.transform.position+=root.forward*.015f*scale;
                    group.AddComponent<PlainGearAttachment>().Configure(bone);
                }
                if(binding==null){binding=new EquipmentPresentation.Binding{definition=item,slot=item.slot};bindings.Add(binding);}
                binding.visuals=binding.visuals.Concat(new[]{group}).ToArray();
                // Every main-hand item suppresses the class's default prop, without consuming an offhand.
                var defaults=root.GetComponentsInChildren<Transform>(true).Where(t=>
                    item.slot==EquipmentSlot.MainHand?(t.name=="Caster Staff"||t.name=="Mage Wand"||
                        t.name.IndexOf("greatsword",StringComparison.OrdinalIgnoreCase)>=0):
                    item.slot==EquipmentSlot.Chest&&t.name=="Mage layered coat");
                binding.baselineVisuals=binding.baselineVisuals.Concat(defaults.Select(t=>t.gameObject))
                    .Concat(bindings.Where(b=>b!=binding&&b.slot==item.slot).SelectMany(b=>b.baselineVisuals)).Distinct().ToArray();
            }
            presentation.Configure(bindings.ToArray());
            GetComponent<HudPortraitRenderer>()?.RequestRefresh();
        }

        void EnsurePalette()
        {
            if(palette!=null)return;
            var colors=new[]{new Color(.64f,.72f,.79f),new Color(.19f,.25f,.31f),new Color(.88f,.83f,.65f),
                new Color(.16f,.10f,.075f),new Color(.19f,.23f,.28f),new Color(.47f,.53f,.58f),
                new Color(.12f,.88f,.57f),new Color(.16f,.57f,1f),new Color(.67f,.23f,.96f),new Color(.025f,.035f,.055f)};
            var names=new[]{"Weathered moon silver","Engraved dark silver","Old ivory","Oiled leather","Nightpelt charcoal","Silver-tipped fur","Healer moonstone","Mage moonstone","Acolyte moonstone","Recessed carving"};
            palette=colors.Select((color,i)=>{
                var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Wolf loot - "+names[i],color=color};
                m.SetFloat("_Smoothness",i<=Dark?.53f:i>=Teal&&i<=Violet?.72f:.22f);
                m.SetFloat("_Metallic",i<=Dark?.68f:0);
                if(i>=Teal&&i<=Violet){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*.65f);}
                return m;
            }).ToArray();
            furTexture=SurfaceTexture(true);leatherTexture=SurfaceTexture(false);
            palette[Fur].mainTexture=furTexture;palette[FurLight].mainTexture=furTexture;
            palette[Leather].mainTexture=leatherTexture;
        }

        static Texture2D SurfaceTexture(bool hair)
        {
            const int size=128;
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name=hair?"Original wolf fur fibers":"Original worn leather grain",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float grain=Mathf.PerlinNoise(x*.48f,y*(hair?.035f:.48f));
                float strands=hair?Mathf.Sin(x*2.3f+Mathf.Sin(y*.095f)*1.6f)*.12f:0;
                float value=Mathf.Clamp01(.58f+grain*.5f+strands);
                pixels[y*size+x]=new Color(value,value,value);
            }
            texture.SetPixels(pixels);texture.Apply(true,true);return texture;
        }

        public static Mesh CreateMesh(string id)
        {
            var m=new Sculpt();
            switch(id)
            {
                case "moonfang-signet":
                    m.Arc(Vector3.zero,.027f,.007f,0,360,Silver,Quaternion.Euler(90,0,0));
                    WolfHead(m,new Vector3(0,.027f,0),.075f,false,Blue);
                    break;
                case "nightpelt-vest":Vest(m);break;
                case "moonwell-staff":Staff(m);break;
                case "starfall-wand":Wand(m,false);break;
                case "duskwake-wand":Wand(m,true);break;
                case "packhunters-fang":Dagger(m);break;
                default:return null;
            }
            return m.Finish("Original wolf loot - "+id);
        }

        static void WolfHead(Sculpt m,Vector3 center,float size,bool skull,int gem)
        {
            int surface=skull?Ivory:Silver;
            Vector3 P(float x,float y,float z)=>center+new Vector3(x,y,z)*size;
            m.Ellipsoid(P(0,0,0),new Vector3(.43f,.49f,.31f)*size,surface);
            m.Ellipsoid(P(0,-.18f,.25f),new Vector3(.24f,.22f,.43f)*size,surface);
            m.Ellipsoid(P(0,-.40f,.26f),new Vector3(.19f,.06f,.32f)*size,Dark);
            m.Ellipsoid(P(0,-.29f,.56f),new Vector3(.16f,.10f,.13f)*size,skull?Dark:Black);
            for(int side=-1;side<=1;side+=2)
            {
                m.Blade(new[]{P(side*.32f,.17f,-.02f),P(side*.39f,skull?.46f:.80f,-.03f),P(side*.08f,.37f,.08f)},.11f*size,surface);
                if(!skull)m.Blade(new[]{P(side*.29f,.30f,.08f),P(side*.35f,.62f,.075f),P(side*.17f,.37f,.16f)},.018f*size,Leather);
                m.Ellipsoid(P(side*.27f,.035f,.25f),new Vector3(.13f,.085f,.075f)*size,Black);
                if(!skull)m.Ellipsoid(P(side*.27f,.04f,.30f),new Vector3(.072f,.044f,.035f)*size,gem);
                // Broad brow and angular cheek give a canine silhouette at gameplay distance.
                m.Tube(new[]{P(side*.09f,.18f,.29f),P(side*.27f,.17f,.31f),P(side*.40f,.08f,.16f)},new[]{.07f,.09f,.04f}.Select(r=>r*size).ToArray(),surface);
                m.Blade(new[]{P(side*.25f,-.08f,.06f),P(side*.51f,-.28f,.03f),P(side*.25f,-.36f,.28f)},.10f*size,surface);
                m.Tube(new[]{P(side*.17f,-.24f,.45f),P(side*.19f,-.43f,.48f),P(side*.15f,-.54f,.57f)},new[]{.055f,.037f,0}.Select(r=>r*size).ToArray(),Ivory);
                if(!skull)for(int j=0;j<3;j++)m.Blade(new[]{P(side*(.31f+j*.035f),-.10f-j*.09f,-.08f),P(side*(.51f+j*.015f),-.24f-j*.11f,-.02f),P(side*.25f,-.32f-j*.08f,.02f)},.08f*size,Dark);
            }
            if(!skull)m.Gem(P(0,.22f,.31f),new Vector3(.075f,.11f,.05f)*size,gem);
        }

        static void Grip(Sculpt m,float bottom,float top,float width)
        {
            m.Tube(new[]{new Vector3(0,bottom,0),new Vector3(0,top,0)},new[]{width,width*.9f},Leather);
            for(float y=bottom+.025f;y<top;y+=.055f)m.Arc(new Vector3(0,y,0),width+.003f,.008f,0,360,Dark,Quaternion.Euler(90,0,0));
            m.Ellipsoid(new Vector3(0,bottom-.025f,0),new Vector3(width*1.4f,.045f,width*1.4f),Silver);
        }

        static void Staff(Sculpt m)
        {
            m.Tube(new[]{new Vector3(0,-.68f,0),new Vector3(.025f,.40f,0),new Vector3(0,.94f,0)},new[]{.027f,.036f,.045f},Leather);
            Grip(m,-.27f,.18f,.040f);
            m.Tube(new[]{new Vector3(0,-.70f,0),new Vector3(0,-.58f,0)},new[]{.015f,.037f},Silver);
            m.Arc(new Vector3(0,1.10f,0),.275f,.042f,35,325,Silver,Quaternion.identity);
            m.Arc(new Vector3(0,1.10f,-.012f),.223f,.012f,38,322,Dark,Quaternion.identity);
            WolfHead(m,new Vector3(0,.99f,.015f),.26f,false,Teal);
            m.Gem(new Vector3(0,1.30f,0),new Vector3(.085f,.145f,.07f),Teal);
            FurWrap(m,new Vector3(0,.76f,0),.085f,.15f);
            for(int side=-1;side<=1;side+=2)
            {
                m.Tube(new[]{new Vector3(side*.11f,.87f,0),new Vector3(side*.15f,.61f,0)},new[]{.008f,.008f},Leather);
                m.Tube(new[]{new Vector3(side*.15f,.63f,0),new Vector3(side*.17f,.51f,.02f),new Vector3(side*.13f,.46f,.05f)},new[]{.026f,.02f,0},Ivory);
            }
        }

        static void Wand(Sculpt m,bool skull)
        {
            int gem=skull?Violet:Blue;
            Grip(m,-.17f,.22f,.033f);
            FurWrap(m,new Vector3(0,.20f,0),.070f,.10f);
            m.Tube(new[]{new Vector3(0,.20f,0),new Vector3(0,.46f,0)},new[]{.04f,.07f},skull?Ivory:Silver);
            WolfHead(m,new Vector3(0,.43f,0),.33f,skull,gem);
            m.Gem(new Vector3(0,.75f,.015f),new Vector3(.115f,.19f,.10f),gem);
            for(int i=0;i<3;i++)
            {
                float a=i*Mathf.PI*2/3;
                var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                m.Tube(new[]{direction*.06f+Vector3.up*.48f,direction*.15f+Vector3.up*.65f,direction*.09f+Vector3.up*.86f},new[]{.035f,.027f,.004f},skull?Ivory:Silver);
            }
            for(int side=-1;side<=1;side+=2)
            {
                m.Tube(new[]{new Vector3(side*.085f,.25f,0),new Vector3(side*.12f,.07f,0)},new[]{.007f,.007f},Leather);
                m.Tube(new[]{new Vector3(side*.12f,.07f,0),new Vector3(side*.13f,-.025f,.01f),new Vector3(side*.10f,-.08f,.03f)},new[]{.022f,.014f,0},Ivory);
            }
        }

        static void Dagger(Sculpt m)
        {
            Grip(m,-.18f,.10f,.037f);
            // Swept, convex canine blade with a raised silver ridge and dark fuller.
            m.Blade(new[]{new Vector3(-.075f,.23f,0),new Vector3(-.09f,.50f,0),new Vector3(-.015f,.75f,0),new Vector3(.18f,.96f,0),new Vector3(.09f,.67f,0),new Vector3(.10f,.40f,0),new Vector3(.08f,.23f,0)},.065f,Silver);
            m.Tube(new[]{new Vector3(.003f,.31f,.036f),new Vector3(-.005f,.51f,.044f),new Vector3(.025f,.69f,.032f),new Vector3(.14f,.90f,.012f)},new[]{.012f,.012f,.009f,0},Dark);
            WolfHead(m,new Vector3(0,.19f,0),.29f,false,Blue);
            for(int side=-1;side<=1;side+=2)m.Tube(new[]{new Vector3(side*.10f,.20f,0),new Vector3(side*.23f,.12f,0),new Vector3(side*.22f,-.035f,.02f)},new[]{.048f,.035f,0},Ivory);
            FurWrap(m,new Vector3(0,-.13f,0),.068f,.065f);
        }

        static void Vest(Sculpt m)
        {
            // Open armholes: the fitted cuirass stays on the torso while both arms animate freely.
            m.Shell(new[]{.17f,.035f,-.19f,-.34f},new[]{.24f,.255f,.22f,.225f},new[]{.165f,.18f,.15f,.16f},Leather);
            m.Shell(new[]{-.24f,-.31f},new[]{.235f,.235f},new[]{.163f,.163f},Dark);
            // Layered individual tapered tufts make a shaggy silhouette, rather than a smooth tube.
            for(int ring=0;ring<3;ring++)for(int i=0;i<36;i++)
            {
                float a=(i+ring*.45f)*Mathf.PI*2/36;
                float irregular=Mathf.Sin(i*2.399f+ring*1.173f);
                a+=irregular*.045f;
                float y=.26f-ring*.035f+irregular*.017f;
                float x=Mathf.Cos(a)*(.23f+ring*.022f+irregular*.012f),z=Mathf.Sin(a)*(.16f+ring*.017f+irregular*.01f);
                float length=.075f+.035f*(1-Mathf.Sin(a))+.02f*Mathf.Sin(i*2.399f+ring);
                var start=new Vector3(x,y,z);
                var spread=new Vector3(Mathf.Cos(a)*.052f,-length,Mathf.Sin(a)*.035f);
                m.Tube(new[]{start,start+spread*.5f,start+spread},new[]{.034f,.020f,0},(i+ring)%5==0?FurLight:Fur);
            }
            for(int i=0;i<20;i++)
            {
                float a=i*Mathf.PI*2/20;
                var p=new Vector3(Mathf.Cos(a)*.224f,-.30f,Mathf.Sin(a)*.16f);
                m.Tube(new[]{p,p+new Vector3(Mathf.Cos(a)*.015f,-.055f,Mathf.Sin(a)*.015f)},new[]{.025f,0},i%5==0?FurLight:Fur);
            }
            WolfHead(m,new Vector3(0,.04f,.19f),.17f,false,Blue);
            m.Gem(new Vector3(0,-.27f,.185f),new Vector3(.035f,.035f,.018f),Blue);
            for(int side=-1;side<=1;side+=2)
            {
                m.Tube(new[]{new Vector3(side*.135f,.13f,.17f),new Vector3(side*.14f,.015f,.21f),new Vector3(side*.10f,-.02f,.22f)},new[]{.026f,.022f,0},Ivory);
                // Crossed lacing below the chest medallion.
                for(int j=0;j<3;j++)m.Tube(new[]{new Vector3(side*.075f,-.07f-j*.047f,.178f),new Vector3(-side*.06f,-.11f-j*.047f,.177f)},new[]{.006f,.006f},Ivory);
                m.Tube(new[]{new Vector3(side*.17f,.10f,.13f),new Vector3(side*.17f,-.12f,.13f),new Vector3(side*.15f,-.24f,.12f)},new[]{.007f,.007f,.007f},Dark);
                for(int j=0;j<4;j++)m.Ellipsoid(new Vector3(side*.17f,.075f-j*.08f,.145f),new Vector3(.010f,.010f,.006f),Silver);
            }
        }

        static void FurWrap(Sculpt m,Vector3 at,float radius,float length)
        {
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI/8;
                var start=at+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
                m.Tube(new[]{start,start+new Vector3(Mathf.Cos(a)*.02f,-length,Mathf.Sin(a)*.02f)},new[]{.032f,0},i%3==0?FurLight:Fur);
            }
        }

        void OnDestroy()
        {
            foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
            if(palette!=null)foreach(var material in palette)if(material!=null)Destroy(material);
            if(furTexture!=null)Destroy(furTexture);if(leatherTexture!=null)Destroy(leatherTexture);
        }

        /// <summary>Combines sculpted solids into one mesh per item; no prop colliders or per-tuft objects.</summary>
        sealed class Sculpt
        {
            readonly List<Vector3> vertices=new();
            readonly List<int>[] triangles=Enumerable.Range(0,10).Select(_=>new List<int>()).ToArray();
            int V(Vector3 p){vertices.Add(p);return vertices.Count-1;}
            void Tri(int a,int b,int c,int material){triangles[material].Add(a);triangles[material].Add(b);triangles[material].Add(c);}
            void Quad(int a,int b,int c,int d,int material){Tri(a,b,c,material);Tri(a,c,d,material);}
            public void Ellipsoid(Vector3 at,Vector3 radii,int material)
            {
                int start=vertices.Count;const int sides=12,rings=8;
                for(int r=0;r<=rings;r++)for(int i=0;i<sides;i++)
                {
                    float theta=r*Mathf.PI/rings,a=i*Mathf.PI*2/sides;
                    V(at+Vector3.Scale(new Vector3(Mathf.Sin(theta)*Mathf.Cos(a),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(a)),radii));
                }
                for(int r=0;r<rings;r++)for(int i=0;i<sides;i++)Quad(start+r*sides+i,start+r*sides+(i+1)%sides,start+(r+1)*sides+(i+1)%sides,start+(r+1)*sides+i,material);
            }
            public void Tube(Vector3[] points,float[] radii,int material)
            {
                int start=vertices.Count;const int sides=8;
                for(int r=0;r<points.Length;r++)
                {
                    var direction=(points[Math.Min(r+1,points.Length-1)]-points[Math.Max(r-1,0)]).normalized;
                    var tangent=Vector3.Cross(direction,Mathf.Abs(direction.z)<.9f?Vector3.forward:Vector3.up).normalized;
                    var bitangent=Vector3.Cross(direction,tangent);
                    for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;V(points[r]+(tangent*Mathf.Cos(a)+bitangent*Mathf.Sin(a))*radii[r]);}
                }
                for(int r=0;r<points.Length-1;r++)for(int i=0;i<sides;i++)Quad(start+r*sides+i,start+r*sides+(i+1)%sides,start+(r+1)*sides+(i+1)%sides,start+(r+1)*sides+i,material);
                int first=V(points[0]),last=V(points[points.Length-1]);
                for(int i=0;i<sides;i++){Tri(first,start+(i+1)%sides,start+i,material);Tri(last,start+(points.Length-1)*sides+i,start+(points.Length-1)*sides+(i+1)%sides,material);}
            }
            public void Arc(Vector3 at,float radius,float thickness,float from,float to,int material,Quaternion rotation)
            {
                int count=Mathf.CeilToInt((to-from)/12)+1;var points=new Vector3[count];var radii=new float[count];
                for(int i=0;i<count;i++){float a=Mathf.Lerp(from,to,(float)i/(count-1))*Mathf.Deg2Rad;points[i]=at+rotation*new Vector3(Mathf.Sin(a)*radius,Mathf.Cos(a)*radius,0);radii[i]=thickness;}
                Tube(points,radii,material);
            }
            public void Blade(Vector3[] outline,float depth,int material)
            {
                Vector3 center=outline.Aggregate(Vector3.zero,(a,b)=>a+b)/outline.Length;
                for(int side=-1;side<=1;side+=2)
                {
                    int middle=V(center+Vector3.forward*depth*side);
                    for(int i=0;i<outline.Length;i++)
                    {
                        int a=V(outline[i]),b=V(outline[(i+1)%outline.Length]);
                        if(side==1)Tri(middle,b,a,material);else Tri(middle,a,b,material);
                    }
                }
            }
            public void Gem(Vector3 at,Vector3 size,int material)
            {
                int top=V(at+Vector3.up*size.y),bottom=V(at-Vector3.up*size.y);
                int start=vertices.Count;const int sides=6;
                for(int i=0;i<sides;i++){float a=i*Mathf.PI/3;V(at+new Vector3(Mathf.Cos(a)*size.x,0,Mathf.Sin(a)*size.z));}
                for(int i=0;i<sides;i++){Tri(top,start+(i+1)%sides,start+i,material);Tri(bottom,start+i,start+(i+1)%sides,material);}
            }
            public void Shell(float[] levels,float[] widths,float[] depths,int material)
            {
                int start=vertices.Count;const int sides=24;
                for(int r=0;r<levels.Length;r++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;V(new Vector3(Mathf.Cos(a)*widths[r],levels[r],Mathf.Sin(a)*depths[r]));}
                for(int r=0;r<levels.Length-1;r++)for(int i=0;i<sides;i++)Quad(start+r*sides+i,start+r*sides+(i+1)%sides,start+(r+1)*sides+(i+1)%sides,start+(r+1)*sides+i,material);
            }
            public Mesh Finish(string name)
            {
                var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
                mesh.SetVertices(vertices);mesh.subMeshCount=triangles.Length;
                mesh.SetUVs(0,vertices.Select(p=>new Vector2(p.x*2.6f,p.y*2.6f)).ToList());
                for(int i=0;i<triangles.Length;i++)mesh.SetTriangles(triangles[i],i);
                mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            }
        }
    }
}
