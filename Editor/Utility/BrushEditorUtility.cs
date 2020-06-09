using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SceneBuilderEditor
{
    public static class BrushEditorUtility
    {
        public static bool DrawTitleFoldout(string name, bool state)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            if (state)
            {
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
            }
            else
            {
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Italic;
            }

            return GUILayout.Button(name, style) ? !state : state;
        }

        public static void SetSpace(Action action, int level)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16 * level);
            action();
            EditorGUILayout.EndHorizontal();
        }

        public static T[] ArrayField<T>(Func<string, T, GUILayoutOption[], T> EditorGUICall, T[] array, string name, int level)
        {
            List<T> list = new List<T>(array);
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SetSpace(() => list[i] = EditorGUICall($"{name} [{i + 1}]", list[i], default), level);
                if (GUILayout.Button("Remove"))
                {
                    list.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            SetSpace(() => 
            {
                if (GUILayout.Button("Add"))
                {
                    list.Add(default);
                }
            }, level);

            return list.ToArray();
        }
    }
}
