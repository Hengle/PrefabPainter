using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineSettings {

    public List<GameObject> prefabInstances = new List<GameObject>();
    public List<Transform> controlPoints = new List<Transform>();

    public int curveResolution = 0;
    public bool loop = false;
    public float distanceBetweenObjects = 1f;
    public bool rotateInstance = true;

}
