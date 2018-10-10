using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    [System.Serializable]
    public class PrefabSettings
    {
        /// <summary>
        /// The prefab which should be instanted and placed at the brush position
        /// </summary>
        [HideInInspector]
        public GameObject prefab;

        /// <summary>
        /// Whether the prefab is used or not
        /// </summary>
        public bool active = true;

        /// <summary>
        /// The probability at which the prefab is chosen to be instantiated.
        /// This value is relative to all other prefabs.
        /// So 0 doesn't mean it won't be instantiated at all, it means it's less probable
        /// to be instantiated than others which don't have 0.
        /// Ranges from 0 (not probable at all) to 1 (highest probability).
        /// The value is relative. If all a
        /// </summary>
        public float probability = 1;

        /// <summary>
        /// The offset that should be added to the instantiated gameobjects position
        /// </summary>
        [HideInInspector]
        public Vector3 positionOffset;

        /// <summary>
        /// Randomize rotation
        /// </summary>
        [HideInInspector]
        public bool randomRotation;

        /// <summary>
        /// Randomize Scale Minimum
        /// </summary>
        [HideInInspector]
        public bool randomScale = false;

        /// <summary>
        /// Randomize Scale Minimum
        /// </summary>
        [HideInInspector]
        public float randomScaleMin = 0.5f;

        /// <summary>
        /// Randomize Scale Maximum
        /// </summary>
        [HideInInspector]
        public float randomScaleMax = 1.5f;


        public PrefabSettings Clone()
        {
            return (PrefabSettings)this.MemberwiseClone();
        }
    }
}
