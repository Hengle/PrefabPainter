using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 
/// Derived from PostProcessing Stack code.
/// Required because I didn't want to use hardcoded strings in searialized properties.
/// 
/// Source: https://github.com/Unity-Technologies/PostProcessing/blob/v2/PostProcessing/Runtime/Utils/RuntimeUtilities.cs
/// </summary>
namespace PrefabPainter
{
    public static class RuntimeUtilities
    {

        // Returns a string path from an expression - mostly used to retrieve serialized properties
        // without hardcoding the field path. Safer, and allows for proper refactoring.
        public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = new StringBuilder();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }

            return sb.ToString();
        }
        
    }
}
