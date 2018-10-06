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
                Transform transform = prefabPainter.splineSettings.controlPoints[i];
                initialPoints[i] = transform.position;

                Gizmos.DrawSphere(initialPoints[i], 0.15f);
            }

            if (prefabPainter.splineSettings.controlPoints.Count < 2)
                return;


            IEnumerable<Vector3> spline = Interpolate.NewCatmullRom(prefabPainter.splineSettings.controlPoints.ToArray(), prefabPainter.splineSettings.curveResolution, prefabPainter.splineSettings.loop);
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

        public void PlaceObjects()
        {
            // Debug.Log("instances: " + instances.Count);

            // clear existing prefabs
            foreach (GameObject go in prefabPainter.splineSettings.prefabInstances)
            {
                PrefabPainter.DestroyImmediate(go);
            }

            prefabPainter.splineSettings.prefabInstances.Clear();

            if (prefabPainter.splineSettings.controlPoints.Count < 2)
                return;

            // put the spline into a list of Vector3's instead of using the iterator
            IEnumerable<Vector3> spline = Interpolate.NewCatmullRom(prefabPainter.splineSettings.controlPoints.ToArray(), prefabPainter.splineSettings.curveResolution, prefabPainter.splineSettings.loop);
            IEnumerator iterator = spline.GetEnumerator();
            List<Vector3> splinePoints = new List<Vector3>();

            // limit the number of points; this has become necessary because with the loop there's an overlap multiple times
            // and this could result in an endless loop in the worst case
            int count = (prefabPainter.splineSettings.curveResolution + 1) * prefabPainter.splineSettings.controlPoints.Count;
            while (iterator.MoveNext() && count >= 0)
            {
                splinePoints.Add((Vector3)iterator.Current);
                count--;
            }

            //distanceToMove represents how much farther we need to progress down the spline before we place the next object
            int nextSplinePointIndex = 1;
            float distanceToMove = prefabPainter.splineSettings.distanceBetweenObjects;

            //our current position on the spline
            Vector3 positionIterator = splinePoints[0];

            GameObject instance;
            Vector3 direction;
            Quaternion rotation;

            // the algorithm skips the first control point, so we need to manually place the first object
            direction = (splinePoints[nextSplinePointIndex] - positionIterator);
            if (prefabPainter.splineSettings.rotateInstance)
            {
                rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                rotation = Quaternion.identity;
            }

            instance = GameObject.Instantiate(prefabPainter.prefab, positionIterator, rotation);

            ApplyPrefabSettings(instance);

            prefabPainter.splineSettings.prefabInstances.Add(instance);

            while (nextSplinePointIndex < splinePoints.Count)
            {
                direction = (splinePoints[nextSplinePointIndex] - positionIterator);
                direction = direction.normalized;
                float distanceToNextPoint = Vector3.Distance(positionIterator, splinePoints[nextSplinePointIndex]);
                if (distanceToNextPoint >= distanceToMove)
                {
                    positionIterator += direction * distanceToMove;

                    // rotation
                    if (prefabPainter.splineSettings.rotateInstance)
                    {
                        rotation = Quaternion.LookRotation(direction);
                    }
                    else
                    {
                        rotation = Quaternion.identity;
                    }

                    instance = GameObject.Instantiate(prefabPainter.prefab, positionIterator, rotation);

                    ApplyPrefabSettings(instance);


                    prefabPainter.splineSettings.prefabInstances.Add(instance);

                    distanceToMove = prefabPainter.splineSettings.distanceBetweenObjects;
                }
                else
                {
                    distanceToMove -= distanceToNextPoint;
                    positionIterator = splinePoints[nextSplinePointIndex++];
                }

            }

            // reparent the children to the container
            foreach (GameObject child in prefabPainter.splineSettings.prefabInstances)
            {
                child.transform.parent = prefabPainter.container.transform;
            }
        }

        private void ApplyPrefabSettings(GameObject go)
        {
            go.transform.position += prefabPainter.positionOffset;

        }

    }
}