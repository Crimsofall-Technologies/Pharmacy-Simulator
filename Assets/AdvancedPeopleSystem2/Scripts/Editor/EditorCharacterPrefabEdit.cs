using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AdvancedPeopleSystem
{

    [CustomEditor(typeof(CharacterCustomization))]
    public class EditorCharacterPrefabEdit : Editor
    {

        bool bodyShape = false;
        bool faceShape = false;
        bool extraShape = false;
        GUIStyle GUIStyle;
        GUIStyle GUIStyleReset;
        GUIStyle GUIStyleSave;


        /*Color SkinColorNew = Color.black;
        Color HairColorNew = Color.black;
        Color EyeColorNew = Color.black;
        Color UnderpantsColorNew = Color.black;*/

        List<string> hairList = new List<string>();
        List<string> beardList = new List<string>();
        List<string> hatList = new List<string>();
        List<string> accessoryList = new List<string>();
        List<string> shirtList = new List<string>();
        List<string> pantsList = new List<string>();
        List<string> shoesList = new List<string>();
        List<string> item1List = new List<string>();

        int selectedEmotionTest = 0;
        List<string> emotionsList = new List<string>();

        float minLod = 0;
        float maxLod = 3;

       // CharacterPreBuilt combinedCharacter;

        float headSize;
        float height;


        Material bodyColors;
        CharacterCustomization cc;


        SerializedObject settingsSerializedObject;
        SerializedProperty settings;
        SerializedProperty emotionPresets;
        SerializedProperty hairPresets;
        SerializedProperty beardPresets;
        SerializedProperty hatsPresets;
        SerializedProperty accessoryPresets;
        SerializedProperty shirtsPresets;
        SerializedProperty pantsPresets;
        SerializedProperty shoesPresets;

        SerializedProperty settingsSelector;

        SerializedProperty updateWhenOffscreenMeshes;
        SerializedProperty applyFeetOffset;

        SerializedProperty skinColor;
        SerializedProperty hairColor;
        SerializedProperty eyeColor;
        SerializedProperty underpantsColor;
        SerializedProperty teethColor;
        SerializedProperty oralCavityColor;

        SerializedProperty combinedCharacter;

        SerializedProperty autoPrefabApply;

        List<CharacterBlendshapeData> faceBlendshapes = new List<CharacterBlendshapeData>();
        List<CharacterBlendshapeData> bodyBlendshapes = new List<CharacterBlendshapeData>();

        List<CharacterBlendshapeData> extraBlendshapes = new List<CharacterBlendshapeData>();
        private void GetColors(CharacterCustomization cc)
        {
            if (bodyColors == null)
                bodyColors = cc.Settings.bodyMaterial;

        }

        void UpdateCallback()
        {
            if (cc.currentBlendshapeAnimation != null && !Application.isPlaying)
                cc.AnimationTick();
        }

        void OnEnable()
        {
            cc = (CharacterCustomization)target;
            
            EditorApplication.CallbackFunction callback = UpdateCallback;
            EditorApplication.update = System.Delegate.Combine(EditorApplication.update, callback) as EditorApplication.CallbackFunction;

            if (cc.selectedsettings != cc.Settings)
                cc.selectedsettings = cc.Settings;
            settings = serializedObject.FindProperty("selectedsettings");
            if(settings.objectReferenceValue != null)
                LoadSettingsProperties();

            cc.InitColors();
        }
        List<string> GetPresetList(CharacterElementType type)
        {
            switch (type)
            {
                case CharacterElementType.Hat:
                    return hatList;
                case CharacterElementType.Shirt:
                    return shirtList;
                case CharacterElementType.Pants:
                    return pantsList;
                case CharacterElementType.Shoes:
                    return shoesList;
                case CharacterElementType.Accessory:
                    return accessoryList;
                case CharacterElementType.Hair:
                    return hairList;
                case CharacterElementType.Beard:
                    return beardList;
                case CharacterElementType.Item1:
                    return item1List;
                default:
                    return new List<string>();
            }
        }
        void LoadSettingsProperties()
        {
            settingsSerializedObject = new SerializedObject(settings.objectReferenceValue);

            settingsSelector = settingsSerializedObject.FindProperty("settingsSelectors");
            emotionPresets = settingsSerializedObject.FindProperty("emotionPresets");
            hairPresets = settingsSerializedObject.FindProperty("hairPresets");
            beardPresets = settingsSerializedObject.FindProperty("beardPresets");
            hatsPresets = settingsSerializedObject.FindProperty("hatsPresets");
            accessoryPresets = settingsSerializedObject.FindProperty("accessoryPresets");
            shirtsPresets = settingsSerializedObject.FindProperty("shirtsPresets");
            pantsPresets = settingsSerializedObject.FindProperty("pantsPresets");
            shoesPresets = settingsSerializedObject.FindProperty("shoesPresets");
            combinedCharacter = serializedObject.FindProperty("combinedCharacter");
            updateWhenOffscreenMeshes = serializedObject.FindProperty("UpdateWhenOffscreenMeshes");
            applyFeetOffset = serializedObject.FindProperty("applyFeetOffset");

            skinColor = serializedObject.FindProperty("Skin");
            eyeColor = serializedObject.FindProperty("Eye");
            hairColor = serializedObject.FindProperty("Hair");
            underpantsColor = serializedObject.FindProperty("Underpants");
            teethColor = serializedObject.FindProperty("Teeth");
            oralCavityColor = serializedObject.FindProperty("OralCavity");

            autoPrefabApply = serializedObject.FindProperty("autoPrefabApply");
            if (cc.Settings != null)
            {
                if (!bodyColors)
                {
                    GetColors(cc);
                }
                //cc.StartupSerializationApply();

                foreach (var e in cc.Settings.characterAnimationPresets)
                {
                    emotionsList.Add(e.name);
                }
                UpdateOutfits();
                minLod = cc.MinLODLevels;
                maxLod = cc.MaxLODLevels;


                headSize = cc.headSizeValue;
                //headOffset = cc.GetBodyShapeWeight("Head_Offset");
                height = cc.heightValue;

                GetActualBlendshapes();
            }
        }
        void UpdateOutfits()
        {
            if (hairList.Count == 0)
            {
                foreach (var hair in cc.Settings.hairPresets)
                    hairList.Add(hair.name);
            }

            if (beardList.Count == 0)
            {
                foreach (var hair in cc.Settings.beardPresets)
                    beardList.Add(hair.name);
            }

            if (hatList.Count == 0)
            {
                foreach (var hat in cc.Settings.hatsPresets)
                    hatList.Add(hat.name);
            }
            if (accessoryList.Count == 0)
            {
                foreach (var accessory in cc.Settings.accessoryPresets)
                    accessoryList.Add(accessory.name);
            }

            if (shirtList.Count == 0)
            {
                foreach (var shirt in cc.Settings.shirtsPresets)
                    shirtList.Add(shirt.name);
            }

            if (pantsList.Count == 0)
            {
                foreach (var pants in cc.Settings.pantsPresets)
                    pantsList.Add(pants.name);
            }
            if (shoesList.Count == 0)
            {
                foreach (var shoes in cc.Settings.shoesPresets)
                    shoesList.Add(shoes.name);
            }
            if (item1List.Count == 0)
            {
                foreach (var item in cc.Settings.item1Presets)
                    item1List.Add(item.name);
            }
        }
        void GetActualBlendshapes()
        {
            faceBlendshapes.Clear();
            bodyBlendshapes.Clear();
            extraBlendshapes.Clear();
            foreach (var fb in cc.GetBlendshapeDatasByGroup(CharacterBlendShapeGroup.Face))
            {
                faceBlendshapes.Add(fb);
            }
            foreach (var bb in cc.GetBlendshapeDatasByGroup(CharacterBlendShapeGroup.Body))
            {
                bodyBlendshapes.Add(bb);
            }
            foreach (var eb in cc.GetBlendshapeDatasByGroup(CharacterBlendShapeGroup.Extra))
            {
                extraBlendshapes.Add(eb);
            }
        }
        void OnDisable()
        {
            EditorApplication.CallbackFunction callback = UpdateCallback;
            EditorApplication.update = System.Delegate.Remove(EditorApplication.update, callback) as EditorApplication.CallbackFunction;
        }

        int saveFormat = 0;
        string[] formatStrings = new string[] { "Json", "Xml", "Binary" };
        string[] formatExt = new string[] { "json", "xml", "bin" };
        int[] formatIndices = new int[] { 0, 1, 2 };
        public override void OnInspectorGUI()
        {

            if (cc.prefabPath == string.Empty || cc.prefabPath.Length == 0 && cc.instanceStatus != CharacterInstanceStatus.NotAPrefabByUser)
                cc.prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cc.gameObject);
            if (cc.prefabPath != string.Empty && cc.instanceStatus == CharacterInstanceStatus.NotAPrefabByUser)
                cc.prefabPath = string.Empty;

            cc.UpdateActualCharacterInstanceStatus();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Advanced People Pack v2.8.12", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView) 
            {
                EditorGUILayout.HelpBox("Some functionality limited in current edit prefab mode.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(settings, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.BeginDisabledGroup(cc.selectedsettings == null || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened);
            if (GUILayout.Button("LOAD/RESET"))
            {
                minLod = 0;
                maxLod = 3;
                cc.MinLODLevels = 0;
                cc.MaxLODLevels = 3;
                if (EditorUtility.DisplayDialog("Initialize character", "Are you sure you want to load new character meshes?\n!! Old meshes and rigs will be destroyed !!", "Yes", "No"))
                {
                    cc.InitializeMeshes();
                    cc.ApplyPrefab();
                    OnEnable();
                }
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("CLEAR CHARACTER"))
            {
                if (EditorUtility.DisplayDialog("Clear character", "Are you sure you want to clear character?\n!! The character hierarchy will be cleared !!", "Yes", "No"))
                {
                    cc.ClearCharacter();
                    cc.ApplyPrefab();
                    OnEnable();
                }
            }

            GUILayout.BeginVertical("GroupBox");
            EditorGUI.BeginDisabledGroup(cc.Settings == null);

            saveFormat = EditorGUILayout.IntPopup("Save format", saveFormat, formatStrings, formatIndices);

            GUILayout.Space(10);

            if (GUILayout.Button("Load Saved Characters"))
            {
                var saved = cc.GetSavedCharacterDatas();

                var listSaved = new List<string>();
                for (int i = 0; i < saved.Count; i++)
                    listSaved.Add(string.Format("{0} {1}", i, saved[i].name));

                DropdownSelectMenu(listSaved, -1, (index) =>
                {
                    cc.ApplySavedCharacterData(saved[index]);
                });
            }
            if (GUILayout.Button("Load character from custom directory"))
            {
                var path = EditorUtility.OpenFilePanel("Open saved character file", "C:/", "");
                if(path != string.Empty)
                {
                    cc.LoadCharacterFromFile(path);
                }
            }
            if (GUILayout.Button("Delete All Saved Characters"))
            {
                cc.ClearSavedData();
            }
            GUILayout.Space(20);

            if (cc.Settings != null)
            {
                if (GUILayout.Button(new GUIContent(" Save Character To File", EditorGUIUtility.FindTexture("d_SaveAs"))))
                {
                    CharacterCustomizationSetup.CharacterFileSaveFormat format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json;

                    if (saveFormat == 0)
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json;
                    }
                    else if (saveFormat == 1)
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Xml;
                    }
                    else if (saveFormat == 2)
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Binary;
                    }
                    
                    string path = EditorUtility.SaveFilePanel(string.Format("Save character to {0} file", formatStrings[saveFormat]), Application.dataPath, cc.gameObject.name, formatExt[saveFormat]);

                    string[] splitted = path.Split('/');
                    string savepath = "";
                    string savename = "";
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        if (i < splitted.Length - 1)
                            savepath += splitted[i] + "/";
                        else
                            savename = "appack25_" + splitted[i].Split('.')[0];
                    }

                    cc.SaveCharacterToFile(format, savepath, savename);
                }
                if (GUILayout.Button(new GUIContent(" Save Character to default folder", EditorGUIUtility.FindTexture("d_SaveAs"))))
                {
                    CharacterCustomizationSetup.CharacterFileSaveFormat format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json;

                    if (saveFormat == 0)
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json;
                    }
                    else if (saveFormat == 1)
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Xml;
                    }
                    else if (saveFormat == 2)
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Binary;
                    }

                    cc.SaveCharacterToFile(format);
                }

            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            if (cc.Settings != null)
            {
                GUILayout.BeginVertical("GroupBox");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(autoPrefabApply);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical("GroupBox");
            GUILayout.Label("Switch character settings");
            GUILayout.Space(10);
            if (cc.Settings != null)
            {
                if (settingsSelector != null)
                {
                    for (int i = 0; i < settingsSelector.arraySize; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(cc.Settings.settingsSelectors[i].name, GUILayout.MaxWidth(150));
                        if (GUILayout.Button("SELECT"))
                        {
                            cc.SwitchCharacterSettings(i);
                            cc.ApplyPrefab();
                            OnEnable();
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
            GUILayout.EndVertical();

            if (settings.objectReferenceValue == null)
                return;

            if (cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.NotAPrefab && cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.Disconnected && cc.Settings != null)
            {

                if (GUIStyle == null)
                {
                    GUIStyle = new GUIStyle();
                    GUIStyle.fontStyle = FontStyle.Bold;
                    GUIStyle.normal.textColor = Color.grey;
                }
                if (GUIStyleReset == null)
                {
                    GUIStyleReset = new GUIStyle(GUI.skin.button);
                    GUIStyleReset.fontStyle = FontStyle.Bold;
                    GUIStyleReset.normal.textColor = (EditorGUIUtility.isProSkin) ? new Color32(255, 187, 187, 255) : new Color32(185, 87, 87, 255);
                }
                if (GUIStyleSave == null)
                {
                    GUIStyleSave = new GUIStyle(GUI.skin.button);
                    GUIStyleSave.fontStyle = FontStyle.Bold;
                    GUIStyleSave.normal.textColor = (EditorGUIUtility.isProSkin)? new Color32(187, 187, 255, 255) : new Color32(115, 87, 185,255);
                }
                EditorGUILayout.Space();


                EditorGUI.BeginDisabledGroup(cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView);

                GUILayout.BeginVertical("GroupBox");
                selectedEmotionTest = EditorGUILayout.Popup("Select aniumation", selectedEmotionTest, emotionsList.ToArray());
                EditorGUILayout.Space();
                if (cc.currentBlendshapeAnimation != null)
                    EditorGUI.BeginDisabledGroup(cc.currentBlendshapeAnimation != null);
                if (GUILayout.Button((cc.currentBlendshapeAnimation != null) ? string.Format("Playback {0:0.00}", 1 - cc.currentBlendshapeAnimation.timer) : "Emotion Test"))
                {
                    cc.PlayBlendshapeAnimation(emotionsList[selectedEmotionTest], 2);
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndVertical();

                EditorGUI.BeginDisabledGroup(cc.IsBaked() || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView);
                GUILayout.BeginVertical("GroupBox");
                EditorGUILayout.LabelField("LOD Levels:", string.Format("LOD{0} - LOD{1}", minLod, maxLod));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.MinMaxSlider(ref minLod, ref maxLod, 0, 3);
                if (EditorGUI.EndChangeCheck())
                {
                    minLod = Mathf.RoundToInt(minLod);
                    maxLod = Mathf.RoundToInt(maxLod);
                }
                if (GUILayout.Button("Apply LOD Levels"))
                {
                    if (EditorUtility.DisplayDialog("Apply LOD Levels", "Are you sure you want to apply new LOD levels?\n!! Character settings will be reset !!", "Yes", "No"))
                    {
                        cc.UnlockPrefab();
                        cc.SetLODRange((int)minLod, (int)maxLod);

                        GetColors(cc);
                        headSize = 0;
                        height = 0;
                        GetActualBlendshapes();
                        cc.LockPrefab();
                        return;
                    }
                }
                EditorGUILayout.Space();

                GUILayout.BeginVertical("GroupBox");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(updateWhenOffscreenMeshes);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    cc.UpdateSkinnedMeshesOffscreenBounds();
                    EditorUtility.SetDirty(cc);
                    if (cc.autoPrefabApply)
                        cc.ApplyPrefab();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("GroupBox");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(applyFeetOffset);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (cc.autoPrefabApply)
                        cc.ApplyPrefab();
                }
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
                GUILayout.Label("EDIT PREFAB IN THE EDITOR", GUIStyle);
                if (GUILayout.Button((cc.isSettingsExpanded) ? new GUIContent(" SETTINGS", EditorGUIUtility.FindTexture("winbtn_mac_close")) : new GUIContent(" SETTINGS", EditorGUIUtility.FindTexture("winbtn_mac_max")), GUILayout.Height(50)))
                {
                    cc.isSettingsExpanded = !cc.isSettingsExpanded;
                }

                if (cc.isSettingsExpanded)
                {
                    GUILayout.BeginVertical("GroupBox");
                    GUILayout.Space(10);
                    var level = EditorGUI.indentLevel;
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(cc.notAPP2Shader);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(skinColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        cc.SetBodyColor(BodyColorPart.Skin, cc.Skin);
                    }
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(eyeColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        cc.SetBodyColor(BodyColorPart.Eye, cc.Eye);
                    }
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(hairColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        cc.SetBodyColor(BodyColorPart.Hair, cc.Hair);
                    }
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(underpantsColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        cc.SetBodyColor(BodyColorPart.Underpants, cc.Underpants);
                    }
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(teethColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        cc.SetBodyColor(BodyColorPart.Teeth, cc.Teeth);
                    }
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(oralCavityColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        cc.SetBodyColor(BodyColorPart.OralCavity, cc.OralCavity);
                    }

                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("Reset Body Colors"))
                    {
                        cc.ResetBodyColors();
                        GetColors(cc);
                        if (cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(cc.IsBaked());
                    
                    

                    void DrawElement(CharacterElementType type, string name)
                    {
                        GUILayout.BeginHorizontal(name, "window");
                        if (GUILayout.Button(string.Format("Select ({0})", (cc.characterSelectedElements.GetSelectedIndex(type) == -1) ? "None" : (cc.GetElementsPreset(type, cc.characterSelectedElements.GetSelectedIndex(type)).name) ?? "None"), GUILayout.Height(25)))
                        {
                            DropdownSelectMenu(GetPresetList(type), cc.characterSelectedElements.GetSelectedIndex(type),(index) =>
                            {
                                cc.SetElementByIndex(type, index);
                                if (cc.autoPrefabApply)
                                    cc.ApplyPrefab();
                            });
                        }
                        EditorGUI.BeginDisabledGroup(cc.characterSelectedElements.GetSelectedIndex(type) == -1);
                        if (GUILayout.Button(EditorGUIUtility.FindTexture("TreeEditor.Trash"), GUILayout.Width(50), GUILayout.Height(25)))
                        {
                            cc.ClearElement(type);
                            if (cc.autoPrefabApply)
                                cc.ApplyPrefab();
                        }
                        EditorGUI.EndDisabledGroup();
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);

                    }
                    GUILayout.BeginVertical("GroupBox");
                    
                    DrawElement(CharacterElementType.Hair, "Hair");
                    if(cc.Settings != null && cc.Settings.beardPresets.Count > 0)
                    DrawElement(CharacterElementType.Beard, "Beard");
                    DrawElement(CharacterElementType.Accessory, "Accessory");
                    DrawElement(CharacterElementType.Hat, "Hat");
                    DrawElement(CharacterElementType.Shirt, "Shirt");
                    DrawElement(CharacterElementType.Pants, "Pants");
                    DrawElement(CharacterElementType.Shoes, "Shoes");
                    DrawElement(CharacterElementType.Item1, "Item1");
                    GUILayout.Space(-10);
                    GUILayout.EndVertical();
                    EditorGUI.BeginChangeCheck();
                    headSize = EditorGUILayout.Slider("Head Size", headSize, -0.25f, 0.25f);
                    if (EditorGUI.EndChangeCheck())
                    {

                        cc.SetHeadSize(headSize);
                        if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened)
                        {
                            EditorUtility.SetDirty(cc);
                        }
                    }
                    EditorGUI.BeginChangeCheck();
                    height = EditorGUILayout.Slider("Height", height, -0.1f, 0.1f);
                    if (EditorGUI.EndChangeCheck())
                    {

                        cc.SetHeight(height);
                        if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened)
                        {
                            EditorUtility.SetDirty(cc);
                        }
                    }


                    EditorGUI.EndDisabledGroup();


                    EditorGUI.BeginDisabledGroup(bodyBlendshapes.Count == 0 || cc.IsBakedPreBuiltAndBakedBlendshapes());
                    bodyShape = EditorGUILayout.Foldout(bodyShape, "BODY BLENDSHAPES");
                    if (bodyShape)
                    {
                        EditorGUI.indentLevel++;
                        foreach(var body in bodyBlendshapes)
                        {
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            var blendshapeValue = (body.blendshapeName != "BreastSize") ? EditorGUILayout.Slider(body.blendshapeName, body.value, 0f, 100f) : EditorGUILayout.Slider(body.blendshapeName, body.value, -25f, 100f);
                            if(GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                blendshapeValue = 0;
                            }
                            
                            if (EditorGUI.EndChangeCheck())
                            {
                                try
                                {
                                    cc.SetBlendshapeValue(body.type, blendshapeValue);
                                    if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened)
                                    {
                                        EditorUtility.SetDirty(cc);
                                    }
                                }catch(Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(faceBlendshapes.Count == 0 || cc.IsBakedPreBuiltAndBakedBlendshapes());
                    faceShape = EditorGUILayout.Foldout(faceShape, "FACE BLENDSHAPES");
                    if (faceShape)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var face in faceBlendshapes)
                        {
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            var blendshapeValue = EditorGUILayout.Slider(face.blendshapeName, face.value, -100f, 100f);
                            if (GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                blendshapeValue = 0;

                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                cc.SetBlendshapeValue(face.type, blendshapeValue);
                                if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened)
                                {
                                    EditorUtility.SetDirty(cc);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(extraBlendshapes.Count == 0 || cc.IsBakedPreBuiltAndBakedBlendshapes());
                    extraShape = EditorGUILayout.Foldout(extraShape, "EXTRA BLENDSHAPES");
                    if (extraShape)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var extra in extraBlendshapes)
                        {
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            var blendshapeValue = EditorGUILayout.Slider(extra.blendshapeName, extra.value, 0f, 150f);
                            if (GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                blendshapeValue = 0;

                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                cc.SetBlendshapeValue(extra.type, blendshapeValue);
                                if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView || cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened)
                                {
                                    EditorUtility.SetDirty(cc);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                    EditorGUI.BeginDisabledGroup(cc.IsBaked());
                    if (GUILayout.Button("Randomize"))
                    {
                        cc.Randomize();
                        GetColors(cc);
                        height = cc.heightValue;
                        headSize = cc.headSizeValue;
                        GetActualBlendshapes();
                        if(cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Reset All", GUIStyleReset))
                    {
                        cc.ResetAll();

                        GetColors(cc);
                        headSize = 0;
                        height = 0;
                        GetActualBlendshapes();
                        if (cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    if (GUILayout.Button("Reset to default clothes", GUIStyleReset))
                    {
                        cc.ResetAll(false);

                        GetColors(cc);
                        headSize = 0;
                        height = 0;
                        GetActualBlendshapes();
                        if (cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();

                   

                    GUILayout.BeginVertical("GroupBox");

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(combinedCharacter, GUIContent.none);

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                    if (GUILayout.Button(EditorGUIUtility.FindTexture("TreeEditor.Trash"), GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        combinedCharacter.objectReferenceValue = null;
                        serializedObject.ApplyModifiedProperties();
                    }
                    GUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(combinedCharacter.objectReferenceValue == null);
                    if (GUILayout.Button("Load PreBuilt Meshes in character"))
                    {
                        cc.BakeCharacter(true);
                        if (cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(!cc.IsBaked());
                    if (GUILayout.Button("Clear Combined Character"))
                    {
                        EditorUtility.DisplayProgressBar("Clear Combine Character", "Process", 1f);
                        cc.ClearBake();
                        EditorUtility.ClearProgressBar();

                        OnEnable();
                    }
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndVertical();

                    if (GUILayout.Button("Save PreBuilt Prefab") && cc.combinedCharacter == null)
                    {
                        cc.EditorSavePreBuiltPrefab();
                    }
                    if (GUILayout.Button("Save PreBuilt Prefab (Bake Blendshapes)") && cc.combinedCharacter == null)
                    {
                        cc.EditorSavePreBuiltPrefab(true);
                    }
                    EditorGUI.BeginDisabledGroup(cc.IsBaked());


                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                    if (GUILayout.Button("Recalculate LODs"))
                    {
                        cc.RecalculateLOD();
                        if (cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    if (GUILayout.Button("Revert bones change"))
                    {
                        cc.RevertBonesChanges();
                        if (cc.autoPrefabApply)
                            cc.ApplyPrefab();
                    }
                    if (cc.GetCharacterInstanceStatus() == CharacterInstanceStatus.Connected)
                    {
                        if (!Application.isPlaying)
                        {
                            if (GUILayout.Button("Save & Apply To Prefab",GUIStyleSave))
                            {
                                cc.ApplyPrefab();
                                GUIUtility.ExitGUI();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Apply Prefab In PLAYMODE", GUIStyleSave))
                            {
                                cc.UnlockPrefab();
                                cc.ApplyPrefabInPlaymode();
                                GUIUtility.ExitGUI();
                            }
                        }
                    }

                    EditorGUI.indentLevel = level;
                    GUILayout.EndVertical();
                }
            }
            if (cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.Connected
                && cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.PrefabEditingInProjectView
                && cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.PrefabStageSceneOpened
                && !Application.isPlaying && cc.Settings != null)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = new Color32(150, 150, 150, 255);
                style.fontSize = 16;
                style.fontStyle = FontStyle.Bold;
                GUILayout.Space(20);
                if (cc.gameObject.scene.name != null)
                {
                    if (cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.NotAPrefabByUser)
                        GUILayout.Label(new GUIContent("Create prefab or use non prefab mode!", EditorGUIUtility.FindTexture("console.infoicon.inactive.sml@2x")), style);
                    if (GUILayout.Button(new GUIContent("Save as new prefab")))
                    {
                        string path = EditorUtility.SaveFilePanelInProject("Save prefab", cc.gameObject.name, "prefab", "Please enter a file name to save character prefab");
                        if (path != string.Empty)
                        {
                            cc.UnlockPrefab();
                            PrefabUtility.SaveAsPrefabAssetAndConnect(cc.gameObject, path, InteractionMode.UserAction);
                            cc.UpdateActualCharacterInstanceStatus(true);
                            cc.prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cc.gameObject);
                            cc.ApplyPrefab();
                        }
                    }
                    if (cc.GetCharacterInstanceStatus() != CharacterInstanceStatus.NotAPrefabByUser)
                    {
                        if (GUILayout.Button("Use System Without Prefab"))
                        {
                            //cs.InitVariables(true);
                            cc.SetNewCharacterInstanceStatus(CharacterInstanceStatus.NotAPrefabByUser);
                        }
                    }
                }
                else
                {
                    GUILayout.Label(new GUIContent("Open prefab or edit in scene!", EditorGUIUtility.FindTexture("console.infoicon.inactive.sml@2x")), style);
                }
                return;

            }
            //serializedObject.UpdateIfRequiredOrScript();
            //serializedObject.ApplyModifiedProperties();
            if(cc.instanceStatus == CharacterInstanceStatus.PrefabEditingInProjectView || cc.instanceStatus == CharacterInstanceStatus.PrefabStageSceneOpened)
                serializedObject.UpdateIfRequiredOrScript();
        }

        public void DropdownSelectMenu(List<string> menuItems, int selected, Action<int> callback)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var item in menuItems)
            {
                int i = menuItems.IndexOf(item);
                if (i == selected)
                {
                    menu.AddDisabledItem(new GUIContent(item));
                }
                else
                {
                    menu.AddItem(new GUIContent(item), false, (index) =>
                    {
                        callback?.Invoke((int)index);
                    }, i);
                }
            }
            menu.ShowAsContext();
        }
    }
}