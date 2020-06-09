using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SceneBuilder.Windows
{
    public class BrushWindow : EditorWindow
    {
        // Window info
        public Brush brush;
        public object brushEditor;
        public bool smallMode = false;
        public bool smallModeLock = false;
        public Vector2 scroll = Vector2.zero;
        public HeaderType headerType = HeaderType.Brush;

        // Create menu info
        public int selectedType = 0;
        public string brushName = "New Brush";
        public List<Brush> importBrushes = new List<Brush>();
        public int importBrush;

        // Change menu info
        public int removeAt = -1;
        public string removeText;

        [MenuItem("Tools/SceneBuilder/Brush window")]
        public static void ShowWindow()
        {
            BrushWindow window = GetWindow<BrushWindow>();
            window.Show();
        }

        public void UpdateBrushEditor()
        {
            if (brushEditor != null)
            {
                BrushUtility.Invoke(brushEditor, "OnDisable");
            }
            brushEditor = BrushUtility.GetBrushEditor(brush);
            BrushUtility.Invoke(brushEditor, "Init", brush, this);
            BrushUtility.UpdateBrushTool(brush);
        }

        public void SetBrush(Brush brush, bool focus = true)
        {
            this.brush = brush;
            if (focus)
            {
                headerType = HeaderType.Brush;
            }
            UpdateBrushEditor();
        }

        public void RemoveBrush(bool focus = true)
        {
            brush = null;
            brushEditor = null;
            if (focus) 
            {
                headerType = HeaderType.Change;
            }
        }

        #region GUI Calls
        private void OnGUI()
        {
            // Сhecks
            if (brush == null && headerType == HeaderType.Brush)
            {
                headerType = HeaderType.Change;
            }
            if (brush != null && brushEditor == null)
            {
                UpdateBrushEditor();
            }

            // Update selected brush tool
            // List<Brush> allBrushes = new List<Brush>(brush.subBrushes) { brush };
            BrushUtility.UpdateBrushTool(brush /*allBrushes[UnityEngine.Random.Range(0, allBrushes.Count - 1)]*/);

            // Set small mode state
            smallModeLock = smallModeLock && position.width <= 373;
            smallMode = position.width <= 373 && !smallModeLock;

            // Draw header with content below
            BeginScrollBar();
            EditorGUILayout.Space();
            DrawHeader();
            switch (headerType)
            {
                // Draw brush editor
                case HeaderType.Brush:
                    BrushUtility.Invoke(brushEditor, "OnGUI");
                    break;
                // Draw change menu
                case HeaderType.Change:
                    DrawChangeMenu();
                    break;
                // Draw create brush menu
                case HeaderType.New:
                    DrawCreateBrushMenu();
                    break;
            }
            EndScrollBar();
        }

        public void BeginScrollBar()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
            GUILayout.BeginVertical();
            scroll = GUILayout.BeginScrollView(
                scroll, false, false, 
                GUILayout.Width(position.width), GUILayout.MinHeight(200), 
                GUILayout.MaxHeight(1000), 
                GUILayout.ExpandHeight(true));
        }

        public void EndScrollBar()
        {
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #region Hedaer 
        public void DrawHeader()
        {
            // Draw small mode state
            if (smallMode)
            {
                GUIStyle modeStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    fontStyle = FontStyle.Italic
                };
                if (GUILayout.Button("(Small mode)", modeStyle))
                    smallModeLock = true;
            }

            // Draw brush name

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, fontStyle = FontStyle.Bold, };
            GUIStyle center = new GUIStyle(labelStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
            GUIStyle left = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleLeft };
            GUIStyle right = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight };

            EditorGUILayout.BeginHorizontal();
            switch (headerType)
            {
                case HeaderType.Brush:
                    DrawBrushHeaderType(center, left, right);
                    break;

                case HeaderType.New:
                    DrawNewHeaderType(center, left, right);
                    break;

                case HeaderType.Change:
                    DrawChangeHeaderType(center, left, right);
                    break;
            }
            EditorGUILayout.EndHorizontal();
        }

        public Color GetGreenColor()
        {
            Color color = Color.Lerp(Color.green, Color.white, 0.5f);
            color.a = 0.35f;
            return color;
        }

        public Color GetYellowColor()
        {
            Color color = Color.Lerp(Color.yellow, Color.white, 0.4f);
            color.a = 0.35f;
            return color;
        }

        private void DrawNewHeaderType(GUIStyle center, GUIStyle left, GUIStyle right)
        {
            center.normal.textColor = GetGreenColor();
            left.normal.textColor = GetYellowColor();

            if (GUILayout.Button(brush == null ? "---" : brush.name, right, GUILayout.MinWidth(85)) && brush != null) headerType = HeaderType.Brush;
            GUILayout.Label("New", center);
            if (GUILayout.Button("Change", left, GUILayout.MinWidth(85))) headerType = HeaderType.Change;
        }

        private void DrawBrushHeaderType(GUIStyle center, GUIStyle left, GUIStyle right)
        {
            right.normal.textColor = GetGreenColor();
            left.normal.textColor = GetYellowColor();

            if (GUILayout.Button("New", right, GUILayout.MinWidth(85))) headerType = HeaderType.New;
            brush.name = GUILayout.TextField(brush.name, center);
            if (GUILayout.Button("Change", left, GUILayout.MinWidth(85))) headerType = HeaderType.Change;
        }

        private void DrawChangeHeaderType(GUIStyle center, GUIStyle left, GUIStyle right)
        {
            right.normal.textColor = GetGreenColor();
            center.normal.textColor = GetYellowColor();

            if (GUILayout.Button("New", right, GUILayout.MinWidth(85))) headerType = HeaderType.New;
            GUILayout.Label("Change", center);
            if (GUILayout.Button(brush == null ? "---" : brush.name, left, GUILayout.MinWidth(85)) && brush != null) headerType = HeaderType.Brush;
        }

        public enum HeaderType
        {
            Brush,
            New,
            Change
        }
        #endregion

        #region Create brush menu
        public void DrawCreateBrushMenu()
        {
            EditorGUILayout.Space();
            UpdateImportBrush();
            DrawCreateMenuFields();
        }

        public void DrawCreateMenuFields()
        {
            // Brush type
            int _selectedType = EditorGUILayout.Popup("Brush type", selectedType, BrushUtility.BrushTypes.Select(t => t.Name).ToArray());
            selectedType = _selectedType;

            // Brush name
            brushName = EditorGUILayout.TextField("Brush name", brushName);

            // Import settings
            if (importBrushes.Count > 0)
            {
                List<string> brushNames = new List<string>() { "None" };
                brushNames.AddRange(importBrushes.Select(b => b.name));
                importBrush = EditorGUILayout.Popup("Import settings from", importBrush, brushNames.ToArray());
            }

            // HelpBox
            if (BrushUtility.FindAllBrushes().Select(b => b.name).Contains(brushName))
            {
                EditorGUILayout.HelpBox($"{brushName} will be replaced.", MessageType.Warning);
            }

            // Create button
            if (GUILayout.Button($"Create: {brushName}"))
            {
                Brush newBrush = (Brush)BrushUtility.CreateNewBrush(BrushUtility.BrushTypes[selectedType], brushName, true);
                if (importBrush > 0)
                {
                    BrushUtility.CopySettings(importBrushes[importBrush - 1], newBrush);
                }
                SetBrush(newBrush);
            }
        }

        public void UpdateImportBrush()
        {
            importBrushes.Clear();
            foreach (Brush brush in BrushUtility.FindAllBrushes())
            {
                string selectedTypeName = BrushUtility.BrushTypes.Select(t => t.Name).ToArray()[selectedType];
                if (brush.GetType().Name == selectedTypeName && brush.name != brushName)
                {
                    importBrushes.Add(brush);
                }
            }
        }
        #endregion

        #region Change menu
        public void DrawChangeMenu()
        {
            EditorGUILayout.Space();
            float start = DrawChangeMenuGridHeader();
            DrawChangeMenuGridContent(start);
            EditorGUILayout.Space();
            DrawChangeMenuAnotherBrush();
        }

        public float DrawChangeMenuGridHeader()
        {
            GUIStyle label = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            float start = GUILayoutUtility.GetLastRect().y + 5;

            // Draw header
            EditorGUI.DrawRect(new Rect(0, start, position.width, 20), new Color(0, 0, 0, 0.2f));  // Background
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Brush", label, GUILayout.MaxWidth(position.width / 4f));
            EditorGUILayout.LabelField("Brush type", label, GUILayout.MaxWidth(position.width / 4f));
            EditorGUILayout.LabelField("Actions", label, GUILayout.MaxWidth(position.width / 2f - 20f));
            EditorGUILayout.EndHorizontal();

            return start;
        }

        public void DrawChangeMenuGridContent(float start)
        {
            GUIStyle label = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };

            // Draw content
            Brush[] brushes = BrushUtility.FindAllBrushes().ToArray();
            for (int i = 0; i < brushes.Length; i++)
            {
                bool isbrushSelected = brush == brushes[i];
                bool isSubBrushSelected = brush != null && brush.subBrushes.Contains(brushes[i]);
                Brush currentBrush = brushes[i];

                // Draw grid background
                Rect rowRect = new Rect(0, start + (i + 1) * 20, position.width, 20);
                if ((i + 1) % 2 == 0)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0, 0, 0, 0.1f));
                }
                if (isbrushSelected)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0, 255, 0, 0.1f));
                }
                if (isSubBrushSelected)
                {
                    EditorGUI.DrawRect(rowRect, new Color(255, 255, 0, 0.1f));
                }

                // Draw brush name and type
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(currentBrush.name, label, GUILayout.MaxWidth(position.width / 4f), GUILayout.MaxHeight(18));
                EditorGUILayout.LabelField(currentBrush.GetType().Name, label, GUILayout.MaxWidth(position.width / 4f), GUILayout.MaxHeight(18));

                // Remove mode off
                if (removeAt != i)
                {

                    // Draw select button
                    string text = isbrushSelected || isSubBrushSelected ? "Selected" : "Select";
                    if (GUILayout.Button(text, GUILayout.MaxWidth(position.width / 4f), GUILayout.MaxHeight(16)))
                    {
                        if (brush == null)
                        {
                            SetBrush(brushes[i], false);
                        }
                        else if (!isSubBrushSelected && currentBrush.GetType().Name == brush.GetType().Name)
                        {
                            brush.subBrushes.Add(currentBrush);
                        }

                        if (isbrushSelected)
                        {
                            RemoveBrush();
                        }
                        else if (isSubBrushSelected)
                        {
                            brush.subBrushes.Remove(currentBrush);
                        }
                    }
                }
                
                if (removeAt != i && GUILayout.Button("Remove", GUILayout.MaxWidth(position.width / 4f), GUILayout.MaxHeight(16)))
                {
                    removeText = $"Write \"{brushes[i].name}\" or \"Cancel\"";
                    removeAt = i;
                }
                if (removeAt == i)
                {
                    removeText = GUILayout.TextField(removeText, label);
                    if (removeText == brushes[i].name)
                    {
                        BrushUtility.DeleteBrush(currentBrush);
                        brush = null;
                        removeAt = -1;
                    }
                    else if (removeText == "Cancel") removeAt = -1;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawChangeMenuAnotherBrush()
        {
            Brush anotherBrush = (Brush)EditorGUILayout.ObjectField("Load another brush", null, typeof(Brush), true);
            if (anotherBrush != null)
            {
                BrushUtility.AddBrush(anotherBrush);
                SetBrush(anotherBrush);
            }
        }
        #endregion
        #endregion
    }
}
