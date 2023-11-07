using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Items {
    public interface PlantableItem {
        public string PrefabPath { get; }
        public List<Tuple<float, string>> GrowPhase { get; }
        public void Plant();
    }
}