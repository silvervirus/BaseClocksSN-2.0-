using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClocks_BZ
{
    using BaseClocks;
    using BaseClocks.Config;
    using Nautilus.Options;
    using Debug = UnityEngine.Debug;

    public class MyModOptions : ModOptions

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
        private readonly ColorPreset[] m_Presets = new ColorPreset[]
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

        private string[] m_DigitalFormatChoiceStrings;
        private string[] m_ColorPresetsChoiceStrings;
        private Dictionary<string, ColorPreset> m_NameToPreset;
        private Transform m_BaseClocksHeaderTransform;
        private Dictionary<string, Component> m_IdToControl;
        private bool m_Syncronizing = false;
        public MyModOptions() : base("Base Clocks")
        {
            OnChanged += Options_Changed;

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
                Debug.Log($"[BaseClocks]{colorPreset.DisplayName} added");
            }

            colorPresetsChoices.Add("Custom");
            m_ColorPresetsChoiceStrings = colorPresetsChoices.ToArray();

            Debug.Log("[BaseClocks]Building Base Clocks Options");
            Color color = BaseClocksConfig.ClockFaceColor;
            AddItem(ModChoiceOption<string>.Create(k_DigitalClockFormatChoiceId, "Digital Clock Time Format", m_DigitalFormatChoiceStrings, (int)BaseClocksConfig.DigitalClockFormat));

            UnityEngine.Debug.Log("[BaseClocks]Format choice added");
            int presetIndex = Array.FindIndex(m_Presets, x => x.Color == color);
            if (presetIndex == -1)
            {
                presetIndex = m_ColorPresetsChoiceStrings.Length - 1;
            }

            Debug.Log("[BaseClocks]Preset Selected");
            AddItem(ModChoiceOption<string>.Create(k_ColorPresetChoiceId, "Clock Face Colour Preset", m_ColorPresetsChoiceStrings, presetIndex));
            var defaultColor = BaseClocksConfig.k_DefaultColor;
            AddItem(ModSliderOption.Create(k_ColorSliderRedId, "Red", 0, 1f, color.r, defaultColor.r, "{0:P0}", 0.01f));

            AddItem(ModSliderOption.Create(k_ColorSliderGreenId, "Green", 0, 1f, color.g, defaultColor.g, "{0:P0}", 0.1f));

            AddItem(ModSliderOption.Create(k_ColorSliderBlueId, "Blue", 0, 1f, color.b, defaultColor.b, "{0:P0}", 0.1f));
            
            uGUI_OptionsPanel optionsPanel = UnityEngine.Object.FindObjectOfType<uGUI_OptionsPanel>();
            Transform pane = optionsPanel.panesContainer.GetChild(FindChildWithText(optionsPanel.tabsContainer, "Mods").parent.GetSiblingIndex());
            m_BaseClocksHeaderTransform = FindChildWithText(pane.Find("Viewport").GetChild(0), this.Name).parent;

            Debug.Log("[BaseClocks]Options Added");
        }

            private void Options_Changed(object sender, OptionEventArgs e)
        {
            if (m_Syncronizing)
            {
                return;
            }
            switch (e)
            {

                case SliderChangedEventArgs sliderArgs:
                    switch (sliderArgs.Id)
                    {
                        case "BaseClocksClockColorR":
                            BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetRed(sliderArgs.Value);
                            SetPresetChoiceToCustom();
                            break;
                        case k_ColorSliderGreenId:
                            BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetGreen(sliderArgs.Value);
                            SetPresetChoiceToCustom();
                            break;
                        case k_ColorSliderBlueId:
                            BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetBlue(sliderArgs.Value);
                            SetPresetChoiceToCustom();
                            
                            break;
                    }
                    BaseClocksConfig.Save();
                    break;
                  

                case ChoiceChangedEventArgs<string> toggleArgs:
                    switch (toggleArgs.Id)
                    {
                        case k_DigitalClockFormatChoiceId:
                            DigitalClockFormat format = (DigitalClockFormat)toggleArgs.Index;
                            BaseClocksConfig.DigitalClockFormat = format;
                            break;
                        case k_ColorPresetChoiceId:
                            if (toggleArgs.Index < m_ColorPresetsChoiceStrings.Length - 1)
                            {
                                BaseClocksConfig.ClockFaceColor = m_NameToPreset[toggleArgs.Value].Color;
                                SyncronizeColorBars();
                                BaseClocksConfig.Save();

                            }
                            break;


                            
                    }
                    BaseClocksConfig.Save();
                    break;

                    
            }

            
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

        private void SetPresetChoiceToCustom()
        {
            CacheControls();

            m_Syncronizing = true;
            Debug.Log("[BaseClocks]Syncronizing");
            (m_IdToControl[k_ColorPresetChoiceId] as uGUI_Choice).value = m_ColorPresetsChoiceStrings.Length - 1;
            m_Syncronizing = false;
            Debug.Log("[BaseClocks]Syncronizing Finished");
            if (m_Syncronizing)
            {
                return;
            }
            BaseClocksConfig.Save();

        }


        private void SyncronizeColorBars()
        {
            if (m_IdToControl == null || m_IdToControl.ContainsValue(null))
            {
                CacheControls();
            }

            Color clockFace = BaseClocksConfig.ClockFaceColor;
            m_Syncronizing = true;
            (m_IdToControl[k_ColorSliderRedId] as Slider).value = clockFace.r;
            (m_IdToControl[k_ColorSliderGreenId] as Slider).value = clockFace.g;
            (m_IdToControl[k_ColorSliderBlueId] as Slider).value = clockFace.b;

            m_Syncronizing = false;
        }

        private void CacheControls()
        {
            m_IdToControl = new Dictionary<string, Component>();

            int headerIndex = m_BaseClocksHeaderTransform.GetSiblingIndex();
            m_IdToControl[k_ColorPresetChoiceId] = m_BaseClocksHeaderTransform.parent.GetChild(headerIndex + 2).GetComponentInChildren<uGUI_Choice>(true);
            m_IdToControl[k_ColorSliderRedId] = m_BaseClocksHeaderTransform.parent.GetChild(headerIndex + 3).GetComponentInChildren<Slider>(true);
            m_IdToControl[k_ColorSliderGreenId] = m_BaseClocksHeaderTransform.parent.GetChild(headerIndex + 4).GetComponentInChildren<Slider>(true);
            m_IdToControl[k_ColorSliderBlueId] = m_BaseClocksHeaderTransform.parent.GetChild(headerIndex + 5).GetComponentInChildren<Slider>(true);
        }
    }
}
