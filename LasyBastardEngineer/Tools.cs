using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LasyBastardEngineer
{
    internal static class Assets
    {
        //cache bundles if multiple characters use the same one
        internal static Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

        internal static AssetBundle LoadAssetBundle(string bundleName)
        {
            if (loadedBundles.ContainsKey(bundleName))
            {
                return loadedBundles[bundleName];
            }

            AssetBundle assetBundle = null;
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(LazyBastardPlugin.instance.Info.Location), bundleName));

            loadedBundles[bundleName] = assetBundle;

            return assetBundle;

        }
    }
    internal static class Materials
    {
        internal static void GetMaterial(GameObject model, string childObject, Color color, ref Material material, float scaleMultiplier = 1, bool replaceAll = false)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Renderer smr = renderer;

                if (string.Equals(renderer.name, childObject))
                {
                    if (color == Color.clear)
                    {
                        UnityEngine.GameObject.Destroy(renderer);
                        return;
                    }

                    if (material == null)
                    {
                        material = new Material(renderer.material);
                        material.mainTexture = renderer.material.mainTexture;
                        material.shader = renderer.material.shader;
                        material.color = color;
                    }
                    renderer.material = material;
                    renderer.transform.localScale *= scaleMultiplier;
                    if (!replaceAll)
                        break;
                }
            }
        }
        internal static void DebugMaterial(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Renderer smr = renderer;
                Debug.Log("Material: " + smr.name.ToString());
            }
        }
        internal static void SwapAllShaders(AssetBundle assetBundle)
        {
            foreach (var material in assetBundle.LoadAllAssets<Material>())
            {
                Log.Debug("Trying to swap shader for " + material.name);
                TrySwapShader(material);
            }

        }

        internal static void TrySwapShader(Material material)
        {
            var shaderName = material.shader.name;
            if (shaderName.Contains("Stubbed"))
            {
                shaderName = shaderName.Replace("Stubbed", string.Empty) + ".shader";
                var replacementShader = Addressables.LoadAssetAsync<Shader>(shaderName).WaitForCompletion();

                if (replacementShader != null)
                {
                    material.shader = replacementShader;
                    //RiskOfRamenMain.LogDebug("Swapped shader " + material.name + "!");
                }
                else
                {
                   Log.Error("Failed to load shader " + shaderName);
                }
            }
            else if (shaderName == "Standard")
            {
                var normalMap = material.GetTexture("_BumpMap");
                var normalStrength = material.GetFloat("_BumpScale");
                var emissionMap = material.GetTexture("_EmissionMap");

                material.shader = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

                material.SetTexture("_NormalMap", normalMap);
                material.SetFloat("_NormalStrength", normalStrength);
                material.SetTexture("_EmTex", emissionMap);

                material.SetColor("_EmColor", new Color(0.2f, 0.2f, 0.2f));
                material.SetFloat("_EmPower", 0.15f);

            }
        }

        public static Material SetColor(this Material material, Color color)
        {
            material.SetColor("_Color", color);
            return material;
        }

        public static Material SetNormal(this Material material, float normalStrength = 1)
        {
            material.SetFloat("_NormalStrength", normalStrength);
            return material;
        }

        public static Material SetEmission(this Material material) => SetEmission(material, 1);
        public static Material SetEmission(this Material material, float emission) => SetEmission(material, emission, Color.white);
        public static Material SetEmission(this Material material, float emission, Color emissionColor)
        {
            material.SetFloat("_EmPower", emission);
            material.SetColor("_EmColor", emissionColor);
            return material;
        }
        public static Material SetCull(this Material material, bool cull = false)
        {
            material.SetInt("_Cull", cull ? 1 : 0);
            return material;
        }

        public static Material SetSpecular(this Material material, float strength)
        {
            material.SetFloat("_SpecularStrength", strength);
            return material;
        }
        public static Material SetSpecular(this Material material, float strength, float exponent)
        {
            material.SetFloat("_SpecularStrength", strength);
            material.SetFloat("SpecularExponent", exponent);
            return material;
        }
    }
}
