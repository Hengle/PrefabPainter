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
    }
}