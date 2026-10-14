using DiceFree.Input;
using UnityEngine;

namespace DiceFree.UI
{
    public static class MinimapCompass
    {
        static Texture2D rose;
        public static Rect BoundsFor(Rect map,MinimapCompassCorner corner)
        {
            if(corner==MinimapCompassCorner.Off)return default;
            float size=Mathf.Clamp(map.width*.27f,44f,74f),padding=4f;
            bool right=corner==MinimapCompassCorner.BottomRight || corner==MinimapCompassCorner.TopRight;
            bool top=corner==MinimapCompassCorner.TopLeft || corner==MinimapCompassCorner.TopRight;
            return new Rect(right?map.xMax-padding-size:map.x+padding,
                top?map.y+padding:map.yMax-padding-size,size,size);
        }
        public static Vector2 DirectionOnMap(Vector3 direction,Vector3 mapUp)
        {
            var up=MinimapHud.MapUpDirection(mapUp);
            var right=Vector3.Cross(Vector3.up,up).normalized;
            return new Vector2(Vector3.Dot(direction,right),-Vector3.Dot(direction,up));
        }
        public static void Draw(Rect map,Vector3 mapUp,MinimapCompassCorner corner)
        {
            if(corner==MinimapCompassCorner.Off)return;
            var bounds=BoundsFor(map,corner);
            if(rose==null)rose=CreateRose();
            Vector2 north=DirectionOnMap(Vector3.forward,mapUp);
            float angle=Mathf.Atan2(north.x,-north.y)*Mathf.Rad2Deg;
            var matrix=GUI.matrix;
            GUIUtility.RotateAroundPivot(angle,bounds.center);
            GUI.DrawTexture(bounds,rose,ScaleMode.StretchToFill,true);
            GUI.matrix=matrix;
            var style=new GUIStyle(GUI.skin.label){
                alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,
                fontSize=Mathf.RoundToInt(Mathf.Clamp(bounds.width*.16f,9f,12f))
            };
            void Label(string label,Vector3 direction)
            {
                Vector2 point=bounds.center+DirectionOnMap(direction,mapUp)*(bounds.width*.40f);
                style.normal.textColor=label=="N"?new Color(1f,.56f,.40f):new Color(1f,.93f,.73f);
                GUI.Label(new Rect(point.x-9,point.y-9,18,18),label,style);
            }
            Label("N",Vector3.forward);Label("E",Vector3.right);
            Label("S",Vector3.back);Label("W",Vector3.left);
        }
        static Vector2 Point(float angle,float radius)=>
            new Vector2(Mathf.Sin(angle),Mathf.Cos(angle))*radius;
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static bool InTriangle(Vector2 point,Vector2 a,Vector2 b,Vector2 c)
        {
            float x=Cross(b-a,point-a),y=Cross(c-b,point-b),z=Cross(a-c,point-c);
            return (x>=0&&y>=0&&z>=0)||(x<=0&&y<=0&&z<=0);
        }
        static Texture2D CreateRose()
        {
            const int size=96;
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Vector2 point=new Vector2(x+.5f-size*.5f,y+.5f-size*.5f);
                float distance=point.magnitude;
                Color color=distance<46?new Color(.018f,.025f,.035f,.82f):Color.clear;
                if(distance>=43&&distance<=44.5f)color=new Color(.83f,.71f,.44f,.92f);
                for(int ray=0;ray<8;ray++)
                {
                    float angle=ray*Mathf.PI*.25f;
                    var tip=Point(angle,ray%2==0?34f:23f);
                    var left=Point(angle-Mathf.PI*.125f,7f);
                    var right=Point(angle+Mathf.PI*.125f,7f);
                    if(InTriangle(point,Vector2.zero,left,tip))
                        color=ray==0?new Color(.95f,.38f,.25f):new Color(.97f,.88f,.61f);
                    if(InTriangle(point,Vector2.zero,tip,right))
                        color=ray==0?new Color(.52f,.13f,.10f):new Color(.48f,.37f,.20f);
                }
                if(distance<2.8f)color=new Color(1f,.92f,.69f);
                pixels[y*size+x]=color;
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){
                name="Minimap compass rose",filterMode=FilterMode.Bilinear,
                wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave
            };
            texture.SetPixels(pixels);texture.Apply(false,true);
            return texture;
        }
    }
}
