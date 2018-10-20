using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    public class SplineModule
    {
        PrefabPainter prefabPainter;

        public SplineModule( PrefabPainter prefabPainter)
        {
            this.prefabPainter = prefabPainter;
        }

        public void OnDrawGizmos()
        {

            if (prefabPainter.mode != PrefabPainter.Mode.Spline)
                return;


            Vector3[] initialPoints = new Vector3[prefabPainter.splineSettings.controlPoints.Count];
            for (int i = 0; i < prefabPainter.splineSettings.controlPoints.Count; i++)
            {
                ControlPoint controlPoint = prefabPainter.splineSettings.controlPoints[i];

                initialPoints[i] = controlPoint.position;

                Gizmos.DrawSphere(initialPoints[i], 0.15f);
            }

            if (prefabPainter.splineSettings.controlPoints.Count < 2)
                return;


            IEnumerable<Vector3> spline = CreateSpline();
            IEnumerator iterator = spline.GetEnumerator();
            iterator.MoveNext();
            var lastPoint = initialPoints[0];
             
            while (iterator.MoveNext())
            {
                Gizmos.DrawLine(lastPoint, (Vector3)iterator.Current);
                lastPoint = (Vector3)iterator.Current;

                //prevent an infinite loop if we want our spline to loop
                if (lastPoint == initialPoints[0])
                    break;

            }

            // debug gizmos
            if( prefabPainter.splineSettings.debug)
            {
                foreach (GameObject prefab in prefabPainter.splineSettings.prefabInstances)
                {

                    Renderer renderer = prefab.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        // LOD case: renderer is in the children
                        renderer = prefab.GetComponentInChildren<Renderer>();
                    }

                    // https://docs.unity3d.com/ScriptReference/Renderer-bounds.html
                    Vector3 center = renderer.bounds.center;
                    float radius = renderer.bounds.extents.magnitude;

                    // sphere
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(center, radius);

                    // rectangle
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(center, renderer.bounds.size);


                }
            }
        }

        /// <summary>
        /// Create an enumerator for the spline points using the current spline settings
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Vector3> CreateSpline()
        {
            // this is how you'd use a bezier curve
            // it isn't useful though, catmullRom is more useful, it goes through the control points. leaving this code here in case someone has use for it
            /*
            IEnumerable<Vector3> bezier = InterpolateExt.NewBezier(InterpolateExt.EaseType.Linear, prefabPainter.splineSettings.controlPoints.ToArray(), prefabPainter.splineSettings.curveResolution);
            */
            
            IEnumerable<Vector3> catmullRom = InterpolateExt.NewCatmullRom(prefabPainter.splineSettings.controlPoints.ToArray(), prefabPainter.splineSettings.curveResolution, prefabPainter.splineSettings.loop);

            return catmullRom;
        }

        private class SplinePoint
        {
            public Vector3 position;
            public int startControlPointIndex;
        }

        private void RemoveAllPrefabInstances()
        {
            foreach (GameObject prefab in prefabPainter.splineSettings.prefabInstances)
            {
                PrefabPainter.DestroyImmediate(prefab);
            }

            prefabPainter.splineSettings.prefabInstances.Clear();

        }

        private void RemovePrefabInstances( int startIndex)
        {
            // startIndex might be -1 if there are no prefabs; prefabInstanceIndex would be -1
            // also needed if you delete control points and only 1 is left
            if (startIndex < 0)
            {
                RemoveAllPrefabInstances();
                return;
            }

            // clear existing prefabs
            for (int i = prefabPainter.splineSettings.prefabInstances.Count - 1; i >= startIndex; i--)
            {
                GameObject prefab = prefabPainter.splineSettings.prefabInstances[i];

                PrefabPainter.DestroyImmediate(prefab);

                prefabPainter.splineSettings.prefabInstances.RemoveAt( i);

            }
        }

        public void PlaceObjects()
        {
            // recreate and redistribute all prefabs if requested
            if (!prefabPainter.splineSettings.reusePrefabs)
            {
                RemoveAllPrefabInstances();
            }

            if (prefabPainter.splineSettings.controlPoints.Count < 2)
            {
                // remove all that's left in case control points were deleted
                RemoveAllPrefabInstances();

                // don't continue any further
                return;
            }

            // put the spline into a list of Vector3's instead of using the iterator
            IEnumerable<Vector3> spline = CreateSpline();
            IEnumerator iterator = spline.GetEnumerator();
            List<SplinePoint> splinePoints = new List<SplinePoint>();

            // limit the number of points; this has become necessary because with the loop there's an overlap multiple times
            // and this could result in an endless loop in the worst case
            int splinePointMaxIndex = (prefabPainter.splineSettings.curveResolution + 1) * prefabPainter.splineSettings.controlPoints.Count;
            int splinePointIndex = 0;
            int segmentIndex = 0;
            int controlPointIndex = 0;

            while (iterator.MoveNext() && splinePointIndex <= splinePointMaxIndex)
            {
                SplinePoint splinePoint = new SplinePoint();
                splinePoint.position = (Vector3)iterator.Current;
                splinePoint.startControlPointIndex = controlPointIndex;

                splinePoints.Add(splinePoint);

                splinePointIndex++;
                segmentIndex++;

                if (segmentIndex > prefabPainter.splineSettings.curveResolution)
                {
                    controlPointIndex++;
                    segmentIndex = 0;
                }
            }


            //distanceToMove represents how much farther we need to progress down the spline before we place the next object
            int nextSplinePointIndex = 1;

            //our current position on the spline
            Vector3 positionIterator = splinePoints[0].position;
           
            // the algorithm skips the first control point, so we need to manually place the first object
            Vector3 direction = (splinePoints[nextSplinePointIndex].position - positionIterator);

            int prefabInstanceIndex = -1;

            // new prefab
            GameObject prefab = AddPrefab(ref prefabInstanceIndex, positionIterator, direction, splinePoints, nextSplinePointIndex - 1);

            float distanceToMove = GetDistanceToMove( prefab);

            while (nextSplinePointIndex < splinePoints.Count)
            {
                direction = (splinePoints[nextSplinePointIndex].position - positionIterator);
                direction = direction.normalized;

                float distanceToNextPoint = Vector3.Distance(positionIterator, splinePoints[nextSplinePointIndex].position);

                if (distanceToNextPoint >= distanceToMove)
                {
                    positionIterator += direction * distanceToMove;

                    // new prefab
                    prefab = AddPrefab(ref prefabInstanceIndex, positionIterator, direction, splinePoints, nextSplinePointIndex - 1);

                    distanceToMove = GetDistanceToMove( prefab);
                }
                else
                {
                    distanceToMove -= distanceToNextPoint;
                    positionIterator = splinePoints[nextSplinePointIndex++].position;
                }

            }

            // remove prefab instances that aren't used anymore
            if( prefabPainter.splineSettings.reusePrefabs)
            {
                RemovePrefabInstances(prefabInstanceIndex);
            }
        }

        private float GetDistanceToMove( GameObject prefab)
        {
            float distanceToMove = 0;

            switch ( prefabPainter.splineSettings.separation)
            {
                case SplineSettings.Separation.Fixed:
                    distanceToMove = prefabPainter.splineSettings.separationDistance;
                    break;

                case SplineSettings.Separation.PrefabBounds:
                    return GetPrefabSize( prefab);

            }

            // don't return 0, we wouldn't want an endless loop because we can't advance further
            if( distanceToMove <= 0)
            {
                Debug.LogError("Distance to move is <= 0. Using 1");
                distanceToMove = 1;
            }

            return distanceToMove;

        }

        private float GetPrefabSize( GameObject prefab)
        {
            Renderer renderer = prefab.GetComponent<Renderer>();
            if (renderer == null)
            {
                // LOD case: renderer is in the children
                renderer = prefab.GetComponentInChildren<Renderer>();
            }

            // https://docs.unity3d.com/ScriptReference/Renderer-bounds.html
            //float radius = renderer.bounds.extents.magnitude;
            float radius = renderer.bounds.size.magnitude;

            return radius;
        }

        /// <summary>
        /// Add a new prefab to the spline
        /// </summary>
        // TODO: rewrite, use different approach: Use fake splines around the current spline. That way the separation distance doesn't matter
        //       and you could have small prefabs on the outer spline, and big prefabs on the inner spline and they would distribute better
        private GameObject AddPrefab( ref int prefabInstanceIndex, Vector3 position, Vector3 direction, List<SplinePoint> splinePoints, int currentSplinePointIndex)
        {
            GameObject instance = null;

            // offset for lanes: lanes are from left to right, center is the spline. 
            // so a spline with 5 lanes has these offset lanes: -2, -1, 0, 1, 2
            int offsetLane = -prefabPainter.splineSettings.lanes / 2;

            for ( var lane=1; lane <= prefabPainter.splineSettings.lanes; lane++)
            {

                // skip center lane if requestred
                if (prefabPainter.splineSettings.skipCenterLane && offsetLane == 0)
                {
                    offsetLane++;
                    continue;
                }

                // get settings for the prefab to instantiate
                PrefabSettings prefabSettings = prefabPainter.GetPrefabSettings();

                // check if we have settings at all
                if (prefabSettings == null)
                    return null;

                // instance will be created or used => advance index
                prefabInstanceIndex++;

                // check if we have to create a new instance or use an existing one
                if (prefabPainter.splineSettings.reusePrefabs)
                {
                    if (prefabInstanceIndex < prefabPainter.splineSettings.prefabInstances.Count)
                    {
                        instance = prefabPainter.splineSettings.prefabInstances[prefabInstanceIndex];
                    }
                }

                // create instance if we don't have one yet
                if( instance == null)
                {
                    instance = GameObject.Instantiate(prefabSettings.prefab);

                    prefabPainter.splineSettings.prefabInstances.Add(instance);
                }

                // reset position & rotation
                instance.transform.position = position;
                instance.transform.rotation = Quaternion.identity;

                ApplyPrefabSettings( offsetLane, prefabSettings, instance, position, direction, splinePoints, currentSplinePointIndex);

                // reparent the child to the container
                instance.transform.parent = prefabPainter.container.transform;

                offsetLane++;
            }

            return instance;
        }

        private void ApplyPrefabSettings( int offsetLane, PrefabSettings prefabSettings, GameObject prefab, Vector3 currentPosition, Vector3 direction, List<SplinePoint> splinePoints, int currentSplinePointIndex)
        {
            // add prefab's position offset
            prefab.transform.position += prefabSettings.positionOffset;

            // lanes
            Vector3 splinePosition = prefab.transform.position;
            Quaternion splineRotation = Quaternion.LookRotation(direction);

            Vector3 addDistanceToDirection = Vector3.zero;

            if (offsetLane == 0)
            {

                // check if the objects should be aligned next to each other
                // TODO this is only done for the center lane currently; we don't have the information about the others
                //      ie this only works in center lane mode currently
                if (prefabPainter.splineSettings.separation == SplineSettings.Separation.PrefabBounds)
                {
                    // move along in the spline rotation, considering the radius
                    addDistanceToDirection = splineRotation * prefab.transform.forward * GetDistanceToMove(prefab) / 2;
                }

            }
            // lane mode, all lanes except center
            else
            {
                // calculate offset distance to spline
                float offsetDistance = offsetLane * prefabPainter.splineSettings.laneDistance;

                // calculate the distance considering the spline direction
                //Vector3 distance = prefabPainter.splineSettings.lanePositionOffset - go.transform.position;
                addDistanceToDirection = splineRotation * prefab.transform.right * offsetDistance;
            }

            prefab.transform.position += addDistanceToDirection;


            // size
            if (prefabSettings.changeScale)
            {
                prefab.transform.localScale = Vector3.one * Random.Range(prefabSettings.scaleMin, prefabSettings.scaleMax);
            }

            // initial rotation
            Quaternion rotation = Quaternion.identity;

            switch(prefabPainter.splineSettings.instanceRotation)
            {
                case SplineSettings.Rotation.Spline:
                    // rotation along spline
                    rotation = Quaternion.LookRotation(direction);
                    break;

                case SplineSettings.Rotation.Prefab:
                    // rotation of the prefab
                    if (prefabSettings.randomRotation)
                    {
                        rotation = Random.rotation;
                    }
                    break;
            }

            // lerp rotation between control points along the spline
            if (prefabPainter.splineSettings.controlPointRotation)
            {

                /*
                Quaternion controlPointRotation = prefabPainter.splineSettings.controlPoints[currentSplinePointIndex].rotation;
                rotation *= controlPointRotation;
                */

                int currentControlPointIndex = splinePoints[currentSplinePointIndex].startControlPointIndex;
                int nextControlPointIndex = currentControlPointIndex + 1;

                // check loop
                if (prefabPainter.splineSettings.loop)
                {
                    if (nextControlPointIndex > prefabPainter.splineSettings.controlPoints.Count - 1)
                    {
                        nextControlPointIndex = 0;
                    }
                }

                Vector3 currentControlPointPosition = prefabPainter.splineSettings.controlPoints[currentControlPointIndex].position;
                Vector3 nextControlPointPosition = prefabPainter.splineSettings.controlPoints[nextControlPointIndex].position;

                // the percentage that a spline point is between control points. ranges from 0 to 1
                float percentageOnSegment = MathUtils.InverseLerp(currentControlPointPosition, nextControlPointPosition, currentPosition);

                // calculate lerp roation
                Quaternion currentControlPointRotation = prefabPainter.splineSettings.controlPoints[currentControlPointIndex].rotation;
                Quaternion nextControlPointRotation = prefabPainter.splineSettings.controlPoints[nextControlPointIndex].rotation;

                Quaternion lerpRotation = Quaternion.Lerp(currentControlPointRotation, nextControlPointRotation, percentageOnSegment);

                // add rotation
                rotation *= lerpRotation;

            }
                       
            prefab.transform.rotation = rotation;

            // add prefab rotation offset
            prefab.transform.Rotate(prefabSettings.rotationOffset);
        }

    }
}