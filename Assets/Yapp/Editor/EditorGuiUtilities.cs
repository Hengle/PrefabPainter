using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Yapp
{
    public class EditorGuiUtilities : MonoBehaviour
    {
        /// <summary>
        /// Min/Max range slider with float fields
        /// </summary>
        /// <param name="label"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        public static void MinMaxEditor( string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);

                minValue = EditorGUILayout.FloatField("", minValue, GUILayout.Width(50));
                EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
                maxValue = EditorGUILayout.FloatField("", maxValue, GUILayout.Width(50));

                if (minValue < minLimit) minValue = minLimit;
                if (maxValue > maxLimit) maxValue = maxLimit;

            }
            GUILayout.EndHorizontal();

        }

        /// <summary>
        /// Min/Max range slider with float fields. Values must be >= 0.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        public static void MinMaxEditor(string minLabel, ref SerializedProperty minValueProperty, string maxLabel, ref SerializedProperty maxValueProperty)
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(minValueProperty, new GUIContent(minLabel));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(maxValueProperty, new GUIContent(maxLabel));
            }
            GUILayout.EndHorizontal();

            // min must never be < 0
            if (minValueProperty.floatValue < 0) minValueProperty.floatValue = 0f;

            // max must never be < min
            if (maxValueProperty.floatValue < minValueProperty.floatValue) maxValueProperty.floatValue = minValueProperty.floatValue;
        }

    }
}