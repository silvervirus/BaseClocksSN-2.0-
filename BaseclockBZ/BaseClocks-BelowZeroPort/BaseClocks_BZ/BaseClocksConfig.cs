using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nautilus.Options;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Nautilus.Options;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace BaseClocks
{
    internal class OldBaseClocksConfig
    {
        public bool UseTwelveHourFormat = false;
        public string Color = "default";

        public static readonly Color32 k_DefaultColor = new Color32(115, 255, 252, 255);
        public const string k_OldConfigPath = "./QMods/BaseClocks/config.json";

        public Color GetActualColor()
        {
            string lowercaseColor = Color.ToLower();
            if (ColorUtility.TryParseHtmlString(lowercaseColor, out Color color))
            {
                return color;
            }
            else
            {
                return k_DefaultColor;
            }
        }
    }

    [Serializable]
    internal class BaseClocksConfig
    {
        private static BaseClocksConfig m_Instance;
        public static readonly Color32 k_DefaultColor = new Color32(115, 255, 252, 255);
        private const string k_ConfigPlayerPrefsKey = "BaseClocksConfig";

        [JsonProperty] private DigitalClockFormat m_DigitalClockFormat;
        [JsonProperty] private Color m_ClockFaceColor;

        public static event EventHandler<DigitalClockFormat> OnFormatChanged;
        public static event EventHandler<Color> OnFaceColorChanged;

        private JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new IgnorePropertiesContractResolver()
        };

        public static DigitalClockFormat DigitalClockFormat
        {
            get
            {
                return m_Instance.m_DigitalClockFormat;
            }
            set
            {
                m_Instance.m_DigitalClockFormat = value;
                OnFormatChanged?.Invoke(m_Instance, value);
            }
        }

        public static Color ClockFaceColor
        {
            get
            {
                return m_Instance.m_ClockFaceColor;
            }
            set
            {
                if (m_Instance.m_ClockFaceColor != value)
                {
                    m_Instance.m_ClockFaceColor = value;
                    OnFaceColorChanged?.Invoke(m_Instance, value);
                }
            }
        }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(k_ConfigPlayerPrefsKey))
            {
                string json = PlayerPrefs.GetString(k_ConfigPlayerPrefsKey, string.Empty);
                JsonSerializerSettings settings = new JsonSerializerSettings();

                m_Instance = JsonConvert.DeserializeObject<BaseClocksConfig>(json);
            }
            else
            {
                //Port the old data
                if (File.Exists(OldBaseClocksConfig.k_OldConfigPath))
                {
                    OldBaseClocksConfig oldConfig = JsonConvert.DeserializeObject<OldBaseClocksConfig>(File.ReadAllText(OldBaseClocksConfig.k_OldConfigPath));
                    if (oldConfig != null)
                    {
                        m_Instance = new BaseClocksConfig();
                        DigitalClockFormat = oldConfig.UseTwelveHourFormat ? DigitalClockFormat.TWELVE_HOUR : DigitalClockFormat.TWENTY_FOUR_HOUR;
                        ClockFaceColor = oldConfig.GetActualColor();

                        File.Delete(OldBaseClocksConfig.k_OldConfigPath);
                        return;
                    }
                }

                m_Instance = new BaseClocksConfig();
                SetToDefaults();
            }

        }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(m_Instance, m_Instance.SerializerSettings);
            Debug.Log($"Serialised: {json}");
            PlayerPrefs.SetString(k_ConfigPlayerPrefsKey, json);
        }

        public static void SetToDefaults()
        {
            DigitalClockFormat = DigitalClockFormat.TWELVE_HOUR;
            ClockFaceColor = k_DefaultColor;
            Save();
        }

        public class IgnorePropertiesContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = _ => ShouldSerialize(member);
                return property;
            }

            private bool ShouldSerialize(MemberInfo member)
            {
                return member.MemberType != MemberTypes.Property;
            }
        }
    }


    internal class BaseClocksModOptions : ModOptions
    {
        private struct ColorPreset
        {
            public string DisplayName;
            public Color Color;

            public ColorPreset(string displayName, Color color)
            {
                DisplayName = displayName;
                Color = color;
            }
        }

        private static readonly ColorPreset[] m_Presets = new ColorPreset[]
        {
            new ColorPreset("Black", Color.black),
            new ColorPreset("Blue", Color.blue),
            new ColorPreset("Cyan", Color.cyan),
            new ColorPreset("Default", BaseClocksConfig.k_DefaultColor),
            new ColorPreset("Grey", Color.gray),
            new ColorPreset("Green", Color.green),
            new ColorPreset("Magenta", Color.magenta),
            new ColorPreset("Red", Color.red),
            new ColorPreset("White", Color.white),
            new ColorPreset("Yellow", Color.yellow)
        };

        private const string k_DigitalClockFormatChoiceId = "BaseClocksDigitalTimeFormat";
        private const string k_ColorPresetChoiceId = "BaseClocksColorPreset";
        private const string k_ColorSliderRedId = "BaseClocksClockColorR";
        private const string k_ColorSliderGreenId = "BaseClocksClockColorG";
        private const string k_ColorSliderBlueId = "BaseClocksClockColorB";
        public const string k_Syncronizing = "Syncronizing";

        private string[] m_DigitalFormatChoiceStrings;
        private string[] m_ColorPresetsChoiceStrings;
        private Dictionary<string, ColorPreset> m_NameToPreset;

        private Transform m_BaseClocksHeaderTransform;
        private Dictionary<string, Component> m_IdToControl;
        public static bool m_Syncronizing = false;

        public BaseClocksModOptions() : base("Base Clocks")
        {
            DigitalClockFormat[] clockFormats = (DigitalClockFormat[])Enum.GetValues(typeof(DigitalClockFormat));
            m_DigitalFormatChoiceStrings = new string[clockFormats.Length];
            for (int i = 0; i < m_DigitalFormatChoiceStrings.Length; i++)
            {
                m_DigitalFormatChoiceStrings[i] = clockFormats[i].ToDisplayString();
            }

            List<string> colorPresetsChoices = new List<string>();
            m_NameToPreset = new Dictionary<string, ColorPreset>();
            foreach (ColorPreset colorPreset in m_Presets)
            {
                colorPresetsChoices.Add(colorPreset.DisplayName);
                m_NameToPreset[colorPreset.DisplayName] = colorPreset;
            }

            colorPresetsChoices.Add("Custom");
            m_ColorPresetsChoiceStrings = colorPresetsChoices.ToArray();



            Color color = BaseClocksConfig.ClockFaceColor;
            var time = ModChoiceOption<string>.Create(k_DigitalClockFormatChoiceId, "Digital Clock Time Format", m_DigitalFormatChoiceStrings, (int)BaseClocksConfig.DigitalClockFormat);
            time.OnChanged += OnChoiceChanged;
            AddItem(time);
            var defaultColor = BaseClocksConfig.k_DefaultColor;
            var red = ModSliderOption.Create(k_ColorSliderRedId, "Red", 0, 255, color.r, 0);
            red.OnChanged += OnSliderChanged;
            AddItem(red);
            var green = ModSliderOption.Create(k_ColorSliderGreenId, "Green", 0, 255, color.g, 0);
            green.OnChanged += OnSliderChanged;
            AddItem(green);
            var blue = ModSliderOption.Create(k_ColorSliderBlueId, "Blue", 0, 255, color.b, 0);
            blue.OnChanged += OnSliderChanged;
            AddItem(blue);





        }


        private void OnSliderChanged(object sender, SliderChangedEventArgs e)
        {
            switch (e.Id)
            {
                case k_ColorSliderRedId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetRed(e.Value);

                    break;
                case k_ColorSliderGreenId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetGreen(e.Value);

                    break;
                case k_ColorSliderBlueId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetBlue(e.Value);

                    break;
            }

            BaseClocksConfig.Save();
        }

        private void OnChoiceChanged(object sender, ChoiceChangedEventArgs<string> e)
        {
            if (m_Syncronizing)
            {
                return;
            }

            switch (e.Id)
            {
                case k_DigitalClockFormatChoiceId:
                    DigitalClockFormat format = (DigitalClockFormat)e.Index;
                    BaseClocksConfig.DigitalClockFormat = format;
                    break;

            }

            BaseClocksConfig.Save();
        }

        public void BuildModOptions()
        {

        }

        private Transform FindChildWithText(Transform root, string text)
        {
            int index = -1;

            for (int i = 0; i < root.childCount; i++)
            {
                Text textComponent = root.GetChild(i).GetComponentInChildren<Text>(true);
                if (textComponent?.text == text)
                {
                    index = i;
                    return textComponent.transform;
                }
            }

            return null;
        }






    }
}