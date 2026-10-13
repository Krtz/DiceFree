using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    /// <summary>Reproducible asset repairs for the Unity 6.6 development build.</summary>
    public static class BuildWarningRepairs
    {
        const string Shaders = "Assets/_DiceFree/Shaders/Compatibility";
        const string GlobalSettings = "Assets/_DiceFree/Settings/UniversalRenderPipelineGlobalSettings.asset";
        const string VillageShader = "Assets/Medieval Village Building Pack/Shaders/RMHA.shader";
        static readonly string[] CollisionMeshes = {
            "Assets/_DiceFree/Art/Blockout/Trimery continuous playable terrain.asset",
            "Assets/_DiceFree/Art/World/NaturePass03/CornbergMeadowTerrainUV.asset"
        };

        [CliCommand("dicefree.build-warnings.repair", "Repair terrain collision flags and URP debug shader references; guard the optional local village shader macros.")]
        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before repairing build assets.");

            foreach (string path in CollisionMeshes)
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) throw new InvalidOperationException("Missing terrain mesh: " + path);
                if (!mesh.HasPreBakeCollisionMesh(false))
                {
                    mesh.SetPreBakeCollisionMesh(false, true);
                    EditorUtility.SetDirty(mesh);
                    AssetDatabase.SaveAssetIfDirty(mesh);
                }
            }

            Directory.CreateDirectory(Shaders);
            var occluder = InstallCoreShader("b3c92426a88625841ab15ca6a7917248", "DebugOccluder");
            var occlusion = InstallCoreShader("d3f0849180c2d0944bc71060693df100", "DebugOcclusionTest");
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GlobalSettings);
            if (settings == null) throw new InvalidOperationException("URP global settings missing.");
            var serialized = new SerializedObject(settings);
            var property = serialized.GetIterator();
            int matched = 0;
            int postProcessingSettings = 0;
            while (property.Next(true))
            {
                if (property.name == "m_DebugOccluderPS")
                { property.objectReferenceValue = occluder; matched++; }
                else if (property.name == "m_DebugOcclusionTestPS")
                { property.objectReferenceValue = occlusion; matched++; }
                else if (property.name == "m_StripUnusedPostProcessingVariantsAndResources")
                {
                    // URP initializes these passes at startup even when a volume does not use them.
                    property.boolValue = false;
                    postProcessingSettings++;
                }
            }
            if (postProcessingSettings == 0) throw new InvalidOperationException("URP post-processing build settings missing.");
            if (matched != 2) throw new InvalidOperationException("Unexpected GPU Resident Drawer resource layout.");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(settings);

            bool vendorShaderRepaired = GuardVillageMacros();
            AssetDatabase.SaveAssets();
            return new {
                marker = "DICEFREE_BUILD_WARNING_ASSETS_REPAIRED",
                collisionMeshes = CollisionMeshes.Length, urpDebugShaders = matched, postProcessingShadersRetained = true, vendorShaderRepaired
            };
        }

        static Shader InstallCoreShader(string guid, string name)
        {
            string source = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(source)) throw new InvalidOperationException("Core shader missing: " + name);
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(source);
            if (package == null) throw new InvalidOperationException("Core shader package missing.");
            string sourceFile = Path.Combine(package.resolvedPath, source.Substring(package.assetPath.Length + 1));
            string text = File.ReadAllText(sourceFile);
            if (!text.Contains("Shader \"Hidden/Core/" + name + "\""))
                throw new InvalidOperationException("Unexpected Core shader identity: " + source);
            // These Core shaders use shared Core includes, but 17.6 tags them for HDRP only.
            // Preserve their rendering code and target our actual Universal pipeline.
            text = "// Adapted from com.unity.render-pipelines.core 17.6.0.\n" +
                "// Copyright (c) 2020 Unity Technologies ApS; Unity Companion License. See LICENSE.md.\n" +
                text.Replace("Shader \"Hidden/Core/" + name + "\"", "Shader \"Hidden/DiceFree/" + name + "\"")
                    .Replace("\"HDRenderPipeline\"", "\"UniversalPipeline\"");
            string destination = Shaders + "/" + name + ".shader";
            if (!File.Exists(destination) || File.ReadAllText(destination) != text)
                File.WriteAllText(destination, text, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(destination);
            if (shader == null || ShaderUtil.ShaderHasError(shader))
                throw new InvalidOperationException("URP compatibility shader failed: " + destination);

            string license = Path.Combine(package.resolvedPath, "LICENSE.md");
            if (!File.Exists(license)) throw new InvalidOperationException("Core shader license missing.");
            string licenseDestination = Shaders + "/LICENSE.md";
            if (!File.Exists(licenseDestination) || File.ReadAllText(licenseDestination) != File.ReadAllText(license))
                File.Copy(license, licenseDestination, true);
            AssetDatabase.ImportAsset(licenseDestination);
            return shader;
        }

        static bool GuardVillageMacros()
        {
            if (!File.Exists(VillageShader)) return false;
            string original = File.ReadAllText(VillageShader);
            string newline = original.Contains("\r\n") ? "\r\n" : "\n";
            string updated = original;
            foreach (string macro in new[] { "UNITY_PASS_FORWARDBASE", "UNITY_PASS_FORWARDADD" })
            {
                if (Regex.IsMatch(updated, @"(?m)^\s*#ifndef[ \t]+" + macro + @"[ \t]*\r?$")) continue;
                updated = Regex.Replace(updated, @"(?m)^([ \t]*)#define[ \t]+" + macro + @"[ \t]*\r?$",
                    match => match.Groups[1].Value + "#ifndef " + macro + newline +
                        match.Groups[1].Value + "#define " + macro + newline +
                        match.Groups[1].Value + "#endif");
            }
            if (updated == original) return false;
            // The licensed source stays local and ignored. Only this repair procedure is tracked.
            Directory.CreateDirectory("Library/BuildWarningBackups");
            string backup = "Library/BuildWarningBackups/RMHA.shader.original";
            if (!File.Exists(backup)) File.WriteAllText(backup, original, new UTF8Encoding(false));
            File.WriteAllText(VillageShader, updated, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(VillageShader, ImportAssetOptions.ForceSynchronousImport);
            return true;
        }
    }
}
