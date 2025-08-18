using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    [CreateAssetMenu(fileName = "NewCharacterSettings", menuName = "Advanced People Pack/Settings", order = 1)]
    public class CharacterSettings : ScriptableObject
    {
        public GameObject OriginalMesh;
        public Material bodyMaterial;
        [Space(20)]
        public List<CharacterAnimationPreset> characterAnimationPresets = new List<CharacterAnimationPreset>();
        [Space(20)]
        public List<CharacterBlendshapeData> characterBlendshapeDatas = new List<CharacterBlendshapeData>();
        [Space(20)]
        public List<CharacterElementsPreset> hairPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> beardPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> hatsPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> accessoryPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> shirtsPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> pantsPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> shoesPresets = new List<CharacterElementsPreset>();
        public List<CharacterElementsPreset> item1Presets = new List<CharacterElementsPreset>();
        [Space(20)]
        public List<CharacterSettingsSelector> settingsSelectors = new List<CharacterSettingsSelector>();
        [Space(20)]
        public RuntimeAnimatorController Animator;
        public Avatar Avatar;
        [Space(20)]
        public CharacterGeneratorSettings generator;

        [Space(20)]
        public CharacterSelectedElements DefaultSelectedElements = new CharacterSelectedElements();

        [Space(20)]
        public bool DisableBlendshapeModifier = false;
        
    }
    [System.Serializable]
    public class CharacterSettingsSelector
    {
        public string name;
        public CharacterSettings settings;
    }
    [System.Serializable]
    public class CharacterElementsPreset
    {
        public string name;
        public Mesh[] mesh;
        public string[] hideParts;
        public float yOffset = 0;
        public Material[] mats;
    }
    [System.Serializable]
    public class CharacterAnimationPreset
    {
        public string name;

        public List<BlendshapeEmotionValue> blendshapes = new List<BlendshapeEmotionValue>();

        public bool UseGlobalBlendCurve = true;
        public AnimationCurve GlobalBlendAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));
        [HideInInspector]
        public float AnimationPlayDuration = 1.0f;
        [HideInInspector]
        public float weightPower = 1.0f;
        [Header("May decrease performance")]
        public bool applyToAllCharacterMeshes;
    }
    [System.Serializable]
    public class CharacterBlendshapeData
    {
        public string blendshapeName;
        public CharacterBlendShapeType type;
        public CharacterBlendShapeGroup group;
        [HideInInspector]
        public float value;

        public CharacterBlendshapeData(string name, CharacterBlendShapeType t, CharacterBlendShapeGroup g, float value = 0f)
        {
            this.blendshapeName = name;
            this.type = t;
            this.group = g;
            this.value = value;
        }
        public CharacterBlendshapeData() { }
    }
    [System.Serializable]
    public class CharacterSelectedElements: ICloneable
    {
        public int Hair = -1;
        public int Beard = -1;
        public int Hat = -1;
        public int Shirt = -1;
        public int Pants = -1;
        public int Shoes = -1;
        public int Accessory = -1;
        public int Item1 = -1;

        public object Clone()
        {
            CharacterSelectedElements clone = new CharacterSelectedElements();
            clone.Hair = this.Hair;
            clone.Beard = this.Beard;
            clone.Hat = this.Hat;
            clone.Shirt = this.Shirt;
            clone.Pants = this.Pants;
            clone.Shoes = this.Shoes;
            clone.Accessory = this.Accessory;
            clone.Item1 = this.Item1;
            return clone;
        }

        public int GetSelectedIndex(CharacterElementType type)
        {
            switch (type)
            {
                case CharacterElementType.Hat:
                    return Hat;
                case CharacterElementType.Shirt:
                    return Shirt;
                case CharacterElementType.Pants:
                    return Pants;
                case CharacterElementType.Shoes:
                    return Shoes;
                case CharacterElementType.Accessory:
                    return Accessory;
                case CharacterElementType.Hair:
                    return Hair;
                case CharacterElementType.Beard:
                    return Beard;
                case CharacterElementType.Item1:
                    return Item1;
            }
            return -1;
        }
        public void SetSelectedIndex(CharacterElementType type, int newIndex)
        {
            switch (type)
            {
                case CharacterElementType.Hat:
                    Hat = newIndex;
                    break;
                case CharacterElementType.Shirt:
                    Shirt = newIndex;
                    break;
                case CharacterElementType.Pants:
                    Pants = newIndex;
                    break;
                case CharacterElementType.Shoes:
                    Shoes = newIndex;
                    break;
                case CharacterElementType.Accessory:
                    Accessory = newIndex;
                    break;
                case CharacterElementType.Hair:
                    Hair = newIndex;
                    break;
                case CharacterElementType.Beard:
                    Beard = newIndex;
                    break;
                case CharacterElementType.Item1:
                    Item1 = newIndex;
                    break;
            }
        }
    }

    public enum CharacterBlendShapeGroup
    {
        Body,
        Face,
        Extra
    }

    public enum CharacterBlendShapeType : int
    {
        Fat = 0,
        Muscles,
        Slimness,
        Thin,
        BreastSize,
        Neck_Width,
        Ear_Size,
        Ear_Angle,
        Jaw_Width,
        Jaw_Offset,
        Jaw_Shift,
        Cheek_Size,
        Chin_Offset,
        Eye_Width,
        Eye_Form,
        Eye_InnerCorner,
        Eye_Corner,
        Eye_Rotation,
        Eye_Offset,
        Eye_OffsetH,
        Eye_ScaleX,
        Eye_ScaleY,
        Eye_Size,
        Eye_Close,
        Eye_Height,
        Brow_Height,
        Brow_Shape,
        Brow_Thickness,
        Brow_Length,
        Nose_Length,
        Nose_Size,
        Nose_Angle,
        Nose_Offset,
        Nose_Bridge,
        Nose_Hump,
        Mouth_Offset,
        Mouth_Width,
        Mouth_Size,
        Mouth_Open,
        Mouth_Bulging,
        LipsCorners_Offset,
        Face_Form,
        Chin_Width,
        Chin_Form,
        Head_Offset,
        Smile,
        Sadness,
        Surprise,
        Thoughtful,
        Angry,
        BackpackOffset
    }

    public enum CharacterInstanceStatus
    {
        /// <summary>
        /// The object is not part of a Prefab instance.
        /// </summary>
        NotAPrefab,
        /// <summary>
        /// The Prefab instance is connected to its Prefab Asset.
        /// </summary>
        Connected,
        /// <summary>
        /// The Prefab instance is not connected to its Prefab Asset.
        /// </summary>
        Disconnected,
        /// <summary>
        /// The Prefab instance is missing its Prefab Asset.
        /// </summary>
        MissingAsset,
        /// <summary>
        /// The object is not part of a Prefab instance by user.
        /// </summary>
        NotAPrefabByUser,
        /// <summary>
        /// The object is prefab stage opened
        /// </summary>
        PrefabStageSceneOpened,
        /// <summary>
        /// The object editing in project view
        /// </summary>
        PrefabEditingInProjectView,
    }

    public class SavedCharacterData
    {
        public string name;
        public string path;
    }
}