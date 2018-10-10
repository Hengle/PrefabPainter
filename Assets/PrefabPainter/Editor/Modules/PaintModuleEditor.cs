using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class PaintModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty brushSize;
         
        #endregion Properties

        #pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414
         
        PrefabPainter gizmo;


        private bool mousePosValid = false;
        private Vector3 mousePos; 

        public PaintModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            brushSize = editor.FindProperty( x => x.paintSettings.brushSize);

        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Paint settings", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(brushSize, new GUIContent("Brush Size"));

            GUILayout.EndVertical();

        }



        public void OnSceneGUI()
        {
            float radius = gizmo.paintSettings.brushSize / 2f;

            int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            {
                mousePos = hit.point;
                mousePosValid = true;

                Handles.color = Color.red;
                Handles.DrawWireDisc(mousePos, hit.normal, radius);


                ///
                /// process mouse events
                ///

                // control key pressed
                if (Event.current.control)
                {
                    // mouse wheel up/down changes the radius
                    if (Event.current.type == EventType.ScrollWheel)
                    {

                        if (Event.current.delta.y > 0)
                        {
                            gizmo.paintSettings.brushSize++;
                            Event.current.Use();
                        }
                        else if (Event.current.delta.y < 0)
                        {
                            gizmo.paintSettings.brushSize--;

                            // TODO: slider
                            if (gizmo.paintSettings.brushSize < 1)
                                gizmo.paintSettings.brushSize = 1;

                            Event.current.Use();
                        }
                    }
                }

                if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
                {
                    // left button = 0; right = 1; middle = 2
                    if (Event.current.button == 0)
                    {
                        PerformEditorAction();
                        Event.current.Use();
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


            // examples about how to show ui info
            // note: Handles.BeginGUI and EndGUI are important, otherwise the default gizmos aren't drawn
            Handles.BeginGUI();

            ShowHandleInfo();

            string[] info = new string[] { "Use ctrl + mousewheel to adjust the brush size\nPress left mouse button and drag to paint prefabs" ,"Children: " + GetChildCount() };
            PrefabPainterEditor.ShowGuiInfo(info);

            Handles.EndGUI();
        }

        #region Common methods


        // TODO: refactor into dedicated class
        private int GetChildCount()
        {
            if (gizmo.container == null)
                return 0;

            return gizmo.container.transform.childCount;

        }


        private Transform[] getContainerChildren()
        {
            if (gizmo.container == null)
                return new Transform[0];

            Transform[] children = gizmo.container.transform.Cast<Transform>().ToArray();

            return children;
        }

        #endregion Common methods

        private void ShowHandleInfo()
        {

            if (!mousePosValid)
                return;

            // example about how to show info at the gizmo
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.blue;
            string text = "Mouse Postion: " + mousePos;
            text += "\n";
            text += "Children: " + GetChildCount();
            Handles.Label(mousePos, text, style);
        }


        #region Paint Prefabs

        /// <summary>
        /// Check if the distance 
        /// </summary>
        private void PerformEditorAction()
        {

            if (!editor.IsEditorSettingsValid())
                return;
             
            bool prefabExists = false;

            // check if a gameobject is already within the brush size
            // allow only 1 instance per bush size
            GameObject container = gizmo.container as GameObject;

            foreach (Transform child in container.transform)
            {
                float dist = Vector3.Distance(mousePos, child.transform.position);

                if (dist <= gizmo.paintSettings.brushSize)
                {
                    prefabExists = true;
                    break;
                }

            }

            if (!prefabExists)
            {
                PrefabSettings prefabSettings = this.gizmo.GetPrefabSettings();

                // new prefab
                GameObject instance = PrefabUtility.InstantiatePrefab( prefabSettings.prefab) as GameObject;

                // size
                if ( prefabSettings.randomScale)
                {
                    instance.transform.localScale = Vector3.one * Random.Range( prefabSettings.randomScaleMin, prefabSettings.randomScaleMax);
                }

                // position
                instance.transform.position = new Vector3(mousePos.x, mousePos.y, mousePos.z);

                // add offset
                instance.transform.position +=  prefabSettings.positionOffset;

                // rotation
                Quaternion rotation;
                if ( prefabSettings.randomRotation)
                {
                    rotation = Random.rotation;
                }
                else
                {
                    rotation = new Quaternion(0, 0, 0, 0);
                }
                instance.transform.rotation = rotation;

                // attach as child of container
                instance.transform.parent = container.transform;

                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");

            }
        }

        #endregion Paint Prefabs
    }

}
