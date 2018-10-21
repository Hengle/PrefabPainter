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
    public class PrefabPainterEditor : BaseEditor<PrefabPainter>
    {
        #region Properties

        SerializedProperty container;
        SerializedProperty mode;

        #endregion Properties

        private PrefabPainter gizmo;

        private PhysicsExtension physicsModule;
        private CopyPasteExtension copyPasteModule;
        private ToolsExtension toolsModule;

        private ContainerModuleEditor containerModule;
        private BrushModuleEditor brushModule;
        private SplineModuleEditor splineModule;

        private PrefabModuleEditor prefabModule;

        private Color defaultColor;

        PrefabPainterEditor editor;

        // TODO handle prefab dragging only in prefab painter editor
        public List<PrefabSettings> newDraggedPrefabs = null;

        GUIContent[] modeButtons;

        public void OnEnable()
        {
            this.editor = this;

            container = FindProperty( x => x.container); 
            mode = FindProperty(x => x.mode);

            this.gizmo = target as PrefabPainter;

            this.brushModule = new BrushModuleEditor(this);
            this.splineModule = new SplineModuleEditor(this);
            this.containerModule = new ContainerModuleEditor(this);
            this.prefabModule = new PrefabModuleEditor(this);
            this.physicsModule = new PhysicsExtension(this);
            this.copyPasteModule = new CopyPasteExtension(this);
            this.toolsModule = new ToolsExtension(this);

            modeButtons = new GUIContent[]
            {
                // TODO: icons
                new GUIContent( "Brush", "Paint prefabs using a brush"),
                new GUIContent( "Spline", "Align prefabs along a spline"),
                new GUIContent( "Operations", "Operations on the container"),
            };
        }

        public PrefabPainter GetPainter()
        {
            return this.gizmo;
        }


        public override void OnInspectorGUI()
        {

            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            editor.serializedObject.Update();

            newDraggedPrefabs = null;

            // draw default inspector elements
            DrawDefaultInspector();
             
            /// 
            /// Version Info
            /// 
            EditorGUILayout.HelpBox("Prefab Painter v0.3 (Alpha)", MessageType.Info);

            /// 
            /// General settings
            /// 


            GUILayout.BeginVertical("box");
            {

                EditorGUILayout.LabelField("General Settings", GUIStyles.BoxTitleStyle);

                EditorGUILayout.BeginHorizontal();

                // container
                EditorGUILayout.PrefixLabel("");

                if (this.gizmo.container == null)
                {
                    editor.SetErrorBackgroundColor();
                }

                EditorGUILayout.PropertyField(container);

                editor.SetDefaultBackgroundColor();

                if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    GameObject newContainer = new GameObject();

                    string name = gizmo.name + " Container" + " (" + (this.gizmo.transform.childCount + 1) + ")";
                    newContainer.name = name;

                    // set parent; reset position & rotation
                    newContainer.transform.SetParent( this.gizmo.transform, false);

                    // set as new value
                    container.objectReferenceValue = newContainer;

                }

                if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    if(container != null)
                    {
                        this.toolsModule.RemoveContainerChildren();
                    }
  
                }

                EditorGUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            ///
            /// mode
            /// 

            GUILayout.BeginVertical("box");
            {

                EditorGUILayout.LabelField("Mode", GUIStyles.BoxTitleStyle);

                EditorGUILayout.BeginHorizontal();

                mode.intValue = GUILayout.Toolbar(mode.intValue, modeButtons);

                EditorGUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            /// 
            /// Mode dependent
            /// 

            switch (this.gizmo.mode)
            {
                case PrefabPainter.Mode.Brush:
                    brushModule.OnInspectorGUI();

                    /// Prefabs
                    this.prefabModule.OnInspectorGUI();

                    break;

                case PrefabPainter.Mode.Spline:
                    splineModule.OnInspectorGUI();

                    /// Prefabs
                    this.prefabModule.OnInspectorGUI();

                    break;

                case PrefabPainter.Mode.Container:
                    containerModule.OnInspectorGUI();

                    /// Physics
                    this.physicsModule.OnInspectorGUI();

                    /// Copy/Paste
                    this.copyPasteModule.OnInspectorGUI();

                    // Tools
                    this.toolsModule.OnInspectorGUI();
                    break;
                    
            }



            // add new prefabs
            if(newDraggedPrefabs != null)
            {
                this.gizmo.prefabSettingsList.AddRange(newDraggedPrefabs);
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            editor.serializedObject.ApplyModifiedProperties();
        }
        public void SetErrorBackgroundColor()
        {
            GUI.backgroundColor = GUIStyles.ErrorBackgroundColor;
        }

        public void SetDefaultBackgroundColor()
        {
            GUI.backgroundColor = GUIStyles.DefaultBackgroundColor;
        }

        public void addGUISeparator()
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
                case PrefabPainter.Mode.Brush:
                    brushModule.OnSceneGUI();
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

        #region Common methods

        public Transform[] getContainerChildren()
        {
            if (gizmo.container == null)
                return new Transform[0];

            Transform[] children = gizmo.container.transform.Cast<Transform>().ToArray();

            return children;
        }

        #endregion Common methods
    }

}