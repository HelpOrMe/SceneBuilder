using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using Random = UnityEngine.Random;

namespace SceneBuilder
{
    [EditorTool("Object Brush")]
    public class ObjectBrushTool : BrushTool
    {
        public static Dictionary<Action<ObjectData>, bool> ToolsFunctions = new Dictionary<Action<ObjectData>, bool>()
        {
            [EditRotation] = true,
            [DrawHandles] = true,
            [SetObject] = true,
        };

        private static ObjectBrush m_brush => (ObjectBrush)brush;
        private static ObjectBrush m_useBrush;
        private static KeyCode m_rotationButton = KeyCode.E;
        private static bool m_rotationKey = false;

        public override Texture2D BrushIcon => PreferencesProvider.GetPreferences().DefaultBrushIcons[1];

        public override void OnToolGUI(EditorWindow window)
        {
            if (IsAvailable())
            {
                List<Brush> allBrushes = new List<Brush>(m_brush.subBrushes) { m_brush };
                m_useBrush = (ObjectBrush)allBrushes[Random.Range(0, allBrushes.Count - 1)];

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    for (int i = 0; i < m_useBrush.drawObjects.Length; i++)
                    {
                        ObjectData data = GetObjectData(i, hit);
                        if (data != null)
                        {
                            foreach (var pair in new Dictionary<Action<ObjectData>, bool>(ToolsFunctions))
                            {
                                if (pair.Value)
                                {
                                    pair.Key(data);
                                }
                            }
                            DrawHandles(data);
                        }
                    }
                }
                window.Repaint();
            }
        }

        public static ObjectData GetObjectData(int index, RaycastHit hit)
        {
            ModelSettings settings = m_useBrush.modelSettings[m_useBrush.drawObjectsSettings[index]];
            GameObject obj = m_useBrush.drawObjects[index];
            Vector3 position = hit.point + obj.transform.localPosition;
            Quaternion rotation = obj.transform.rotation;
            Vector3 scale = obj.transform.localScale;
            Vector3 normal = settings.RotateByMesh ? hit.normal : Vector3.up;

            if (settings.RotateByMesh)
            {
                if (Physics.Raycast(hit.point + normal, -normal, out RaycastHit objHit))
                {
                    rotation = Quaternion.Euler(Quaternion.LookRotation(normal).eulerAngles + m_useBrush.originRotation);
                    position = rotation * (position - objHit.point) + objHit.point;
                }
            }

            if (settings.MoveToMesh)
            {
                if (Physics.Raycast(position + normal * 2, -normal, out RaycastHit objHit))
                {
                    position = objHit.point;
                }
                else
                {
                    return null;
                }
            }

            // Handle random offset
            position = HandleRandomOffset(settings.randomPositionOffset, position);
            rotation = Quaternion.Euler(HandleRandomOffset(settings.randomRotationOffset, rotation.eulerAngles));
            scale = HandleRandomOffset(settings.randomScaleOffset, scale);

            // Handle offset
            position = HandleOffset(settings.positionOffset, position);
            rotation = Quaternion.Euler(HandleOffset(settings.rotationOffset, rotation.eulerAngles));
            scale = HandleOffset(settings.scaleOffset, scale);

            if (settings.useEnemyColliders)
            {
                List<string> enemyColliderNames = new List<string>(settings.enemyColliderNames);
                if (!settings.enemyColliderNames.Contains("$TurnOffAutoColliders"))
                {
                    {
                        List<Brush> allBrushes = new List<Brush>(m_brush.subBrushes) { m_brush };
                        foreach (ObjectBrush brush in allBrushes)
                        {
                            foreach (GameObject colObj in brush.drawObjects)
                            {
                                enemyColliderNames.Add(colObj.name);
                            }
                        }
                    }
                }

                if (enemyColliderNames.Any(name => name != null && hit.collider.name.Contains(name)))
                {
                    return null;
                }

                if (Physics.Raycast(position + normal * 2, -normal, out RaycastHit objHit))
                {
                    if (enemyColliderNames.Any(name => name != null && objHit.collider.name.Contains(name)))
                    {
                        return null;
                    }
                }
            }

            return new ObjectData()
            {
                settings = settings,
                obj = obj,
                position = position,
                rotation = rotation,
                scale = scale,
                normal = normal,
                hit = hit,
            };
        }

        private static Vector3 HandleOffset(OffsetSettings settings, Vector3 value)
        {
            if (settings.useOffset)
            {
                value += settings.offset;
            }
            return value;
        }

        private static Vector3 HandleRandomOffset(RandomOffsetSettings settings, Vector3 value)
        {
            if (settings.useOffset)
            {
                switch (settings.randomType)
                {
                    case (RandomType.Range):
                        value += new Vector3(
                            Random.Range(settings.randomRangesMin.x, settings.randomRangesMax.x),
                            Random.Range(settings.randomRangesMin.y, settings.randomRangesMax.y),
                            Random.Range(settings.randomRangesMin.z, settings.randomRangesMax.z));
                        break;

                    case (RandomType.Between):
                        if (settings.randomArray.Length > 0)
                        {
                            value += settings.randomArray[Random.Range(0, settings.randomArray.Length - 1)];
                        }
                        break;
                }
            }
            return value;
        }

        public static void EditRotation(ObjectData data)
        {
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == m_rotationButton)
            {
                ToolsFunctions[SetObject] = true;
                m_rotationKey = false;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == m_rotationButton)
            {
                Event.current.Use();  // Skip tool hotkey
                ToolsFunctions[SetObject] = false;
                m_rotationKey = true;
                data.settings.rotationOffset.useOffset = true;
            }

            // If the rotation key was pressed
            if (m_rotationKey)
            {
                Vector3 normal = data.normal;
                Vector3 position = data.position;
                ModelSettings settings = data.settings;
                Vector3 from = Quaternion.Euler(settings.rotationOffset.offset) * (Vector3.right * 0.6f);

                // Arc
                Handles.color = new Color(255, 255, 255, 0.3f);
                Handles.DrawWireArc(position, normal, from, 360, 0.6f);

                Handles.color = new Color(255, 255, 255, 0.7f);
                Handles.DrawLine(position, position + from);

                float x = Event.current.delta.x;
                float y = Event.current.delta.y;
                settings.rotationOffset.offset += normal * (Mathf.Abs(x) > Mathf.Abs(y) ? x : y);
            }
        }

        public static void DrawHandles(ObjectData data)
        {
            float radius = 0.45f;

            GameObject obj = data.obj;
            Quaternion rotation = data.rotation;
            Vector3 position = data.position;
            Vector3 normal = data.normal;

            Color red = Color.Lerp(Color.red, Color.white, 0.3f);
            Color green = Color.Lerp(Color.green, Color.white, 0.3f);
            Color blue = Color.Lerp(Color.blue, Color.white, 0.3f);
            Color normalColor = Color.white;
            Color rotationColor = new Color(255, 255, 255, 0.3f);

            // Normal line
            Handles.color = normalColor;
            Handles.DrawLine(position, position - normal * (radius * 1.5f));
            // Rotation line
            Handles.color = rotationColor;
            Handles.DrawLine(position, position + (rotation * obj.transform.forward) * radius);
            // Up axis line
            Handles.color = green;
            Handles.DrawLine(position, position + obj.transform.up * radius);
            // Right axis line
            Handles.color = red;
            Handles.DrawLine(position, position + obj.transform.right * radius);
            // Forward axis line
            Handles.color = blue;
            Handles.DrawLine(position, position + obj.transform.forward * radius);
            // Arc
            Handles.color = GetObjectColor(obj);
            Vector3 from = Vector3.one - new Vector3(Math.Abs(normal.x), Math.Abs(normal.y), Math.Abs(normal.z));
            Handles.DrawWireArc(position, normal, from, 360, radius);
        }

        public static void SetObject(ObjectData data)
        {
            ModelSettings settings = data.settings;
            GameObject obj = data.obj;
            Vector3 position = data.position;
            Quaternion rotation = data.rotation;
            Vector3 scale = data.scale;

            // Set object
            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                (!settings.RandomSet || Random.value <= settings.RandomSetPercent))
            {
                Transform parent = Selection.activeTransform;
                if (parent != null && (parent.gameObject == obj || parent.gameObject == m_useBrush.selectedObject))
                {
                    parent = null;
                    Selection.activeTransform = null;
                }
                GameObject newObject = Instantiate(obj, position, rotation, parent);
                newObject.name = obj.name;
                newObject.transform.localScale = scale;
            }
        }

        private static Color GetObjectColor(GameObject obj)
        {
            // Get material color from Renderer component
            Renderer renderer = obj.GetComponent<Renderer>();
            Color targetColor = Color.white;
            if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_Color"))
                targetColor = renderer.sharedMaterial.GetColor("_Color");
            return targetColor;
        }

        private Bounds GetObjectBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(obj.transform.position, Vector3.one);

            // Try get bounds from collider
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                bounds = collider.bounds;
            }
            else
            {
                // Try get bounds from renderer
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    bounds = renderer.bounds;
                }
            }
            return bounds;
        }

        public class ObjectData
        {
            public ModelSettings settings;
            public GameObject obj;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public Vector3 normal;
            public RaycastHit hit;
        }
    }
}
