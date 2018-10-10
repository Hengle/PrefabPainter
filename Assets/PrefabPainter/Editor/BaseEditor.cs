using System;
using UnityEngine;
using UnityEditor;
using System.Linq.Expressions;

namespace PrefabPainter
{
    public class BaseEditor<T> : Editor
        where T : MonoBehaviour
    {
        protected T m_Target
        {
            get { return (T)target; }
        }

        public SerializedProperty FindProperty<TValue>(Expression<Func<T, TValue>> expr)
        {
            return serializedObject.FindProperty(RuntimeUtilities.GetFieldPath(expr));
        }
    }
}
