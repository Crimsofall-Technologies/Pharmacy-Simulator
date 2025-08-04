using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AdvancedPeopleSystem
{
    [CreateAssetMenu(fileName = "NewCharacterGenerator", menuName = "Advanced People Pack/CharacterGenerator", order = 1)]
    public class CharacterGeneratorSettings : ScriptableObject
    {

        public MinMaxIndex hair;
        public MinMaxIndex beard;
        public MinMaxIndex hat;
        public MinMaxIndex accessory;
        public MinMaxIndex shirt;
        public MinMaxIndex pants;
        public MinMaxIndex shoes;
        [Space(10)]
        public MinMaxColor skinColors = new MinMaxColor();
        public MinMaxColor eyeColors = new MinMaxColor();
        public MinMaxColor hairColors = new MinMaxColor();
        [Space(10)]
        public MinMaxBlendshapes headSize;
        public MinMaxBlendshapes headOffset;
        public MinMaxBlendshapes height;
        public MinMaxBlendshapes fat;
        public MinMaxBlendshapes muscles;
        public MinMaxBlendshapes thin;
        public MinMaxBlendshapes slimness;
        public MinMaxBlendshapes breastSize;
        [Space(15)]
        public List<MinMaxFacialBlendshapes> facialBlendshapes = new List<MinMaxFacialBlendshapes>();
        [Space(15)]
        public List<GeneratorExclude> excludes = new List<GeneratorExclude>();      
    }
    [System.Serializable]
    public class MinMaxIndex
    {
        public int Min;
        public int Max;
        public int GetRandom(int max)
        {
            var v = Random.Range(Min, Max);
            return Mathf.Clamp(v, -1, max);
        }
    }
    [System.Serializable]
    public class MinMaxColor
    {
        public List<Color> minColors = new List<Color>();
        public List<Color> maxColors = new List<Color>();

        public Color GetRandom()
        {
            var index = Random.Range(0, minColors.Count);

            return Color.Lerp(minColors[index], maxColors[index], Random.Range(0f, 1f));
        }
    }
    [System.Serializable]
    public class MinMaxBlendshapes
    {
        [Range(-100, 100)]
        public float Min;
        [Range(-100, 100)]
        public float Max;
        public float GetRandom()
        {
            return Random.Range(Min, Max);
        }
    }
    public enum ExcludeItem
    {
        Hair,
        Beard,
        Hat,
        Accessory,
        Shirt,
        Pants,
        Shoes
    }
    [System.Serializable]
    public class ExcludeIndexes
    {
        public ExcludeItem item;
        public int index;
    }
    [System.Serializable]
    public class GeneratorExclude
    {
        public ExcludeItem ExcludeItem;
        public int targetIndex;
        public List<ExcludeIndexes> exclude = new List<ExcludeIndexes>();
    }
    [System.Serializable]
    public class MinMaxFacialBlendshapes
    {
        public string name;
        [Range(-100, 100)]
        public float Min;
        [Range(-100, 100)]
        public float Max;

        public float GetRandom()
        {
            return Random.Range(Min, Max);
        }
    }
}