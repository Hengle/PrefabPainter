using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yapp
{
    [System.Serializable]
    public class SplineSettings
    {

        public enum AttachMode
        {
            Bounds,
            Between
        }

        public enum Rotation
        {
            Spline,
            Prefab
        }

        public enum Separation
        {
            Fixed,
            Range,
            PrefabBounds
        }

        [Range (0,10)]
        public int curveResolution = 0;
        public bool loop = false;

        public Separation separation = Separation.Fixed;

        /// <summary>
        /// Fixed separation distance
        /// </summary>
        public float separationDistance = 1f;

        /// <summary>
        /// Additional separation distance minimum when using Range or Bounds
        /// </summary>
        public float separationDistanceMin = 0f;

        /// <summary>
        /// Additional separation distance maximum when using Range or Bounds
        /// </summary>
        public float separationDistanceMax = 1f;

        public Rotation instanceRotation = Rotation.Prefab;

        public AttachMode attachMode = AttachMode.Bounds;
        public bool controlPointRotation = false;

        [Range (1,10)]
        public int lanes = 1;
        public float laneDistance = 1;
        public bool skipCenterLane = false;

        /// <summary>
        /// Snap to the closest gameobject / terrain up or down relative to the spline controlpoint position
        /// </summary>
        public bool snap = false;

        /// <summary>
        /// Recreate the prefabs on every change of the spline
        /// </summary>
        public bool reusePrefabs = true;

        public bool debug = false;

        // internal properties
        public bool dirty = false;

        public List<GameObject> prefabInstances = new List<GameObject>();

        public List<ControlPoint> controlPoints = new List<ControlPoint>();
    }
}
