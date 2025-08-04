using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    [Serializable]
    public class PreBuiltBlendshape
    {
        [SerializeField] public string name;
        [SerializeField] public float weight;
        public PreBuiltBlendshape(string name, float weight)
        {
            this.name = name;
            this.weight = weight;
        }
    }
}
