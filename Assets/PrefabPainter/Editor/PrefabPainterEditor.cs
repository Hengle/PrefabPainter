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

            // draw default inspector elements
            DrawDefaultInspector();


            /// 
            /// Version Info
            /// 
            EditorGUILayout.HelpBox("Prefab Painter v0.1 (Alpha)", MessageType.Info);

            /// 
            /// General settings
            /// 

            GUILayout.BeginVertical("box"); 

            EditorGUILayout.LabelField("General Settings", GUIStyles.BoxTitleStyle);

            this.gizmo.container = (GameObject)EditorGUILayout.ObjectField("Container", this.gizmo.container, typeof(GameObject), true);
            this.gizmo.mode = (PrefabPainter.Mode) EditorGUILayout.EnumPopup("Mode", this.gizmo.mode);

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

            EditorGUILayout.LabelField("Prefab", GUIStyles.BoxTitleStyle);

            this.gizmo.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", this.gizmo.prefab, typeof(GameObject), true);

            this.gizmo.positionOffset = EditorGUILayout.Vector3Field("Position Offset", this.gizmo.positionOffset);

            this.gizmo.randomRotation = EditorGUILayout.Toggle("Random Rotation", this.gizmo.randomRotation);
            this.gizmo.randomScale = EditorGUILayout.Toggle("Random Scale", this.gizmo.randomScale);

            this.gizmo.randomScaleMin = EditorGUILayout.FloatField("Random Scale Min", this.gizmo.randomScaleMin);
            this.gizmo.randomScaleMax = EditorGUILayout.FloatField("Random Scale Max", this.gizmo.randomScaleMax);

            GUILayout.EndVertical();

            /// Physics
            this.physicsModule.OnInspectorGUI();

            /// Copy/Paste
            this.copyPasteModule.OnInspectorGUI();

            // Tools
            this.toolsModule.OnInspectorGUI();

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

    }
}