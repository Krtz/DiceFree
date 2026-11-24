using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Items;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class AshfallReachExpansionAuthoring
{
    private static ItemShop.Offer[] riverTraderStock;

    private static void LoadRiverTraderStock()
    {
        var source = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        riverTraderStock = source.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ItemShop>(true))
            .Where(shop => shop.Offers.Length > 0).OrderBy(shop => shop.name.Contains("Merchant") ? 0 : 1)
            .First().Offers.Take(6).Select(offer => new ItemShop.Offer
            {
                item = offer.item, goldPrice = offer.goldPrice, provisionalPrice = offer.provisionalPrice
            }).ToArray();
    }

    private static void FortifyCinderwatch(Transform parent)
    {
        var fort = Group(parent, "Cinderwatch palisade", Vector3.zero);
        var post = PalisadeMesh();
        Wall(fort, post, new(-12,-31), new(2,-31));
        Wall(fort, post, new(9,-31), new(20,-31));
        Wall(fort, post, new(20,-31), new(20,-62));
        Wall(fort, post, new(20,-62), new(8,-62));
        Wall(fort, post, new(2,-62), new(-12,-62));
        Wall(fort, post, new(-12,-62), new(-12,-44));
        Wall(fort, post, new(-12,-37), new(-12,-31));
        FortGate(fort, new(5.5f,-31), 0, 7);
        FortGate(fort, new(5,-62), 0, 6);
        FortGate(fort, new(-12,-40.5f), 90, 7);
        Lantern(fort, Ground(10,-33));
        Lantern(fort, Ground(9,-60));
        Lantern(fort, Ground(-10,-36));
        Road(parent, "Cinderwatch to Willowford", AshfallReach.WillowfordRoad, 2.45f);
    }

    private static Mesh PalisadeMesh()
    {
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var triangles = new List<int>();
        const int sides = 9;
        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2 / sides;
            vertices.Add(new Vector3(Mathf.Sin(angle)*.24f, -.18f, Mathf.Cos(angle)*.24f));
            vertices.Add(new Vector3(Mathf.Sin(angle)*.24f, 2.85f, Mathf.Cos(angle)*.24f));
            uv.Add(new Vector2(i/(float)sides,0));
            uv.Add(new Vector2(i/(float)sides,2));
        }
        vertices.Add(new Vector3(.055f,3.45f,.02f));
        uv.Add(new Vector2(.5f,2.5f));
        for (int i=0;i<sides;i++)
        {
            int a=i*2,b=(i+1)%sides*2;
            triangles.AddRange(new[]{a,b,a+1,b,b+1,a+1,a+1,b+1,sides*2});
        }
        var mesh=new Mesh{name="Sharpened cedar palisade post"};
        mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
        return SaveMesh(mesh);
    }

    private static void Wall(Transform parent, Mesh post, Vector2 from, Vector2 to)
    {
        var delta=to-from;
        int count=Mathf.CeilToInt(delta.magnitude/.53f);
        float heading=Mathf.Atan2(delta.x,delta.y)*Mathf.Rad2Deg;
        for(int i=0;i<=count;i++)
        {
            var p=Vector2.Lerp(from,to,i/(float)count);
            var trunk=Surface(parent,"Sharpened cedar stake",post,i%5==0?oak:cedar);
            trunk.localPosition=Ground(p);
            trunk.localRotation=Quaternion.Euler(0,i*43,0);
            trunk.localScale=new Vector3(1,1+Mathf.Sin(i*2.9f)*.055f,1);
        }
        var center=Ground((from+to)*.5f);
        var section=Group(parent,"Solid palisade section",center);
        section.localRotation=Quaternion.Euler(0,heading,0);
        Solid(section,"Palisade blocks creatures and projectiles",Vector3.up*1.4f,new(.70f,3.4f,delta.magnitude+.35f));
        Part(section,"Lower binding rail",new(0,.65f,0),new(.38f,.19f,delta.magnitude),oak);
        Part(section,"Upper binding rail",new(0,2.1f,0),new(.38f,.18f,delta.magnitude),oak);
    }

    private static void FortGate(Transform parent,Vector2 point,float heading,float width)
    {
        var gate=Group(parent,"Open timber gate",Ground(point));
        gate.localRotation=Quaternion.Euler(0,heading,0);
        for(int side=-1;side<=1;side+=2)
        {
            Part(gate,"Gatepost",new(side*(width*.5f+.1f),1.9f,0),new(.58f,4.0f,.65f),oak);
            Part(gate,"Bronze gate binding",new(side*(width*.5f+.1f),2.5f,0),new(.63f,.17f,.72f),bronze);
        }
        Part(gate,"High lintel",new(0,3.8f,0),new(width+1.1f,.50f,.64f),oak);
        Part(gate,"Weather cap",new(0,4.13f,0),new(width+1.4f,.20f,.97f),roof);
    }

    private static void BuildRiverlands(Transform parent)
    {
        var vale=Group(parent,"Steamroot Vale",Vector3.zero);
        BuildRiver(vale);
        BuildRiverBridge(vale);
        Road(vale,"Willowford bridge and riverbank trail",AshfallReach.RiverbankTrail,2.0f);
        BuildWillowford(parent,vale);
        var oakTree=Load<GameObject>(Art+"CornbergBatch/Trees/Tree_Oak.fbx");
        var willow=Load<GameObject>(Art+"CornbergBatch/Trees/Tree_Willow.fbx");
        var birch=Load<GameObject>(Art+"CornbergBatch/Trees/Tree_Birch.fbx");
        var forest=Group(vale,"Lush river woods",Vector3.zero);
        for(int i=0;i<370;i++)
        {
            var p=new Vector2(Range(-248,-49),Range(-173,27));
            if((p-AshfallReach.Willowford).magnitude<34 ||
                Mathf.Abs(p.x-AshfallReach.RiverX(p.y))<AshfallReach.RiverHalfWidth(p.y)+7 ||
                DistanceToRoad(p,AshfallReach.WillowfordRoad)<6 ||
                DistanceToRoad(p,AshfallReach.RiverbankTrail)<5 ||
                Mathf.Abs(p.y+62)<6&&p.x<-168&&p.x>-196)continue;
            var model=i%5==0?willow:i%3==0?birch:oakTree;
            var tree=(GameObject)PrefabUtility.InstantiatePrefab(model,forest);
            tree.transform.localPosition=Ground(p)-Vector3.up*.07f;
            tree.transform.localScale=Vector3.one*Range(.87f,1.55f);
            tree.transform.localRotation=Quaternion.Euler(0,Range(0,360),0);
            if(p.x>-230&&p.y>-153)
            {
                tree.layer=SolidLayer;
                var collision=tree.AddComponent<CapsuleCollider>();
                collision.radius=.43f;collision.height=3.4f;collision.center=Vector3.up*1.7f;
            }
            if(i%3==0)
                Rock(forest,Ground(p+Vector2.right*1.5f),new(1.1f,.5f,1.2f),i%2==0?heath:slate);
        }
        for(int i=0;i<175;i++)
        {
            float x=Range(-255,42),z=Range(-370,-163);
            var tree=(GameObject)PrefabUtility.InstantiatePrefab(i%3==0?willow:oakTree,forest);
            tree.transform.localPosition=Ground(x,z);
            tree.transform.localScale=Vector3.one*Range(1.0f,2.0f);
            tree.transform.localRotation=Quaternion.Euler(0,Range(0,360),0);
        }
        Waypost(vale,"STEAMROOT VALE",Ground(-69,-46),165,3.65f);
        Waypost(vale,"WILLOWFORD",Ground(-123,-57),168,3.5f);
    }

    private static void BuildRiver(Transform parent)
    {
        var river=Group(parent,"Cornberg river continuing across the south face",Vector3.zero);
        var vertices=new List<Vector3>();
        var uv=new List<Vector2>();
        var triangles=new List<int>();
        int index=0;
        for(float z=35;z>=-390;z-=1)
        {
            float x=AshfallReach.RiverX(z),width=AshfallReach.RiverHalfWidth(z);
            vertices.Add(new Vector3(x-width,AshfallReach.RiverWaterHeight,z));
            vertices.Add(new Vector3(x+width,AshfallReach.RiverWaterHeight,z));
            uv.Add(new Vector2(0,(35-z)*.20f));uv.Add(new Vector2(1,(35-z)*.20f));
            if(index>0)
            {
                int a=(index-1)*2;
                triangles.AddRange(new[]{a,a+1,a+2,a+1,a+3,a+2});
            }
            if(z>-155&&Mathf.Abs(z+62)>2.9f)
                Solid(river,"River channel excludes walking",new Vector3(x,AshfallReach.RiverWaterHeight,z),
                    new Vector3(width*2+.25f,3.4f,1.2f));
            if(index%5==0&&z>-150)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    var bank=Ground(x+side*(width+Range(3.5f,5.4f)),z);
                    Rock(river,bank+Vector3.up*.08f,new(Range(.7f,1.8f),Range(.35f,.8f),Range(.8f,1.65f)),slate);
                    for(int reed=0;reed<3;reed++)
                        Part(river,"Riverbank reed",bank+new Vector3(reed*.12f,.55f,reed*.09f),
                            new(.055f,Range(.85f,1.3f),.065f),heath,rotation:Quaternion.Euler(0,0,reed*8-8));
                }
            }
            index++;
        }
        var mesh=new Mesh{name="Cornberg river on Face 2"};
        mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
        Surface(river,"Flowing water from Cornberg",SaveMesh(mesh),Load<Material>(Handcrafted+"Cornberg river water.mat"));
        float sourceX=-18+Mathf.Sin(-53*.065f)*2;
        if(Mathf.Abs(vertices[0].x+2.3f+AshfallReach.WorldOneGateX-sourceX)>.002f)
            throw new InvalidOperationException("The river does not align with Cornberg at the shared face edge.");
    }

    private static void BuildRiverBridge(Transform parent)
    {
        var bridge=Group(parent,"Willowford timber bridge",new Vector3(AshfallReach.RiverX(-62),1.10f,-62));
        var deck=Part(bridge,"Walkable timber bridge",Vector3.zero,new(23,.35f,6),oak);
        deck.gameObject.layer=WalkableLayer;
        deck.gameObject.AddComponent<BoxCollider>().center=Vector3.up*.34f;
        for(int i=0;i<43;i++)
            Part(bridge,"Weathered deck plank",new(-11.3f+i*.535f,.23f,0),new(.51f,.12f,5.9f),i%6==0?oak:cedar);
        for(int side=-1;side<=1;side+=2)
        {
            Part(bridge,"Solid bridge railing",new(0,.95f,side*2.9f),new(23,.17f,.16f),oak);
            Solid(bridge,"Bridge rail collision",new(0,.85f,side*2.98f),new(23,1.5f,.24f));
            for(int i=0;i<9;i++)
            {
                float x=-11+i*2.75f;
                Part(bridge,"Railing post",new(x,.70f,side*2.9f),new(.22f,1.65f,.24f),oak);
                Part(bridge,"Bronze post cap",new(x,1.55f,side*2.9f),new(.30f,.12f,.30f),bronze);
            }
        }
        for(int side=-1;side<=1;side+=2)
            Part(bridge,"Bridge footing",new(side*7.4f,-1.2f,0),new(1.2f,3.7f,5),basalt);
        Lantern(parent,Ground(AshfallReach.RiverX(-62)+12,-57));
        Lantern(parent,Ground(AshfallReach.RiverX(-62)-12,-57));
    }

    private static void BuildWillowford(Transform expansion,Transform parent)
    {
        var town=Group(parent,"Willowford",Vector3.zero);
        var thatch=Load<Material>(Handcrafted+"Thatch.mat");
        Cabin(town,"Willowford inn",-152,-48,9.2f,6.2f,180,thatch);
        Cabin(town,"River outfitter",-137,-50,7.6f,5.4f,-155,thatch);
        Cabin(town,"Southbank cottage",-154,-77,6.8f,5.0f,0,thatch);
        Cabin(town,"Weaver cottage",-140,-77,6.6f,5.2f,0,thatch);
        Cabin(town,"Boatwright",-164,-72,7,5.0f,70,thatch);
        Cabin(town,"Willowbank lodge",-167,-50,6.8f,5.0f,100,thatch);
        Cabin(town,"River granary",-123,-77,8,6,-90,thatch);
        var spring=Object.Instantiate(expansion.Find("Cinderwatch/Cinderwatch spring").gameObject,town);
        spring.name="Willowford spring";
        spring.transform.localPosition=Ground(-150,-64);
        var healer=spring.GetComponent<HealerInteraction>();
        healer.ConfigureName("Drink from the Willowford spring");
        healer.ConfigureApproach(Group(town,"Willowford spring approach",Ground(-147.8f,-64)));
        var trader=Resident(town,"Willowford Outfitter","outfitter","Villager",new(-137,-55),new(-137,-57),190,
            "Willowford keeps the river road supplied. The bridge leads to the western groves; follow the eastern road to Cinderwatch.",
            "npc.willowford.");
        var talk=trader.GetComponent<ConversationTarget>();
        var shop=trader.AddComponent<ItemShop>();
        shop.ConfigureName("Willowford Outfitter");
        shop.Configure(riverTraderStock);
        var approach=town.Find("Willowford Outfitter approach");
        shop.ConfigureApproach(approach);
        var context=trader.AddComponent<ContextualNpc>();
        context.Configure("npc.willowford.outfitter",shop,talk);
        context.ConfigureName("Willowford Outfitter");
        context.ConfigureApproach(approach);
        Resident(town,"Willowford Riverkeeper","riverkeeper","Herbalist",new(-154,-58),new(-152,-58),125,
            "The water arrives from the green face above the crossing. Keep to the bridge when you cross; the channel is deeper than it looks.",
            "npc.willowford.");
        Resident(town,"Willowford Boatwright","boatwright","Farmer",new(-159,-69),new(-157,-69),90,
            "The river is our road through the groves. We bring timber up to the bridge and leave a clear path for travellers.",
            "npc.willowford.");
        foreach(var p in new[]{new Vector2(-165,-58),new Vector2(-153,-54),new Vector2(-139,-59),
            new Vector2(-133,-67),new Vector2(-157,-72),new Vector2(-169,-68)})
            Lantern(town,Ground(p));
        Waypost(town,"WILLOWFORD",Ground(-145,-59),167,3.55f);
        Road(town,"Willowford south street",new[]{new Vector2(-157,-62),new Vector2(-157,-70),new Vector2(-143,-70),new Vector2(-137,-62)},2);
        for(int i=0;i<7;i++)
        {
            var p=Ground(-144+i*.65f,-54);
            Part(town,"Outfitter supply crate",p+Vector3.up*.42f,new(.59f,.84f,.68f),i%2==0?oak:cedar);
        }
    }
}
