using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    /// <summary>
    /// Prefab Painter allows you to paint prefabs in the scene
    /// </summary>
    [ExecuteInEditMode()]
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PrefabPainter))]
    public class PrefabPainterEditor : Editor
    {

        private PrefabPainter gizmo;

        private PhysicsExtension physicsModule;
        private CopyPasteExtension copyPasteModule;
        private ContainerModuleEditor containerModule;

        private PaintModuleEditor paintModule;
        private SplineModuleEditor splineModule;
        private ToolsExtension toolsModule;

        public void OnEnable()
        {
            this.gizmo = target as PrefabPainter;

            this.paintModule = new PaintModuleEditor(this);
            this.splineModule = new SplineModuleEditor(this);
            this.containerModule = new ContainerModuleEditor(this);
            this.physicsModule = new PhysicsExtension(this);
            this.copyPasteModule = new CopyPasteExtension(this);
            this.toolsModule = new ToolsExtension(this);

        }

        public PrefabPainter GetPainter()
        {
            return this.gizmo;
        }

        public override void OnInspectorGUI()
        {

            List<PrefabSettings> newDraggedPrefabs = null;

            // draw default inspector elements
            DrawDefaultInspector();


            /// 
            /// Version Info
            /// 
            EditorGUILayout.HelpBox("Prefab Painter v0.2 (Alpha)", MessageType.Info);

            /// 
            /// General settings
            /// 

            GUILayout.BeginVertical("box");
            {

                EditorGUILayout.LabelField("General Settings", GUIStyles.BoxTitleStyle);

                this.gizmo.container = (GameObject)EditorGUILayout.ObjectField("Container", this.gizmo.container, typeof(GameObject), true);
                this.gizmo.mode = (PrefabPainter.Mode)EditorGUILayout.EnumPopup("Mode", this.gizmo.mode);

            }
            GUILayout.EndVertical();

            ///
            /// draw custom components
            /// 

            /// 
            /// Mode dependent
            /// 

            switch (this.gizmo.mode)
            {
                case PrefabPainter.Mode.Paint:
                    paintModule.OnInspectorGUI();
                    break;

                case PrefabPainter.Mode.Spline:
                    splineModule.OnInspectorGUI();
                    break;

                case PrefabPainter.Mode.Container:
                    containerModule.OnInspectorGUI();
                    break;
                    
            }

            /// 
            /// Prefab
            /// 

            GUILayout.BeginVertical("box");
            {

                EditorGUILayout.LabelField("Prefab", GUIStyles.BoxTitleStyle);

                GUILayout.BeginHorizontal();
                {

                    GUILayout.BeginVertical();
                    {

                        // drop area
                        Rect prefabDropArea = GUILayoutUtility.GetRect(0.0f, 24.0f, GUIStyles.DropAreaStyle, GUILayout.ExpandWidth(true) );
                        GUI.Box(prefabDropArea, "Drop prefabs here in order to use them", GUIStyles.DropAreaStyle);
                        

                        Event evt = Event.current;
                        switch (evt.type)
                        {
                            case EventType.DragUpdated:
                            case EventType.DragPerform:
                                 
                                if (prefabDropArea.Contains(evt.mousePosition))
                                {

                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                    if (evt.type == EventType.DragPerform)
                                    {
                                        DragAndDrop.AcceptDrag();

                                        // list of new prefabs that should be created via drag/drop
                                        // we can't do it in the drag/drop code itself, we'd get exceptions like
                                        //   ArgumentException: Getting control 12's position in a group with only 12 controls when doing dragPerform. Aborting
                                        // followed by
                                        //   Unexpected top level layout group! Missing GUILayout.EndScrollView/EndVertical/EndHorizontal? UnityEngine.GUIUtility:ProcessEvent(Int32, IntPtr)
                                        // they must be added when everything is done (currently at the end of this method)
                                        newDraggedPrefabs = new List<PrefabSettings>();

                                        foreach (Object droppedObject in DragAndDrop.objectReferences)
                                        {

                                            // allow only prefabs
                                            if (PrefabUtility.GetPrefabType(droppedObject) == PrefabType.None)
                                            {
                                                Debug.Log("Not a gameobject: " + droppedObject);
                                                continue;
                                            }

                                            // new settings
                                            PrefabSettings prefabSettings = new PrefabSettings();

                                            // initialize with dropped prefab
                                            prefabSettings.prefab = droppedObject as GameObject;

                                            newDraggedPrefabs.Add(prefabSettings);

                                        }
                                    }
                                }
                                break;
                        }

                    }

                    GUILayout.EndVertical();

                }

                GUILayout.EndHorizontal();


                for (int i = 0; i < gizmo.prefabSettingsList.Count; i++)
                {
                    if (i > 0)
                        addGUISeparator();

                    PrefabSettings prefabSettings = this.gizmo.prefabSettingsList[i];

                    GUILayout.BeginHorizontal();
                    {
                        // preview
                        // try to get the asset preview
                        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabSettings.prefab);
                        // if no asset preview available, try to get the mini thumbnail
                        if (!previewTexture)
                        {
                            previewTexture = AssetPreview.GetMiniThumbnail(prefabSettings.prefab);
                        }
                        // if a preview is available, paint it
                        if (previewTexture)
                        {
                            //GUILayout.Label(previewTexture, EditorStyles.objectFieldThumb, GUILayout.Width(50), GUILayout.Height(50)); // without border, but with size
                            GUILayout.Label(previewTexture, GUILayout.Width(50), GUILayout.Height(50)); // without border, but with size

                            //GUILayout.Box(previewTexture); // with border
                            //GUILayout.Label(previewTexture); // no border
                            //GUILayout.Box(previewTexture, GUILayout.Width(50), GUILayout.Height(50)); // with border and size
                            //EditorGUI.DrawPreviewTexture(new Rect(25, 60, 100, 100), previewTexture); // draws it in absolute coordinates

                        }

                        // right alin the buttons
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Add", EditorStyles.miniButton))
                        {
                            this.gizmo.prefabSettingsList.Insert(i + 1, new PrefabSettings());
                        }
                        if (GUILayout.Button("Duplicate", EditorStyles.miniButton))
                        {
                            PrefabSettings newPrefabSettings = prefabSettings.Clone();
                            this.gizmo.prefabSettingsList.Insert(i + 1, newPrefabSettings);
                        }
                        if (GUILayout.Button("Reset", EditorStyles.miniButton))
                        {
                            // remove existing
                            this.gizmo.prefabSettingsList.RemoveAt(i);

                            // add new
                            this.gizmo.prefabSettingsList.Insert(i, new PrefabSettings());

                        }
                        if (GUILayout.Button("Remove", EditorStyles.miniButton))
                        {
                            this.gizmo.prefabSettingsList.Remove(prefabSettings);
                        }

                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(4);

                    prefabSettings.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabSettings.prefab, typeof(GameObject), true);

                    prefabSettings.active = EditorGUILayout.Toggle("Active", prefabSettings.active);
                    prefabSettings.probability = EditorGUILayout.Slider("Probability", prefabSettings.probability, 0, 1);

                    prefabSettings.positionOffset = EditorGUILayout.Vector3Field("Position Offset", prefabSettings.positionOffset);

                    prefabSettings.randomRotation = EditorGUILayout.Toggle("Random Rotation", prefabSettings.randomRotation);
                    prefabSettings.randomScale = EditorGUILayout.Toggle("Random Scale", prefabSettings.randomScale);

                    prefabSettings.randomScaleMin = EditorGUILayout.FloatField("Random Scale Min", prefabSettings.randomScaleMin);
                    prefabSettings.randomScaleMax = EditorGUILayout.FloatField("Random Scale Max", prefabSettings.randomScaleMax);

                }
            }

            GUILayout.EndVertical();

            /// Physics
            this.physicsModule.OnInspectorGUI();

            /// Copy/Paste
            this.copyPasteModule.OnInspectorGUI();

            // Tools
            this.toolsModule.OnInspectorGUI();

            // add new prefabs
            if(newDraggedPrefabs != null)
            {
                this.gizmo.prefabSettingsList.AddRange(newDraggedPrefabs);
            }
            

        }


        private void addGUISeparator()
        {
            // space
            GUILayout.Space(10);

            // separator line
            GUIStyle separatorStyle = new GUIStyle(GUI.skin.box);
            separatorStyle.stretchWidth = true;
            separatorStyle.fixedHeight = 2;
            GUILayout.Box("", separatorStyle);
        }

        private void OnSceneGUI()
        {
            this.gizmo = target as PrefabPainter;

            if (this.gizmo == null)
                return;

            switch (this.gizmo.mode)
            {
                case PrefabPainter.Mode.Paint:
                    paintModule.OnSceneGUI();
                    break;

                case PrefabPainter.Mode.Spline:
                    splineModule.OnSceneGUI();
                    break;

                case PrefabPainter.Mode.Container:
                    containerModule.OnSceneGUI();
                    break;
            }

            SceneView.RepaintAll();
        }

        public static void ShowGuiInfo(string[] texts)
        {

            float windowWidth = Screen.width;
            float windowHeight = Screen.height;
            float panelWidth = 500;
            float panelHeight = 100;
            float panelX = windowWidth * 0.5f - panelWidth * 0.5f;
            float panelY = windowHeight - panelHeight;
            Rect infoRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            Color textColor = Color.white;
            Color backgroundColor = Color.red;

            var defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = textColor;

            GUILayout.BeginArea(infoRect);
            {
                EditorGUILayout.BeginVertical();
                {
                    foreach (string text in texts)
                    {
                        GUILayout.Label(text, labelStyle);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();

            GUI.backgroundColor = defaultColor;
        }

        public bool IsEditorSettingsValid()
        {
            // container must be set
            if (this.gizmo.container == null)
            {
                return false;
            }

            // check prefabs
            foreach (PrefabSettings prefabSettings in this.gizmo.prefabSettingsList)
            {
                // prefab must be set
                if ( prefabSettings.prefab == null)
                {
                    return false;
                }


            }

            return true;
        }
    }

}