using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    [System.Serializable]
    public class BrushSettings
    {
        public enum Distribution
        {
            Center,
            Poisson
        }

        public float brushSize = 2.0f;
        public bool alignToTerrain = false;
        public Distribution distribution = Distribution.Center;

        /// <summary>
        /// The size of a disc in the poisson distribution.
        /// The smaller, the more discs will be inside the brush
        /// </summary>
        public float poissonDiscSize = 1.0f;

        /// <summary>
        /// Allow prefab overlaps or not.
        /// </summary>
        public bool allowOverlap = false;
    }
}
