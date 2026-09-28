using UnityEngine;
using static DiceFree.EditorTools.BlockoutShapes;
using static DiceFree.EditorTools.CornbergLandscape;

namespace DiceFree.EditorTools
{
    internal static class CornbergVillage
    {
        internal static void Create(Transform parent)
        {
            var village = Group("04 - Cornberg village", parent);
            Building("General goods", 17,-13, 7,6, village);
            var forge = Building("Blacksmith", -5,16, 7,7, village);
            Box("Forge chimney", new Vector3(-7,5.5f,18), new Vector3(1.8f,5,1.8f), Stone, forge);
            Box("Forge hearth", new Vector3(-5,0.8f,10.7f), new Vector3(2.8f,1.6f,1.8f), Stone, forge);
            Box("Banked coals", new Vector3(-5,1.7f,10.7f), new Vector3(1.9f,0.15f,1), Glow, forge, false);
            var bank = Building("Bank", 13,17, 7,6, village);
            Box("Storehouse chest", new Vector3(13,0.6f,12.4f), new Vector3(2.2f,1.2f,1.1f), Timber, bank);
            var brewery = Building("Brewery", -4,-14, 8,6, village);
            for (int i = 0; i < 3; i++)
                Shape("Working brew vat", PrimitiveType.Cylinder, new Vector3(-9+i*2.3f,1.1f,-9), new Vector3(1.8f,1.1f,1.8f), Timber, brewery);
            for (int i = 0; i < 2; i++)
            {
                Box("Community table", new Vector3(-6+i*5,0.85f,-5), new Vector3(3,0.25f,1.6f), Timber, brewery);
                Box("Communal bench", new Vector3(-6+i*5,0.4f,-6.7f), new Vector3(3,0.3f,0.55f), Timber, brewery);
            }
            Building("Farmhouse", 29,24,7,7,village);
            Building("Cottage", 14,32,6,6,village);
            Building("Cottage", -3,32,6,7,village);
            Building("Cottage", -5,-29,6,6,village);
            Building("Cottage", 12,-29,7,6,village);
            Building("Barn", 40,24,8,9,village);
            var house = Building("Abandoned house - locked", 36,-26,7,7,village);
            var board = Box("Boarded door", new Vector3(36,1.5f,-29.6f), new Vector3(2.4f,0.2f,0.2f), Timber, house);
            board.transform.rotation = Quaternion.Euler(0,0,25);
            Box("Old lock", new Vector3(36,1.1f,-29.8f), new Vector3(0.25f,0.35f,0.15f), Stone, house, false);

            var green = Group("Village green - well and ancient stone", village);
            for (int i = 0; i < 12; i++)
            {
                var angle = i * Mathf.PI * 2 / 12;
                var stone = Box("Well ring stone", new Vector3(-2+Mathf.Cos(angle)*1.45f,0.6f,3+Mathf.Sin(angle)*1.45f),
                    new Vector3(0.75f,1.2f,0.65f), Stone, green);
                stone.transform.rotation = Quaternion.Euler(0,-angle*Mathf.Rad2Deg,0);
            }
            Shape("Magical well water", PrimitiveType.Cylinder, new Vector3(-2,0.48f,3), new Vector3(2.4f,0.03f,2.4f), Glow, green, false);
            Label("Magical well", new Vector3(-2,2.7f,3), green);
            ResurrectionStone(green);
            for (int i = 0; i < 3; i++)
                Box("Green seating", new Vector3(2+i*3,0.5f,9), new Vector3(2,0.35f,0.6f), Timber, green);
            Tree(Ground(24,-19), 8, village); Tree(Ground(23,32),10,village);
            Tree(Ground(-10,27),9,village); Tree(Ground(43,-30),9,village);
            Fields(parent);
        }

        private static Transform Building(string name, float x, float z, float width, float depth, Transform parent)
        {
            var group = Group(name, parent); var y = Height(x,z);
            Box("Fieldstone foundation", new Vector3(x,y+0.35f,z), new Vector3(width+0.3f,0.7f,depth+0.3f), Stone,group);
            Box("Timber and lime walls", new Vector3(x,y+2,z), new Vector3(width,3.4f,depth),Plaster,group);
            foreach (float side in new[]{-1f,1f})
            {
                Box("Corner beam",new Vector3(x+side*(width/2-0.15f),y+2,z-depth/2-0.05f),new Vector3(0.3f,3.5f,0.3f),Timber,group,false);
                Box("Window",new Vector3(x+side*width*0.29f,y+2.4f,z-depth/2-0.08f),new Vector3(1,1.1f,0.1f),Dark,group,false);
            }
            Box("Door - exterior blockout",new Vector3(x,y+1.3f,z-depth/2-0.08f),new Vector3(1.3f,2.3f,0.12f),Timber,group,false);
            var vertices = new[] { new Vector3(-0.5f,0,-0.5f),new Vector3(0.5f,0,-0.5f),new Vector3(0,1,-0.5f),
                new Vector3(-0.5f,0,0.5f),new Vector3(0.5f,0,0.5f),new Vector3(0,1,0.5f) };
            var mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Art/Blockout/Gabled roof.asset") ??
                SaveMesh("Gabled roof",vertices,new[]{0,2,1,3,4,5,0,3,5,0,5,2,1,2,5,1,5,4,0,1,4,0,4,3});
            var roof = MeshObject("Gabled roof",mesh,Roof,group);
            roof.transform.position = new Vector3(x,y+3.7f,z); roof.transform.localScale = new Vector3(width+1.1f,2.4f,depth+1.1f);
            Label(name,new Vector3(x,y+6.7f,z),group);
            return group;
        }

        private static void ResurrectionStone(Transform parent)
        {
            const float size = 2.2f;
            var rotation = Quaternion.AngleAxis(25, Vector3.up) * Quaternion.FromToRotation(Vector3.one.normalized,Vector3.up);
            var center = new Vector3(8,Mathf.Sqrt(3)*size/2,3);
            var die = Box("Ancient stone d6 - corner resting",center,Vector3.one*size,Stone,parent);
            die.transform.rotation = rotation;
            // Camera-facing side has one luminous pip. Activation/respawn are Phase 2.
            var faces = new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};
            var counts = new[]{1,6,2,5,3,4};
            for(int face=0;face<6;face++)
            {
                var normal=faces[face]; var tangent=Vector3.Cross(normal,Vector3.up);
                if(tangent.sqrMagnitude<0.1f) tangent=Vector3.right;
                tangent.Normalize(); var bitangent=Vector3.Cross(normal,tangent);
                for(int pip=0;pip<counts[face];pip++)
                {
                    float u=counts[face]==1 ? 0 : (pip%2==0 ? -0.23f:0.23f);
                    float v=counts[face]==1 ? 0 : (pip/2-((counts[face]-1)/2f)*0.5f)*0.3f;
                    var local=normal*(size/2+0.02f)+(tangent*u+bitangent*v)*size;
                    var dot=Shape("Pip "+counts[face],PrimitiveType.Sphere,center+rotation*local,
                        new Vector3(0.28f,0.28f,0.09f),face==0?Glow:Dark,parent,false);
                    dot.transform.rotation=Quaternion.LookRotation(rotation*normal);
                }
            }
            Label("Ancient stone",new Vector3(8,5.1f,3),parent);
        }

        private static void Fields(Transform parent)
        {
            var fields=Group("05 - Corn fields and pasture",parent);
            foreach(float z in new[]{-12f,15f})
            {
                Box("Tilled field",new Vector3(47,0.03f,z),new Vector3(21,0.08f,12),Soil,fields,false);
                for(int row=0;row<6;row++)
                for(int col=0;col<10;col++)
                    Shape("Crop placeholder",PrimitiveType.Cube,new Vector3(38+col*1.9f,0.48f,z-4.8f+row*1.8f),
                        new Vector3(0.35f,0.9f,0.35f),Crop,fields,false);
                for(int i=0;i<8;i++)
                    Box("Field fence post",new Vector3(36+i*3,0.55f,z-6.5f),new Vector3(0.2f,1.1f,0.2f),Timber,fields,false);
                Box("Field fence rail",new Vector3(46.5f,0.75f,z-6.5f),new Vector3(21,0.15f,0.15f),Timber,fields,false);
            }
        }
    }
}
