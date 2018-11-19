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
        /// The offset that should be added to the instantiated gameobjects position.
        /// This is useful to correct the position of prefabs. 
        /// It's also useful in combination with the physics module in order to let e. g. pumpkins fall naturally on the terrain.
        /// </summary>
        public Vector3 positionOffset;

        /// <summary>
        /// The offset that should be added to the instantiated gameobjects rotation.
        /// This is useful to correct the rotation of prefabs.
        /// The offset is Vector3, uses Eulers.
        /// </summary>
        public Vector3 rotationOffset;

        /// <summary>
        /// Randomize rotation
        /// </summary>
        [HideInInspector]
        public bool randomRotation;

        /// <summary>
        /// Randomize Scale Minimum
        /// </summary>
        [HideInInspector]
        public bool changeScale = false;

        /// <summary>
        /// Randomize Scale Minimum
        /// </summary>
        [HideInInspector]
        public float scaleMin = 0.5f;

        /// <summary>
        /// Randomize Scale Maximum
        /// </summary>
        [HideInInspector]
        public float scaleMax = 1.5f;

        /// <summary>
        /// Storing asset GUID here for future reference (performance reasons)
        /// </summary>
        public string assetGUID = null;

        /// <summary>
        /// Vegetation Studio Pro vspro_VegetationItemID
        /// </summary>
        public string vspro_VegetationItemID = null;

        public PrefabSettings Clone()
        {
            return (PrefabSettings)this.MemberwiseClone();
        }
    }
}
