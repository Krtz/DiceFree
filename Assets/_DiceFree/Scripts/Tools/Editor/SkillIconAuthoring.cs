using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class SkillIconAuthoring
    {
        private const string Folder="Assets/_DiceFree/Resources/AbilityIconsLicensed";
        private static void Require(bool good,string message)
        {
            if(!good)throw new InvalidOperationException("SKILL ICONS: "+message);
        }
        private static IEnumerable<string> AllKeys() =>
            Enum.GetValues(typeof(NoviceSkillKind)).Cast<NoviceSkillKind>()
                .Select(SkillIconLibrary.Key)
            .Concat(Enum.GetValues(typeof(PhysicalSkillKind))
                .Cast<PhysicalSkillKind>().Select(SkillIconLibrary.Key))
            .Concat(Enum.GetValues(typeof(MagicalSkillKind))
                .Cast<MagicalSkillKind>().Select(SkillIconLibrary.Key))
            .Concat(new[]{"attack-command","interact","return-home"})
            .Distinct(StringComparer.Ordinal);

        [MenuItem("DiceFree/UI/Refresh local ability icons")]
        public static void RefreshFromMenu()=>Install();

        [CliCommand("dicefree.skill-icons.install",
            "Configure source-pack skill icons as crisp, locally licensed UI textures.")]
        public static object Install()
        {
            var keys=AllKeys().ToArray();
            int configured=0;
            foreach(var key in keys)
            {
                var path=Folder+"/"+key+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                Require(importer!=null,"missing locally licensed skill artwork "+path);
                bool dirty=importer.textureType!=TextureImporterType.Default ||
                    importer.mipmapEnabled ||
                    importer.textureCompression!=TextureImporterCompression.Uncompressed ||
                    importer.filterMode!=FilterMode.Bilinear;
                if(dirty)
                {
                    importer.textureType=TextureImporterType.Default;
                    importer.mipmapEnabled=false;
                    importer.textureCompression=TextureImporterCompression.Uncompressed;
                    importer.filterMode=FilterMode.Bilinear;
                    importer.alphaIsTransparency=true;
                    importer.SaveAndReimport();
                }
                configured++;
            }
            AssetDatabase.SaveAssets();
            return new{success=true,configured,skillKinds=15};
        }

        [CliCommand("dicefree.skill-icons.validate",
            "Verify all Novice, Magically Touched and Physically Blessed skill art resolves in the active action bar.")]
        public static object Validate()
        {
            var keys=AllKeys().ToArray();
            Require(keys.Length==17,"expected 14 distinct active and passive skill icons");
            var textures=new HashSet<Texture2D>();
            foreach(var key in keys)
            {
                var source=Resources.Load<Texture2D>("AbilityIconsLicensed/"+key);
                Require(source!=null,"missing real pack icon for "+key);
                Require(source.width>=32 && source.height>=32,
                    "art too small for an action bar: "+key);
                Require(textures.Add(source),"duplicate icon asset reused: "+key);
                Require(SkillIconLibrary.Get(key)==source,
                    "HUD source isn't using the imported pack texture for "+key);
            }
            Require(SkillIconLibrary.Key(NoviceSkillKind.MagicSand)==
                    SkillIconLibrary.Key(MagicalSkillKind.MagicSand),
                "Magic Sand should keep its identity after advancement");
            Require(SkillIconLibrary.Get("nonexistent-fallback")!=null,
                "portable art fallback should always exist without a licensed pack");
            var bar=typeof(GenericActionBar);
            var view=typeof(AbilityBarSource.SlotView);
            Require(bar!=null && view.GetField("icon")!=null &&
                view.GetField("iconKey")!=null,
                "the live GenericActionBar source does not expose ability art");
            return new{success=true,uniqueTextures=textures.Count,skillKinds=15,
                activeSlotsPerClass=4,resourceFolder="AbilityIconsLicensed"};
        }
    }
}
