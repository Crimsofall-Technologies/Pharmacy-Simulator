using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AdvancedPeopleSystem
{
    /// <summary>
    /// Class for customization character (v2.8.12)
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Advanced People Pack/Character Customizable", -1)]
    public class CharacterCustomization : MonoBehaviour
    {

        public static bool VerifyClothPreset = true;

        [SerializeField] public bool isSettingsExpanded = false;

        public CharacterSettings  selectedsettings;

        public CharacterSettings Settings => _settings;

        [SerializeField] private CharacterSettings _settings;

        public List<CharacterPart> characterParts = new List<CharacterPart>();

        public string prefabPath = string.Empty;

        [SerializeField] public CharacterInstanceStatus instanceStatus;

        public Transform originHip;

        public Transform headHip;

        public List<ClothesAnchor> clothesAnchors = new List<ClothesAnchor>();

        public Animator animator;

        public CharacterSelectedElements characterSelectedElements = new CharacterSelectedElements();

        public float heightValue = 0;
        public float headSizeValue = 0;
        public float feetOffset = 0;
        
        public List<CharacterBlendshapeData> characterBlendshapeDatas = new List<CharacterBlendshapeData>();

        public Color Skin,
        Eye,
        Hair,
        Underpants,
        OralCavity,
        Teeth;

        public MaterialPropertyBlock bodyPropertyBlock;

        public CurrentBlendshapeAnimation currentBlendshapeAnimation;

        public CombinerState CurrentCombinerState;

        public CharacterPreBuilt combinedCharacter;

        public Transform ProbesAnchorOverride;

        public CharacterGeneratorSettings CharacterGenerator_settings;

        public bool UpdateWhenOffscreenMeshes = true;

        [SerializeField]
        public int MinLODLevels = 0;
        [SerializeField]
        public int MaxLODLevels = 3;

        LODGroup _lodGroup;

        public Transform _transform;

        public bool applyFeetOffset = true;

        public bool notAPP2Shader = false;

        public bool autoPrefabApply = true;

        private bool preBuiltMeshesHasBlendshapes = true;

        private void Awake()
        {
            this._transform = transform;
            _lodGroup = GetComponent<LODGroup>();
            UpdateSkinnedMeshesOffscreenBounds();
        }

        private void Update()
        {
            AnimationTick();
        }

        private void LateUpdate()
        {
            if (feetOffset != 0 && applyFeetOffset)
                SetFeetOffset(new Vector3(0, feetOffset, 0));
        }
        /// <summary>
        /// Process face blendshape animation by 1 tick
        /// </summary>
        public void AnimationTick()
        {
            if (currentBlendshapeAnimation != null)
            {
                currentBlendshapeAnimation.timer += Time.deltaTime * currentBlendshapeAnimation.preset.AnimationPlayDuration;
                for (var i = 0; i < currentBlendshapeAnimation.preset.blendshapes.Count; i++)
                {
                    if (currentBlendshapeAnimation.preset.UseGlobalBlendCurve)
                        SetBlendshapeValue(currentBlendshapeAnimation.preset.blendshapes[i].BlendType, currentBlendshapeAnimation.preset.blendshapes[i].BlendValue * currentBlendshapeAnimation.preset.weightPower * currentBlendshapeAnimation.preset.GlobalBlendAnimationCurve.Evaluate(currentBlendshapeAnimation.timer));
                    else
                        SetBlendshapeValue(currentBlendshapeAnimation.preset.blendshapes[i].BlendType, currentBlendshapeAnimation.preset.blendshapes[i].BlendValue * currentBlendshapeAnimation.preset.weightPower * currentBlendshapeAnimation.preset.blendshapes[i].BlendAnimationCurve.Evaluate(currentBlendshapeAnimation.timer));
                }
                if (currentBlendshapeAnimation.timer >= 1.0f)
                    currentBlendshapeAnimation = null;
            }
        }

        #region Basic functions
        /// <summary>
        /// Switch character settings by selector index
        /// </summary>
        public void SwitchCharacterSettings(int settingsIndex)
        {
            if (Settings.settingsSelectors.Count - 1 >= settingsIndex)
            {
                var selector = Settings.settingsSelectors[settingsIndex];
                InitializeMeshes(selector.settings, true);
            }
        }
        /// <summary>
        /// Switch character settings by selector name
        /// </summary>
        public void SwitchCharacterSettings(string selectorName)
        {
            for(int i = 0; i < Settings.settingsSelectors.Count;i++)
            {
                if(Settings.settingsSelectors[i].name == selectorName)
                {
                    SwitchCharacterSettings(i);
                    return;
                }
            }
        }
        /// <summary>
        /// Clear character data and customization
        /// </summary>
        public void ClearCharacter()
        {
            _settings = null;
            UnlockPrefab();
            List<GameObject> objectsToDelete = new List<GameObject>();
            for (var i = 0; i < _transform.childCount; i++)
                objectsToDelete.Add(_transform.GetChild(i).gameObject);
            if (objectsToDelete.Count > 0)
                DestroyObjects(objectsToDelete.ToArray());

            characterParts.Clear();
            clothesAnchors.Clear();
            DestroyImmediate(_lodGroup);
            DestroyImmediate(animator);
            _lodGroup = null;
            animator = null;
            LockPrefab();
        }
        /// <summary>
        /// Init character by settings
        /// </summary>
        public void InitializeMeshes(CharacterSettings newSettings = null, bool resetAll = true)
        {
            _transform = transform;
            if(selectedsettings == null && newSettings == null)
            {
                Debug.LogError("_settings = null, Unable to initialize character");
            }
            else
            {
                if (resetAll && _settings != null)
                    ResetAll(true);
                _settings = (newSettings != null) ? newSettings : selectedsettings;

                if (newSettings != null)
                    selectedsettings = newSettings;
#if UNITY_EDITOR
                ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(_settings.OriginalMesh));

                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    Debug.LogErrorFormat("The loaded model <b>'{0}>'</b> must have <b><color=#206f38>(Humanoid Rig)</color></b>, your model has <b><color=#d63d3d>({1} Rig)</color></b>", _settings.OriginalMesh.name, importer.animationType.ToString());
                    return;
                }
#endif
            }
#if UNITY_EDITOR
            if (prefabPath == string.Empty || prefabPath.Length == 0)
               prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
#endif

            UnlockPrefab();
            List<GameObject> objectsToDelete = new List<GameObject>();
            for (var i = 0; i < _transform.childCount; i++)
                objectsToDelete.Add(_transform.GetChild(i).gameObject);
            if (objectsToDelete.Count > 0)
                DestroyObjects(objectsToDelete.ToArray());

            characterBlendshapeDatas.Clear();
            foreach (var blendshapeData in _settings.characterBlendshapeDatas)
            {
                characterBlendshapeDatas.Add(new CharacterBlendshapeData(blendshapeData.blendshapeName, blendshapeData.type, blendshapeData.group));
            }

            GameObject characterGO = UnityEngine.Object.Instantiate(_settings.OriginalMesh, _transform);
            characterGO.name = "Character";



            ProbesAnchorOverride = new GameObject("Probes Anchor").GetComponent<Transform>();
            ProbesAnchorOverride.parent = _transform;
            ProbesAnchorOverride.localPosition = Vector3.up * 1.5f;
            ProbesAnchorOverride.localRotation = Quaternion.identity;


            List<Transform> childTransforms = new List<Transform>();
            List<Transform> destroyList = new List<Transform>();
            for (int i = 0; i < characterGO.transform.childCount;i++)
            {
                var childObj = characterGO.transform.GetChild(i);
                childTransforms.Add(childObj);
            }

            characterParts.Clear();
            clothesAnchors.Clear();

            foreach(var obj in childTransforms)
            {
                obj.SetParent(_transform);
                var skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
                var Objparams = obj.name.Split('_');
                string objType = Objparams[0];
                string lodLevel = (Objparams.Length==3) ? Objparams[2] : "-";
                int lod = -1;
                Match match = Regex.Match(lodLevel, @"(\d+)");
                if (match.Success)
                {
                    lod = int.Parse(match.Groups[1].Value);
                }
                if(lod != -1 && lod < MinLODLevels || lod > MaxLODLevels)
                {
                    DestroyObjects(new GameObject[] { obj.gameObject });
                    continue;
                }

                if (skinnedMesh != null)
                {
                    skinnedMesh.updateWhenOffscreen = true;
                    skinnedMesh.probeAnchor = ProbesAnchorOverride;
                }

                if (objType != "ACCESSORY" && objType != "HAT" && objType != "PANTS" && objType != "SHIRT" && objType != "SHOES" && objType != "ITEM1" && objType.ToLowerInvariant() != "hips")
                {
                    if (skinnedMesh == null)
                        continue;

                    skinnedMesh.sharedMaterials = new Material[0];

                    if (objType == "COMBINED")
                        skinnedMesh.gameObject.SetActive(false);
                    else
                        skinnedMesh.sharedMaterials = new Material[] { _settings.bodyMaterial };

                    var characterPart = characterParts.Find(f => f.name == objType);
                    if (characterPart == null)
                    {
                        var newCharacterPart = new CharacterPart();
                        newCharacterPart.name = objType;
                        newCharacterPart.skinnedMesh.Add(skinnedMesh);
                        characterParts.Add(newCharacterPart);
                    }
                    else
                    {
                        characterPart.skinnedMesh.Add(skinnedMesh);
                    }
                }else if(objType.ToLowerInvariant() == "hips")
                {
                    obj.SetSiblingIndex(0);

                    originHip = obj;

                    var allHips = originHip.GetComponentsInChildren<Transform>();

                    headHip = allHips.First(f => f.name.ToLowerInvariant() == "head");

                }
                else if(objType == "HAT" || objType == "SHIRT" || objType == "PANTS" || objType == "SHOES" || objType == "ACCESSORY" || objType == "ITEM1")
                {
                    if (skinnedMesh == null)
                        continue;

                    skinnedMesh.gameObject.SetActive(false);
                    var clothesAnchor = clothesAnchors.Find(f => f.partType.ToString().ToLowerInvariant() == objType.ToLowerInvariant());
                    if(clothesAnchor == null)
                    {
                        var newClothesAnchor = new ClothesAnchor();

                        newClothesAnchor.partType = (CharacterElementType)Enum.Parse(typeof(CharacterElementType), objType.ToLowerInvariant(), true);
                        newClothesAnchor.skinnedMesh.Add(skinnedMesh);
                        clothesAnchors.Add(newClothesAnchor);
                    }
                    else
                    {
                        clothesAnchor.skinnedMesh.Add(skinnedMesh);
                    }
                }

            }

            DestroyObjects(new GameObject[] { characterGO });
            _lodGroup = GetComponent<LODGroup>();
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
            if(_lodGroup != null && MinLODLevels == MaxLODLevels)
            {
                DestroyImmediate(_lodGroup);
            }
            else
            {
                if(_lodGroup == null)
                _lodGroup = gameObject.AddComponent<LODGroup>();
            }
            animator.avatar = _settings.Avatar;
            animator.runtimeAnimatorController = _settings.Animator;
            animator.Rebind();

            ResetAll(false);
            RecalculateLOD();

            if (!_settings.bodyMaterial.HasProperty("_SkinColor"))
                notAPP2Shader = true;
            else
            {
                Skin = _settings.bodyMaterial.GetColor("_SkinColor");
                Eye = _settings.bodyMaterial.GetColor("_EyeColor");
                Hair = _settings.bodyMaterial.GetColor("_HairColor");
                Underpants = _settings.bodyMaterial.GetColor("_UnderpantsColor");
                OralCavity = _settings.bodyMaterial.GetColor("_OralCavityColor");
                Teeth = _settings.bodyMaterial.GetColor("_TeethColor");
            }
            LockPrefab();
        }

        /// <summary>
        /// Update bounds all character meshes 
        /// </summary>
        public void UpdateSkinnedMeshesOffscreenBounds()
        {
            List<SkinnedMeshRenderer> allMeshes = GetAllMeshes();

            foreach (SkinnedMeshRenderer mesh in allMeshes)
            {
                mesh.updateWhenOffscreen = UpdateWhenOffscreenMeshes;
               
                if (!UpdateWhenOffscreenMeshes)
                {
                    if (GetCharacterInstanceStatus() != CharacterInstanceStatus.PrefabEditingInProjectView && GetCharacterInstanceStatus() != CharacterInstanceStatus.PrefabStageSceneOpened)
                        StartCoroutine(UpdateBounds());
                    else
                    {
                        UpdateBounds();
                    }

                    IEnumerator UpdateBounds()
                    {
                        mesh.updateWhenOffscreen = true;
                        if (Application.isPlaying)
                            yield return new WaitForEndOfFrame();
                       // else
                       //     yield return new WaitForSeconds(0.1f);

                        Bounds bounds = new Bounds();
                        Vector3 center = mesh.localBounds.center;
                        Vector3 extents = mesh.localBounds.extents * 1.4f;
                        bounds.center = center;
                        bounds.extents = extents;
                        mesh.updateWhenOffscreen = false;
                        mesh.localBounds = bounds;

                        if (_lodGroup != null)
                            _lodGroup.RecalculateBounds();
                    }

                }

            }
        }

        /// <summary>
        /// Get all character selectors
        /// </summary>
        /// <returns></returns>
        public List<CharacterSettingsSelector> GetCharacterSettingsSelectors()
        {
            return Settings.settingsSelectors;
        }
        /// <summary>
        /// Reset body material from all character parts to default
        /// </summary>
        public void ResetBodyMaterial()
        {
            if (Settings == null)
                return;
            foreach (var part in characterParts)
            {              
                foreach (var sm in part.skinnedMesh)
                {
                    sm.sharedMaterial = _settings.bodyMaterial;
                }
            }

            var hair = GetCharacterPart("HAIR");
            var selectedHair = GetElementsPreset(CharacterElementType.Hair, characterSelectedElements.GetSelectedIndex(CharacterElementType.Hair));
            if (selectedHair != null)
            {
                var mats = selectedHair.mats.ToList();
                if (selectedHair.mats != null && selectedHair.mats.Length > 0)
                {
                    for (var i = 0; i < hair.skinnedMesh.Count; i++)
                    {
                        hair.skinnedMesh[i].sharedMaterials = mats.ToArray();

                        for (var m = 0; m < mats.Count; m++)
                            if (mats[m].name == _settings.bodyMaterial.name)
                                hair.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, m);
                    }
                }
            }

            var beard = GetCharacterPart("BEARD");
            var selectedBeard = GetElementsPreset(CharacterElementType.Beard, characterSelectedElements.GetSelectedIndex(CharacterElementType.Beard));
            if (selectedBeard != null)
            {
                var mats = selectedBeard.mats.ToList();
                if (selectedBeard.mats != null && selectedBeard.mats.Length > 0)
                {
                    for (var i = 0; i < beard.skinnedMesh.Count; i++)
                    {
                        beard.skinnedMesh[i].sharedMaterials = mats.ToArray();

                        for (var m = 0; m < mats.Count; m++)
                            if (mats[m].name == _settings.bodyMaterial.name)
                                beard.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, m);
                    }
                }
            }

            var shoes = GetClothesAnchor(CharacterElementType.Shoes);
            for (var i = 0; i < shoes.skinnedMesh.Count; i++)
            {
                var mats = shoes.skinnedMesh[i].sharedMaterials.ToList();
                for (var m = 0; m < mats.Count; m++)
                {
                    if (mats[m].name == _settings.bodyMaterial.name)
                    {
                        mats[m] = _settings.bodyMaterial;
                        shoes.skinnedMesh[i].sharedMaterials = mats.ToArray();
                    }
                }
            }
            if (CurrentCombinerState == CombinerState.Combined || CurrentCombinerState == CombinerState.UsedPreBuitMeshes)
            {
                List<SkinnedMeshRenderer> combinedMeshes = GetCharacterPart("COMBINED").skinnedMesh;
                for (var i = 0; i < combinedMeshes.Count; i++)
                {
                    if (combinedMeshes[i] != null)
                    {
                        List<Material> mats = combinedMeshes[i].sharedMaterials.ToList();
                        for (var m = 0; m < mats.Count; m++)
                        {
                            if (mats[m].name == _settings.bodyMaterial.name)
                            {
                                mats[m] = _settings.bodyMaterial;
                                combinedMeshes[i].sharedMaterials = mats.ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize body colors
        /// </summary>
        public void InitColors()
        {
            if(Settings == null)
            {
                return;
            }
            if (bodyPropertyBlock == null)
            {
                bodyPropertyBlock = new MaterialPropertyBlock();
            }
            SetBodyColor(BodyColorPart.Skin, Skin);
            SetBodyColor(BodyColorPart.Eye, Eye);
            SetBodyColor(BodyColorPart.Hair, Hair);
            SetBodyColor(BodyColorPart.Underpants, Underpants);
            SetBodyColor(BodyColorPart.Teeth, Teeth);
            SetBodyColor(BodyColorPart.OralCavity, OralCavity);
        }

        /// <summary>
        /// Reset body colors to default values
        /// </summary>
        public void ResetBodyColors()
        {
            if (notAPP2Shader || !_settings)
                return;
            if(_settings.bodyMaterial.HasProperty("_SkinColor"))
                SetBodyColor(BodyColorPart.Skin, _settings.bodyMaterial.GetColor("_SkinColor"));
            if (_settings.bodyMaterial.HasProperty("_EyeColor"))
                SetBodyColor(BodyColorPart.Eye, _settings.bodyMaterial.GetColor("_EyeColor"));
            if (_settings.bodyMaterial.HasProperty("_HairColor"))
                SetBodyColor(BodyColorPart.Hair, _settings.bodyMaterial.GetColor("_HairColor"));
            if (_settings.bodyMaterial.HasProperty("_UnderpantsColor"))
                SetBodyColor(BodyColorPart.Underpants, _settings.bodyMaterial.GetColor("_UnderpantsColor"));
            if (_settings.bodyMaterial.HasProperty("_OralCavityColor"))
                SetBodyColor(BodyColorPart.OralCavity, _settings.bodyMaterial.GetColor("_OralCavityColor"));
            if (_settings.bodyMaterial.HasProperty("_TeethColor"))
                SetBodyColor(BodyColorPart.Teeth, _settings.bodyMaterial.GetColor("_TeethColor"));
        }
       
        /// <summary>
        /// Set body shape (blend shapes)
        /// </summary>
        /// <param name="type">Body type</param>
        /// <param name="weight">Weight (0 to 100)</param>
        /// <param name="forPart">Apply to specific body parts</param>
        /// <param name="forClothPart">Apply to specific cloth parts</param>
        public void SetBlendshapeValue(CharacterBlendShapeType type, float weight, string[] forPart = null, CharacterElementType[] forClothPart = null)
        {
            try
            {
                var typeName = type.ToString();

                if (CurrentCombinerState != CombinerState.Combined && CurrentCombinerState != CombinerState.UsedPreBuitMeshes)
                {
                    foreach (var part in characterParts)
                    {
                        if (forPart != null && !forPart.Contains(part.name))
                            continue;
                        foreach (var skinnedMesh in part.skinnedMesh)
                        {
                            if (skinnedMesh != null && skinnedMesh.sharedMesh != null)
                            {
                                for (var i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; i++)
                                {                                   
                                    if (typeName == skinnedMesh.sharedMesh.GetBlendShapeName(i))
                                    {
                                        var bIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(typeName);
                                        if(bIndex != -1 && !Settings.DisableBlendshapeModifier)
                                            skinnedMesh.SetBlendShapeWeight(bIndex, weight);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var clothPart in clothesAnchors)
                    {
                        if (forClothPart != null && !forClothPart.Contains(clothPart.partType))
                            continue;

                        foreach (var skinnedMesh in clothPart.skinnedMesh)
                        {
                            if (skinnedMesh != null && skinnedMesh.sharedMesh != null)
                            {
                                for (var i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; i++)
                                {
                                    if (typeName == skinnedMesh.sharedMesh.GetBlendShapeName(i))
                                    {
                                        var bIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(typeName);
                                        if(bIndex != -1 && !Settings.DisableBlendshapeModifier)
                                            skinnedMesh.SetBlendShapeWeight(bIndex, weight);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<SkinnedMeshRenderer> combinedMeshes = GetCharacterPart("COMBINED").skinnedMesh;
                    foreach (var combinedMesh in combinedMeshes)
                    {
                        if (combinedMesh.sharedMesh != null)
                        {
                            for (var i = 0; i < combinedMesh.sharedMesh.blendShapeCount; i++)
                            {
                                if (!Settings.DisableBlendshapeModifier && typeName == combinedMesh.sharedMesh.GetBlendShapeName(i))
                                {
                                    combinedMesh.SetBlendShapeWeight(combinedMesh.sharedMesh.GetBlendShapeIndex(typeName), weight);
                                }
                            }
                        }
                    }
                }
                GetBlendshapeData(type).value = weight;
            }catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        /// <summary>
        /// Change LOD level
        /// </summary>
        /// <param name="lodLevel">LOD level (0-3). Value < 0 return to standard LOD processing.</param>
        public void ForceLOD(int lodLevel)
        {
            if (lodLevel > MaxLODLevels - MinLODLevels)
                return;
            if (lodLevel != 0)
            {
                _lodGroup.ForceLOD(lodLevel);
            }
            else
            {
                _lodGroup.ForceLOD(-1);
            }
        }
        /// <summary>
        /// Set character element
        /// </summary>
        /// <param name="type">Type of clothes</param>
        /// <param name="index">Index of element</param>
        public void SetElementByIndex(CharacterElementType type, int index)
        {
            
            if(Settings == null)
            {
                Debug.LogError("settings = null");
                return;
            }
            if (type == CharacterElementType.Hair)
            {
                SetHairByIndex(index);
                RecalculateShapes();
                return;
            }
            if(type == CharacterElementType.Beard)
            {
                SetBeardByIndex(index);
                RecalculateShapes();
                return;
            }
            ClothesAnchor ca = GetClothesAnchor(type);
            CharacterElementsPreset clothPreset = GetElementsPreset(type, characterSelectedElements.GetSelectedIndex(type));
            CharacterElementsPreset newPreset = GetElementsPreset(type, index);

            var verified = VerifyPreset(newPreset);

            if (!verified)
                return;

            float yOffset = 0f;

            if (clothPreset != null && (newPreset != null || index == -1))
                UnHideParts(clothPreset.hideParts, type);

            if (newPreset != null)
            {
                if (newPreset.mesh.Length == 0)
                {
                    Debug.LogErrorFormat(string.Format("Not found meshes for <{0}> element", newPreset.name));
                    return;
                }
                if (type == CharacterElementType.Shirt)
                {
                    GetBlendshapeData(CharacterBlendShapeType.BackpackOffset).value = 100;
                    if (characterSelectedElements.GetSelectedIndex(CharacterElementType.Item1) != -1)
                        SetBlendshapeValue(CharacterBlendShapeType.BackpackOffset, 100);
                }
                yOffset = newPreset.yOffset;
                for (var i = 0; i < (MaxLODLevels - MinLODLevels) + 1; i++)
                {
                    var element_index = i + MinLODLevels;

                    if (!ca.skinnedMesh[i].gameObject.activeSelf && !IsBaked())
                        ca.skinnedMesh[i].gameObject.SetActive(true);

                    
                    ca.skinnedMesh[i].sharedMesh = null;
                   

                    ca.skinnedMesh[i].sharedMesh = newPreset.mesh[element_index];
                    if (newPreset.mats != null && newPreset.mats.Length > 0)
                    {
                        var mats = newPreset.mats.ToList();

                        ca.skinnedMesh[i].sharedMaterials = mats.ToArray();

                        for (var m = 0; m < mats.Count; m++)
                            if (mats[m].name == _settings.bodyMaterial.name)
                                ca.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, m);
                    }

                    for (var blendIndex = 0; blendIndex < ca.skinnedMesh[i].sharedMesh.blendShapeCount; blendIndex++)
                    {
                        if (ca.skinnedMesh[i] != null && ca.skinnedMesh[i].sharedMesh != null)
                        {
                            var blendName = ca.skinnedMesh[i].sharedMesh.GetBlendShapeName(blendIndex);

                            var blendshapeData = GetBlendshapeData(blendName);

                            if (blendshapeData != null && !Settings.DisableBlendshapeModifier)
                                ca.skinnedMesh[i].SetBlendShapeWeight(blendIndex, blendshapeData.value);
                        }
                    }
                }
                HideParts(newPreset.hideParts);
            }
            else
            {
                if (type == CharacterElementType.Shirt)
                {
                    GetBlendshapeData(CharacterBlendShapeType.BackpackOffset).value = 0;
                    if (characterSelectedElements.GetSelectedIndex(CharacterElementType.Item1) != -1)
                        SetBlendshapeValue(CharacterBlendShapeType.BackpackOffset, 0);
                }
                if (index != -1)
                {
                    Debug.LogError(string.Format("Element <{0}> with index {1} not found. Please check Character Presets arrays.", type.ToString(), index));
                    return;
                }

                if (ca != null && ca.skinnedMesh != null)
                {
                    foreach (var sm in ca.skinnedMesh)
                    {
                        if (sm != null)
                        {
                            sm.sharedMesh = null;
                            sm.gameObject.SetActive(false);
                        }
                    }
                }
            }
            if (type == CharacterElementType.Shoes)
            {
                SetFeetOffset(new Vector3(0, yOffset, 0));
                feetOffset = yOffset;
            }

            characterSelectedElements.SetSelectedIndex(type,index);
        }

        bool VerifyPreset(CharacterElementsPreset preset)
        {
#if UNITY_EDITOR
            if (preset != null && VerifyClothPreset)
            {
                bool correctBonesStructure = preset.mesh[0].bindposes.Length == characterParts[0].skinnedMesh[0].sharedMesh.bindposes.Length;
                bool correctAnimationType = true;
                bool correctFileScale = true;
                ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(preset.mesh[0]));
                if(importer.fileScale != 1.0f || importer.globalScale != 1.0f)
                    correctFileScale = false;
                if(importer.animationType != ModelImporterAnimationType.Generic && importer.animationType != ModelImporterAnimationType.Human)
                {
                    correctAnimationType = false;
                }
                if (!correctFileScale)
                {
                    Debug.LogWarningFormat("Cloth Preset <{0}> : ({1}..) <color=#81a2ff><b>meshes</b></color> or <color=#81a2ff><b>hips</b></color> does not have correct scale factor\nMake sure your model and all parts have standard transformation before export, <b>Scale (1,1,1)</b>, <b>Rotation (0,0,0)</b>, otherwise the model may disappear and not render correctly", preset.name, preset.mesh[0].name);
                }

                if(!correctBonesStructure || !correctAnimationType)
                {
                    if(!correctBonesStructure && correctAnimationType)
                        Debug.LogErrorFormat("Cloth Preset <{0}> : ({1}..) meshes does not have the same bone structure as the current character, so clothing cannot be selected.\nPlease make sure you are using the default character rig, and the clothing model rig has not been modified", preset.name, preset.mesh[0].name);
                    if(!correctAnimationType)
                        Debug.LogErrorFormat("Cloth Preset <{0}> : ({1}..) meshes does not have correct animation type, so clothing cannot be selected.\nPlease set rig value to <b>'Generic'</b> or <b>'Humanoid'</b> (model {2})", preset.name, preset.mesh[0].name, importer.assetPath);
                    return false;
                }
            }
#endif
            return true;
        }

        /// <summary>
        /// Clear character element
        /// </summary>
        /// <param name="type">Element type</param>
        public void ClearElement(CharacterElementType type)
        {
            if (type == CharacterElementType.Hair)
            {
                SetHairByIndex(-1);
                return;
            }
            if (type == CharacterElementType.Beard)
            {
                SetBeardByIndex(-1);
                return;
            }
            SetElementByIndex(type, -1);
        }
        /// <summary>
        /// Set character height
        /// </summary>
        /// <param name="height">Height offset</param>
        public void SetHeight(float height)
        {
            heightValue = height;

            if (originHip != null)
                originHip.localScale = new Vector3(1 + height / 1.4f, 1 + height, 1 + height);
            
        }
        /// <summary>
        /// Set character head size
        /// </summary>
        /// <param name="size">Head scale, recommended value (-0.25 to 0.25) </param>
        public void SetHeadSize(float size)
        {
            headSizeValue = size;

            if (headHip != null)
                headHip.localScale = Vector3.one + Vector3.one * size;
        }

        /// <summary>
        /// Set character feet offset. Usually used for tall shoes.
        /// </summary>
        /// <param name="offset">Offset value</param>
        public void SetFeetOffset(Vector3 offset)
        {
            if(originHip != null)
            originHip.localPosition = offset;
        }

        /// <summary>
        /// Set character hair by index
        /// </summary>
        private void SetHairByIndex(int index)
        {
            CharacterPart hair = GetCharacterPart("Hair");
            if (hair == null || hair.skinnedMesh.Count <= 0)
                return;
            if (index != -1)
            {
                var hairPreset = _settings.hairPresets.ElementAtOrDefault(index);

                if (hairPreset == null)
                {
                    Debug.LogError(string.Format("Hair with index {0} not found", index));
                    return;
                }
                var verified = VerifyPreset(hairPreset);

                if (!verified)
                    return;
                for (var i = 0; i < (MaxLODLevels - MinLODLevels) + 1; i++)
                {
                    var hair_index = i + MinLODLevels;
                    if (hair.skinnedMesh.Count > 0 && hair.skinnedMesh.Count - 1 >= i && hair.skinnedMesh[i] != null)
                    {
                        if (!hair.skinnedMesh[i].gameObject.activeSelf)
                        {
                            hair.skinnedMesh[i].gameObject.SetActive(true);
                        }
                        hair.skinnedMesh[i].sharedMesh = _settings.hairPresets[index].mesh[hair_index];
                    }

                    if (hairPreset.mats != null && hairPreset.mats.Length > 0)
                    {
                        var mats = hairPreset.mats.ToList();

                        hair.skinnedMesh[i].sharedMaterials = mats.ToArray();

                        for (var m = 0; m < mats.Count; m++)
                            if (mats[m].name == _settings.bodyMaterial.name)
                                hair.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, m);
                    }


                }
            }
            else
            {
                foreach (var hairsm in hair.skinnedMesh)
                {
                    hairsm.sharedMesh = null;
                    hairsm.gameObject.SetActive(false);
                }
            }
            characterSelectedElements.SetSelectedIndex(CharacterElementType.Hair, index);
        }
        /// <summary>
        /// Set beard for male character
        /// </summary>
        /// <param name="index"></param>
        private void SetBeardByIndex(int index)
        {
            CharacterPart beard = GetCharacterPart("Beard");

            if (beard == null || beard.skinnedMesh.Count <= 0)
                return;
            if (index != -1)
            {
                var beardPreset = _settings.beardPresets.ElementAtOrDefault(index);

                if (beardPreset == null)
                {
                    Debug.LogError(string.Format("Beard with index {0} not found", index));
                    return;
                }

                var verified = VerifyPreset(beardPreset);

                if (!verified)
                    return;

                for (var i = 0; i < (MaxLODLevels - MinLODLevels) + 1; i++)
                {
                    var beard_index = i + MinLODLevels;

                    if (!beard.skinnedMesh[i].gameObject.activeSelf)
                    {
                        beard.skinnedMesh[i].gameObject.SetActive(true);
                    }
                    beard.skinnedMesh[i].sharedMesh = _settings.beardPresets[index].mesh[beard_index];


                    if (beardPreset.mats != null && beardPreset.mats.Length > 0)
                    {
                        var mats = beardPreset.mats.ToList();

                        beard.skinnedMesh[i].sharedMaterials = mats.ToArray();

                        for (var m = 0; m < mats.Count; m++)
                            if (mats[m].name == _settings.bodyMaterial.name)
                                beard.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, m);
                    }

                }
            }
            else
            {
                foreach (var beardsm in beard.skinnedMesh)
                {
                    beardsm.sharedMesh = null;
                    beardsm.gameObject.SetActive(false);
                }
            }
            characterSelectedElements.SetSelectedIndex(CharacterElementType.Beard, index);

            
        }
        /// <summary>
        /// Get clothes anchor(the mesh to which the clothes are attached) by type
        /// </summary>
        /// <param name="type">Cloth type</param>
        /// <returns></returns>
        public ClothesAnchor GetClothesAnchor(CharacterElementType type)
        {
            foreach (ClothesAnchor p in clothesAnchors)
            {
                if (p.partType == type)
                    return p;
            }
            return null;
        }
        /// <summary>
        /// Get character part by name
        /// </summary>
        /// <param name="name">Part name</param>
        /// <returns></returns>
        public CharacterPart GetCharacterPart(string name)
        {         
            foreach (CharacterPart p in characterParts)
            {
                if (p.name.ToLowerInvariant() == name.ToLowerInvariant())
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Get all character meshes by LOD level
        /// </summary>
        /// <param name="lod">LOD index</param>
        /// <returns>All skinned meshes by LOD</returns>
        public List<SkinnedMeshRenderer> GetAllMeshesByLod(int lod)
        {
            List<SkinnedMeshRenderer> returnMeshes = new List<SkinnedMeshRenderer>();
            foreach (CharacterPart p in characterParts)
            {
                if(p.skinnedMesh.Count>=lod)
                returnMeshes.Add(p.skinnedMesh[lod]);
            }
            foreach(ClothesAnchor ca in clothesAnchors)
            {
                if (ca.skinnedMesh.Count >= lod)
                    returnMeshes.Add(ca.skinnedMesh[lod]);
            }
            return returnMeshes;
        }

        /// <summary>
        /// Get all character meshes
        /// </summary>
        /// <returns>All skinned meshes</returns>
        public List<SkinnedMeshRenderer> GetAllMeshes()
        {
            List<SkinnedMeshRenderer> returnMeshes = new List<SkinnedMeshRenderer>();
            foreach (CharacterPart p in characterParts)
            {
                returnMeshes.AddRange(p.skinnedMesh);
            }
            foreach (ClothesAnchor ca in clothesAnchors)
            {
                returnMeshes.AddRange(ca.skinnedMesh);
            }
            return returnMeshes;
        }

        /// <summary>
        /// Get all character meshes
        /// </summary>
        /// <param name="onlyBodyMeshes">If you need get only body meshes</param>
        /// <param name="excludeNames">Exclude meshes by name</param>
        /// <returns></returns>
        public List<SkinnedMeshRenderer> GetAllMeshes(bool onlyBodyMeshes = false, string[] excludeNames = null)
        {
            List<SkinnedMeshRenderer> returnMeshes = new List<SkinnedMeshRenderer>();
            foreach (CharacterPart p in characterParts)
            {
                if (excludeNames != null && excludeNames.Contains(p.name))
                    continue;
                returnMeshes.AddRange(p.skinnedMesh);
            }
            if (onlyBodyMeshes)
            {
                foreach (ClothesAnchor ca in clothesAnchors)
                {
                    returnMeshes.AddRange(ca.skinnedMesh);
                }
            }
            return returnMeshes;
        }
        /// <summary>
        /// Hide character parts
        /// </summary>
        /// <param name="parts">Array of parts to hide</param>
        public void HideParts(string[] parts)
        {
            foreach (string p in parts)
            {
                foreach (CharacterPart cp in characterParts)
                {
                    if (cp.name.ToLowerInvariant() == p.ToLowerInvariant())
                    {
                        foreach (var mesh in cp.skinnedMesh)
                            mesh.enabled = false;
                    }
                }
            }
        }
        /// <summary>
        /// UnHide character parts
        /// </summary>
        /// <param name="parts">Array of parts to unhide</param>
        /// <param name="hidePartsForElement">Hide parts for this cloth type</param>
        public void UnHideParts(string[] parts, CharacterElementType hidePartsForElement)
        {
            foreach (string p in parts)
            {
                bool ph_in_shirt = false, ph_in_pants = false, ph_in_shoes = false;

                #region Code to exclude the UnHide parts of the character if are hidden in other presets
                int shirt_i = characterSelectedElements.GetSelectedIndex(CharacterElementType.Shirt);
                int pants_i = characterSelectedElements.GetSelectedIndex(CharacterElementType.Pants);
                int shoes_i = characterSelectedElements.GetSelectedIndex(CharacterElementType.Shoes);

                if (shirt_i != -1 && hidePartsForElement != CharacterElementType.Shirt)
                {
                    foreach (var shirtPart in GetElementsPreset(CharacterElementType.Shirt, shirt_i).hideParts)
                    {
                        if (shirtPart == p)
                        {
                            ph_in_shirt = true;
                            break;
                        }
                    }
                }
                if (pants_i != -1 && hidePartsForElement != CharacterElementType.Pants)
                {
                    foreach (var pantPart in GetElementsPreset(CharacterElementType.Pants, pants_i).hideParts)
                    {
                        if (pantPart == p)
                        {
                            ph_in_pants = true;
                            break;
                        }
                    }
                }
                if (shoes_i != -1 && hidePartsForElement != CharacterElementType.Shoes)
                {
                    foreach (var shoesPart in GetElementsPreset(CharacterElementType.Shoes, shoes_i).hideParts)
                    {
                        if (shoesPart == p)
                        {
                            ph_in_shoes = true;
                            break;
                        }
                    }
                }

                if (ph_in_shirt || ph_in_pants || ph_in_shoes)
                    continue;
                #endregion

                foreach (CharacterPart cp in characterParts)
                {
                    if (cp.name.ToLowerInvariant() == p.ToLowerInvariant())
                        foreach (var mesh in cp.skinnedMesh)
                            mesh.enabled = true;
                }
            }
        }
        /// <summary>
        /// Set body color by type
        /// </summary>
        /// <param name="bodyColorPart">Body part to change color</param>
        /// <param name="color">New color</param>
        public void SetBodyColor(BodyColorPart bodyColorPart, Color color)
        {
            if (notAPP2Shader)
                return;
            if (bodyPropertyBlock == null)
                bodyPropertyBlock = new MaterialPropertyBlock();

            switch (bodyColorPart)
            {
                case BodyColorPart.Skin:
                    bodyPropertyBlock.SetColor("_SkinColor", color);
                    break;
                case BodyColorPart.Eye:
                    bodyPropertyBlock.SetColor("_EyeColor", color);
                    break;
                case BodyColorPart.Hair:
                    bodyPropertyBlock.SetColor("_HairColor", color);
                    break;
                case BodyColorPart.Underpants:
                    bodyPropertyBlock.SetColor("_UnderpantsColor", color);
                    break;
                case BodyColorPart.Teeth:
                    bodyPropertyBlock.SetColor("_TeethColor", color);
                    break;
                case BodyColorPart.OralCavity:
                    bodyPropertyBlock.SetColor("_OralCavityColor", color);
                    break;
            }


            List<SkinnedMeshRenderer> allBodyMeshes = (IsBaked()) ? GetCharacterPart("COMBINED").skinnedMesh : GetAllMeshes(true, new string[] { "COMBINED" });

            foreach (var mesh in allBodyMeshes)
            {
                for (int matIndex = 0; matIndex < mesh.sharedMaterials.Length; matIndex++)
                {
                    if (mesh.sharedMaterials[matIndex].shader == _settings.bodyMaterial.shader)
                        mesh.SetPropertyBlock(bodyPropertyBlock, matIndex);
                }
            }
            
            switch (bodyColorPart)
            {
                case BodyColorPart.Skin:
                    Skin = color;
                    break;
                case BodyColorPart.Eye:
                    Eye = color;
                    break;
                case BodyColorPart.Hair:
                    Hair = color;
                    break;
                case BodyColorPart.Underpants:
                    Underpants = color;
                    break;
                case BodyColorPart.OralCavity:
                    OralCavity = color;
                    break;
                case BodyColorPart.Teeth:
                    Teeth = color;
                    break;
            }
        }
        /// <summary>
        /// Get the used color of a specific part of the body
        /// </summary>
        /// <param name="bodyColorPart">Body part</param>
        /// <returns></returns>
        public Color GetBodyColor(BodyColorPart bodyColorPart)
        {
            Color outColor = Color.clear;
            switch (bodyColorPart)
            {
                case BodyColorPart.Skin:
                    outColor = Skin;
                    break;
                case BodyColorPart.Eye:
                    outColor = Eye;
                    break;
                case BodyColorPart.Hair:
                    outColor = Hair;
                    break;
                case BodyColorPart.Underpants:
                    outColor = Underpants;
                    break;
                case BodyColorPart.OralCavity:
                    outColor = OralCavity;
                    break;
                case BodyColorPart.Teeth:
                    outColor = Teeth;
                    break;
                default:
                    outColor = Color.clear;
                    break;
            }
            outColor = new Color(outColor.r, outColor.g, outColor.b, 1f);
            return outColor;
        }

        /// <summary>
        /// Set character setup, use setup class
        /// </summary>
        /// <param name="characterCustomizationSetup">Setup class</param>
        public void SetCharacterSetup(CharacterCustomizationSetup characterCustomizationSetup)
        {
            characterCustomizationSetup.ApplyToCharacter(this);
        }
        /// <summary>
        /// Generate setup class from current character
        /// </summary>
        /// <returns>New CharacterCustomizationSetup instance</returns>
        public CharacterCustomizationSetup GetSetup()
        {
            var ccs = new CharacterCustomizationSetup();
            foreach(var blendshape in characterBlendshapeDatas)
            {
                ccs.blendshapes.Add(new CharacterBlendshapeData(blendshape.blendshapeName, blendshape.type, blendshape.group, blendshape.value));
            }
            ccs.MinLod = MinLODLevels;
            ccs.MaxLod = MaxLODLevels;
            ccs.selectedElements.Accessory = characterSelectedElements.Accessory;
            ccs.selectedElements.Beard = characterSelectedElements.Beard;
            ccs.selectedElements.Hair = characterSelectedElements.Hair;
            ccs.selectedElements.Hat = characterSelectedElements.Hat;
            ccs.selectedElements.Item1 = characterSelectedElements.Item1;
            ccs.selectedElements.Pants = characterSelectedElements.Pants;
            ccs.selectedElements.Shirt = characterSelectedElements.Shirt;
            ccs.selectedElements.Shoes = characterSelectedElements.Shoes;

            ccs.Height = heightValue;
            ccs.HeadSize = headSizeValue;

            ccs.SkinColor = new float[4] { Skin.r, Skin.g, Skin.b, Skin.a };
            ccs.HairColor = new float[4] { Hair.r, Hair.g, Hair.b, Hair.a };
            ccs.UnderpantsColor = new float[4] { Underpants.r, Underpants.g, Underpants.b, Underpants.a };
            ccs.TeethColor = new float[4] { Teeth.r, Teeth.g, Teeth.b, Teeth.a };
            ccs.OralCavityColor = new float[4] { OralCavity.r, OralCavity.g, OralCavity.b, OralCavity.a };
            ccs.EyeColor = new float[4] { Eye.r, Eye.g, Eye.b, Eye.a };

            ccs.settingsName = Settings.name;
            return ccs;
        }
        /// <summary>
        /// Get character settings in string format 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetSetupStringData(CharacterCustomizationSetup.CharacterFileSaveFormat format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json)
        {
            CharacterCustomizationSetup characterCustomizationSetup = GetSetup();
            return characterCustomizationSetup.Serialize(format);
        }
        /// <summary>
        /// Apply settings to character by string data
        /// </summary>
        /// <param name="data">Character String Data</param>
        /// <param name="format"></param>
        public void ApplySetupByString(string data, CharacterCustomizationSetup.CharacterFileSaveFormat format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json)
        {
            if (data != string.Empty)
            {
                CharacterCustomizationSetup characterCustomizationSetup = CharacterCustomizationSetup.Deserialize(data, format);
                if(characterCustomizationSetup != null)
                    characterCustomizationSetup.ApplyToCharacter(this);
            }
        }

        /// <summary>
        /// Apply saved file data to current character
        /// </summary>
        public void ApplySavedCharacterData(SavedCharacterData data)
        {
            if(data != null)
                LoadCharacterFromFile(data.path);
        }

        /// <summary>
        /// Load character from file
        /// </summary>
        public void LoadCharacterFromFile(string path)
        {
            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path);
                var data = File.ReadAllText(path);
                
                if(data.Length > 0)
                {
                    CharacterCustomizationSetup.CharacterFileSaveFormat format = CharacterCustomizationSetup.CharacterFileSaveFormat.Binary;
                    if (ext == ".json")
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Json;
                    }else if(ext == ".xml")
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Xml;
                    }else if (ext == ".bin")
                    {
                        format = CharacterCustomizationSetup.CharacterFileSaveFormat.Binary;
                    }
                    else
                    {
                        Debug.LogError("File format not supported - " + ext);
                        return;
                    }
                    var setup = CharacterCustomizationSetup.Deserialize(data, format);
                    if(setup != null)
                    {
                        SetCharacterSetup(setup);
                        Debug.Log(string.Format("Loaded {0} save", path));
                    }
                }
            }
        }
        /// <summary>
        /// Gets a list of character saves from the directory
        /// </summary>
        /// <returns>Save list</returns>
        public List<SavedCharacterData> GetSavedCharacterDatas(string path = "")
        {
            List<SavedCharacterData> savedCharacterDatas = new List<SavedCharacterData>();
            var dataPath = Application.persistentDataPath;
            var folderPath = string.Format("{0}/{1}", dataPath, "apack_characters_data");          
            folderPath = string.Format("{0}/{1}", folderPath, Settings.name);
            if (!Directory.Exists(folderPath))
            {
                return savedCharacterDatas;
            }
            else
            {
                var filesList = Directory.GetFiles(folderPath, "appack25*");
                foreach(var file in filesList)
                {
                    SavedCharacterData savedData = new SavedCharacterData();


                    var name = Path.GetFileName(file);
                    var nameSplit = name.Split('_');
                    var clearName = "";
                    for (int i = 1; i < nameSplit.Length - 1; i++)
                        clearName += nameSplit[i]+ ((i != nameSplit.Length-2) ? "_" : "");

                    savedData.name = clearName;
                    savedData.path = file;
                    savedCharacterDatas.Add(savedData);
                }
            }
            return savedCharacterDatas;
        }

        /// <summary>
        /// Delete specific saved character
        /// </summary>
        public void ClearSavedData(SavedCharacterData data)
        {
            if(data != null)
            {
                if (File.Exists(data.path))
                {
                    File.Delete(data.path);
                }
            }
        }

        /// <summary>
        /// Delete all character saves from default directory
        /// </summary>
        public void ClearSavedData()
        {
            var datas = GetSavedCharacterDatas();
            foreach(var data in datas)
            {
                ClearSavedData(data);
            }
            Debug.Log(string.Format("Removed {0} saves", datas.Count));
        }

        /// <summary>
        /// Save character to file
        /// </summary>
        /// <param name="format">Save format</param>
        /// <param name="path">Directory</param>
        /// <param name="name">File name</param>
        public void SaveCharacterToFile(CharacterCustomizationSetup.CharacterFileSaveFormat format, string path = "", string name = "")
        {
            var savepath = path;
            string formatString = "json";
            switch (format)
            {
                case CharacterCustomizationSetup.CharacterFileSaveFormat.Json:
                    formatString = "json";
                    break;
                case CharacterCustomizationSetup.CharacterFileSaveFormat.Xml:
                    formatString = "xml";
                    break;
                case CharacterCustomizationSetup.CharacterFileSaveFormat.Binary:
                    formatString = "bin";
                    break;
            }
            if (path.Length == 0)
            {
                string[] formatExt = new string[] { "json", "xml", "bin" };

                var dataPath = Application.persistentDataPath;              
                var folderPath = string.Format("{0}/{1}", dataPath, "apack_characters_data");
                
                folderPath = string.Format("{0}/{1}", folderPath, Settings.name);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string savedFileName = gameObject.name;

                savepath = string.Format("{0}/appack25_{1}_{2}.{3}", folderPath, savedFileName, DateTimeOffset.Now.ToUnixTimeSeconds(), formatString);
            }
            else
            {
                savepath = string.Format("{0}/{1}_{2}.{3}", path, name, DateTimeOffset.Now.ToUnixTimeSeconds(), formatString);
            }
            var data = GetSetup().Serialize(format);
            if (data.Length > 0)
            {
                File.WriteAllText(savepath, data, System.Text.Encoding.UTF8);
                Debug.Log(string.Format("Character data saved to ({0})", savepath));
            }
        }

        /// <summary>
        /// Recalculate all blendshapes
        /// </summary>
        public void RecalculateShapes()
        {
            foreach(var blendshape in characterBlendshapeDatas)
            {
                SetBlendshapeValue(blendshape.type, blendshape.value);
            }
        }

        GameObject prebuiltPrefab;

        /// <summary>
        /// Save PreBuilt character (editor only)
        /// </summary>
        public void EditorSavePreBuiltPrefab(bool bakeBlendshapes = false)
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFolderPanel("Save PreBuilt character prefab in folder", "Assets", "");
            if (path != string.Empty)
            {
                prebuiltPrefab = GameObject.Instantiate(_settings.OriginalMesh);

                if (prebuiltPrefab == null)
                {
                    Debug.LogError("Prebuilt Prefab == null");
                    return;
                }

                prebuiltPrefab.name = string.Format("{0} - PreBuilt #{1}", this.gameObject.name, this.gameObject.GetHashCode());

                Animator animator = prebuiltPrefab.GetComponent<Animator>();
                animator.avatar = this.animator.avatar;
                animator.runtimeAnimatorController = this.animator.runtimeAnimatorController;
                animator.applyRootMotion = this.animator.applyRootMotion;
                animator.updateMode = this.animator.updateMode;
                animator.cullingMode = this.animator.cullingMode;

                Transform[] bones = null;
                Transform rootBone = null;
                Matrix4x4[] bindPoses = null;

                var objsInPrefabObj = prebuiltPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (var del in objsInPrefabObj)
                {
                    if (del.sharedMesh != null && bones == null)
                    {
                        bones = del.bones;
                        rootBone = del.rootBone;
                        bindPoses = del.sharedMesh.bindposes;
                    }
                    GameObject.DestroyImmediate(del.gameObject);
                }

                EditorUtility.DisplayProgressBar("Make Pre Built Character", "Make prebuilt prefab", 0.1f);
                CharacterCustomizationCombiner.MakeCombinedMeshes(this, bakeBlendshapes, prebuiltPrefab, 0.06f, (newMeshes) =>
                {
                    try
                    {
                        List<SkinnedMeshRenderer> combinedMeshes = new List<SkinnedMeshRenderer>();
                        foreach (SkinnedMeshRenderer combinedMesh in newMeshes)
                        {
                            combinedMesh.gameObject.SetActive(true);
                            combinedMesh.bones = bones;
                            combinedMesh.rootBone = rootBone;
                            combinedMesh.sharedMesh.bindposes = bindPoses;
                            combinedMeshes.Add(combinedMesh);
                        }

                        foreach(var b in bones)
                        {
                            if (b.name == headHip.name)
                                b.localScale = headHip.localScale;                          
                        }
                        if (rootBone.parent != null && rootBone.parent.name == originHip.name)
                            rootBone.parent.localScale = originHip.localScale;

                        string prefabName = prebuiltPrefab.name.Trim();

                        path = "Assets" + path.Remove(0, Application.dataPath.Length);

                        string newFolderPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(path, prefabName));

                        EditorUtility.DisplayProgressBar("Make Pre Built Character", "Create folder for prefab", 0.2f);

                        AssetDatabase.CreateFolder(newFolderPath, "CombinedMeshes");
                        EditorUtility.DisplayProgressBar("Make Pre Built Character", "Create folder for prefab meshes", 0.3f);
                        AssetDatabase.CreateFolder(newFolderPath, "Materials");
                        EditorUtility.DisplayProgressBar("Make Pre Built Character", "Create folder for prefab materials", 0.4f);
                        CharacterPreBuilt characterPreBuilt = ScriptableObject.CreateInstance<CharacterPreBuilt>();
                        List<SkinnedMeshRenderer> allMeshes = new List<SkinnedMeshRenderer>();
                        List<List<Material>> allMaterials = new List<List<Material>>();

                        EditorUtility.DisplayProgressBar("Make Pre Built Character", "Create scriptableobject", 0.5f);

                        for (int i = 0; i < combinedMeshes.Count; i++)
                        {

                            EditorUtility.DisplayProgressBar("Make Pre Built Character", "Save combined meshes " + i, 0.5f + ((0.45f / combinedMeshes.Count) * (i)));

                            AssetDatabase.CreateAsset(combinedMeshes[i].sharedMesh, string.Format("{0}/CombinedMeshes/{1}.asset", newFolderPath, combinedMeshes[i].sharedMesh.name));
                        }


                        PreBuiltData preBuiltData = new PreBuiltData();
                        preBuiltData.GroupName = "Baked";

                        for (int m = 0; m < combinedMeshes.Count; m++)
                        {
                            allMeshes.Add(combinedMeshes[m]);
                            preBuiltData.meshes.Add(combinedMeshes[m].sharedMesh);
                        }
                        characterPreBuilt.preBuiltDatas.Add(preBuiltData);
                        characterPreBuilt.settings = Settings;

                        List<Material> newMaterials = new List<Material>();

                        
                        foreach (Material mat in allMeshes[0].sharedMaterials)
                        {
                            Material exportMaterial = null;

                            for (int x = 0; x < newMaterials.Count; x++)
                            {
                                exportMaterial = newMaterials.Find(m => m.name == mat.name);
                                if (exportMaterial != null)
                                {
                                    newMaterials.Add(exportMaterial);
                                    break;
                                }
                            }
                            if (exportMaterial != null)
                                continue;

                            exportMaterial = Material.Instantiate(mat);
                            exportMaterial.name = mat.name;  
                            
                            if(exportMaterial.shader.name == Settings.bodyMaterial.shader.name)
                            {
                                var skinID = Shader.PropertyToID("_SkinColor");
                                var eyeID = Shader.PropertyToID("_EyeColor");
                                var hairID = Shader.PropertyToID("_HairColor");
                                var underpantsID = Shader.PropertyToID("_UnderpantsColor");
                                var oralCavityID = Shader.PropertyToID("_OralCavityColor");
                                var teethID = Shader.PropertyToID("_TeethColor");

                                if (exportMaterial.HasProperty(skinID))
                                    exportMaterial.SetColor(skinID, Skin);
                                if(exportMaterial.HasProperty(eyeID))
                                    exportMaterial.SetColor(eyeID, Eye);
                                if(exportMaterial.HasProperty(hairID))
                                    exportMaterial.SetColor(hairID, Hair);
                                if(exportMaterial.HasProperty(underpantsID))
                                    exportMaterial.SetColor(underpantsID, Underpants);
                                if(exportMaterial.HasProperty(oralCavityID))
                                    exportMaterial.SetColor(oralCavityID, OralCavity);
                                if(exportMaterial.HasProperty(teethID))
                                    exportMaterial.SetColor(teethID, Teeth);
                            }

                            string matPath = string.Format("{0}/Materials/{1}.mat", newFolderPath, exportMaterial.name);
                            AssetDatabase.CreateAsset(exportMaterial, matPath);

                            if (!newMaterials.Contains(exportMaterial))
                                newMaterials.Add(AssetDatabase.LoadAssetAtPath<Material>(matPath));
                        }
                        

                        for (int i = 0; i < allMeshes.Count; i++)
                        {

                                List<Material> sharedMaterials = new List<Material>();
                                allMeshes[i].GetSharedMaterials(sharedMaterials);
                                allMeshes[i].gameObject.SetActive(true);
                                sharedMaterials = newMaterials;

                                allMeshes[i].sharedMaterials = sharedMaterials.ToArray();
                            
                        }

                        characterPreBuilt.preBuiltDatas[0].materials.AddRange(newMaterials.ToArray());

                        foreach (var blendshape in characterBlendshapeDatas)
                        {
                            characterPreBuilt.blendshapes.Add(new PreBuiltBlendshape(blendshape.blendshapeName, blendshape.value));
                        }

                        AssetDatabase.CreateAsset(characterPreBuilt, string.Format("{0}/{1}.asset", newFolderPath, prefabName));

                        EditorUtility.DisplayProgressBar("Make Pre Built Character", "Save final prefab", 1.0f);

                        LODGroup lodGroup = null;
                        if (allMeshes.Count > 1)
                        {
                            lodGroup = prebuiltPrefab.AddComponent<LODGroup>();


                            float[][] LOD_Distance = new float[][] {
                              new float[] {0f,   0f,   0f, 0f },
                              new float[] {0.3f, 0f,   0f, 0f },
                              new float[] {0.4f, 0.1f, 0f, 0f },
                              new float[] {0.5f, 0.2f, 0.05f, 0f },
                              new float[] {0.6f, 0.3f, 0.1f, 0.03f, 0f },
                              new float[] {0.6f, 0.3f, 0.15f, 0.07f,  0.02f, 0f }
                            };
                            LOD[] lods = new LOD[allMeshes.Count];
                            for (int lod = 0; lod < allMeshes.Count; lod++)
                            {
                                lods[lod] = new LOD(LOD_Distance[(MaxLODLevels - MinLODLevels)][lod], new Renderer[] { allMeshes[lod] });
                            }
                            lodGroup.SetLODs(lods);
                            lodGroup.RecalculateBounds();
                        }

                        PrefabUtility.SaveAsPrefabAssetAndConnect(prebuiltPrefab, string.Format("{0}/{1}.prefab", newFolderPath, prebuiltPrefab.name.Replace(" ", string.Empty)), InteractionMode.UserAction);
                        if (allMeshes.Count > 1 && lodGroup != null)
                            lodGroup.RecalculateBounds();

                        PrefabUtility.ApplyPrefabInstance(prebuiltPrefab, InteractionMode.UserAction);
                        EditorUtility.ClearProgressBar();
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(string.Format("{0}/{1}.asset", newFolderPath, prefabName)));

                        GameObject.DestroyImmediate(prebuiltPrefab);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        EditorUtility.ClearProgressBar();
                        
                        GameObject.DestroyImmediate(prebuiltPrefab);
                    }
                });
                CurrentCombinerState = CombinerState.NotCombined;
            }
#else
         Debug.LogError("Pre Built Character can be created only in editor.");
#endif
        }
        /// <summary>
        /// Combine all character parts to single mesh (include all LODs)
        /// </summary>
        public void BakeCharacter(bool usePreBuiltMeshes = false, bool bakeBlendshapes = false)
        {
            if (usePreBuiltMeshes)
            {
                if(combinedCharacter == null)
                {
                    Debug.LogError("CombinedCharacter variable == null");
                    return;
                }
                if (combinedCharacter != null && combinedCharacter.settings != Settings)
                {
                    Debug.LogError("PreBuilt settings not equal current character settings");
                    return;
                }
                if (combinedCharacter.preBuiltDatas == null || combinedCharacter.preBuiltDatas.Count == 0)
                {
                    Debug.LogErrorFormat("CombinedCharacter object({0}) is not valid!",  combinedCharacter.name);
                    return;
                }
                if (combinedCharacter.preBuiltDatas[0].meshes.Count < (MaxLODLevels-MinLODLevels)+1)
                {
                    Debug.LogErrorFormat("CombinedCharacter number of meshes({0}) is less than the number of LODs({1}) in the character\nTry combine character again or change LODs count", combinedCharacter.preBuiltDatas[0].meshes.Count, (MaxLODLevels - MinLODLevels) + 1);
                    return;
                }
            }
            foreach(var cm in characterParts)
            {
                if (cm.name.ToLowerInvariant() == "combined")
                    cm.skinnedMesh.ForEach((m) =>
                    {
                        m.sharedMesh = null;
                        m.gameObject.SetActive(false);
                    });
            }
            try
            {
                void MeshesProcess(bool usePreBuilt = false)
                {
                    foreach (var cm in characterParts)
                    {
                        if (cm.name.ToLowerInvariant() != "combined")
                        {
                            cm.skinnedMesh.ForEach((m) =>
                            {
                                m.gameObject.SetActive(false);
                            });
                        }
                        else
                        {
                            
                            for(int i = 0; i < cm.skinnedMesh.Count; i++)
                            {
                                if (usePreBuilt)
                                {
                                    cm.skinnedMesh[i].sharedMesh = combinedCharacter.preBuiltDatas[0].meshes[i];
                                    cm.skinnedMesh[i].sharedMaterials = combinedCharacter.preBuiltDatas[0].materials.ToArray();
                                }
                                cm.skinnedMesh[i].gameObject.SetActive(true);
                            }

                        }
                    }
                    foreach (var ca in clothesAnchors)
                    {
                        ca.skinnedMesh.ForEach((m) =>
                        {
                            m.gameObject.SetActive(false);
                        });
                    }
                    if(Application.isPlaying)
                        InitColors();
                }
                if (!usePreBuiltMeshes)
                {
                    CharacterCustomizationCombiner.AddBakeToQueue(this, bakeBlendshapes, null, 0f, (meshes) =>
                    {
                        MeshesProcess();
                        RecalculateShapes();
                        RecalculateLOD();
                    });
                    /*CharacterCustomizationCombiner.MakeCombinedMeshes(this, bakeBlendshapes, null, 0f, (meshes) =>
                    {
                        MeshesProcess();
                        RecalculateShapes();
                        RecalculateLOD();
                    });*/
                }
                else
                {
                    MeshesProcess(true);
                    CurrentCombinerState = CombinerState.UsedPreBuitMeshes;
                    InitColors();
                    var combined = GetCharacterPart("Combined");
                    preBuiltMeshesHasBlendshapes = combined.skinnedMesh[0].sharedMesh.blendShapeCount > 0;
                }
            }catch(Exception ex)
            {
                Debug.LogException(ex);
                ClearBake();
            }

            RecalculateShapes();
            RecalculateLOD();
        }
        /// <summary>
        /// Clear all combine meshes, and enable customizable mode
        /// </summary>
        public void ClearBake()
        {
            foreach (var cm in characterParts)
            {
                if (cm.name.ToLowerInvariant() == "combined")
                {
                    cm.skinnedMesh.ForEach((m) =>
                    {
                        if (m.sharedMesh != null && CurrentCombinerState != CombinerState.UsedPreBuitMeshes)
                        {
                            m.sharedMesh.Clear();
                            m.sharedMesh.ClearBlendShapes();
                        }
                        if (CurrentCombinerState == CombinerState.UsedPreBuitMeshes)
                            m.sharedMesh = null;

                        m.sharedMaterials = new Material[0];
                        m.gameObject.SetActive(false);
                    });
                }
                else
                {
                    cm.skinnedMesh.ForEach((m) =>
                    {
                        if(m.sharedMesh != null)
                        m.gameObject.SetActive(true);
                    });
                }
            }
            foreach (var ca in clothesAnchors)
            {
                ca.skinnedMesh.ForEach((m) =>
                {
                    if(m.sharedMesh != null)
                    m.gameObject.SetActive(true);
                });
            }
            CurrentCombinerState = CombinerState.NotCombined;
            RecalculateShapes();
            RecalculateLOD();
            Resources.UnloadUnusedAssets();
            ApplyPrefab();
        }

        /// <summary>
        /// Recalculate LODs
        /// </summary>
        public void RecalculateLOD()
        {
            if (!_lodGroup && MinLODLevels != MaxLODLevels)
                _lodGroup = GetComponent<LODGroup>();
            else if(MinLODLevels == MaxLODLevels)
                return;

            LOD[] lods;
            float[][] lods_p = new float[][] {
            new float[] {0.5f, 0.2f, 0.05f, 0f },
            new float[] {0.4f, 0.1f, 0f, 0f },
            new float[] {0.3f,   0f,   0f, 0f },
            new float[] {0f,   0f,   0f, 0f }
        };


            lods = new LOD[(MaxLODLevels - MinLODLevels) + 1];
            for (int i = 0; i < (MaxLODLevels - MinLODLevels) + 1; i++)
            {
                if (clothesAnchors.ElementAtOrDefault(i) != null)
                {
                    var renderer = new List<SkinnedMeshRenderer>();
                    foreach (var cp in characterParts)
                    {
                        renderer.Add(cp.skinnedMesh[i]);
                    }
                    foreach (var ca in clothesAnchors)
                    {
                        renderer.Add(ca.skinnedMesh[i]);
                    }
                    lods[i] = new LOD(lods_p[3 - (MaxLODLevels - MinLODLevels)][i], renderer.ToArray());
                }

            }
            

            _lodGroup.SetLODs(lods);
            _lodGroup.RecalculateBounds();
        }
        /// <summary>
        /// Change the number of LODs
        /// </summary>
        /// <param name="minLod">Lower LOD</param>
        /// <param name="maxLod">Higher LOD</param>
        public void SetLODRange(int minLod, int maxLod)
        {
            if (IsBaked())
                return;
            
            MinLODLevels = minLod;
            MaxLODLevels = maxLod;
            InitializeMeshes();
        }
        /// <summary>
        ///  Get combined state
        /// </summary>
        /// <returns></returns>
        public bool IsBaked()
        {
            return (CurrentCombinerState == CombinerState.Combined || CurrentCombinerState == CombinerState.UsedPreBuitMeshes);
        }

        public bool IsBakedPreBuiltAndBakedBlendshapes()
        {
            return (CurrentCombinerState == CombinerState.UsedPreBuitMeshes && !preBuiltMeshesHasBlendshapes);
        }
        /// <summary>
        /// Get element preset
        /// </summary>
        /// <param name="type">Preset type</param>
        /// <param name="index">Preset index</param>
        /// <returns></returns>
        public CharacterElementsPreset GetElementsPreset(CharacterElementType type, int index)
        {
            var presets = GetElementsPresets(type);
            return (presets.Count > 0 && presets.Count-1 >= index && index != -1) ? presets[index] : null;
        }
        /// <summary>
        /// Get element preset
        /// </summary>
        /// <param name="type">Preset type</param>
        /// <param name="name">Preset name</param>
        /// <returns></returns>
        public CharacterElementsPreset GetElementsPreset(CharacterElementType type, string name)
        {
            var presets = GetElementsPresets(type);
            return (presets.Count > 0) ? presets.Find(f => f.name == name) : null;
        }

        /// <summary>
        /// Get all presets by type
        /// </summary>
        public List<CharacterElementsPreset> GetElementsPresets(CharacterElementType type)
        {
            switch (type)
            {
                case CharacterElementType.Hat:
                    return _settings.hatsPresets;
                case CharacterElementType.Shirt:
                    return _settings.shirtsPresets;
                case CharacterElementType.Pants:
                    return _settings.pantsPresets;
                case CharacterElementType.Shoes:
                    return _settings.shoesPresets;
                case CharacterElementType.Accessory:
                    return _settings.accessoryPresets;
                case CharacterElementType.Hair:
                    return _settings.hairPresets;
                case CharacterElementType.Beard:
                    return _settings.beardPresets;
                case CharacterElementType.Item1:
                    return _settings.item1Presets;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Play animation
        /// </summary>
        /// <param name="animationName">Animation name</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="weightPower">Animation power</param>
        public void PlayBlendshapeAnimation(string animationName, float duration = 1f, float weightPower = 1f)
        {
            if (currentBlendshapeAnimation != null)
                StopBlendshapeAnimations();

            var blendshapeAnimation = new CurrentBlendshapeAnimation();
            foreach (var ep in _settings.characterAnimationPresets)
            {
                if (ep.name == animationName)
                {
                    blendshapeAnimation.preset = ep;
                    break;
                }
            }
            foreach (var blendshapeValue in blendshapeAnimation.preset.blendshapes)
            {
                var blendshapeData = GetBlendshapeData(blendshapeValue.BlendType);
                if(blendshapeData != null)
                    blendshapeAnimation.blendShapesTemp.Add(new BlendshapeEmotionValue() { BlendType = blendshapeValue.BlendType, BlendValue = blendshapeData.value });
            }
            blendshapeAnimation.preset.AnimationPlayDuration = 1.0f / duration;
            blendshapeAnimation.preset.weightPower = weightPower;
            currentBlendshapeAnimation = blendshapeAnimation;
        }
        /// <summary>
        /// Stop any animations
        /// </summary>
        public void StopBlendshapeAnimations()
        {
            if (currentBlendshapeAnimation != null)
            {
                for (var i = 0; i < currentBlendshapeAnimation.preset.blendshapes.Count; i++)
                {
                    SetBlendshapeValue(currentBlendshapeAnimation.preset.blendshapes[i].BlendType, currentBlendshapeAnimation.blendShapesTemp[i].BlendValue);
                }
            }
        }

        /// <summary>
        /// Reset all character changes
        /// </summary>
        /// <param name="ignore_settingsDefaultElements">Ignore default element from settings</param>
        public void ResetAll(bool ignore_settingsDefaultElements = true)
        {
            //StartupSerializationApplied = string.Empty;
            ResetBodyColors();

            foreach (var blendshape in characterBlendshapeDatas)
                SetBlendshapeValue(blendshape.type, 0);

            
            SetHeadSize(0);
            SetHeight(0);

            foreach (CharacterElementType type in Enum.GetValues(typeof(CharacterElementType)).Cast<CharacterElementType>().ToList())
            {
                SetElementByIndex(type, -1);
            }

            characterSelectedElements = (ignore_settingsDefaultElements) ? new CharacterSelectedElements() : (CharacterSelectedElements)_settings.DefaultSelectedElements.Clone();

            if (!ignore_settingsDefaultElements)
            {
                foreach(CharacterElementType type in Enum.GetValues(typeof(CharacterElementType)).Cast<CharacterElementType>().ToList())
                {
                    var index = characterSelectedElements.GetSelectedIndex(type);
                    if (index != -1)
                    {
                        SetElementByIndex(type, index);
                    }
                }
            }
        }
        /// <summary>
        /// Randomize character customization
        /// </summary>
        public void Randomize()
        {
            if (IsBaked())
                ClearBake();
            CharacterGenerator.Generate(this);
        }
        /// <summary>
        /// Get character animator
        /// </summary>
        /// <returns></returns>
        public Animator GetAnimator()
        {
            return animator;
        }
        public void UnlockPrefab()
        {
            if (Application.isPlaying)
                return;

#if UNITY_EDITOR

            GameObject contentsRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            if (contentsRoot != null)
                PrefabUtility.UnpackPrefabInstance(contentsRoot, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            
#endif
        }
        public void LockPrefab(string custompath = "")
        {
            if (Application.isPlaying)
                return;

#if UNITY_EDITOR

            if (prefabPath != string.Empty || custompath != string.Empty && !Application.isPlaying)
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, (custompath != string.Empty) ? custompath : prefabPath, InteractionMode.UserAction);
                if (custompath != string.Empty)
                    prefabPath = custompath;
                else if (prefabPath == string.Empty)
                {
                    prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                }
            }
            else
                EditorUtility.SetDirty(gameObject);
#endif
        }
        public void ApplyPrefab()
        {
            if (Settings != null)
            {
                if (!applyFeetOffset)
                    SetFeetOffset(Vector3.zero);
                else
                    SetFeetOffset(new Vector3(0, feetOffset, 0));
            }
#if UNITY_EDITOR
            if (prefabPath == string.Empty || prefabPath.Length == 0 && instanceStatus != CharacterInstanceStatus.NotAPrefabByUser)
                prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
#endif
            ResetBodyMaterial();

#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Saving prefab changes", "Process", 1f);

            if (currentBlendshapeAnimation != null)
            {
                currentBlendshapeAnimation.timer = 1.0f;
                AnimationTick();
                currentBlendshapeAnimation = null;
            }

            if (prefabPath != string.Empty && instanceStatus != CharacterInstanceStatus.NotAPrefabByUser 
                && instanceStatus != CharacterInstanceStatus.PrefabEditingInProjectView 
                && instanceStatus != CharacterInstanceStatus.PrefabStageSceneOpened 
                && instanceStatus != CharacterInstanceStatus.MissingAsset 
                && !Application.isPlaying)
            {
                var path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                if (path != string.Empty && path != prefabPath)
                    prefabPath = path;
                PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.UserAction);
            }
            else
                EditorUtility.SetDirty(gameObject);
            EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// Revert all bones transformations
        /// </summary>
        public void RevertBonesChanges()
        {

            var meshOriginal = Settings.OriginalMesh.GetComponentsInChildren<SkinnedMeshRenderer>()[0];
            var allBones = meshOriginal.bones;
            var rootBone = meshOriginal.rootBone;

            var bonesThis = GetCharacterPart("Head").skinnedMesh[0].bones;

            originHip.localPosition = new Vector3(0,feetOffset,0);


            for (int i = 0; i < bonesThis.Length; i++)
            {
                bonesThis[i].localPosition = allBones[i].localPosition;
                bonesThis[i].localRotation = allBones[i].localRotation;
            }
        }
        public void ApplyPrefabInPlaymode()
        {
#if UNITY_EDITOR
            if (IsBaked())
            {
                Debug.LogError("Can't save the character because the character is combined");
                return;
            }
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            CharacterCustomization characterSystem = contentsRoot.GetComponent<CharacterCustomization>();
            Transform[] bonesOriginal = characterSystem.GetCharacterPart("HEAD").skinnedMesh[0].bones;

            GameObject saveGO = gameObject;

            CharacterCustomization characterSystemSave = saveGO.GetComponent<CharacterCustomization>();

            Transform[] bones = characterSystemSave.GetCharacterPart("HEAD").skinnedMesh[0].bones;
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].localPosition = bonesOriginal[i].localPosition;
                bones[i].localRotation = bonesOriginal[i].localRotation;
            }
            PrefabUtility.SaveAsPrefabAsset(saveGO, prefabPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
#endif
        }

        /// <summary>
        /// Update actual character instance status
        /// </summary>
        public void UpdateActualCharacterInstanceStatus(bool igroneUserNonPrefab = false)
        {
            if (Application.isPlaying || (instanceStatus == CharacterInstanceStatus.NotAPrefabByUser && !igroneUserNonPrefab))
                return;
#if UNITY_EDITOR
            bool stageOpened = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
            bool editingInProjectView = (gameObject.scene.name == null);

            if (!stageOpened && !editingInProjectView)
            {
                PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                instanceStatus = (CharacterInstanceStatus)prefabStatus;
            }
            else
            {
                if (stageOpened)
                    instanceStatus = CharacterInstanceStatus.PrefabStageSceneOpened;
                if (editingInProjectView)
                    instanceStatus = CharacterInstanceStatus.PrefabEditingInProjectView;
            }
            if (prefabPath != string.Empty && instanceStatus == CharacterInstanceStatus.NotAPrefab)
            {
                instanceStatus = CharacterInstanceStatus.NotAPrefabByUser;
            }
#endif
        }

        /// <summary>
        /// Get current character instance status
        /// </summary>
        /// <returns></returns>
        public CharacterInstanceStatus GetCharacterInstanceStatus()
        {
            return instanceStatus;
        }

        /// <summary>
        /// Set new character instance status
        /// </summary>
        /// <param name="characterInstanceStatus"></param>
        public void SetNewCharacterInstanceStatus(CharacterInstanceStatus characterInstanceStatus)
        {
            instanceStatus = characterInstanceStatus;
        }

        /// <summary>
        /// Get character blendshape data by type
        /// </summary>
        public CharacterBlendshapeData GetBlendshapeData(CharacterBlendShapeType type)
        {
            foreach (var bData in characterBlendshapeDatas)
                if (bData.type == type)
                    return bData;

            return null;
        }
        /// <summary>
        /// Get character blendshape data by blendshape name
        /// </summary>
        public CharacterBlendshapeData GetBlendshapeData(string name)
        {
            foreach (var bData in characterBlendshapeDatas)
                if (bData.blendshapeName == name)
                    return bData;

            return null;
        }
        /// <summary>
        /// Get character blendshape data by group
        /// </summary>
        public List<CharacterBlendshapeData> GetBlendshapeDatasByGroup(CharacterBlendShapeGroup group)
        {
            List<CharacterBlendshapeData> returnDatas = new List<CharacterBlendshapeData>();
            foreach (var bData in characterBlendshapeDatas)
                if (bData.group == group)
                    returnDatas.Add(bData);

            return returnDatas;
        }

        private void DestroyObjects(UnityEngine.Object[] objects)
        {
            foreach (var go in objects)
            {
                if (go != null)
                {
#if UNITY_EDITOR
                    UnityEngine.Object.DestroyImmediate(go);
#else
                    UnityEngine.Object.Destroy(go);
#endif
                }
            }
        }
        #endregion 
    }
    #region Basic classes and enum
    public enum CharacterElementType : int
    {
        Hat,
        Shirt,
        Pants,
        Shoes,
        Accessory,
        Hair,
        Beard,
        Item1
    }
    public enum CombinerState : byte
    {
        NotCombined,
        InProgressCombineMesh,
        InProgressBlendshapeTransfer,
        InProgressClear,
        Combined,
        UsedPreBuitMeshes
    }
    public enum BodyColorPart : int
    {
        Skin,
        Eye,
        Hair,
        Underpants,
        OralCavity,
        Teeth
    }

    [System.Serializable]
    public class CharacterPart
    {
        public string name;
        public List<SkinnedMeshRenderer> skinnedMesh;
        public CharacterPart()
        {
            skinnedMesh = new List<SkinnedMeshRenderer>();
        }
    }
    [System.Serializable]
    public class ClothesAnchor
    {
        public CharacterElementType partType;
        public List<SkinnedMeshRenderer> skinnedMesh;
        public ClothesAnchor()
        {
            skinnedMesh = new List<SkinnedMeshRenderer>();
        }
    }

    [System.Serializable]
    public class BlendshapeEmotionValue
    {
        public CharacterBlendShapeType BlendType;
        [Range(-100f, 100f)]
        public float BlendValue;
        public AnimationCurve BlendAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
    }

    public class CurrentBlendshapeAnimation
    {
        public CharacterAnimationPreset preset;
        public List<BlendshapeEmotionValue> blendShapesTemp = new List<BlendshapeEmotionValue>();
        public float timer = 0;
    }

    #endregion

}