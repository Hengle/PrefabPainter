using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    public class SplineSettings
    {

        public enum AttachMode
        {
            Bounds,
            Between
        }

        public enum Rotation
        {
            Prefab,
            Spline,
            Identity
        }

        public List<GameObject> prefabInstances = new List<GameObject>();
        public List<ControlPoint> controlPoints = new List<ControlPoint>();

        public int curveResolution = 0;
        public bool loop = false;
        public float distanceBetweenObjects = 1f;
        public Rotation instanceRotation = Rotation.Prefab;

        public bool dirty = false;
        public AttachMode attachMode = AttachMode.Bounds;
    }
}
