using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;
using R2API.Utils;
using R2API;

using System.Security;
using System.Security.Permissions;
using RoR2.Projectile;
using UnityEngine.Networking;
using HarmonyLib;
using MonoMod.Cil;
using System.Reflection;
using Mono.Cecil.Cil;
using RoR2BepInExPack.VanillaFixes;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace LasyBastardEngineer
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI))]
    [BepInPlugin( "com.Borbo.LazyBastardEngineer", "LazyBastardEngineer", "2.2.0")]

    internal partial class Base : BaseUnityPlugin
    {
        private static ConfigFile CustomConfigFile { get; set; }
        public static ConfigEntry<bool> ForceUnlock { get; set; }
        public static ConfigEntry<bool> AnnounceWhenFail { get; set; }
        public static Harmony Harmony;

        public static string modPrefix = string.Format("@{0}+{1}", "LazyBastardEngineer", "lazybastardengi");
        public static Sprite skinIcon = LoadoutAPI.CreateSkinIcon(new Color(1f, 0.7f, 0.3f), new Color(0.7f, 0.5f, 0.3f), new Color(0.3f, 0.3f, 0.3f), new Color(0.8f, 0.8f, 0.8f));

        public static AssetBundle skinBundle = LoadAssetBundleResourcesProvider(modPrefix, LasyBastardEngineer.Properties.Resources.lazybastardengi);
        public static string skinsPath = "Assets/LazyBastardSkins/";
        public static UnlockableDef unlock;

        public static GameObject engiBodyPrefab;
        public static GameObject turretBodyPrefab;
        public static GameObject walkerBodyPrefab;

        public static GameObject grenadePrefab;
        public static GameObject grenadeGhost;

        public static GameObject minePrefab;
        public static GameObject mineGhost;
        public static GameObject spiderPrefab;
        public static GameObject spiderGhost;

        public static AssetBundle LoadAssetBundleResourcesProvider(string prefix, byte[] resourceBytes)
        {
            if (resourceBytes == null) throw new ArgumentNullException(nameof(resourceBytes));
            if (string.IsNullOrEmpty(prefix) || !prefix.StartsWith("@")) throw new ArgumentException("Invalid prefix format", nameof(prefix));

            var bundle = AssetBundle.LoadFromMemory(resourceBytes);
            if (bundle == null) throw new NullReferenceException(string.Format("{0} did not resolve to an assetbundle.", nameof(resourceBytes)));

            return bundle;
        }


        private void Awake()
        {
            InitializeConfig();

            unlock = ScriptableObject.CreateInstance<UnlockableDef>();
            unlock.cachedName = "Skins.Engineer.LazyBastard";
            unlock.nameToken = "ACHIEVEMENT_LAZYBASTARDENGINEER_NAME";
            unlock.achievementIcon = skinIcon;
            ContentAddition.AddUnlockableDef(unlock);

            SwapAllShaders(skinBundle);
            AddFactorioSkin();

            LanguageAPI.Add("FACTORIO_SKIN_ENGINEER", "Power Armor MK2");
            LanguageAPI.Add("ACHIEVEMENT_LAZYBASTARDENGINEER_NAME", "Engineer: Lazy Bastard");
            LanguageAPI.Add("ACHIEVEMENT_LAZYBASTARDENGINEER_DESCRIPTION", "As Engineer, beat the game or obliterate without using your Primary, Secondary, or Utility skills.");

            /*
            LanguageAPI.Add("DRIFTER_SKIN_MERC", "Drifter");
            LanguageAPI.Add("DASHMASTER_SKIN_MERC", "Dash Master");

            LanguageAPI.Add("DRIFTER_SKINUNLOCKABLE_ACHIEVEMENT_NAME", "Mercenary: Panacea");
            LanguageAPI.Add("DRIFTER_SKINUNLOCKABLE_ACHIEVEMENT_DESC", "As Mercenary, successfully chain 100 Blinding Assault dashes without missing.");
            LanguageAPI.Add("DRIFTER_SKINUNLOCKABLE_UNLOCKABLE_NAME", "Mercenary: Panacea");

            LanguageAPI.Add("DASHMASTER_SKINUNLOCKABLE_ACHIEVEMENT_NAME", "Mercenary: The Dash Eternal");
            LanguageAPI.Add("DASHMASTER_SKINUNLOCKABLE_ACHIEVEMENT_DESC", "As Mercenary, perform <style=cIsUtility>the ultimate chain dash.</style>");
            LanguageAPI.Add("DASHMASTER_SKINUNLOCKABLE_UNLOCKABLE_NAME", "Mercenary: The Dash Eternal");
            */

            // literally lifted from RMB, remove if you don't need """force unlock"""
            Harmony = new Harmony("com.Borbo.LazyBastardEngineer");
            Harmony.PatchAll(typeof(PatchAchievementDefs));
        }

        // literally lifted from RMB, remove if you don't need """force unlock"""
        [HarmonyPatch(typeof(SaferAchievementManager), nameof(SaferAchievementManager.SaferCollectAchievementDefs))]
        public class PatchAchievementDefs
        {
            public static void ILManipulator(ILContext il, MethodBase original, ILLabel retLabel)
            {
                ILCursor c = new ILCursor(il);
                c.Index = 0;
                c.GotoNext(x => x.MatchCastclass<RegisterAchievementAttribute>(), x => x.MatchStloc(11));
                c.Index += 2;
                c.Emit(OpCodes.Ldloc, 10);
                c.Emit(OpCodes.Ldloc, 11);
                c.EmitDelegate<Func<Type, RegisterAchievementAttribute, RegisterAchievementAttribute>>((type, achievementAttribute) =>
                {
                    if (ForceUnlock.Value && achievementAttribute != null && achievementAttribute.unlockableRewardIdentifier == "Skins.Engineer.LazyBastard") return null;
                    return achievementAttribute;
                });
                c.Emit(OpCodes.Stloc, 11);
            }
        }

        void InitializeConfig()
        {
            CustomConfigFile = new ConfigFile(Paths.ConfigPath + "\\LazyBastardEngi.cfg", true);

            ForceUnlock = CustomConfigFile.Bind(
                "Achievements",
                "Force Unlock Lazy Bastard Skin",
                false,
                "Set this to true to bypass the unlock requirement for the Lazy Bastard Skin."
                );
            AnnounceWhenFail = CustomConfigFile.Bind(
                "Debug",
                "Announce When Fail",
                true,
                "Set this to true to add an announcement to the chat when the Lazy Bastard challenge fails. "
                );
        }

        private void AddFactorioSkin()
        {
            //Getting character's prefab
            engiBodyPrefab = Resources.Load<GameObject>("prefabs/characterbodies/EngiBody");
            turretBodyPrefab = Resources.Load<GameObject>("prefabs/characterbodies/EngiTurretBody");
            walkerBodyPrefab = Resources.Load<GameObject>("prefabs/characterbodies/EngiWalkerTurretBody");

            grenadePrefab = Resources.Load<GameObject>("prefabs/projectiles/EngiGrenadeProjectile");
            grenadeGhost = Resources.Load<GameObject>("prefabs/projectileghosts/EngiGrenadeGhost");

            minePrefab = Resources.Load<GameObject>("prefabs/projectiles/EngiMine");//.InstantiateClone("LazyBastardMine", true);
            mineGhost = Resources.Load<GameObject>("prefabs/projectileghosts/EngiMineGhost");
            spiderPrefab = Resources.Load<GameObject>("prefabs/projectiles/SpiderMine");//.InstantiateClone("LazyBastardMine", true);
            spiderGhost = Resources.Load<GameObject>("prefabs/projectileghosts/SpiderMineGhost");

            //Getting necessary components
            Renderer[] renderers = engiBodyPrefab.GetComponentsInChildren<Renderer>(true);
            ModelSkinController skinController = engiBodyPrefab.GetComponentInChildren<ModelSkinController>();
            GameObject mdl = skinController.gameObject;


            LoadoutAPI.SkinDefInfo skin = new LoadoutAPI.SkinDefInfo
            {
                Icon = skinIcon,
                Name = "LazyBastardEngineer",
                NameToken = "FACTORIO_SKIN_ENGINEER",

                RootObject = mdl,
                BaseSkins = new SkinDef[] { skinController.skins[0] },
                UnlockableDef = ForceUnlock.Value ? null : unlock,
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                RendererInfos = new CharacterModel.RendererInfo[1]
                {
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = skinBundle.LoadAsset<Material>(skinsPath + "matLazyBastard.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[4]
                    }
                },
                MeshReplacements = new SkinDef.MeshReplacement[1]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = skinBundle.LoadAsset<Mesh>(skinsPath + "EngiMesh.mesh"),
                        renderer = renderers[4]
                    }
                },
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[2]
                {
                    new SkinDef.ProjectileGhostReplacement
                    {
                        projectilePrefab = minePrefab,
                        projectileGhostReplacementPrefab = NewProjectileGhost(mineGhost, "EngiMineGhost.prefab")//ModifyProjectileGhost(engiMineGhost, "matLazyBastardWeapons.mat", "EngiMineMesh.mesh")
                    },
                    new SkinDef.ProjectileGhostReplacement
                    {
                        projectilePrefab = grenadePrefab,
                        projectileGhostReplacementPrefab = NewProjectileGhost(grenadeGhost, "EngiGrenadeGhost.prefab")
                    }
                },
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[2]
                {
                    new SkinDef.MinionSkinReplacement
                    {
                        minionBodyPrefab = turretBodyPrefab,
                        minionSkin = GetSkinFromTurretBody(turretBodyPrefab, "matLazyBastard.mat", "EngiTurretMesh.mesh")
                    },
                    new SkinDef.MinionSkinReplacement
                    {
                        minionBodyPrefab = walkerBodyPrefab,
                        minionSkin = GetSkinFromTurretBody(walkerBodyPrefab, "matLazyBastard.mat", "EngiWalkerTurretMesh.mesh")
                    }
                }
            };

            //Adding new skin to a character's skin controller
            LoadoutAPI.AddSkinToCharacter(engiBodyPrefab, skin);
        }

        GameObject ModifyProjectileGhost(GameObject ghostPrefab, string material, string mesh)
        {
            Material newMat = skinBundle.LoadAsset<Material>(skinsPath + material);
            Mesh newMesh = skinBundle.LoadAsset<Mesh>(skinsPath + mesh);

            if (newMat == null || newMesh == null || ghostPrefab == null)
            {
                Debug.Log("New skin for " + ghostPrefab.name + " failed to load.");
                return ghostPrefab;
            }
            GameObject newGhost = ghostPrefab.InstantiateClone("LazyBastard" + ghostPrefab.name, true);
            newGhost.AddComponent<NetworkIdentity>();

            Renderer[] ghostRenderers = newGhost.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in ghostRenderers)
            {
                r.sharedMaterial = newMat;
            }
            foreach (MeshFilter m in newGhost.GetComponentsInChildren<MeshFilter>(true))
            {
                m.sharedMesh = newMesh;
            }
            SkinnedMeshRenderer[] ghostSkinnedRenderers = newGhost.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer s in ghostSkinnedRenderers)
            {
                s.sharedMesh = newMesh;
            }

            //engiMinePrefab.GetComponentInChildren<ProjectileController>().ghostPrefab = ghostPrefab;
            //projectilePrefabs.Add(engiMinePrefab);

            return newGhost;
        }

        GameObject NewProjectileGhost(GameObject ghostPrefab, string prefab)
        {
            GameObject newGhost = skinBundle.LoadAsset<GameObject>(skinsPath + prefab);

            if (newGhost == null)
            {
                Debug.Log("New ghost skin for " + ghostPrefab.name + " failed to load.");
                return ghostPrefab;
            }

            newGhost.AddComponent<NetworkIdentity>();
            newGhost.AddComponent<ProjectileGhostController>();

            return newGhost;
        }

        private SkinDef GetSkinFromTurretBody(GameObject bodyPrefab, string materialName, string meshName)
        {
            Material newMat = skinBundle.LoadAsset<Material>(skinsPath + materialName);
            Mesh newMesh = skinBundle.LoadAsset<Mesh>(skinsPath + meshName);

            if (newMat == null || newMesh == null || bodyPrefab == null)
            {
                Debug.Log("New skin for " + bodyPrefab.name + " failed to load.");
                return bodyPrefab.GetComponentInChildren<ModelSkinController>().skins[0];
            }

            Renderer[] renderers = bodyPrefab.GetComponentsInChildren<Renderer>(true);
            ModelSkinController skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
            GameObject mdl = skinController.gameObject;


            Debug.Log("Creating turret skin...");
            LoadoutAPI.SkinDefInfo skin = new LoadoutAPI.SkinDefInfo
            {
                Icon = skinIcon,
                Name = "LazyBastard" + bodyPrefab.name,
                NameToken = "FACTORIO_SKIN_" + bodyPrefab.name.ToUpper(),

                RootObject = mdl,
                BaseSkins = new SkinDef[] { skinController.skins[0] },
                UnlockableDef = null,
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                RendererInfos = new CharacterModel.RendererInfo[1]
                {
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = newMat,
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[0]
                    }
                },
                MeshReplacements = new SkinDef.MeshReplacement[1]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = newMesh,
                        renderer = renderers[0]
                    }
                },
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0]
            };

            SkinDef newSkin = LoadoutAPI.CreateNewSkinDef(skin);
            LoadoutAPI.AddSkinToCharacter(bodyPrefab, newSkin);

            return newSkin;
        }

        SkinDef GetNewSkinFromTurretBody(GameObject bodyPrefab, string material, string mesh)
        {
            Material newMat = skinBundle.LoadAsset<Material>(skinsPath + material);
            Mesh newMesh = skinBundle.LoadAsset<Mesh>(skinsPath + mesh);

            if (newMat == null || newMesh == null || bodyPrefab == null)
            {
                Debug.Log("New skin for " + bodyPrefab.name + " failed to load.");
                return bodyPrefab.GetComponentInChildren<ModelSkinController>().skins[0];
            }

            Renderer[] renderers = bodyPrefab.GetComponentsInChildren<Renderer>(true);
            ModelSkinController skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
            GameObject mdl = skinController.gameObject;

            LoadoutAPI.SkinDefInfo skin = new LoadoutAPI.SkinDefInfo
            {
                Icon = skinIcon,
                Name = "LazyBastard" + bodyPrefab.name,
                NameToken = "FACTORIO_SKIN_" + bodyPrefab.name.ToUpper(),

                RootObject = mdl,
                BaseSkins = new SkinDef[] { skinController.skins[0] },
                UnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                RendererInfos = new CharacterModel.RendererInfo[1]
                {
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = newMat,
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[0]
                    }
                },
                MeshReplacements = new SkinDef.MeshReplacement[1]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = newMesh,
                        renderer = renderers[0]
                    }
                },
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0]
            };

            LoadoutAPI.AddSkinToCharacter(bodyPrefab, skin);
            SkinDef newSkin = LoadoutAPI.CreateNewSkinDef(skin);

            //Debug.Log("FUCK!!! " + bodyPrefab.name);
            return newSkin;
        }

        //big thanks to sandman (???)
        public void SwapAllShaders(AssetBundle bundle)
        {
            Material[] array = bundle.LoadAllAssets<Material>();
            Material[] array2 = array;
            foreach (Material val in array2)
            {
                switch (val.shader.name)
                {
                    case "Stubbed Hopoo Games/Deferred/Standard":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgstandard");
                        break;
                    case "Stubbed Hopoo Games/Deferred/Snow Topped":
                        val.shader = Resources.Load<Shader>("shaders/deferred/hgsnowtopped");
                        break;
                    case "Stubbed Hopoo Games/FX/Cloud Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgcloudremap");
                        break;
                    case "Stubbed Hopoo Games/FX/Cloud Intersection Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgintersectioncloudremap");
                        break;
                    case "Stubbed Hopoo Games/FX/Opaque Cloud Remap":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgopaquecloudremap");
                        break;
                    case "Stubbed Hopoo Games/FX/Distortion":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgdistortion");
                        break;
                    case "Stubbed Hopoo Games/FX/Solid Parallax":
                        val.shader = Resources.Load<Shader>("shaders/fx/hgsolidparallax");
                        break;
                    case "Stubbed Hopoo Games/Environment/Distant Water":
                        val.shader = Resources.Load<Shader>("shaders/environment/hgdistantwater");
                        break;
                }
            }
        }
    }
}
