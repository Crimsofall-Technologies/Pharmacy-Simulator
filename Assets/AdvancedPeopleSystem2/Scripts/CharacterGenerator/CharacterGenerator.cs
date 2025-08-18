using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    public static class CharacterGenerator
    {
        public static void Generate(CharacterCustomization cc)
        {
            int CheckExclude(int hair, int beard, int hat, int accessory, int shirt, int pants, int shoes, List<ExcludeIndexes> excludeIndexes)
            {
                int out_index = 0;
                if (excludeIndexes.Count == 0)
                {
                    out_index = -1;
                }
                else
                {
                    foreach (var e in excludeIndexes)
                    {
                        if (e.item == ExcludeItem.Hair && hair == e.index)
                        {
                            out_index = -1;
                            break;
                        }

                        if (e.item == ExcludeItem.Beard && beard == e.index)
                        {
                            out_index = -1;
                            break;
                        }

                        if (e.item == ExcludeItem.Hat && hat == e.index)
                        {
                            out_index = -1;
                            break;
                        }
                        if (e.item == ExcludeItem.Accessory && accessory == e.index)
                        {
                            out_index = -1;
                            break;
                        }

                        if (e.item == ExcludeItem.Shirt && shirt == e.index)
                        {
                            out_index = -1;
                            break;
                        }

                        if (e.item == ExcludeItem.Pants && pants == e.index)
                        {
                            out_index = -1;
                            break;
                        }
                        if (e.item == ExcludeItem.Shoes && shoes == e.index)
                        {
                            out_index = -1;
                            break;
                        }
                    }
                }
                return out_index;
            }
            var generatorSettings = cc.Settings.generator;

            int hairIndex = generatorSettings.hair.GetRandom(cc.Settings.hairPresets.Count);
            int beardIndex = generatorSettings.beard.GetRandom(cc.Settings.beardPresets.Count);
            int hatIndex = generatorSettings.hat.GetRandom(cc.Settings.hatsPresets.Count);
            int accessoryIndex = generatorSettings.accessory.GetRandom(cc.Settings.accessoryPresets.Count);
            int shirtIndex = generatorSettings.shirt.GetRandom(cc.Settings.shirtsPresets.Count);
            int pantsIndex = generatorSettings.pants.GetRandom(cc.Settings.pantsPresets.Count);
            int shoesIndex = generatorSettings.shoes.GetRandom(cc.Settings.shoesPresets.Count);
            float headSize = generatorSettings.headSize.GetRandom();
            float headOffset = generatorSettings.headOffset.GetRandom();
            float height = generatorSettings.height.GetRandom();

            foreach (var exclude in generatorSettings.excludes)
            {
                var r = CheckExclude(hairIndex, beardIndex, hatIndex, accessoryIndex, shirtIndex, pantsIndex, shoesIndex, exclude.exclude);
                if (r == -1)
                {
                    if (exclude.ExcludeItem == ExcludeItem.Hair && hairIndex == exclude.targetIndex)
                    {
                        hairIndex = -1;
                    }
                    if (exclude.ExcludeItem == ExcludeItem.Beard && beardIndex == exclude.targetIndex)
                    {
                        beardIndex = -1;
                    }
                    if (exclude.ExcludeItem == ExcludeItem.Hat && hatIndex == exclude.targetIndex)
                    {
                        hatIndex = -1;
                    }
                    if (exclude.ExcludeItem == ExcludeItem.Accessory && accessoryIndex == exclude.targetIndex)
                    {
                        accessoryIndex = -1;
                    }
                    if (exclude.ExcludeItem == ExcludeItem.Shirt && shirtIndex == exclude.targetIndex)
                    {
                        shirtIndex = -1;
                    }
                    if (exclude.ExcludeItem == ExcludeItem.Pants && pantsIndex == exclude.targetIndex)
                    {
                        pantsIndex = -1;
                    }
                    if (exclude.ExcludeItem == ExcludeItem.Shoes && shoesIndex == exclude.targetIndex)
                    {
                        shoesIndex = -1;
                    }
                }
            }

            cc.SetHeadSize(headSize);
            cc.SetHeight(height);

            cc.SetBlendshapeValue(CharacterBlendShapeType.Fat, generatorSettings.fat.GetRandom());

            if (cc.Settings.characterBlendshapeDatas.Exists(x => x.blendshapeName == "Muscles"))
            {
                if (cc.GetBlendshapeData(CharacterBlendShapeType.Fat).value <= 50)
                    cc.SetBlendshapeValue(CharacterBlendShapeType.Muscles, generatorSettings.muscles.GetRandom());
                else
                    cc.SetBlendshapeValue(CharacterBlendShapeType.Muscles, generatorSettings.muscles.GetRandom() * 0.25f);
            }
            if (cc.Settings.characterBlendshapeDatas.Exists(x => x.blendshapeName == "Thin"))
            {
                if (cc.GetBlendshapeData(CharacterBlendShapeType.Fat).value <= 25)
                    cc.SetBlendshapeValue(CharacterBlendShapeType.Thin, generatorSettings.thin.GetRandom());
                else
                    cc.SetBlendshapeValue(CharacterBlendShapeType.Thin, generatorSettings.thin.GetRandom() * 0.25f);
            }
            if (cc.Settings.characterBlendshapeDatas.Exists(x => x.blendshapeName == "Slimness"))
            {
                if (cc.GetBlendshapeData(CharacterBlendShapeType.Fat).value <= 25)
                    cc.SetBlendshapeValue(CharacterBlendShapeType.Slimness, generatorSettings.slimness.GetRandom());
                else
                    cc.SetBlendshapeValue(CharacterBlendShapeType.Slimness, generatorSettings.slimness.GetRandom() * 0.25f);
            }
            if (cc.Settings.characterBlendshapeDatas.Exists(x => x.blendshapeName == "BreastSize"))
            {
                if (cc.GetBlendshapeData(CharacterBlendShapeType.Fat).value <= 50)
                    cc.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, generatorSettings.breastSize.GetRandom());
                else
                    cc.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, generatorSettings.breastSize.GetRandom() * 0.25f);
            }

            cc.SetElementByIndex(CharacterElementType.Hair, hairIndex);
            cc.SetElementByIndex(CharacterElementType.Beard, beardIndex);
            cc.SetElementByIndex(CharacterElementType.Accessory, accessoryIndex);
            cc.SetElementByIndex(CharacterElementType.Shirt, shirtIndex);
            cc.SetElementByIndex(CharacterElementType.Pants, pantsIndex);
            cc.SetElementByIndex(CharacterElementType.Shoes, shoesIndex);
            cc.SetElementByIndex(CharacterElementType.Hat, hatIndex);

            cc.SetBodyColor(BodyColorPart.Skin, generatorSettings.skinColors.GetRandom());
            cc.SetBodyColor(BodyColorPart.Hair, generatorSettings.hairColors.GetRandom());
            cc.SetBodyColor(BodyColorPart.Eye, generatorSettings.eyeColors.GetRandom());

            cc.SetBlendshapeValue(CharacterBlendShapeType.Head_Offset, headOffset);

            foreach (var fbs in generatorSettings.facialBlendshapes)
            {
                if (Enum.TryParse(fbs.name, out CharacterBlendShapeType faceShapeType))
                    cc.SetBlendshapeValue(faceShapeType, fbs.GetRandom());
            }
        }
    }
}