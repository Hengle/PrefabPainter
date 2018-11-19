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
            Poisson,
            FallOff,
            FallOff2d
        }

        public float brushSize = 2.0f;

        [Range(0, 360)]
        public int brushRotation = 0;

        public bool alignToTerrain = false;
        public Distribution distribution = Distribution.Center;

        /// <summary>
        /// The size of a disc in the poisson distribution.
        /// The smaller, the more discs will be inside the brush
        /// </summary>
        public float poissonDiscSize = 1.0f;

        /// <summary>
        /// Falloff curve
        /// </summary>
        public AnimationCurve fallOffCurve = AnimationCurve.Linear(1, 1, 1, 1);

        public AnimationCurve fallOff2dCurveX = AnimationCurve.Linear(1, 1, 1, 1);
        public AnimationCurve fallOff2dCurveZ = AnimationCurve.Linear(1, 1, 1, 1);

        [Range(1,50)]
        public int curveSamplePoints = 10;

        // slope
        public float slopeMin = 0;
        public float slopeMinLimit = 0;
        public float slopeMax = 90;
        public float slopeMaxLimit = 90;

        /// <summary>
        /// Allow prefab overlaps or not.
        /// </summary>
        public bool allowOverlap = false;

        /// <summary>
        /// Optionally spawn into the Persistent Storage of Vegetation Studio Pro
        /// </summary>
        public bool spawnToVSPro = false;
    }
}
