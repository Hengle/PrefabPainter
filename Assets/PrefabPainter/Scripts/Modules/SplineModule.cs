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

        public void PlaceObjects()
        {

            // clear existing prefabs
            foreach (GameObject go in prefabPainter.splineSettings.prefabInstances)
            {
                PrefabPainter.DestroyImmediate(go);
            }

            prefabPainter.splineSettings.prefabInstances.Clear();

            if (prefabPainter.splineSettings.controlPoints.Count < 2)
                return;

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
            float distanceToMove = prefabPainter.splineSettings.distanceBetweenObjects;

            //our current position on the spline
            Vector3 positionIterator = splinePoints[0].position;
           
            // the algorithm skips the first control point, so we need to manually place the first object
            Vector3 direction = (splinePoints[nextSplinePointIndex].position - positionIterator);

            // new prefab
            AddPrefab(positionIterator, direction, splinePoints, nextSplinePointIndex - 1);

            while (nextSplinePointIndex < splinePoints.Count)
            {
                direction = (splinePoints[nextSplinePointIndex].position - positionIterator);
                direction = direction.normalized;

                float distanceToNextPoint = Vector3.Distance(positionIterator, splinePoints[nextSplinePointIndex].position);

                if (distanceToNextPoint >= distanceToMove)
                {
                    positionIterator += direction * distanceToMove;

                    // new prefab
                    AddPrefab(positionIterator, direction, splinePoints, nextSplinePointIndex - 1);

                    distanceToMove = prefabPainter.splineSettings.distanceBetweenObjects;
                }
                else
                {
                    distanceToMove -= distanceToNextPoint;
                    positionIterator = splinePoints[nextSplinePointIndex++].position;
                }

            }
        }


        /// <summary>
        /// Add a new prefab to the spline
        /// </summary>
        private void AddPrefab( Vector3 position, Vector3 direction, List<SplinePoint> splinePoints, int currentSplinePointIndex)
        {

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
                    return;

                GameObject instance = GameObject.Instantiate( prefabSettings.prefab, position, Quaternion.identity);
                
                ApplyPrefabSettings( offsetLane, prefabSettings, instance, position, direction, splinePoints, currentSplinePointIndex);

                prefabPainter.splineSettings.prefabInstances.Add(instance);

                // reparent the child to the container
                instance.transform.parent = prefabPainter.container.transform;

                offsetLane++;
            }

        }

        private void ApplyPrefabSettings( int offsetLane, PrefabSettings prefabSettings, GameObject go, Vector3 currentPosition, Vector3 direction, List<SplinePoint> splinePoints, int currentSplinePointIndex)
        {
            int lanes = prefabPainter.splineSettings.lanes;

            // add position
            go.transform.position += prefabSettings.positionOffset;

            // lanes
            Vector3 splinePosition = go.transform.position;
            Quaternion splineRotation = Quaternion.LookRotation(direction);

            // calculate offset distance to spline
            float offsetDistance = offsetLane * prefabPainter.splineSettings.laneDistance;

            // calculate the distance considering the spline direction
            //Vector3 distance = prefabPainter.splineSettings.lanePositionOffset - go.transform.position;
            Vector3 addDistanceToDirection = splineRotation * go.transform.right * offsetDistance;

            go.transform.position += addDistanceToDirection;


            // size
            if (prefabSettings.changeScale)
            {
                go.transform.localScale = Vector3.one * Random.Range(prefabSettings.scaleMin, prefabSettings.scaleMax);
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
                       
            go.transform.rotation = rotation;

            // add prefab rotation offset
            go.transform.Rotate(prefabSettings.rotationOffset);
        }

    }
}