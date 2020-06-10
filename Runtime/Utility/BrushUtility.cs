using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using SceneBuilder.Windows;

namespace SceneBuilder
{
    public static class BrushUtility
    {
        public static List<Type> BrushTypes
        {
            get
            {
                List<Type> brushTypes = new List<Type>();
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t.IsSubclassOf(typeof(Brush)))
                    {
                        brushTypes.Add(t);
                    }
                }
                return brushTypes;
            }
        }

        public static T CreateNewBrush<T>(string name = "New Brush", bool focusOn = false) where T : Brush
        {
            return (T)CreateNewBrush(typeof(T), name, focusOn);
        }

        public static object CreateNewBrush(Type brushType, string name = "New Brush", bool focusOn = false)
        {
            if (!brushType.IsSubclassOf(typeof(Brush)))
                throw new ArgumentException("Invalid brush class: " + brushType.FullName);

            string path = PreferencesProvider.GetPreferences().PathToBrushes;
            AssetDatabase.GUIDToAssetPath(SceneBuilderUtility.CreatePath(path));

            Brush newBrush = (Brush)ScriptableObject.CreateInstance(brushType);
            AssetDatabase.CreateAsset(newBrush, $"{path}/{name}.asset");
            AssetDatabase.SaveAssets();

            if (focusOn)
            {
                Selection.activeObject = newBrush;
                EditorUtility.FocusProjectWindow();
            }
            return newBrush;
        }

        public static void AddBrush(Brush brush)
        {
            string brushesPath = PreferencesProvider.GetPreferences().PathToBrushes;
            string brushPath = AssetDatabase.GetAssetPath(brush);
            AssetDatabase.MoveAsset(brushPath, brushesPath + "/" + brushPath.Split('/').Last());
        }

        public static IEnumerable<Brush> FindAllBrushes()
        {
            string localPath = PreferencesProvider.GetPreferences().PathToBrushes;
            string fullPath = Path.GetFullPath(localPath);
            string[] brushFolder = Directory.GetFiles(fullPath);

            foreach (string filePath in brushFolder)
            {
                string fileName = filePath.Split('\\').Last();

                if (fileName.EndsWith(".meta"))
                    continue;
                if (!fileName.EndsWith(".asset"))
                {
                    Debug.LogWarning($"Wrong file in brush directory {fileName}");
                    continue;
                }

                Brush brush = AssetDatabase.LoadAssetAtPath<Brush>($"{localPath}/{fileName}");
                if (brush != null)
                {
                    yield return brush;
                }
            }
        }

        public static void DeleteBrush(Brush brush)
        {
            AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(brush));
        }

        public static void CopySettings(Brush from, Brush to)
        {
            Type fromType = from.GetType();
            Type toType = to.GetType();

            foreach (FieldInfo fromfield in fromType.GetFields())
            {
                FieldInfo toField = toType.GetField(fromfield.Name);
                if (toField != null)
                {
                    toField.SetValue(to, fromfield.GetValue(from));
                }
            }
        }

        public static object GetBrushEditor(Brush brush)
        {
            if (brush == null) return null;

            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Unity.SceneBuilder.Editor");
            Type editorType = assembly.GetType($"SceneBuilderEditor.BrushEditor");
            Type editorAttrType = assembly.GetType($"SceneBuilderEditor.CustomBrushEditorAttribute");

            foreach (Type t in assembly.GetTypes())
            {
                if (t.IsSubclassOf(editorType))
                {
                    foreach (Attribute attr in t.GetCustomAttributes())
                    {
                        Type attrType = attr.GetType();
                        if (attrType.Name == "CustomBrushEditorAttribute")
                        {
                            if ((string)attrType.GetField("brushType").GetValue(attr) == brush.GetType().Name)
                            {
                                return Activator.CreateInstance(t);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static void Invoke(object editor, string method, params object[] parameters)
        {
            MethodInfo m = editor.GetType().GetMethod(method);
            if (m != null)
            {
                m.Invoke(editor, parameters);
            }
        }


        public static void UpdateBrushTool(Brush brush)
        {
            // Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Unity.SceneBuilder.Editor");
            // Type editorType = assembly.GetType($"SceneBuilderEditor.BrushTool");
            // editorType.GetField("brush").SetValue(null, brush);
            BrushTool.brush = brush;
        }
    }
}
