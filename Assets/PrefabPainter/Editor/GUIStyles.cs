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

    }
}