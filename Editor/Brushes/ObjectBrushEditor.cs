using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SceneBuilder;
using SceneBuilder.Windows;

namespace SceneBuilderEditor
{
    [CustomBrushEditor(typeof(ObjectBrush))]
    public class ObjectBrushEditor : BrushEditor
    {
        private ObjectBrush m_brush;
        public Editor selectedObjectEditor;

        public bool brushSettingsShown = true;
        public bool modelSettingsShown = true;
        public bool modelGridShown = true;

        public Rect brushSettingsSpaceRect;
        public Rect modelSettingsSpaceRect;
        public Rect modelGridSpaceRect;

        public bool ClearAllSettingsCheck = false;
        public float ClearAllSettingsTimeout = 2f;
        public float ClearAllSettingsCheckTime = 0f;
        
        public float gridStep = 20f;
        public bool PreviewMode = true;
        public bool PreviewShown = false;
        public int PreviewShownAt = -1;
        public Editor gridModelPreviewEditor;

        public override void Init(Brush brush, BrushWindow window)
        {
            base.Init(brush, window);
            m_brush = (ObjectBrush)brush;

            // Set the cube as default draw object
            if (m_brush.selectedObject == null)
            {
                m_brush.selectedObject = AssetDatabase.LoadAssetAtPath<GameObject>(PreferencesProvider.PathToDefaultBrushObjects + "/Cube.prefab");
                ResetDrawObjects();
            }
        }

        public override void OnDisable()
        {
            Object.DestroyImmediate(selectedObjectEditor);
            Object.DestroyImmediate(gridModelPreviewEditor);
        }

        #region GUI Calls
        public override void OnGUI()
        {
            if (EditorApplication.isPlaying)
                PreviewMode = false;

            EditorGUILayout.Space();
            SelectedObjectInspector();
            ResetDrawObjects();

            Rect _brushSettingsSpaceRect = EditorGUILayout.GetControlRect(GUILayout.MaxHeight(9));
            if (Event.current.type == EventType.Repaint) brushSettingsSpaceRect = _brushSettingsSpaceRect;
            brushSettingsShown = BrushEditorUtility.DrawTitleFoldout("Brush settings", brushSettingsShown);
            if (brushSettingsShown) DrawBrushSettings();

            Rect _modelSettingsSpaceRect = EditorGUILayout.GetControlRect(GUILayout.MaxHeight(9));
            if (Event.current.type == EventType.Repaint) modelSettingsSpaceRect = _modelSettingsSpaceRect;
            modelSettingsShown = BrushEditorUtility.DrawTitleFoldout("Brush model settings", modelSettingsShown);
            if (modelSettingsShown) DrawModelSettings();

            Rect _modelGridSpaceRect = EditorGUILayout.GetControlRect(GUILayout.MaxHeight(9));
            if (Event.current.type == EventType.Repaint) modelGridSpaceRect = _modelGridSpaceRect;
            modelGridShown = BrushEditorUtility.DrawTitleFoldout("Model grid", modelGridShown);
            if (modelGridShown) DrawModelGrid();
        }

        public void SelectedObjectInspector()
        {
            if (m_brush.selectedObject != null)
            {
                if (selectedObjectEditor == null || selectedObjectEditor.target != m_brush.selectedObject)
                {
                    // Destroy old editor to avoid error
                    if (selectedObjectEditor != null)
                        Object.DestroyImmediate(selectedObjectEditor);

                    selectedObjectEditor = Editor.CreateEditor(m_brush.selectedObject);
                }

                GUIStyle bgColor = new GUIStyle();
                bgColor.normal.background = EditorGUIUtility.whiteTexture;
                Rect rect = GUILayoutUtility.GetRect(256, 256);

                if (!EditorApplication.isPlaying)
                {
                    selectedObjectEditor.OnInteractivePreviewGUI(rect, bgColor);
                }
                else
                {
                    Color color = Color.black;
                    color.a = 0.1f;
                    EditorGUI.DrawRect(rect, color); 
                    GUI.DrawTexture(rect, PreferencesProvider.GetPreferences().EditorPlayModePreviewIcon);
                }
            }
        }

        public void ResetDrawObjects()
        {
            if (m_brush.selectedObject != null)
            {
                List<GameObject> _drawObjects = new List<GameObject>();
                if (m_brush.unpuckObject)
                    foreach (Transform child in m_brush.selectedObject.transform)
                        _drawObjects.Add(child.gameObject);
                else _drawObjects.Add(m_brush.selectedObject);

                m_brush.drawObjects = _drawObjects.ToArray();
                m_brush.drawObjectsSettings = new int[m_brush.drawObjects.Length];
                m_brush.drawObjectsUsage = new bool[m_brush.drawObjects.Length];

                for (int i = 0; i < m_brush.drawObjectsUsage.Length; i++)
                    m_brush.drawObjectsUsage[i] = true;
            }
        }

        #region Brush settings
        public void DrawBrushSettings()
        {
            // Draw fields that edit drawObjects
            GameObject _selectedObject = (GameObject)EditorGUILayout.ObjectField("Brush object", m_brush.selectedObject, typeof(GameObject), true);
            bool _unpackObject = EditorGUILayout.Toggle("Unpack brush object", m_brush.unpuckObject);

            if (_unpackObject != m_brush.unpuckObject || _selectedObject != m_brush.selectedObject)
            {
                m_brush.unpuckObject = _unpackObject;
                m_brush.selectedObject = _selectedObject;
                ResetDrawObjects();
            }

            // Draw another fields 
            EditorGUIUtility.wideMode = true;  // Fix Vector3Field height
            m_brush.originRotation = EditorGUILayout.Vector3Field("Origin rotation", m_brush.originRotation);
            EditorGUIUtility.wideMode = false;
        }
        #endregion

        #region Model Settings
        public void DrawModelSettings()
        {
            // Draw Brush ModelSettings
            foreach (ModelSettings settings in new List<ModelSettings>(m_brush.modelSettings))
                DrawModelSettingsField(settings);

            EditorGUILayout.BeginHorizontal();
            // Set the "Are you sure.." button ahead simple add&remove buttons for the correct display in one GUI call
             if (ClearAllSettingsCheck)
            {
                float delta = Mathf.Round(ClearAllSettingsTimeout - (ClearAllSettingsCheckTime - Time.realtimeSinceStartup));
                if (GUILayout.Button($"Are you sure you want to delete {m_brush.modelSettings.Count} items? [{delta}/{ClearAllSettingsTimeout}s]"))
                {
                    ClearAllSettingsCheck = false;
                    m_brush.modelSettings.Clear();
                }

                if (m_brush.modelSettings.Count < 5 || delta > ClearAllSettingsTimeout)
                    ClearAllSettingsCheck = false;
            }

            // Draw add & remove settings buttons
            if (!ClearAllSettingsCheck)
            {
                if (m_brush.modelSettings.Count == 0 || GUILayout.Button("Create new ModelSettings"))
                    m_brush.modelSettings.Add(new ModelSettings());
                if (m_brush.modelSettings.Count >= 5 && GUILayout.Button("Clear all settings"))
                {
                    ClearAllSettingsCheck = true;
                    ClearAllSettingsCheckTime = Time.realtimeSinceStartup + ClearAllSettingsTimeout;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public void DrawModelSettingsField(ModelSettings settings)
        {
            DrawModelSettingsFieldHeader(settings);
            DrawModelSettingsFieldContent(settings);
        }

        public void DrawModelSettingsFieldHeader(ModelSettings settings)
        {
            // Draw header
            EditorGUILayout.BeginHorizontal();

            // Draw foldout
            settings.showed = EditorGUILayout.Foldout(settings.showed, "More", true);

            // Create new text style
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };

            // Draw small text-buttons
            if (!settings.showed && !window.smallMode)
            {
                string on = "✓";
                string off = "✕";

                string mtm = settings.MoveToMesh ? on : off;
                string rbm = settings.RotateByMesh ? on : off;
                string upo = settings.positionOffset.useOffset ? on : off;
                string uro = settings.rotationOffset.useOffset ? on : off;
                string uso = settings.scaleOffset.useOffset ? on : off;

                if (GUILayout.Button($"MtM[{mtm}]", labelStyle, GUILayout.MaxWidth(45))) settings.MoveToMesh = !settings.MoveToMesh;
                if (GUILayout.Button($"RbM[{rbm}]", labelStyle, GUILayout.MaxWidth(45))) settings.RotateByMesh = !settings.RotateByMesh;
                if (GUILayout.Button($"uPO[{upo}]", labelStyle, GUILayout.MaxWidth(45))) settings.positionOffset.useOffset = !settings.positionOffset.useOffset;
                if (GUILayout.Button($"uRO[{uro}]", labelStyle, GUILayout.MaxWidth(45))) settings.rotationOffset.useOffset = !settings.rotationOffset.useOffset;
                if (GUILayout.Button($"uSO[{uso}]", labelStyle, GUILayout.MaxWidth(45))) settings.scaleOffset.useOffset = !settings.scaleOffset.useOffset;
            }

            // Draw name field
            settings.name = EditorGUILayout.TextField(settings.name, labelStyle);

            // Draw remove button
            if (m_brush.modelSettings.Count > 1 && GUILayout.Button("Remove"))
                m_brush.modelSettings.Remove(settings);
            EditorGUILayout.EndHorizontal();
        }

        public void DrawModelSettingsFieldContent(ModelSettings settings)
        {
            if (settings.showed)
            {
                // Draw mesh fields
                BrushEditorUtility.SetSpace(() => settings.MoveToMesh = EditorGUILayout.Toggle("Move to mesh", settings.MoveToMesh), 1);
                BrushEditorUtility.SetSpace(() => settings.RotateByMesh = EditorGUILayout.Toggle("Rotate by mesh", settings.RotateByMesh), 1);

                // Draw offsets
                DrawOffsetSettngsField(settings.positionOffset, "position");
                DrawOffsetSettngsField(settings.rotationOffset, "rotation");
                DrawOffsetSettngsField(settings.scaleOffset, "scale");

                // Draw random offsets
                DrawRandomOffsetSettingsField(settings.randomPositionOffset, "position");
                DrawRandomOffsetSettingsField(settings.randomRotationOffset, "rotation");
                DrawRandomOffsetSettingsField(settings.randomScaleOffset, "scale");
                
                // Draw random set
                BrushEditorUtility.SetSpace(
                    () => settings.RandomSet = EditorGUILayout.Toggle("Random set", settings.RandomSet), 1);
                if (settings.RandomSet)
                {
                    BrushEditorUtility.SetSpace(
                        () => settings.RandomSetPercent = EditorGUILayout.Slider("Set percent", settings.RandomSetPercent, 0, 1), 2);
                }

                // Draw SetOnlyOnFreeSpace
                BrushEditorUtility.SetSpace(
                    () => settings.useEnemyColliders = EditorGUILayout.Toggle("Use enemy colliders", settings.useEnemyColliders), 1);
                if (settings.useEnemyColliders)
                {
                    settings.enemyColliderNames = BrushEditorUtility.ArrayField(EditorGUILayout.TextField, settings.enemyColliderNames, "Col. name", 2);
                }
                EditorGUILayout.Space();
            }
        }

        public void DrawOffsetSettngsField(OffsetSettings settings, string name)
        {
            BrushEditorUtility.SetSpace(
                    () => settings.useOffset = EditorGUILayout.Toggle($"Use {name} offset", settings.useOffset), 1);
            if (settings.useOffset)
            {
                BrushEditorUtility.SetSpace(
                () => settings.offset = EditorGUILayout.Vector3Field($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)} offset", settings.offset), 2);
            }
        }

        public void DrawRandomOffsetSettingsField(RandomOffsetSettings settings, string name)
        {
            // Draw random position offset
            BrushEditorUtility.SetSpace(
                () => settings.useOffset = EditorGUILayout.Toggle($"Random {name} offset", settings.useOffset), 1);
            if (settings.useOffset)
            {
                BrushEditorUtility.SetSpace(
                    () => settings.randomType = (RandomType)EditorGUILayout.EnumPopup($"Random {name} type", settings.randomType), 2);
                switch (settings.randomType)
                {
                    case RandomType.Range:
                        BrushEditorUtility.SetSpace(
                            () => settings.randomRangesMin = EditorGUILayout.Vector3Field("Min", settings.randomRangesMin), 3);
                        BrushEditorUtility.SetSpace(
                            () => settings.randomRangesMax = EditorGUILayout.Vector3Field("Max", settings.randomRangesMax), 3);
                        break;

                    case RandomType.Between:
                        BrushEditorUtility.SetSpace(
                            () => settings.randomArrayShowed = EditorGUILayout.Foldout(settings.randomArrayShowed, $"Between {name}s", true), 2);
                        if (settings.randomArrayShowed)
                        {
                            settings.randomArray = BrushEditorUtility.ArrayField(EditorGUILayout.Vector3Field, settings.randomArray, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name), 3);
                        }
                        break;
                }
            }
        }
        #endregion

        #region Model Grid
        public void DrawModelGrid()
        {
            DrawGridHeader();
            DrawGridContent();
        }

        public void DrawGridHeader()
        {
            GUIStyle leftStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.LowerLeft };
            float columnSize = window.position.width / 5f;
            float gridStart = modelGridSpaceRect.y + 9 + 18f;

            Rect rowRect = new Rect(0, gridStart, window.position.width, 20);
            Color color = Color.black;
            color.a = 0.15f;
            EditorGUI.DrawRect(rowRect, color);

            EditorGUILayout.BeginHorizontal();
            // Draw model column name
            if (GUILayout.Button(PreviewMode ? "Model+P" : "Model", leftStyle, GUILayout.Width(columnSize)))
                PreviewMode = !PreviewMode;
            GUILayout.Space(columnSize * 1.05f);
            
            // Draw settings column namewindow.scrollPos
            EditorGUILayout.LabelField("Settings", leftStyle, GUILayout.Width(columnSize));
            GUILayout.Space(columnSize * 0.85f);

            // Draw settings usage name
            EditorGUILayout.LabelField("Usage", leftStyle, GUILayout.Width(columnSize / 1.5f));
            EditorGUILayout.EndHorizontal();
        }

        public void DrawGridContent()
        {
            if (m_brush.drawObjects != null)
            {
                // All styles
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
                GUIStyle toggleStyle = new GUIStyle(EditorStyles.toggle) { alignment = TextAnchor.MiddleLeft };
                GUIStyle popupStyle = new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleLeft };
                
                // Get grid position in window
                float columnSize = window.position.width / 5f;
                float gridStart = modelGridSpaceRect.y + 9 + 18f * 2;
                float windowTop = window.scroll.y;
                float windowBottom = windowTop + window.position.height;
                float contentScrolledTop = windowTop - gridStart - (windowTop - gridStart) % gridStep;
                float contentScrolledBottom = windowBottom - gridStart - (windowBottom - gridStart) % gridStep;

                for (int i = 0; i < m_brush.drawObjects.Length; i++)
                {
                    // Change row color
                    if ((i + 1) % 2 == 0)
                    {
                        float x = PreviewShown && PreviewShownAt == i - 1 ? columnSize * 2f : 0;
                        Rect rowRect = new Rect(x, gridStart + i * gridStep, window.position.width, gridStep);
                        Color color = Color.black;
                        color.a = 0.1f;
                        EditorGUI.DrawRect(rowRect, color);
                    }

                    EditorGUILayout.BeginHorizontal();

                    // Draw model name
                    EditorGUILayout.LabelField(m_brush.drawObjects[i].name, labelStyle, 
                        GUILayout.Width(columnSize * 2f), GUILayout.Height(18));
                    
                    // Draw preview
                    if (PreviewMode)
                    {
                        Rect rect = new Rect(0, gridStart + i * gridStep, columnSize * 2f, gridStep);
                        // Skip row under preview
                        if (PreviewShownAt == i - 1)
                            rect.y += gridStep;
                        if (PreviewShownAt == i)
                            rect.height += gridStep;
                        
                        if (rect.Contains(Event.current.mousePosition))
                        {
                            PreviewShown = true;
                            PreviewShownAt = i;
                            if (PreviewShownAt != i)
                                rect.height += gridStep;
                            ShowGridModelPreview(m_brush.drawObjects[i], rect);
                        }
                        else if (PreviewShownAt == i)
                        {
                            PreviewShown = false;
                            PreviewShownAt = -1;
                        }
                    }

                    // Draw settings popup
                    string[] settingsNames = m_brush.modelSettings.Select(s => s.name).ToArray();
                    m_brush.drawObjectsSettings[i] = EditorGUILayout.Popup(m_brush.drawObjectsSettings[i], settingsNames, popupStyle, 
                        GUILayout.Width(columnSize), GUILayout.Height(18));
                    GUILayout.Space(columnSize);

                    // Draw usage field
                    m_brush.drawObjectsUsage[i] = EditorGUILayout.Toggle(m_brush.drawObjectsUsage[i], toggleStyle, 
                        GUILayout.Width(columnSize / 8), GUILayout.Height(18));
                    EditorGUILayout.EndHorizontal();

                    // Draw space for preview
                    if (PreviewShown && PreviewShownAt == i)
                    {
                        EditorGUILayout.GetControlRect(GUILayout.Height(18));
                    }
                }
            }
        }

        public void ShowGridModelPreview(GameObject obj, Rect rect)
        {
            if (gridModelPreviewEditor == null || gridModelPreviewEditor.target != obj)
            {
                // Destroy old editor to avoid error
                if (gridModelPreviewEditor != null)
                    Object.DestroyImmediate(gridModelPreviewEditor);
                gridModelPreviewEditor = Editor.CreateEditor(obj);
            }

            // Draw object preview
            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = EditorGUIUtility.whiteTexture;
            gridModelPreviewEditor.OnInteractivePreviewGUI(rect, bgColor);
        }
        #endregion

        #endregion
    }
}
