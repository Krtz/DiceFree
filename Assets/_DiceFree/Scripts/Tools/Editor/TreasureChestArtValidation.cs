using System;
using System.IO;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using DiceFree.Items;

namespace DiceFree.EditorTools
{
    internal static class TreasureChestArtValidation
    {
        const string Resource="Assets/_DiceFree/Resources/WorldLoot/";
        const string Screenshot="Assets/_DiceFree/Art/Validation/Previews/WorldTreasureChest_Unity.png";
        [CliCommand("dicefree.loot.chest.prepare","Convert licensed chest materials to URP Lit and validate model.")]
        public static object Prepare()
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Resource+"TreasureChestModel.fbx");
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Resource+"TreasureChestTexture.png");
            if(model==null||texture==null)throw new InvalidOperationException("Licensed chest model/texture missing");
            string[] names={"TreasureChestWood","TreasureChestGold"};
            foreach(string name in names)
            {
                var material=AssetDatabase.LoadAssetAtPath<Material>(Resource+name+".mat");
                if(material==null)throw new InvalidOperationException("Missing "+name);
                material.shader=Shader.Find("Universal Render Pipeline/Lit");
                if(material.shader==null)throw new InvalidOperationException("URP Lit missing");
                material.SetTexture("_BaseMap",texture);
                material.SetColor("_BaseColor",Color.white);
                material.SetFloat("_Metallic",name.Contains("Gold")?.55f:0f);
                material.SetFloat("_Smoothness",name.Contains("Gold")?.48f:.18f);
                EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();
            return new{success=true,model=model.name,texture=texture.name,
                renderers=model.GetComponentsInChildren<Renderer>(true).Length};
        }

        [CliCommand("dicefree.loot.chest.capture","Render actual licensed 3D treasure chest used for ground loot.")]
        public static object Capture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode required");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Unsaved user scene");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            RenderTexture tex=null,old=RenderTexture.active;
            Texture2D bmp=null;
            Camera camera=null;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);
                UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());
                var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor",new Color(.19f,.21f,.23f));
                floor.GetComponent<Renderer>().sharedMaterial=material;
                var chest=WorldLootVisual.Create(Vector3.zero,"Original fallback chest",1.45f,
                    forceOriginal:true);
                if(chest.GetComponentsInChildren<Collider>(true).Length!=1)
                    throw new InvalidOperationException("Only the single pickup collider should exist");
                var model=chest.transform.GetChild(0);
                var renderers=model.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length<1)throw new InvalidOperationException("No chest geometry");
                var key=new GameObject("Sun").AddComponent<Light>();
                key.type=LightType.Directional;key.intensity=1.6f;key.shadows=LightShadows.None;
                key.transform.rotation=Quaternion.Euler(44,-34,0);
                camera=new GameObject("Treasure camera").AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=1.75f;
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.12f,.145f,.18f);
                camera.transform.position=new Vector3(2.9f,2.35f,-3.4f);
                camera.transform.LookAt(new Vector3(0,.55f,0));
                tex=new RenderTexture(1280,720,24);
                camera.targetTexture=tex;camera.Render();
                RenderTexture.active=tex;
                bmp=new Texture2D(1280,720,TextureFormat.RGB24,false);
                bmp.ReadPixels(new Rect(0,0,1280,720),0,0);bmp.Apply();
                File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName,Screenshot),bmp.EncodeToPNG());
                AssetDatabase.ImportAsset(Screenshot,ImportAssetOptions.ForceUpdate);
                return new{success=true,renderers=renderers.Length,colliders=1,path=Screenshot,
                    size=model.lossyScale.ToString()};
            }
            finally
            {
                if(camera!=null)camera.targetTexture=null;
                RenderTexture.active=old;
                if(tex!=null){tex.Release();UnityEngine.Object.DestroyImmediate(tex);}
                if(bmp!=null)UnityEngine.Object.DestroyImmediate(bmp);
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
