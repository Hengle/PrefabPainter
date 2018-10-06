using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class SplineModuleEditor
    {
        private enum ADD_MODE
        {
            Bounds,
            Inbetween
        }

        private static int maxCurveResolution = 10;
        PrefabPainter gizmo;

        private bool mousePosValid = false;
        private Vector3 mousePos;

        private ADD_MODE addMode = ADD_MODE.Bounds;

        public SplineModuleEditor(PrefabPainter gizmo)
        {
            this.gizmo = gizmo;
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Spline settings", GUIStyles.BoxTitleStyle);

            this.gizmo.splineSettings.curveResolution = EditorGUILayout.IntSlider("Curve Resolution", this.gizmo.splineSettings.curveResolution, 0, maxCurveResolution);
            this.gizmo.splineSettings.loop = EditorGUILayout.Toggle("Loop", this.gizmo.splineSettings.loop);
            this.gizmo.splineSettings.distanceBetweenObjects = EditorGUILayout.FloatField("Distance between Objects", this.gizmo.splineSettings.distanceBetweenObjects);
            this.gizmo.splineSettings.rotateInstance = EditorGUILayout.Toggle("Rotate Objects", this.gizmo.splineSettings.rotateInstance);

            if (GUILayout.Button("Place Objects"))
            {
                gizmo.splineModule.PlaceObjects();
            }

            if (GUILayout.Button("Clear Spline"))
            {
                ClearSpline( true);
            }

            if (GUILayout.Button("New Spline"))
            {
                ClearSpline( false);
            }

            GUILayout.EndVertical();
        }

        // About the position hanlde see example https://docs.unity3d.com/ScriptReference/Handles.PositionHandle.html
        public void OnSceneGUI()
        {
            int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;



            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            {
                mousePos = hit.point;
                mousePosValid = true;


                ///
                /// process mouse events
                ///

                //if (EditorWindow.focusedWindow)
                {
                    
                    switch (Event.current.type)
                    {
                        case EventType.KeyDown:
                            {
                                if (Event.current.keyCode == KeyCode.A)
                                {
                                    // toggle add mode
                                    if ( addMode == ADD_MODE.Bounds)
                                        addMode = ADD_MODE.Inbetween;
                                    else
                                        addMode = ADD_MODE.Bounds;
                                }
                                break;
                            }
                    }
                }

                // control key pressed
                if ( Event.current.shift)
                {
                    mousePos = hit.point;
                    mousePosValid = true;

                    // shift + ctrl = delete
                    bool deleteMode = Event.current.control;

                    int addControlPointIndex = FindClosestControlPoint(mousePos);

                    //Handles.DrawSphere(controlId, mousePos, Quaternion.identity, 0.3f);
                    Color handleColor;
                    if (deleteMode)
                    {
                        handleColor = Color.red; // red = delete

                    }
                    // draw attachment line
                    else
                    {
                        handleColor = Color.gray; // gray = add   
                    }

                    // draw gray circle
                    float radius = 0.1f;
                    Handles.color = handleColor;
                    Handles.DrawWireDisc(mousePos, hit.normal, radius);

                    // draw line to closest point
                    if (gizmo.splineSettings.controlPoints.Count > 0)
                    {
                        // draw indicator line to closest control point
                        Vector3 lineStartPosition = gizmo.splineSettings.controlPoints.ElementAt(addControlPointIndex).position;
                        Vector3 lineEndPosition = mousePos;

                        Handles.DrawLine(lineStartPosition, lineEndPosition);

                        // draw additional indicator line to the control point which is next in the list after the closest control point
                        if (!deleteMode)
                        {
                            int neighbourIndex;
                            if( addControlPointIndex > 0)
                            {
                                neighbourIndex = addControlPointIndex - 1;
                            }
                            else
                            {
                                neighbourIndex = addControlPointIndex + 1;
                            }

                            if (addMode == ADD_MODE.Inbetween && neighbourIndex >= 0 && neighbourIndex <= gizmo.splineSettings.controlPoints.Count - 1)
                            {

                                Vector3 neighbourLineStartPosition = gizmo.splineSettings.controlPoints.ElementAt(neighbourIndex).position;
                                Vector3 neighbourLineEndPosition = mousePos;

                                Handles.DrawLine(neighbourLineStartPosition, neighbourLineEndPosition);
                            }
                        }

                    }

                    if ( Event.current.type == EventType.MouseDown)
                    {
                        // delete node
                        if(deleteMode)
                        {
                            bool canDelete = gizmo.splineSettings.controlPoints.Count > 0;

                            if (Event.current.button == 0 && canDelete)
                            {
                                Vector3 mousePosition = new Vector3(mousePos.x, mousePos.y, mousePos.z);

                                int controlPointIndex = FindClosestControlPoint(mousePosition);

                                // remove the closest point
                                if( controlPointIndex != -1)
                                {
                                    RemoveControlPoint(controlPointIndex);
                                }
                                

                                Event.current.Use();
                            }
                        }
                        // add new node
                        else
                        {
                            // left button = 0; right = 1; middle = 2
                            if (Event.current.button == 0)
                            {
                                Vector3 position = new Vector3(mousePos.x, mousePos.y, mousePos.z);

                                AddControlPoint(position, addControlPointIndex);

                                Event.current.Use();

                            }
                        }
                    }
                }
            }
            else
            {
                mousePosValid = false;
            }

            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlId);
            }

            // TODO: that's performance intense
            placeObjectsOnSpline();

            // show info
            Handles.BeginGUI();

            string[] info = new string[] { "Add Control Point: shift + click", "Remove control point: shift + ctrl + click", "Change Add Mode: A, Current Add Mode: " + addMode};
            PrefabPainterEditor.ShowGuiInfo(info);

            Handles.EndGUI();
        }

        private void placeObjectsOnSpline()
        {
//            if (gizmo.splineSettings.controlPoints.Count < 2)
//                return;

            bool nodesChanged = false;

            // position handles
            foreach (Transform transform in gizmo.splineSettings.controlPoints)
            {
                if (transform == null)
                    continue;

                Vector3 oldPosition = transform.position;
                Vector3 newPosition = Handles.PositionHandle(oldPosition, Quaternion.identity);
                transform.position = newPosition;

                /* rotation not used yet
                Quaternion oldRotation = transform.rotation;
                Quaternion newRotation = Handles.RotationHandle(oldRotation, newPosition);
                transform.rotation = newRotation;
                */

                if (oldPosition != newPosition)
                {
                    nodesChanged = true;
                }

            }

            // Debug.Log("Nodes changed: " + nodesChanged);

            // create objects
            // if( nodesChanged)
            {
                gizmo.splineModule.PlaceObjects();
            }
            
        }

        private int FindClosestControlPoint(Vector3 position)
        {
            int controlPointIndex = -1;
            float smallestDistance = float.MaxValue;

            for (var i = 0; i < gizmo.splineSettings.controlPoints.Count; i++)
            {

                // bounds mode: skip all that aren't bounds
                if( addMode == ADD_MODE.Bounds)
                {
                    if (i != 0 && i != gizmo.splineSettings.controlPoints.Count - 1)
                        continue;
                }

                Transform transform = gizmo.splineSettings.controlPoints.ElementAt(i);
                float distance = Vector3.Distance(transform.position, position);

                if (i == 0 || distance < smallestDistance)
                {
                    controlPointIndex = i;
                    smallestDistance = distance;
                }

            }

            return controlPointIndex;
        }

        /*
        private int GetClosestNeighbour( int index)
        {
            int neighbourIndex = -1;

            int minIndex = 0;
            int maxIndex = gizmo.splineSettings.controlPoints.Count - 1;

            int prevIndex = index - 1;
            int nextIndex = index + 1;

            if (prevIndex < minIndex)
            {
                neighbourIndex = nextIndex;
            }
            else if (nextIndex > maxIndex)
            {
                neighbourIndex = prevIndex;
            }
            else
            {
                float prevDistance = Vector3.Distance(gizmo.splineSettings.controlPoints.ElementAt(index).position, gizmo.splineSettings.controlPoints.ElementAt(prevIndex).position);
                float nextDistance = Vector3.Distance(gizmo.splineSettings.controlPoints.ElementAt(index).position, gizmo.splineSettings.controlPoints.ElementAt(nextIndex).position);

                if( prevDistance < nextDistance)
                {
                    neighbourIndex = prevIndex;
                }
                else
                {
                    neighbourIndex = nextIndex;
                }
            }

            if( neighbourIndex < minIndex || neighbourIndex > maxIndex)
            {
                neighbourIndex = -1;
            }

            return neighbourIndex;
        }
        */

        private void AddControlPoint( Vector3 position, int closestControlPointIndex)
        {
            // create control point gameobject
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.SetActive(false);
            sphere.name = "Anchor " + (gizmo.splineSettings.controlPoints.Count + 1);

            // LogControlPoints();

            // no control points yet
            if (closestControlPointIndex == -1)
            {
                gizmo.splineSettings.controlPoints.Add( sphere.transform);
            }
            // first control point: insert before
            else if (closestControlPointIndex == 0)
            {
                switch (addMode)
                {
                    case ADD_MODE.Bounds:
                        gizmo.splineSettings.controlPoints.Insert(0, sphere.transform);
                        break;
                    case ADD_MODE.Inbetween:
                        gizmo.splineSettings.controlPoints.Insert(1, sphere.transform);
                        break;
                }
            }
            // last control point: add after
            else if (closestControlPointIndex == gizmo.splineSettings.controlPoints.Count - 1)
            {
                switch (addMode)
                {
                    case ADD_MODE.Bounds:
                        gizmo.splineSettings.controlPoints.Add(sphere.transform);
                        break;
                    case ADD_MODE.Inbetween:
                        gizmo.splineSettings.controlPoints.Insert(gizmo.splineSettings.controlPoints.Count - 1, sphere.transform);
                        break;
                }
            }
            // inbetween control points
            else
            {
                int newControlPointIndex = closestControlPointIndex;
                gizmo.splineSettings.controlPoints.Insert(newControlPointIndex, sphere.transform);
            }

            // reparent the anchors with the prefab painter gameobject
            sphere.transform.parent = gizmo.transform;
        }

        private void LogControlPoints()
        {
            Debug.Log("Control Points:");
            for (int i = 0; i < gizmo.splineSettings.controlPoints.Count; i++)
            {
                Transform t = gizmo.splineSettings.controlPoints[i];
                Debug.Log("Control Point " + i + ":" + t.name);
            }
        }

        private void RemoveControlPoint( int index)
        {
            Transform controlPoint = gizmo.splineSettings.controlPoints.ElementAt(index);

            GameObject parentGameObject = controlPoint.gameObject;
            PrefabPainter.DestroyImmediate(parentGameObject);

            gizmo.splineSettings.controlPoints.RemoveAt(index);

        }

        private void ClearSpline( bool removePrefabInstances)
        {
            foreach (Transform transform in gizmo.splineSettings.controlPoints)
            {

                GameObject parentGameObject = transform.gameObject;
                PrefabPainter.DestroyImmediate(parentGameObject);
            }

            if (removePrefabInstances)
            {

                foreach (GameObject go in gizmo.splineSettings.prefabInstances)
                {
                    PrefabPainter.DestroyImmediate(go);
                }
            }

            gizmo.splineSettings.prefabInstances.Clear();

            gizmo.splineSettings.controlPoints.Clear();


        }


    }
}
