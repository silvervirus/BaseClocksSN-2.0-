#if DEBUG
#define LOG
#define kINCLUDE_TEST_BUILDABLES
#endif

//#define SKIP_DIGITAL

using System.Collections.Generic;
using System.Reflection;
using System.IO;

using HarmonyLib;
using UnityEngine;
using Nautilus;

using Nautilus.Handlers;

using UnityEngine.UI;
using Newtonsoft.Json;


using System;
using Nautilus.Crafting;
using Nautilus.Assets;
using System.Diagnostics.CodeAnalysis;

using TMPro;
using System.Text.RegularExpressions;
using BepInEx;
using System.Linq;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using Debug = UnityEngine.Debug;
using static CraftData;
using BaseClocks_BZ;
using Unity.Collections.LowLevel.Unsafe;

namespace BaseClocks
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.snmodding.nautilus", BepInDependency.DependencyFlags.HardDependency)]
    public  class QPatch : BaseUnityPlugin
    {
        #region[Declarations]
        public const string
            MODNAME = "BaseClocks",
            AUTHOR = "Cookie",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";
        #endregion
        public static Harmony harmony;

        public static string s_ModPath
        {
            get;
            private set;
        }

        private const string k_ClassID = "ActualTimeAnalogueClock";
      
        
#if LOG && INCLUDE_TEST_BUILDABLES
        private const string k_ClassID_Materials = "MaterialBalls";
        private const string k_ClassID_TextureTest = "TextureTest";
#endif
        private const string k_ClassID_Digital = "ActualTimeDigitalClock";

        private static readonly string[] s_DefaultKeywords = new string[]
        {
            "MARMO_EMISSION",
            "MARMO_SPECMAP",
        };

       
        public void Awake()
        {
            
            Debug.Log("Patching base clocks");
            harmony = new Harmony("com.baseclocksbz.mod");
            BaseClocksConfig.Load();
            
            OptionsPanelHandler.RegisterModOptions(new BaseClocksModOptions());

            AssetBundle assetBundle = AssetBundle.LoadFromFile("./BepInEx/plugins/BaseClocks/clocks");
            AssetBundle assetBundleTMP = AssetBundle.LoadFromFile("./BepInEx/plugins/BaseClocks/clocks_tmp");

            s_ModPath = "./BepInEx/plugins/BaseClocks/";
            string signAssetPundlePath = null;
           
            var dirs = Directory.GetFiles(UnityEngine.Application.streamingAssetsPath, "sign.*", SearchOption.AllDirectories);
            Debug.Log($"[BaseClocks]Found {(dirs != null ? dirs.Length : 0)} potential sign asset bundle directory(s)");

            foreach (string dir in dirs)
            {
                if (dir.Contains("\\sign.prefab"))
                {
                    signAssetPundlePath = dir.Replace("\\", "/");
                    break;
                }
            }

            AssetBundle signAssetBundle = null;
            if (!string.IsNullOrEmpty(signAssetPundlePath))
            {
               signAssetBundle = AssetBundle.LoadFromFile(signAssetPundlePath);
            }
            
            bool safeToLoadDigitalClock = !string.IsNullOrEmpty(signAssetPundlePath) && signAssetBundle != null;
#if LOG
            foreach (string s in signAssetBundle.GetAllAssetNames())
            {
                Debug.Log(s);
            }
#endif


#if !SKIP_DIGITAL
            GameObject sign = signAssetBundle.LoadAsset<GameObject>("Assets/AddressableResources/Submarine/Build/Sign.prefab");
            Debug.Log($"[BaseClocks]Sign loaded: {sign != null}");
#if LOG
            foreach (var component in sign.GetComponentsInChildren<Component>(true))
            {
                Debug.Log(component.GetType().Name);
            }
#endif
            TMP_FontAsset signFontTmp = sign.GetComponentInChildren<TextMeshProUGUI>(true).font;

#endif
            Debug.Log("[BaseClocks]Finding Shaders");
            Shader marmosetUber = Shader.Find("MarmosetUBER");
            Material marmosetUberMat = new Material(marmosetUber);


#if LOG
            string desktopPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fullpath = string.Concat(desktopPath, "/MarmosetUBERProperties.txt");

            using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
            {
                PrintShaderProperty("_Color", marmosetUberMat, tw);
                PrintShaderProperty("_ReflectColor", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_Cube", marmosetUberMat, tw);
                PrintShaderProperty("_MainTex", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_MarmoSpecEnum", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_Roughness", marmosetUberMat, tw);
                PrintShaderProperty("_Glossiness", marmosetUberMat, tw);
                PrintShaderProperty("_Gloss", marmosetUberMat, tw);
                PrintShaderProperty("_Metal", marmosetUberMat, tw);
                PrintShaderProperty("_Metallic", marmosetUberMat, tw);
                PrintShaderProperty("_Metalness", marmosetUberMat, tw);
                PrintShaderProperty("_Metallicness", marmosetUberMat, tw);
                PrintShaderProperty("_ReflectColor", marmosetUberMat, tw);
                PrintShaderProperty("_Reflectivity", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_Spec", marmosetUberMat, tw);
                PrintShaderProperty("_SpecTex", marmosetUberMat, tw);
                PrintShaderProperty("_SpecColor", marmosetUberMat, tw);
                PrintShaderProperty("_SpecColor2", marmosetUberMat, tw);
                PrintShaderProperty("_SpecColor3", marmosetUberMat, tw);
                PrintShaderProperty("_SpecCubeIBL", marmosetUberMat, tw);
                PrintShaderProperty("_SpecInt", marmosetUberMat, tw);
                PrintShaderProperty("_SpecGlossMap", marmosetUberMat, tw);
                PrintShaderProperty("_Specular", marmosetUberMat, tw);
                PrintShaderProperty("_Shininess", marmosetUberMat, tw);
                PrintShaderProperty("_SpecularAmount", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_EnableGlow", marmosetUberMat, tw);
                PrintShaderProperty("_SIGMap", marmosetUberMat, tw);

                PrintShaderProperty("_AffectedByDayNightCycle", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_SelfIllumination", marmosetUberMat, tw);
                PrintShaderProperty("_EnableGlow", marmosetUberMat, tw);
                PrintShaderProperty("_GlowColor", marmosetUberMat, tw);
                PrintShaderProperty("_Illum", marmosetUberMat, tw);
                PrintShaderProperty("_GlowStrength", marmosetUberMat, tw);
                PrintShaderProperty("_GlowStrengthNight", marmosetUberMat, tw);

                tw.Line();

                PrintShaderProperty("_Fresnel", marmosetUberMat, tw);
                PrintShaderProperty("_FresnelFade", marmosetUberMat, tw);

                tw.Line();

                PrintShaderProperty("_BaseLight", marmosetUberMat, tw);
                PrintShaderProperty("_AO", marmosetUberMat, tw);
                tw.Close();
            }
#endif

            //Analogue clock
            Debug.Log("[BaseClocks]Getting analogueClockBuildable");
            GameObject analogueBaseClock = assetBundle.LoadAsset<GameObject>("Actual Time Analog Clock UGUI");
            Debug.Log("[BaseClocks]Patching analogueClockBuildable");

            Nautilus.Utility.PrefabUtils.AddBasicComponents( analogueBaseClock, k_ClassID,TechType.None,LargeWorldEntity.CellLevel.Near);

            ReplaceMaterialShader(analogueBaseClock, marmosetUber, true, true);

            ApplySkyApplier(analogueBaseClock);

            Constructable constructable = analogueBaseClock.AddComponent<Constructable>();

            constructable.allowedOnWall = true;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = false;
            constructable.allowedOutside = false;
            constructable.model = analogueBaseClock.transform.GetChild(0).gameObject;

            DestroyPhysicsComponents(analogueBaseClock);

            ConstructableBounds constructableBounds = analogueBaseClock.AddComponent<ConstructableBounds>();

            TechTag techTag = analogueBaseClock.AddComponent<TechTag>();
            BaseAnalogueClock actualTimeAnalogueClock = analogueBaseClock.AddComponent<BaseAnalogueClock>();

            actualTimeAnalogueClock.HourHand = analogueBaseClock.transform.GetChild(1).GetChild(1);
            actualTimeAnalogueClock.MinuteHand = analogueBaseClock.transform.GetChild(1).GetChild(2);
            actualTimeAnalogueClock.SecondHand = analogueBaseClock.transform.GetChild(1).GetChild(3);
            var renderer = actualTimeAnalogueClock.HourHand.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = new Color(Convert.ToByte(BaseClocksConfig.ClockFaceColor.r / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.g / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.b / 255f),1);
           
                
            var renderer1 = actualTimeAnalogueClock.MinuteHand.GetComponent<Renderer>();
            if (renderer1 != null)
                renderer1.material.color = new Color(Convert.ToByte(BaseClocksConfig.ClockFaceColor.r / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.g / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.b / 255f), 1);
          
                
            var renderer2 = actualTimeAnalogueClock.SecondHand.GetComponent<Renderer>();
            if (renderer2 != null)
                renderer2.material.color = new Color(Convert.ToByte(BaseClocksConfig.ClockFaceColor.r / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.g / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.b / 255f),1);
            var Clock = actualTimeAnalogueClock.GetComponentInChildren<MeshRenderer>(true);
            if (Clock != null)
                Clock.material.color = new Color(Convert.ToByte(BaseClocksConfig.ClockBodyColor.r /255f), Convert.ToByte(BaseClocksConfig.ClockBodyColor.g /255f), Convert.ToByte(BaseClocksConfig.ClockBodyColor.b /255f), 1);
           

            RecipeData techData = new RecipeData();
            techData.Ingredients.Add(new Ingredient(TechType.Titanium, 1));
            techData.Ingredients.Add(new Ingredient(TechType.CopperWire, 1));
            
            BaseClockBuildable analogueClockBuildable = new BaseClockBuildable("ActualTimeAnalogueClock", "Analogue Clock", "An Analogue clock.", "analogueClock", analogueBaseClock.gameObject);
            analogueClockBuildable.SetPdaGroupCategory(TechGroup.Miscellaneous, TechCategory.MiscHullplates);
            KnownTechHandler.AddRequirementForUnlock(analogueClockBuildable.Info.TechType, TechType.Peeper);
            analogueClockBuildable.SetGameObject(analogueBaseClock.gameObject);
            analogueClockBuildable.Register();
            Debug.Log("[BaseClocks]Patched analogueClockBuildable");

            //Digital clock
#if !SKIP_DIGITAL
            if (safeToLoadDigitalClock)
            {
                Debug.Log("[BaseClocks]Getting digitalClockBuildable");
                GameObject digitalBaseClock = assetBundleTMP.LoadAsset<GameObject>("Actual Time Digital Clock TMP");
                Debug.Log("[BaseClocks]Patching digitalClockBuildable");

                Nautilus.Utility.PrefabUtils.AddBasicComponents(digitalBaseClock.gameObject,
                                                                k_ClassID_Digital,
                                                                TechType.None,
                                                                LargeWorldEntity.CellLevel.Near); 

                ReplaceMaterialShader(digitalBaseClock, marmosetUber, true, true);

                ApplySkyApplier(digitalBaseClock);

                constructable = digitalBaseClock.AddComponent<Constructable>();

                constructable.allowedOnWall = true;
                constructable.allowedInSub = true;
                constructable.allowedOnGround = false;
                constructable.allowedOutside = false;
                constructable.model = digitalBaseClock.transform.GetChild(0).gameObject;

                DestroyPhysicsComponents(digitalBaseClock);

                constructableBounds = digitalBaseClock.AddComponent<ConstructableBounds>();

                techTag = digitalBaseClock.AddComponent<TechTag>();

                BaseDigitalClockTMP digitalClock = digitalBaseClock.AddComponent<BaseDigitalClockTMP>();
                digitalClock.Text = digitalBaseClock.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                digitalClock.SetFont(signFontTmp);
                var renderer4 = digitalClock.Text.GetComponent<TextMeshProUGUI>();
                if (renderer2 != null)
                    digitalClock.Text.color = new Color(Convert.ToByte(BaseClocksConfig.ClockFaceColor.g / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.g / 255f), Convert.ToByte(BaseClocksConfig.ClockFaceColor.b / 255f), 1);

                var DClock = digitalBaseClock.GetComponentInChildren<Renderer>(true);
                if (DClock != null)
                    DClock.materials[0].color = new Color(Convert.ToByte(BaseClocksConfig.ClockBodyColor.r / 255f), Convert.ToByte(BaseClocksConfig.ClockBodyColor.g / 255f), Convert.ToByte(BaseClocksConfig.ClockBodyColor.b / 255f), 1);

                techData = new RecipeData();
                techData.Ingredients.Add(new Ingredient(TechType.Titanium, 1));
                techData.Ingredients.Add(new Ingredient(TechType.CopperWire, 1));

                LanguageHandler.SetLanguageLine(BaseClock.k_SetGameTime, "Set to Normal Time");
                LanguageHandler.SetLanguageLine(BaseClock.k_SetSystemTime, "Set to System Time");
                
                BaseClockBuildable digitalClockBuildable = new BaseClockBuildable("ActualTimeDigitalClock", "Digital Clock", "A Digital clock.", "digitalClock", digitalBaseClock.gameObject);
                digitalClockBuildable.SetPdaGroupCategory(TechGroup.Miscellaneous, TechCategory.MiscHullplates);
                KnownTechHandler.AddRequirementForUnlock(digitalClockBuildable.Info.TechType, TechType.Peeper);
                
                digitalClockBuildable.SetGameObject(digitalBaseClock.gameObject);
                digitalClockBuildable.Register();
                UnityEngine.Debug.Log("[BaseClocks]Patched digitalClockBuildable");
            }
            else
            {
                Debug.Log("[BaseClocks]Digital clock patching skipped as doing so will result in a crash");
            }
#endif

#if INCLUDE_TEST_BUILDABLES
            //Material balls.
            techType = TechTypePatcher.AddTechType(k_ClassID_Materials, "Material Balls", "Material Test");

            GameObject materialBalls = assetBundle.LoadAsset<GameObject>("Material Balls");

            Utility.AddBasicComponents(ref materialBalls, k_ClassID_Materials);
            DestroyPhysicsComponents(materialBalls);

            constructable = materialBalls.AddComponent<Constructable>();

            constructable.allowedOnWall = false;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOnConstructables = false;
            constructable.allowedOutside = true;
            constructable.model = materialBalls.transform.GetChild(0).gameObject;

            constructable.name = "Material Balls";

            constructableBounds = materialBalls.AddComponent<ConstructableBounds>();

            techTag = materialBalls.AddComponent<TechTag>();
            techTag.type = techType;

            ReplaceMaterialShader(materialBalls, marmosetUber, false, true);
            AddSkyApplier(materialBalls);

            materialBalls.AddComponent<MaterialLogger>();

            CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(k_ClassID_Materials, "Submarine/Build/MaterialBalls", materialBalls, techType));

            techDataHelper = new TechDataHelper();
            techDataHelper._ingredients = new List<IngredientHelper>();
            techDataHelper._ingredients.Add(new IngredientHelper(TechType.Titanium, 1));
            techDataHelper._techType = techType;

            CraftDataPatcher.customTechData.Add(techType, techDataHelper);
            CraftDataPatcher.customBuildables.Add(techType);

            dictionary[TechGroup.InteriorModules][TechCategory.InteriorModule].Add(techType);

            //Texture test.
            techType = TechTypePatcher.AddTechType(k_ClassID_TextureTest, "Texture Test", "Texture Test");

            GameObject textureTest = assetBundle.LoadAsset<GameObject>("Texture Test");

            Utility.AddBasicComponents(ref textureTest, k_ClassID_TextureTest);
            DestroyPhysicsComponents(textureTest);

            constructable = textureTest.AddComponent<Constructable>();

            constructable.allowedOnWall = true;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = false;
            constructable.allowedOnCeiling = false;
            constructable.allowedOnConstructables = false;
            constructable.allowedOutside = false;
            constructable.model = textureTest.transform.GetChild(0).gameObject;

            constructable.name = "Texture Test";

            constructableBounds = textureTest.AddComponent<ConstructableBounds>();

            techTag = textureTest.AddComponent<TechTag>();
            techTag.type = techType;

            ReplaceMaterialShader(textureTest, marmosetUber, true, true);

            AddSkyApplier(textureTest);

            textureTest.AddComponent<MaterialLogger>();

            CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(k_ClassID_TextureTest, "Submarine/Build/TextureTest", textureTest, techType));

            techDataHelper = new TechDataHelper();
            techDataHelper._ingredients = new List<IngredientHelper>();
            techDataHelper._ingredients.Add(new IngredientHelper(TechType.Titanium, 1));
            techDataHelper._techType = techType;

            CraftDataPatcher.customTechData.Add(techType, techDataHelper);
            CraftDataPatcher.customBuildables.Add(techType);

            dictionary[TechGroup.InteriorModules][TechCategory.InteriorModule].Add(techType);
#endif
#if LOG
            //Print small locker objects and components to desktop.
            fullpath = string.Concat(desktopPath, "/FabricatorComponents.txt");

            GameObject fabricator = Resources.Load<GameObject>("Submarine/Build/Fabricator");
            GameObject medicalCabinet = Resources.Load<GameObject>("Submarine/Build/MedicalCabinet");

            if (fabricator != null)
            {
                using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
                {
                    PrintComponents(fabricator, tw);
                    tw.Close();
                }
                MaterialLogger.LogMaterialsToDesktop(fabricator);
            }

            fullpath = string.Concat(desktopPath, "/SignComponents.txt");

            if (sign != null)
            {
                using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
                {
                    PrintComponents(sign, tw);
                    tw.Close();
                }
            }

            fullpath = string.Concat(desktopPath, "/MedicalCabinetComponents.txt");
            if (medicalCabinet != null)
            {
                using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
                {
                    PrintComponents(medicalCabinet, tw);
                    tw.Close();
                }
                MaterialLogger.LogMaterialsToDesktop(medicalCabinet);
            }

            MaterialLogger.LogMaterialsToDesktop(analogueBaseClock);


           
            MonoBehaviour.Destroy(marmosetUberMat);
#endif
            //The asset bundle should be unloaded otherwise it breaks the sign ingame
            if (signAssetBundle != null)
            {
                signAssetBundle.Unload(true);
            }
        }

        public static string GetOldSaveDirectory()
        {
            return string.Concat(s_ModPath, "./BepInEx/plugins/BaseClocks_BZ/");
        }

        private static void ApplySkyApplier(GameObject gameObject)
        {
            SkyApplier skyApplier = gameObject.GetComponent<SkyApplier>();
            skyApplier.renderers = gameObject.GetComponentsInChildren<Renderer>();
        }

        private static List<Renderer> _Renderers = new List<Renderer>(32);
        private static void ReplaceMaterialShader(GameObject gameObject, Shader shader, bool emmisive = false, bool specular = false)
        {
            _Renderers.Clear();
            gameObject.GetComponentsInChildren<Renderer>(_Renderers);

            foreach (Renderer renderer in _Renderers)
            {
                renderer.sharedMaterial.shader = shader;

                if (emmisive)
                {
                    renderer.sharedMaterial.EnableKeyword("MARMO_EMISSION");
                }
                if (specular)
                {
                    renderer.sharedMaterial.EnableKeyword("MARMO_SPECMAP");
                }
            }
        }

        private static void DestroyPhysicsComponents(GameObject gameObject)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            WorldForces worldForces = gameObject.GetComponent<WorldForces>();
            GameObject.DestroyImmediate(rigidbody);
            GameObject.DestroyImmediate(worldForces);
        }

#if LOG
        private static void PrintShaderProperty(string property,Material material, System.IO.TextWriter textWriter)
        {
            textWriter.Write("Has ");
            textWriter.Write(property);
            textWriter.Write(" = ");

            bool hasProperty = material.HasProperty(property);

            textWriter.WriteLine(hasProperty);
        }

        static  List<MonoBehaviour> _MonoBehaviours = new List<MonoBehaviour>(32);
        static void PrintComponents(GameObject gameObject, System.IO.TextWriter textWriter, int indentation = 1)
        {
            _MonoBehaviours.Clear();
            gameObject.GetComponents<MonoBehaviour>(_MonoBehaviours);

            textWriter.WriteIndented(gameObject.name, indentation);
            textWriter.Write(":\n");

            indentation++;
            foreach (MonoBehaviour monoBehaviour in _MonoBehaviours)
            {
                PrintComponentFactory.PrintComponent(monoBehaviour, textWriter, indentation);
            }

            Transform transform = gameObject.transform;
            int children = transform.childCount;

            for (int i = 0; i < children; i++)
            {
                PrintComponents(transform.GetChild(i).gameObject, textWriter, indentation);
            }
        }
#endif
    }

    internal class BaseClockBuildable : CustomPrefab
    {
        private GameObject m_Prefab;
        
      
        private string m_IconName;
        [SetsRequiredMembers]
        public BaseClockBuildable(string classId, string displayName, string description, string iconName, GameObject prefab)
            : base(classId, displayName, description, RamuneLib.Utils.ImageUtils.GetSprite(iconName))
        {
            m_Prefab = prefab;
           ;
            m_IconName = iconName;
            this.SetPdaGroupCategory(TechGroup.Miscellaneous, TechCategory.Misc).SetBuildable();
            this.SetRecipe(new(new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Copper, 1)));
            SetGameObject(prefab);
            Register();
        }

        public string GetAssetsFolder()
        {
            return "./BepInEx/plugins/BaseClocks/Assets/";
        }

        public string IconFileName => m_IconName;

        public  TechType RequiredForUnlock => TechType.Peeper;

        public  GameObject GetGameObject()
        {
            return m_Prefab;
        }

       
    }
}
