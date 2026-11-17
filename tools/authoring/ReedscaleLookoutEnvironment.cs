using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class ReedscaleLookoutAuthoring
{
    private static void BuildEnvironment(Transform root)
    {
        Stream(root);
        Bridge(root);
        TreesAndBanks(root);
        var camp=Group(root,"Reedscale lookout camp",Vector3.zero);
        Palisade(camp,new Vector2(-61,392),new Vector2(-47,386));
        Palisade(camp,new Vector2(-37.9f,386),new Vector2(-29.4f,386.8f));
        Palisade(camp,new Vector2(-61,392),new Vector2(-59,408));
        Palisade(camp,new Vector2(-59,408),new Vector2(-49,414));
        Palisade(camp,new Vector2(-49,414),new Vector2(-33,413));
        Palisade(camp,new Vector2(-33,413),new Vector2(-26.5f,403));
        Palisade(camp,new Vector2(-26.5f,403),new Vector2(-27.5f,393));
        Tower(camp,Ground(new Vector2(-31,389)));
        Shelter(camp,Ground(new Vector2(-46,409)));
        Shelter(camp,Ground(new Vector2(-55,396)));
        Brazier(camp,Ground(new Vector2(-39,389)),.86f);
        Brazier(camp,Ground(new Vector2(-50,403)),.64f);
        Banner(camp,Ground(new Vector2(-46.6f,387.1f)),2.9f);
        for(int i=0;i<5;i++)
        {
            var at=Ground(new Vector2(-55.5f+i*.75f,405.5f+Mathf.Sin(i*.8f)));
            Part(camp,"Ration crate",PrimitiveType.Cube,at+Vector3.up*.34f,P(.72f,.67f,.67f),i%2==0?wood:bark);
            Part(camp,"Crate strap",PrimitiveType.Cube,at+P(.22f,.35f,.343f),P(.065f,.69f,.04f),bronze);
        }
    }

    private static void Stream(Transform root)
    {
        var river=Group(root,"Creek, wet boulders and reeds",Vector3.zero);
        for(int band=0;band<2;band++)
        {
            const int steps=65;
            var vertices=new Vector3[steps*2];
            var uv=new Vector2[vertices.Length];
            var tris=new List<int>();
            float width=band==0?2.37f:1.54f, y=band==0?5.28f:5.305f;
            for(int i=0;i<steps;i++)
            {
                float x=Mathf.Lerp(-75,-14,i/(float)(steps-1));
                float z=CreekZ(x);
                float edge=width+.21f*Mathf.Sin(i*.64f);
                vertices[i*2]=P(x,y,z-edge);
                vertices[i*2+1]=P(x,y,z+edge);
                if(i<steps-1)Quad(tris,i*2,i*2+1,i*2+2,i*2+3);
            }
            string name=band==0?"Creek shallows":"Creek channel";
            Surface(river,name,SaveMesh(name,vertices,uv,tris),band==0?shallows:darkWater);
        }
        for(int i=0;i<22;i++)
        {
            float x=Pick(-69,-16);
            if(Mathf.Abs(x+36)<3.6f)continue;
            float z=CreekZ(x)+(i%2==0?1:-1)*Pick(2.2f,3.4f);
            var point=Ground(new Vector2(x,z));
            Rock(river,point,Pick(.38f,1.0f),false);
            for(int k=0;k<5;k++)
            {
                var reed=Part(river,"Creek sedge",PrimitiveType.Sphere,point+P(k*.12f-.25f,.32f+k*.036f,.55f),
                    P(.06f,.75f+k*.08f,.095f),reeds);
                reed.localRotation=Quaternion.Euler(9+k*7,i*39,(k-2)*9);
            }
        }
        foreach(float end in new[]{-75f,-14f})
        {
            for(int i=0;i<5;i++)
            {
                float x=end+(end< -30?1:-1)*(i%2)*1.1f;
                float z=CreekZ(end)+(i-2)*1.02f;
                Rock(river,Ground(new Vector2(x,z)),1.42f+i%3*.45f,false);
            }
        }
    }

    private static float BridgeHeight(float z) => Mathf.Lerp(Height(-36,365.8f)+.045f,Height(-36,374.8f)+.045f,
        Mathf.InverseLerp(365.8f,374.8f,z));

    private static void Bridge(Transform root)
    {
        const float south=365.8f,north=374.8f, x=-36;
        var bridge=Group(root,"Larch footbridge",Vector3.zero);
        var a=P(x-1.75f,BridgeHeight(south),south);
        var b=P(x-1.75f,BridgeHeight(north),north);
        var c=P(x+1.75f,BridgeHeight(south),south);
        var d=P(x+1.75f,BridgeHeight(north),north);
        Surface(bridge,"Continuous footbridge deck",SaveMesh("Footbridge deck",new[]{a,b,c,d},new Vector2[4],
            new List<int>{0,1,2,2,1,3},true),bark,8);
        for(int i=0;i<22;i++)
        {
            float z=Mathf.Lerp(south,north,(i+.5f)/22);
            Part(bridge,"Hewn deck board",PrimitiveType.Cube,P(x,BridgeHeight(z)+.01f,z),P(3.42f,.12f,.39f),i%4==0?bark:wood);
        }
        foreach(float side in new[]{-1f,1f})
        {
            float rail=x+side*1.66f;
            for(int i=0;i<4;i++)
            {
                float z=Mathf.Lerp(south+.18f,north-.18f,i/3f);
                Beam(bridge,"Bridge stanchion",P(rail,BridgeHeight(z)-.23f,z),P(rail,BridgeHeight(z)+1.08f,z),.13f,bark);
                Part(bridge,"Rope lashing",PrimitiveType.Cylinder,P(rail,BridgeHeight(z)+.76f,z),P(.31f,.09f,.31f),ivory);
            }
            Beam(bridge,"Bridge rail",P(rail,BridgeHeight(south)+.82f,south),P(rail,BridgeHeight(north)+.82f,north),.075f,wood);
            Barrier(bridge,P(rail,BridgeHeight(south),south),P(rail,BridgeHeight(north),north),1.14f,.13f);
        }
    }

    private static void TreesAndBanks(Transform root)
    {
        var greenery=Group(root,"Forest approach and rocky ridge",Vector3.zero);
        var trees=new List<Vector2>();
        for(int i=0;i<86;i++)
        {
            var at=new Vector2(Pick(-73,-16),Pick(355,423));
            if(at.y>380&&at.x>-59&&at.x<-25||Mathf.Abs(at.y-CreekZ(at.x))<5||
                Trail.Zip(Trail.Skip(1),(a,b)=>DistanceTo(at,a,b)).Min()<3.7f||
                trees.Any(other=>Vector2.Distance(other,at)<4.2f))continue;
            trees.Add(at);
            var type=i%3==0?"Willow":i%3==1?"Cypress":"Twisted";
            float size=Pick(4.7f,8.8f);
            var tree=Existing(greenery,WorldArt+"CornbergBatch/Trees/Tree_"+type+".fbx",type,Ground(at),size,Pick(0,360));
            Solid(tree,P(0,size*.22f,0),P(.84f,size*.44f,.84f));
        }
        for(int i=0;i<38;i++)
        {
            var at=new Vector2(Pick(-70,-16),Pick(357,421));
            if(at.y>380&&at.x>-56&&at.x<-27||Mathf.Abs(at.y-CreekZ(at.x))<3.2f||
                Trail.Zip(Trail.Skip(1),(a,b)=>DistanceTo(at,a,b)).Min()<2.8f)continue;
            Rock(greenery,Ground(at),Pick(.52f,1.85f),false);
        }
        for(int i=0;i<13;i++)
        {
            var at=new Vector2(-72+i*4.32f,422+Mathf.Sin(i*.8f)*1.2f);
            Rock(greenery,Ground(at),3.25f+Mathf.Cos(i*.9f)*.85f,true);
        }
        for(int i=0;i<9;i++)
        {
            float z=381+i*4.4f;
            Rock(greenery,Ground(new Vector2(-14.3f+Mathf.Sin(i)*.85f,z)),2.5f+Mathf.Sin(i*.8f)*.75f,false);
        }
    }

    private static float DistanceTo(Vector2 point,Vector2 a,Vector2 b)
    {
        var delta=b-a;
        return Vector2.Distance(point,a+delta*Mathf.Clamp01(Vector2.Dot(point-a,delta)/delta.sqrMagnitude));
    }

    private static void Rock(Transform parent,Vector3 at,float size,bool solid)
    {
        var rock=Existing(parent,WorldArt+"VisualBatch01/Models/rock_0"+(int)Pick(1,4.99f)+".fbx",
            "Layered slate",at,size,Pick(0,360),size<1?darkStone:stone);
        if(solid)Solid(rock,P(0,size*.40f,0),P(size*1.06f,size*.80f,size*.92f));
    }

    private static Transform Existing(Transform parent,string path,string name,Vector3 point,float height,float turn,Material recolor=null)
    {
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(model==null)throw new InvalidOperationException("Missing scenery "+path);
        var node=Group(parent,name,point);
        var copy=(GameObject)PrefabUtility.InstantiatePrefab(model,node);
        foreach(var collider in copy.GetComponentsInChildren<Collider>())collider.enabled=false;
        var renderers=copy.GetComponentsInChildren<MeshRenderer>();
        var bounds=renderers[0].bounds;
        foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
        float scaleFactor=height/Mathf.Max(.01f,bounds.size.y);
        copy.transform.localScale*=scaleFactor;
        copy.transform.localPosition=(point-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z))*scaleFactor;
        node.localRotation=Quaternion.Euler(0,turn,0);
        if(recolor!=null)foreach(var renderer in renderers)
            renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>recolor).ToArray();
        return node;
    }

    private static void Palisade(Transform camp,Vector2 from,Vector2 to)
    {
        int count=Mathf.CeilToInt(Vector2.Distance(from,to)/.69f);
        for(int i=0;i<=count;i++)
        {
            var point=Ground(Vector2.Lerp(from,to,i/(float)count));
            float h=2.0f+.28f*Mathf.Sin(i*1.71f);
            Beam(camp,"Palisade stave",point-Vector3.up*.18f,point+P(.11f,h,0),.30f,i%4==0?wood:bark);
            Part(camp,"Rounded stave cap",PrimitiveType.Capsule,point+P(.11f,h,0),P(.58f,.34f,.58f),i%4==0?wood:bark)
                .localRotation=Quaternion.Euler(8,0,0);
        }
        var a=Ground(from);var b=Ground(to);
        Beam(camp,"Palisade rail",a+Vector3.up*1.16f,b+Vector3.up*1.16f,.13f,wood);
        Barrier(camp,a,b,2.65f,.49f);
    }

    private static void Tower(Transform camp,Vector3 at)
    {
        var tower=Group(camp,"Reedscale watchtower",at);
        foreach(float side in new[]{-1f,1f})
        foreach(float front in new[]{-1f,1f})
        {
            var foot=P(side*1.95f,0,front*1.80f);
            var deck=P(side*1.56f,6.5f,front*1.55f);
            Part(tower,"Slate tower footing",PrimitiveType.Cube,foot+Vector3.up*.23f,P(.92f,.66f,.91f),stone);
            Beam(tower,"Leaning larch tower post",foot+Vector3.up*.25f,deck,.26f,bark);
            Beam(tower,"Lookout canopy post",deck,deck+Vector3.up*2.3f,.115f,wood);
            Beam(tower,"Tower diagonal brace",foot+Vector3.up*.84f,P(-side*1.64f,5.60f,front*1.65f),.115f,wood);
            Part(tower,"Bronze lashing band",PrimitiveType.Cylinder,deck-Vector3.up*.43f,P(.58f,.12f,.58f),bronze);
            Solid(tower,foot+Vector3.up*1.3f,P(.73f,2.6f,.73f));
        }
        Part(tower,"Raised watch platform",PrimitiveType.Cube,P(0,6.50f,0),P(4.65f,.42f,4.3f),wood);
        for(int i=0;i<12;i++)
        {
            float x=-2.2f+i*.4f;
            Beam(tower,"Platform parapet",P(x,6.66f,-2.04f),P(x,7.42f+(i%2)*.24f,-2.04f),.12f,bark);
        }
        for(int side=-1;side<=1;side+=2)
        {
            Part(tower,"Angled ochre canopy",PrimitiveType.Cube,P(side*1.18f,9.11f,0),P(2.74f,.12f,5.25f),canvas)
                .localRotation=Quaternion.Euler(0,0,side*-24f);
            for(int i=0;i<9;i++)
            {
                float z=-2.5f+i*.58f;
                Beam(tower,"Canopy ribs",P(0,9.67f,z),P(side*2.48f,8.61f,z),.048f,bark);
            }
        }
        Beam(tower,"Roof ridge",P(0,9.71f,-2.68f),P(0,9.71f,2.68f),.10f,wood);
        Banner(tower,P(0,5.04f,-2.28f),2.58f);
        Brazier(tower,P(-1.55f,6.76f,-1.46f),.53f);
    }

    private static void Shelter(Transform camp,Vector3 point)
    {
        var stall=Group(camp,"Sentry hide shelter",point);
        for(int side=-1;side<=1;side+=2)
        {
            Beam(stall,"Shelter post",P(side*2,0,-1.28f),P(side*2,2.95f,-1.28f),.12f,wood);
            Beam(stall,"Shelter post",P(side*2,0,1.58f),P(side*2,2.22f,1.58f),.12f,wood);
        }
        Part(stall,"Tilted hide canopy",PrimitiveType.Cube,P(0,2.64f,.14f),P(4.64f,.14f,3.5f),canvas)
            .localRotation=Quaternion.Euler(14.3f,0,0);
        Part(stall,"Leather screen",PrimitiveType.Cube,P(0,1.17f,1.55f),P(4.13f,2.2f,.08f),leather);
        Solid(stall,P(0,1.1f,1.58f),P(3.88f,2.2f,.15f));
    }

    private static void Banner(Transform parent,Vector3 at,float height)
    {
        var root=Group(parent,"Reedscale split banner",at);
        Beam(root,"Banner spar",Vector3.zero,Vector3.up*height,.08f,bark);
        Beam(root,"Banner crossbar",P(-.64f,height-.18f,0),P(.66f,height-.18f,0),.055f,bronze);
        Part(root,"Indigo banner cloth",PrimitiveType.Cube,P(0,height-.83f,-.07f),P(.99f,1.20f,.045f),teal);
        for(int i=0;i<3;i++)
            Part(root,"Reedscale stitch",PrimitiveType.Cube,P((i-1)*.20f,height-.77f,-.10f),P(.075f,.39f,.018f),ivory)
                .localRotation=Quaternion.Euler(0,0,-25);
        for(int side=-1;side<=1;side+=2)
            Part(root,"Banner swallowtail",PrimitiveType.Cube,P(side*.31f,height-1.48f,-.07f),P(.32f,.19f,.046f),teal);
    }

    private static void Brazier(Transform parent,Vector3 at,float size)
    {
        var root=Group(parent,"Bronze fire basket",at);
        Part(root,"Basalt base",PrimitiveType.Cylinder,P(0,.1f,0),P(size,.12f,size),darkStone);
        Part(root,"Bronze cup",PrimitiveType.Cylinder,P(0,.47f,0),P(size*.84f,.17f,size*.84f),bronze);
        Part(root,"Glowing coals",PrimitiveType.Sphere,P(0,.60f,0),P(size*.76f,.16f,size*.76f),embers);
        for(int i=0;i<6;i++)
        {
            float a=i*Mathf.PI/3;
            var radial=P(Mathf.Cos(a),0,Mathf.Sin(a))*size*.40f;
            Beam(root,"Basket ribs",radial*.6f+Vector3.up*.28f,radial+Vector3.up*.69f,.035f,darkStone);
        }
        var point=root.gameObject.AddComponent<Light>();
        point.type=LightType.Point;point.range=4.6f;point.intensity=2.4f;
        point.color=ColorOf("FFAD56");point.shadows=LightShadows.None;point.cullingMask=~(1<<31);
    }
}
