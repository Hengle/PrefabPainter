using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    public class GUIStyles
    {

        private static GUIStyle _boxTitleStyle;
        public static GUIStyle BoxTitleStyle
        {
            get
            {
                if (_boxTitleStyle == null)
                {
                    _boxTitleStyle = new GUIStyle("Label");
                    _boxTitleStyle.fontStyle = FontStyle.Italic;
                }
                return _boxTitleStyle;
            }
        }

        private static GUIStyle _dropAreaStyle;
        public static GUIStyle DropAreaStyle
        {
            get
            {
                if (_dropAreaStyle == null)
                {
                    _dropAreaStyle = new GUIStyle("box");
                    _dropAreaStyle.fontStyle = FontStyle.Italic;
                    _dropAreaStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _dropAreaStyle;
            }
        }

        public static Color DefaultBackgroundColor = GUI.backgroundColor;
        public static Color ErrorBackgroundColor = new Color( 1f,0f,0f,0.7f); // red tone

        public static Color BrushNoneInnerColor = new Color(0f, 0f, 1f, 0.05f); // blue tone
        public static Color BrushNoneOuterColor = new Color(0f, 0f, 1f, 1f); // blue tone

        public static Color BrushAddInnerColor = new Color(0f, 1f, 0f, 0.05f); // green tone
        public static Color BrushAddOuterColor = new Color(0f, 1f, 0f, 1f); // green tone

        public static Color BrushRemoveInnerColor = new Color(1f, 0f, 0f, 0.05f); // red tone
        public static Color BrushRemoveOuterColor = new Color(1f, 0f, 0f, 1f); // red tone

    }
}