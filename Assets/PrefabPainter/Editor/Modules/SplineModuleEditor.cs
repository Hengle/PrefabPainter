using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class SplineModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty curveResolution;
        SerializedProperty loop;
        SerializedProperty distanceBetweenObjects;
        SerializedProperty lanes;
        SerializedProperty laneDistance;
        SerializedProperty skipCenterLane;
        SerializedProperty instanceRotation;
        SerializedProperty controlPointRotation;
        SerializedProperty attachMode;
        SerializedProperty dirty;

        #endregion Properties

        // avoid endless loop by limiting min distance between objects to a value above 0
        private static readonly float minDistanceBetweenObjectsd = 0.01f;

        PrefabPainterEditor editor;
        PrefabPainter gizmo;

        private bool mousePosValid = false;
        private Vector3 mousePos;

        public SplineModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            curveResolution = editor.FindProperty( x => x.splineSettings.curveResolution);
            loop = editor.FindProperty(x => x.splineSettings.loop);
            distanceBetweenObjects = editor.FindProperty(x => x.splineSettings.distanceBetweenObjects);

            lanes = editor.FindProperty(x => x.splineSettings.lanes);
            laneDistance = editor.FindProperty(x => x.splineSettings.laneDistance);
            skipCenterLane = editor.FindProperty(x => x.splineSettings.skipCenterLane);

            instanceRotation = editor.FindProperty(x => x.splineSettings.instanceRotation);
            controlPointRotation =  editor.FindProperty(x => x.splineSettings.controlPointRotation);
            attachMode = editor.FindProperty(x => x.splineSettings.attachMode);

            dirty =  editor.FindProperty(x => x.splineSettings.dirty);

        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Spline settings", GUIStyles.BoxTitleStyle);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(curveResolution, new GUIContent("Curve Resolution"));
            EditorGUILayout.PropertyField(loop, new GUIContent("Loop"));
            EditorGUILayout.PropertyField(distanceBetweenObjects, new GUIContent("Distance between Objects"));
            EditorGUILayout.PropertyField(lanes, new GUIContent("Lanes"));
            EditorGUILayout.PropertyField(laneDistance, new GUIContent("Lane Distance"));
            EditorGUILayout.PropertyField(skipCenterLane, new GUIContent("Skip Center Lane"));

            EditorGUILayout.PropertyField(instanceRotation, new GUIContent("Rotation"));

            // allow control point rotation only in spline rotation mode
            SplineSettings.Rotation selectedInstanceRotation = (SplineSettings.Rotation)System.Enum.GetValues(typeof(SplineSettings.Rotation)).GetValue(instanceRotation.enumValueIndex);

            if (selectedInstanceRotation == SplineSettings.Rotation.Spline)
            {
                EditorGUILayout.PropertyField(controlPointRotation, new GUIContent("Control Point Rotation"));
            }

            EditorGUILayout.PropertyField(attachMode, new GUIContent("Attach Mode"));


            bool changed = EditorGUI.EndChangeCheck();

            if( changed)
            {
                // avoid endless loop by limiting min distance between objects to a value above 0
                if( distanceBetweenObjects.floatValue <= 0)
                    distanceBetweenObjects.floatValue = minDistanceBetweenObjectsd;


                // allow control point rotation only in spline rotation mode
                if (selectedInstanceRotation != SplineSettings.Rotation.Spline)
                {
                    controlPointRotation.boolValue = false;
                }

            }

            dirty.boolValue |= changed;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
            {
                ClearSpline(false);
            }

            if (GUILayout.Button("Clear"))
            {
                ClearSpline(true);
            }

            if (GUILayout.Button("Update"))
            {
                dirty.boolValue |= true;
                PerformEditorAction(); // TODO: draw later at a core place
            }


            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            PerformEditorAction();

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
                /// process mouse & keyboard events
                ///

                //if (EditorWindow.focusedWindow)
                {
                    
                    switch (Event.current.type)
                    {
                        case EventType.KeyDown:
                            {
                                // toggle add mode
                                if (Event.current.shift && Event.current.keyCode == KeyCode.A)
                                {
                                    // toggle add mode
                                    
                                    SplineSettings.AttachMode selectedAttachMode = gizmo.splineSettings.attachMode;

                                    /*
                                    SplineSettings.AttachMode selectedAttachMode = (SplineSettings.AttachMode)System.Enum.GetValues(typeof(SplineSettings.AttachMode)).GetValue(attachMode.enumValueIndex);

                                    int boundsIndex = ArrayUtility.IndexOf((SplineSettings.AttachMode[])System.Enum.GetValues(typeof(SplineSettings.AttachMode)), SplineSettings.AttachMode.Bounds);
                                    int betweenIndex = ArrayUtility.IndexOf((SplineSettings.AttachMode[])System.Enum.GetValues(typeof(SplineSettings.AttachMode)), SplineSettings.AttachMode.Between);
                                    */

                                    if (selectedAttachMode == SplineSettings.AttachMode.Bounds)
                                        gizmo.splineSettings.attachMode = SplineSettings.AttachMode.Between;
                                    else
                                        gizmo.splineSettings.attachMode = SplineSettings.AttachMode.Bounds;

                                    // trigger repaint, so that the enumpopup will be updated
                                    editor.Repaint();
                                 };

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

                            if (gizmo.splineSettings.attachMode == SplineSettings.AttachMode.Between && neighbourIndex >= 0 && neighbourIndex <= gizmo.splineSettings.controlPoints.Count - 1)
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

            DrawSplineGizmos();

            // create gameobjects
            if( mousePosValid)
            {
                PerformEditorAction();
            }
            

            // show info
            Handles.BeginGUI();

            string[] info = new string[] { "Add Control Point: shift + click", "Remove control point: shift + ctrl + click", "Change Attach Mode: shift + A, Current: " + gizmo.splineSettings.attachMode };
            PrefabPainterEditor.ShowGuiInfo(info);

            Handles.EndGUI();
        }

        private void PerformEditorAction()
        {
            if (!editor.IsEditorSettingsValid())
                return;

            if (!gizmo.splineSettings.dirty)
                return;

            gizmo.splineModule.PlaceObjects();

            gizmo.splineSettings.dirty = false;
        }

        private void DrawSplineGizmos()
        {

            bool nodesChanged = false;

            // dizmos
            foreach (ControlPoint controlPoint in gizmo.splineSettings.controlPoints)
            {

                // position handles
                Vector3 oldPosition = controlPoint.position;
                Vector3 newPosition = Handles.PositionHandle(oldPosition, controlPoint.rotation);
                controlPoint.position = newPosition;

                if (oldPosition != newPosition)
                {
                    nodesChanged = true;
                }

                // rotation depends on whether it's enabled or not
                if (gizmo.splineSettings.controlPointRotation)
                {
                    Quaternion oldRotation = controlPoint.rotation;
                    Quaternion newRotation = Handles.RotationHandle(oldRotation, controlPoint.position);
                    controlPoint.rotation = newRotation;

                    if (oldRotation != newRotation)
                    {
                        nodesChanged = true;
                    }

                }

            }

            gizmo.splineSettings.dirty |= nodesChanged;
        }

        private int FindClosestControlPoint(Vector3 position)
        {
            int controlPointIndex = -1;
            float smallestDistance = float.MaxValue;

            for (var i = 0; i < gizmo.splineSettings.controlPoints.Count; i++)
            {

                // bounds mode: skip all that aren't bounds
                if(gizmo.splineSettings.attachMode == SplineSettings.AttachMode.Bounds)
                {
                    if (i != 0 && i != gizmo.splineSettings.controlPoints.Count - 1)
                        continue;
                }

                ControlPoint controlPoint = gizmo.splineSettings.controlPoints.ElementAt(i);
                float distance = Vector3.Distance(controlPoint.position, position);

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
            // create control point
            ControlPoint controlPoint = new ControlPoint();
            controlPoint.position = position;

            // LogControlPoints();

            // no control points yet
            if (closestControlPointIndex == -1)
            {
                gizmo.splineSettings.controlPoints.Add(controlPoint);
            }
            // first control point: insert before
            else if (closestControlPointIndex == 0)
            {
                switch (gizmo.splineSettings.attachMode)
                {
                    case SplineSettings.AttachMode.Bounds:
                        gizmo.splineSettings.controlPoints.Insert(0, controlPoint);
                        break;
                    case SplineSettings.AttachMode.Between:
                        gizmo.splineSettings.controlPoints.Insert(1, controlPoint);
                        break;
                }
            }
            // last control point: add after
            else if (closestControlPointIndex == gizmo.splineSettings.controlPoints.Count - 1)
            {
                switch (gizmo.splineSettings.attachMode)
                {
                    case SplineSettings.AttachMode.Bounds:
                        gizmo.splineSettings.controlPoints.Add(controlPoint);
                        break;
                    case SplineSettings.AttachMode.Between:
                        gizmo.splineSettings.controlPoints.Insert(gizmo.splineSettings.controlPoints.Count - 1, controlPoint);
                        break;
                }
            }
            // inbetween control points
            else
            {
                int newControlPointIndex = closestControlPointIndex;
                gizmo.splineSettings.controlPoints.Insert(newControlPointIndex, controlPoint);
            }

            // trigger recreation of gameobjects
            gizmo.splineSettings.dirty |= true;
            PerformEditorAction(); // TODO: draw later at a core place

        }

        private void RemoveControlPoint( int index)
        {
            gizmo.splineSettings.controlPoints.RemoveAt(index);

            // trigger recreation of gameobjects
            gizmo.splineSettings.dirty |= true;
            PerformEditorAction(); // TODO: draw later at a core place

        }

        private void ClearSpline( bool removePrefabInstances)
        {

            gizmo.splineSettings.controlPoints.Clear();

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


        private void LogControlPoints()
        {
            Debug.Log("Control Points:");
            for (int i = 0; i < gizmo.splineSettings.controlPoints.Count; i++)
            {
                ControlPoint controlPoint = gizmo.splineSettings.controlPoints[i];
                Debug.Log("Control Point " + i + ":" + controlPoint.position);
            }
        }

    }
}
