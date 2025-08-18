using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    public class CharacterPreBuilt : ScriptableObject
    {
        [SerializeField] public CharacterSettings settings;
        [SerializeField] public List<PreBuiltData> preBuiltDatas = new List<PreBuiltData>();
        [SerializeField] public List<PreBuiltBlendshape> blendshapes = new List<PreBuiltBlendshape>();
    }
}

