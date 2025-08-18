using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace AdvancedPeopleSystem
{

    [System.Serializable]
    public class CharacterCustomizationSetup
    {

        public string settingsName;


        public CharacterSelectedElements selectedElements = new CharacterSelectedElements();

        public List<CharacterBlendshapeData> blendshapes = new List<CharacterBlendshapeData>();

        public int MinLod;
        public int MaxLod;
        
        public float[] SkinColor,
                     EyeColor,
                     HairColor,
                     UnderpantsColor,
                     TeethColor,
                     OralCavityColor;

        public float Height;
        public float HeadSize;

        public CharacterCustomizationSetup() { }

        /// <summary>
        /// Apply character data to the specified character 
        /// </summary>
        /// <param name="cc">The character to which you want to apply the data</param>
        public void ApplyToCharacter(CharacterCustomization cc)
        {
            if(cc.Settings == null && settingsName != cc.Settings.name)
            {
                Debug.LogError("Character settings are not compatible with saved data");
                return;
            }

            cc.SetBodyColor(BodyColorPart.Skin, new Color(SkinColor[0], SkinColor[1], SkinColor[2], SkinColor[3]));
            cc.SetBodyColor(BodyColorPart.Eye, new Color(EyeColor[0], EyeColor[1], EyeColor[2], EyeColor[3]));
            cc.SetBodyColor(BodyColorPart.Hair, new Color(HairColor[0], HairColor[1], HairColor[2], HairColor[3]));
            cc.SetBodyColor(BodyColorPart.Underpants, new Color(UnderpantsColor[0], UnderpantsColor[1], UnderpantsColor[2], UnderpantsColor[3]));
            cc.SetBodyColor(BodyColorPart.Teeth, new Color(TeethColor[0], TeethColor[1], TeethColor[2], TeethColor[3]));
            cc.SetBodyColor(BodyColorPart.OralCavity, new Color(OralCavityColor[0], OralCavityColor[1], OralCavityColor[2], OralCavityColor[3]));

            cc.SetElementByIndex(CharacterElementType.Hair, selectedElements.Hair);
            cc.SetElementByIndex(CharacterElementType.Accessory, selectedElements.Accessory);
            cc.SetElementByIndex(CharacterElementType.Hat, selectedElements.Hat);
            cc.SetElementByIndex(CharacterElementType.Pants, selectedElements.Pants);
            cc.SetElementByIndex(CharacterElementType.Shoes, selectedElements.Shoes);
            cc.SetElementByIndex(CharacterElementType.Shirt, selectedElements.Shirt);
            cc.SetElementByIndex(CharacterElementType.Beard, selectedElements.Beard);
            cc.SetElementByIndex(CharacterElementType.Item1, selectedElements.Item1);

            cc.SetHeight(Height);
            cc.SetHeadSize(HeadSize);

            foreach (var blendshape in blendshapes)
            {
                cc.SetBlendshapeValue(blendshape.type, blendshape.value);
            }
            cc.ApplyPrefab();

        }
        #region Serializer
        public string Serialize(CharacterFileSaveFormat format)
        {
            string data = string.Empty;
            switch (format)
            {
                case CharacterFileSaveFormat.Json:
                    data = JsonUtility.ToJson(this, true);
                    break;
                case CharacterFileSaveFormat.Xml:
                    XmlSerializer serializer = new XmlSerializer(typeof(CharacterCustomizationSetup));
                    using (StringWriter textWriter = new StringWriter())
                    {
                        serializer.Serialize(textWriter, this);
                        data = textWriter.ToString();
                    }
                    break;
                case CharacterFileSaveFormat.Binary:
                    using (MemoryStream stream = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(stream, this);
                        data = Convert.ToBase64String(stream.ToArray());
                    }
                    break;
            }
            return data;
        }
        public static CharacterCustomizationSetup Deserialize(string data, CharacterFileSaveFormat format)
        {
            CharacterCustomizationSetup characterDataLoader = null;
            switch (format)
            {
                case CharacterFileSaveFormat.Json:
                    characterDataLoader = JsonUtility.FromJson<CharacterCustomizationSetup>(data);
                    break;
                case CharacterFileSaveFormat.Xml:
                    XmlSerializer serializer = new XmlSerializer(typeof(CharacterCustomizationSetup));
                    using (StringReader textReader = new StringReader(data))
                    {
                        characterDataLoader = (CharacterCustomizationSetup)serializer.Deserialize(textReader);
                    }
                    break;
                case CharacterFileSaveFormat.Binary:
                    using (MemoryStream textReader = new MemoryStream(Convert.FromBase64String(data)))
                    {
                        characterDataLoader = (CharacterCustomizationSetup)new BinaryFormatter().Deserialize(textReader);
                    }
                    break;
            }
            return characterDataLoader;
        }

        #endregion

        public enum CharacterFileSaveFormat
        {
            Json,
            Xml,
            Binary
        }
    }
}