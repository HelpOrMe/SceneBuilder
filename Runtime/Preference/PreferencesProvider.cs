using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneBuilder
{
    public class PreferencesProvider : SettingsProvider
    {
        private static PreferencesObject m_instance;
        private static SerializedObject m_serializedInstance;

        public static readonly string PathToContent = "Packages/com.sah_ed.scenebuilder/Content";
        public static readonly string PathToPreferencesAsset = PathToContent + "/Preferences/PreferencesObject.asset";
        public static readonly string PathToDefaultBrushObjects = PathToContent + "/DefaultBrushModels";
        public static readonly string PathToDefaultIcons = PathToContent + "/DefaultBrushIcons";

        public PreferencesProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

        public static PreferencesObject GetPreferences()
        {
            if (m_instance == null)
            {
                m_instance = AssetDatabase.LoadAssetAtPath<PreferencesObject>(PathToPreferencesAsset);
                if (m_instance == null)
                {
                    m_instance = ScriptableObject.CreateInstance<PreferencesObject>();
                    AssetDatabase.CreateAsset(m_instance, PathToPreferencesAsset);
                    AssetDatabase.SaveAssets();
                    LoadDefaultIcons();
                }
            }
            return m_instance;
        }

        public static void LoadDefaultIcons()
        {
            PreferencesObject preferences = GetPreferences();
            preferences.DefaultBrushIcons = new Texture2D[2]
            {
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathToDefaultIcons + "/Default Brush icon.png"),
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathToDefaultIcons + "/Default Object Brush icon.png"),
            };
            preferences.EditorPlayModePreviewIcon = preferences.DefaultBrushIcons[0];
        }

        public static SerializedObject GetSerializedPreferences()
        {
            if (m_serializedInstance == null)
                m_serializedInstance = new SerializedObject(GetPreferences());
            return m_serializedInstance;
        }

        public override void OnGUI(string searchContext)
        {
            foreach (System.Reflection.FieldInfo field in typeof(PreferencesObject).GetFields())
            {
                EditorGUILayout.PropertyField(GetSerializedPreferences().FindProperty(field.Name), true);
            }
            m_serializedInstance.ApplyModifiedProperties();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            PreferencesProvider provider = new PreferencesProvider("Preferences/SceneBuilder", SettingsScope.User)
            {
                keywords = GetSearchKeywordsFromSerializedObject(GetSerializedPreferences()),
            };
            return provider;
        }
    }
}