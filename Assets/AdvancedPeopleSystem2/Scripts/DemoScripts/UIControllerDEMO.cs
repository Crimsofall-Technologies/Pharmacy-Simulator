using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AdvancedPeopleSystem;
using System.Collections.Generic;
using System;

namespace AdvancedPeopleSystem
{
    /// <summary>
    /// This script was created to demonstrate api, I do not recommend using it in your projects.
    /// </summary>
    public class UIControllerDEMO : MonoBehaviour
    {
        [Space(5)]
        [Header("I do not recommend using it in your projects")]
        [Header("This script was created to demonstrate api")]

        public CharacterCustomization CharacterCustomization;
        [Space(15)]

        public Text playbutton_text;

        public Text bake_text;
        public Text lod_text;

        public Text panelNameText;

        public Slider fatSlider;
        public Slider musclesSlider;
        public Slider thinSlider;

        public Slider slimnessSlider;
        public Slider breastSlider;

        public Slider heightSlider;

        public Slider legSlider;

        public Slider headSizeSlider;

        public Slider headOffsetSlider;

        public Slider[] faceShapeSliders;

        public RectTransform HairPanel;
        public RectTransform BeardPanel;
        public RectTransform ShirtPanel;
        public RectTransform PantsPanel;
        public RectTransform ShoesPanel;
        public RectTransform HatPanel;
        public RectTransform AccessoryPanel;
        public RectTransform BackpackPanel;

        public Image[] hairButtonImages;
        public Image[] beardButtonImages;
        public Image[] shirtButtonImages;
        public Image[] pantsButtonImages;
        public Image[] shoesButtonImages;
        public Image[] hatButtonImages;
        public Image[] accessoryButtonImages;
        public Image[] backpackButtonImages;

        public RectTransform FaceEditPanel;
        public RectTransform BaseEditPanel;

        public RectTransform SkinColorPanel;
        public RectTransform EyeColorPanel;
        public RectTransform HairColorPanel;
        public RectTransform UnderpantsColorPanel;

        public RectTransform EmotionsPanel;

        public RectTransform SavesPanel;
        public RectTransform SavesPanelList;
        public RectTransform SavesPrefab;
        public List<RectTransform> SavesList = new List<RectTransform>();

        public Image SkinColorButtonColor;
        public Image EyeColorButtonColor;
        public Image HairColorButtonColor;
        public Image UnderpantsColorButtonColor;

        public Vector3[] CameraPositionForPanels;
        public Vector3[] CameraEulerForPanels;
        int currentPanelIndex = 0;

        public Camera Camera;

        public RectTransform femaleUI;
        public RectTransform maleUI;

        public Color32 equipButtonDefault = new Color32(55, 55, 55, 164);
        public Color32 equipButtonSelected = new Color32(59, 118, 86, 164);


        private void Start()
        {
            if (CharacterCustomization.Settings.name == "FemaleSettings")
            {
                femaleUI.gameObject.SetActive(true);
                maleUI.gameObject.SetActive(false);
            }
            else
            {
                maleUI.gameObject.SetActive(true);
                femaleUI.gameObject.SetActive(false);
            }
            if (CharacterCustomization != null)
            {
                UpdateUI();
            }
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        #region ButtonEvents
        public void SwitchCharacterSettings(string name)
        {
            CharacterCustomization.SwitchCharacterSettings(name);
            if (name == "Male")
            {
                maleUI.gameObject.SetActive(true);
                femaleUI.gameObject.SetActive(false);
            }
            if (name == "Female")
            {
                femaleUI.gameObject.SetActive(true);
                maleUI.gameObject.SetActive(false);
            }
        }
        public void ShowFaceEdit()
        {
            FaceEditPanel.gameObject.SetActive(true);
            BaseEditPanel.gameObject.SetActive(false);
            currentPanelIndex = 1;
            panelNameText.text = "FACE CUSTOMIZER";
        }

        public void ShowBaseEdit()
        {
            FaceEditPanel.gameObject.SetActive(false);
            BaseEditPanel.gameObject.SetActive(true);
            currentPanelIndex = 0;
            panelNameText.text = "BASE CUSTOMIZER";
        }

        public void SetFaceShape(int index)
        {
            var faceBlendshape = CharacterCustomization.GetBlendshapeDatasByGroup(CharacterBlendShapeGroup.Face);


            CharacterCustomization.SetBlendshapeValue(faceBlendshape[index].type, faceShapeSliders[index].value);
        }

        public void SetHeadOffset()
        {
            CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Head_Offset, headOffsetSlider.value);
        }

        public void BodyFat()
        {
            CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, fatSlider.value);
        }
        public void BodyMuscles()
        {
            CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, musclesSlider.value);
        }
        public void BodyThin()
        {
            CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, thinSlider.value);
        }

        public void BodySlimness()
        {
            CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Slimness, slimnessSlider.value);
        }
        public void BodyBreast()
        {
            CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, breastSlider.value);
        }
        public void SetHeight()
        {
            CharacterCustomization.SetHeight(heightSlider.value);
        }
        public void SetHeadSize()
        {
            CharacterCustomization.SetHeadSize(headSizeSlider.value);
        }
        int lodIndex;
        public void Lod_Event(int next)
        {
            lodIndex += next;
            if (lodIndex < 0)
                lodIndex = 3;
            if (lodIndex > 3)
                lodIndex = 0;

            lod_text.text = lodIndex.ToString();

            CharacterCustomization.ForceLOD(lodIndex);
        }
        public void SetNewSkinColor(Color color)
        {
            SkinColorButtonColor.color = color;
            CharacterCustomization.SetBodyColor(BodyColorPart.Skin, color);
        }
        public void SetNewEyeColor(Color color)
        {
            EyeColorButtonColor.color = color;
            CharacterCustomization.SetBodyColor(BodyColorPart.Eye, color);
        }
        public void SetNewHairColor(Color color)
        {
            HairColorButtonColor.color = color;
            CharacterCustomization.SetBodyColor(BodyColorPart.Hair, color);
        }
        public void SetNewUnderpantsColor(Color color)
        {
            UnderpantsColorButtonColor.color = color;
            CharacterCustomization.SetBodyColor(BodyColorPart.Underpants, color);
        }
        public void VisibleSkinColorPanel(bool v)
        {
            HideAllPanels();
            SkinColorPanel.gameObject.SetActive(v);
        }
        public void VisibleEyeColorPanel(bool v)
        {
            HideAllPanels();
            EyeColorPanel.gameObject.SetActive(v);
        }
        public void VisibleHairColorPanel(bool v)
        {
            HideAllPanels();
            HairColorPanel.gameObject.SetActive(v);
        }
        public void VisibleUnderpantsColorPanel(bool v)
        {
            HideAllPanels();
            UnderpantsColorPanel.gameObject.SetActive(v);
        }
        public void ShirtPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                ShirtPanel.gameObject.SetActive(false);
            else
                ShirtPanel.gameObject.SetActive(true);
        }
        public void PantsPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                PantsPanel.gameObject.SetActive(false);
            else
                PantsPanel.gameObject.SetActive(true);
        }
        public void ShoesPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                ShoesPanel.gameObject.SetActive(false);
            else
                ShoesPanel.gameObject.SetActive(true);
        }
        public void BackpackPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                BackpackPanel.gameObject.SetActive(false);
            else
                BackpackPanel.gameObject.SetActive(true);
        }
        public void HairPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                HairPanel.gameObject.SetActive(false);
            else
                HairPanel.gameObject.SetActive(true);

            currentPanelIndex = (v) ? 1 : 0;
        }
        public void BeardPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                BeardPanel.gameObject.SetActive(false);
            else
                BeardPanel.gameObject.SetActive(true);

            currentPanelIndex = (v) ? 1 : 0;
        }
        public void HatPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                HatPanel.gameObject.SetActive(false);
            else
                HatPanel.gameObject.SetActive(true);
            currentPanelIndex = (v) ? 1 : 0;
        }
        public void EmotionsPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                EmotionsPanel.gameObject.SetActive(false);
            else
                EmotionsPanel.gameObject.SetActive(true);
            currentPanelIndex = (v) ? 1 : 0;
        }
        public void AccessoryPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
                AccessoryPanel.gameObject.SetActive(false);
            else
                AccessoryPanel.gameObject.SetActive(true);
            currentPanelIndex = (v) ? 1 : 0;
        }

        public void SavesPanel_Select(bool v)
        {
            HideAllPanels();
            if (!v)
            {
                SavesPanel.gameObject.SetActive(false);
                foreach (var save in SavesList)
                {
                    Destroy(save.gameObject);
                }
                SavesList.Clear();
            }
            else
            {
                var saves = CharacterCustomization.GetSavedCharacterDatas();
                for (int i = 0; i < saves.Count; i++)
                {
                    var savePrefab = Instantiate(SavesPrefab, SavesPanelList);
                    int index = i;
                    savePrefab.GetComponent<Button>().onClick.AddListener(() => SaveSelect(index));
                    savePrefab.GetComponentInChildren<Text>().text = string.Format("({0}) {1}", index, saves[i].name);
                    SavesList.Add(savePrefab);
                }
                SavesPanel.gameObject.SetActive(true);
            }
        }
        public void SaveSelect(int index)
        {
            var saves = CharacterCustomization.GetSavedCharacterDatas();
            CharacterCustomization.ApplySavedCharacterData(saves[index]);
            UpdateUI();
        }
        public void EmotionsChange_Event(int index)
        {
            var anim = CharacterCustomization.Settings.characterAnimationPresets[index];
            if (anim != null)
                CharacterCustomization.PlayBlendshapeAnimation(anim.name, 2f);
        }
        public void HairChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Hair, index);
        }
        public void BeardChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Beard, index);
        }
        public void ShirtChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Shirt, index);
        }
        public void PantsChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Pants, index);
        }
        public void ShoesChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Shoes, index);
        }
        public void BackpackChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Item1, index);
        }
        public void HatChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Hat, index);
        }

        public void AccessoryChange_Event(int index)
        {
            CharacterCustomization.SetElementByIndex(CharacterElementType.Accessory, index);
        }


        Dictionary<CharacterElementType, Image> selectedButtons = new Dictionary<CharacterElementType, Image>()
    {
        { CharacterElementType.Hair, null },
        { CharacterElementType.Beard, null },
        { CharacterElementType.Shirt, null },
        { CharacterElementType.Pants, null },
        { CharacterElementType.Shoes, null },
        { CharacterElementType.Hat, null },
        { CharacterElementType.Accessory, null },
        { CharacterElementType.Item1, null }
    };

        public void ClearEquipButton(CharacterElementType type)
        {
            Image buttonImage = null;
            if (selectedButtons.TryGetValue(type, out buttonImage))
            {
                if (buttonImage != null)
                    buttonImage.color = equipButtonDefault;
                selectedButtons[type] = null;
            }
        }

        public void ClearButtonHair()
        {
            ClearEquipButton(CharacterElementType.Hair);
        }
        public void ClearButtonBeard()
        {
            ClearEquipButton(CharacterElementType.Beard);
        }
        public void ClearButtonHat()
        {
            ClearEquipButton(CharacterElementType.Hat);
        }
        public void ClearButtonAccessory()
        {
            ClearEquipButton(CharacterElementType.Accessory);
        }
        public void ClearButtonShirt()
        {
            ClearEquipButton(CharacterElementType.Shirt);
        }
        public void ClearButtonPants()
        {
            ClearEquipButton(CharacterElementType.Pants);
        }
        public void ClearButtonShoes()
        {
            ClearEquipButton(CharacterElementType.Shoes);
        }
        public void ClearButtonBackpacks()
        {
            ClearEquipButton(CharacterElementType.Item1);
        }

        public void ToggleButtonHair(Image image)
        {
            toogleButton(CharacterElementType.Hair, image);
        }
        public void ToggleButtonBeard(Image image)
        {
            toogleButton(CharacterElementType.Beard, image);
        }
        public void ToggleButtonHat(Image image)
        {
            toogleButton(CharacterElementType.Hat, image);
        }
        public void ToggleButtonAccessory(Image image)
        {
            toogleButton(CharacterElementType.Accessory, image);
        }
        public void ToggleButtonShirt(Image image)
        {
            toogleButton(CharacterElementType.Shirt, image);
        }
        public void ToggleButtonPants(Image image)
        {
            toogleButton(CharacterElementType.Pants, image);
        }
        public void ToggleButtonShoes(Image image)
        {
            toogleButton(CharacterElementType.Shoes, image);
        }
        public void ToggleButtonBackpack(Image image)
        {
            toogleButton(CharacterElementType.Item1, image);
        }
        void toogleButton(CharacterElementType type, Image image)
        {
            ClearEquipButton(type);
            if (image != null)
            {
                selectedButtons[type] = image;
                selectedButtons[type].color = equipButtonSelected;
            }
        }

        void UpdateUIFor(CharacterElementType type)
        {
            int index = CharacterCustomization.characterSelectedElements.GetSelectedIndex(type);

            Image[] buttonImages = null;

            switch (type)
            {
                case CharacterElementType.Hat:
                    buttonImages = hatButtonImages;
                    break;
                case CharacterElementType.Shirt:
                    buttonImages = shirtButtonImages;
                    break;
                case CharacterElementType.Pants:
                    buttonImages = pantsButtonImages;
                    break;
                case CharacterElementType.Shoes:
                    buttonImages = shoesButtonImages;
                    break;
                case CharacterElementType.Accessory:
                    buttonImages = accessoryButtonImages;
                    break;
                case CharacterElementType.Hair:
                    buttonImages = hairButtonImages;
                    break;
                case CharacterElementType.Beard:
                    buttonImages = beardButtonImages;
                    break;
                case CharacterElementType.Item1:
                    buttonImages = backpackButtonImages;
                    break;
            }

            if (index == -1)
            {
                ClearEquipButton(type);
            }
            else
            {
                if (buttonImages.Length - 1 >= index)
                {
                    toogleButton(type, buttonImages[index]);
                }
            }
        }
        public void UpdateUI()
        {
            if (CharacterCustomization != null)
            {
                foreach (var type in Enum.GetValues(typeof(CharacterElementType)))
                {
                    UpdateUIFor((CharacterElementType)type);

                    SkinColorButtonColor.color = CharacterCustomization.GetBodyColor(BodyColorPart.Skin);
                    EyeColorButtonColor.color = CharacterCustomization.GetBodyColor(BodyColorPart.Eye);
                    HairColorButtonColor.color = CharacterCustomization.GetBodyColor(BodyColorPart.Hair);
                    UnderpantsColorButtonColor.color = CharacterCustomization.GetBodyColor(BodyColorPart.Underpants);
                }
                headSizeSlider.value = CharacterCustomization.headSizeValue;
                headOffsetSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.Head_Offset)?.value ?? 0;
                heightSlider.value = CharacterCustomization.heightValue;
                if (breastSlider != null)
                    breastSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.BreastSize)?.value ?? 0;

                if (fatSlider != null)
                    fatSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.Fat)?.value ?? 0;

                if (musclesSlider != null)
                    musclesSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.Muscles)?.value ?? 0;

                if (thinSlider != null)
                    thinSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.Thin)?.value ?? 0;

                if (slimnessSlider != null)
                    slimnessSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.Slimness)?.value ?? 0;

                if (breastSlider != null)
                    breastSlider.value = CharacterCustomization.GetBlendshapeData(CharacterBlendShapeType.BreastSize)?.value ?? 0;

                foreach (var facialSlider in faceShapeSliders)
                {
                    var blendshapeData = CharacterCustomization.GetBlendshapeData(facialSlider.transform.parent.name);
                    if (blendshapeData != null)
                    {
                        facialSlider.value = blendshapeData.value;
                    }
                }
            }
        }
        public void HideAllPanels()
        {
            SkinColorPanel.gameObject.SetActive(false);
            EyeColorPanel.gameObject.SetActive(false);
            HairColorPanel.gameObject.SetActive(false);
            UnderpantsColorPanel.gameObject.SetActive(false);
            if (EmotionsPanel != null)
                EmotionsPanel.gameObject.SetActive(false);
            if (BeardPanel != null)
                BeardPanel.gameObject.SetActive(false);
            HairPanel.gameObject.SetActive(false);
            ShirtPanel.gameObject.SetActive(false);
            PantsPanel.gameObject.SetActive(false);
            ShoesPanel.gameObject.SetActive(false);
            BackpackPanel.gameObject.SetActive(false);
            HatPanel.gameObject.SetActive(false);
            AccessoryPanel.gameObject.SetActive(false);
            SavesPanel.gameObject.SetActive(false);

            currentPanelIndex = 0;
        }
        public void SaveToFile()
        {
            CharacterCustomization.SaveCharacterToFile(CharacterCustomizationSetup.CharacterFileSaveFormat.Json);
        }
        public void ClearFromFile()
        {
            SavesPanel.gameObject.SetActive(false);
            CharacterCustomization.ClearSavedData();
        }
        public void Randimize()
        {
            CharacterCustomization.Randomize();

            UpdateUI();

        }

        bool walk_active = false;

        public void PlayAnim()
        {
            walk_active = !walk_active;

            CharacterCustomization.GetAnimator().SetBool("walk", walk_active);

            playbutton_text.text = (walk_active) ? "STOP" : "PLAY";
        }
        #endregion

        bool canvasVisible = true;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                canvasVisible = !canvasVisible;

                GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().enabled = canvasVisible;
            }
            Camera.transform.position = Vector3.Lerp(Camera.transform.position, CameraPositionForPanels[currentPanelIndex], Time.deltaTime * 5);
            Camera.transform.eulerAngles = Vector3.Lerp(Camera.transform.eulerAngles, CameraEulerForPanels[currentPanelIndex], Time.deltaTime * 5);
        }
    }
}