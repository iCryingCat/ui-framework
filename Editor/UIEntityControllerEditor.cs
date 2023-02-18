using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Com.BaiZe.GameBase;
using Com.BaiZe.GameBase.Editor;
using Com.BaiZe.SharpToolSet;
using UnityEditor;

using UnityEngine;
using UnityEngine.UI;

namespace Com.BaiZe.UIFramework.Editor
{
    [CustomEditor(typeof(UIEntityController))]
    public class UIEntityControllerEditor : UnityEditor.Editor
    {
        private UIEntityController entityController;

        private SerializedProperty listCompUnits;
        private SerializedProperty luaUIEntityRefPath;

        public const string TAG0 = "--[[UNITY LUA AUTO INJECT THEN]]";
        public const string TAG1 = "--[[UNITY LUA AUTO INJECT END]]";

        private void OnEnable()
        {
            this.entityController = this.target as UIEntityController;

            this.listCompUnits = this.serializedObject.FindProperty("listCompUnits");
            this.luaUIEntityRefPath = this.serializedObject.FindProperty("luaUIEntityRefPath");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();
            this.ShortcutMenu();
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            this.CompUnitsList();
            EditorGUILayout.Space();
            this.CompUnitDragArea();
            EditorGUILayout.EndVertical();
        }

        private void ShortcutMenu()
        {
            EditorGUILayout.BeginHorizontal();
            bool updateCode = GUILayout.Button("更新代码");
            bool jumpCode = GUILayout.Button("打开脚本");
            if (updateCode)
            {
                this.UpdateLuaUnityComponent();
            }
            if (jumpCode)
            {
                EditorHelper.OpenAssetByCodeEditor(this.luaUIEntityRefPath.stringValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CompUnitsList()
        {
            for (int unitIndex = 0; unitIndex < this.listCompUnits.arraySize; ++unitIndex)
            {
                var fieldName = this.listCompUnits.GetArrayElementAtIndex(unitIndex).FindPropertyRelative("fieldName").stringValue;
                var gameObject = this.listCompUnits.GetArrayElementAtIndex(unitIndex).FindPropertyRelative("gameObject").objectReferenceValue as GameObject;
                var component = this.listCompUnits.GetArrayElementAtIndex(unitIndex).FindPropertyRelative("component").objectReferenceValue as Component;

                EditorGUILayout.BeginHorizontal();
                this.PinInstance(unitIndex, gameObject);
                this.UIEntityField(unitIndex, fieldName, component.GetType());
                this.UIEntityCompUnit(unitIndex, gameObject, component);
                this.RemoveUnitRef(unitIndex);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void PinInstance(int unitIndex, GameObject gameObject)
        {
            string instanceName = "{0}.{1}".Format(unitIndex, gameObject.name);
            bool pin = GUILayout.Button(instanceName, GUILayout.ExpandWidth(true));
            if (pin) EditorGUIUtility.PingObject(gameObject);
        }

        private void UIEntityField(int unitIndex, string fieldName, Type component)
        {
            string fieldNameNew = EditorGUILayout.TextField(fieldName, GUILayout.ExpandWidth(true));
            if (fieldNameNew != fieldName)
            {
                this.listCompUnits.GetArrayElementAtIndex(unitIndex).FindPropertyRelative("fieldName").stringValue = fieldNameNew;
                EditorUtility.SetDirty(this.target);
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.UpdateIfRequiredOrScript();
            }
        }

        private void UIEntityCompUnit(int unitIndex, GameObject gameObject, Component component)
        {
            List<string> componentOptions = new List<string>();
            int compIndex = 0;
            Component[] compsHangOn = gameObject.GetComponents<Component>();
            for (int i = 0; i < compsHangOn.Length; ++i)
            {
                Component comp = compsHangOn[i];
                if (comp == component) compIndex = i;
                string compName = comp.GetType().ToString().Suffix(".");
                componentOptions.Add("{0}.{1}".Format(i, compName));
            }
            int optionIndex = EditorGUILayout.Popup(compIndex, componentOptions.ToArray(), GUILayout.ExpandWidth(true));
            if (optionIndex != compIndex)
            {
                this.listCompUnits.GetArrayElementAtIndex(unitIndex).FindPropertyRelative("component")
                    .objectReferenceValue = compsHangOn[optionIndex];
                var fieldName = this.listCompUnits.GetArrayElementAtIndex(unitIndex).FindPropertyRelative("fieldName");
                fieldName.stringValue = FormatFieldNameWithCompType(fieldName.stringValue, compsHangOn[optionIndex].GetType());

                EditorUtility.SetDirty(this.target);
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.UpdateIfRequiredOrScript();
            }
        }

        private void RemoveUnitRef(int unitIndex)
        {
            if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
            {
                this.listCompUnits.DeleteArrayElementAtIndex(unitIndex);

                EditorUtility.SetDirty(this.target);
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.UpdateIfRequiredOrScript();
            }
        }

        private void CompUnitDragArea()
        {
            Rect rect = EditorGUILayout.GetControlRect(true, 60);
            if (rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (Event.current.type == EventType.DragExited)
                {
                    var objs = DragAndDrop.objectReferences;
                    for (int i = 0; i < objs.Length; ++i)
                    {
                        var instance = objs[i] as GameObject;
                        if (!instance) return;
                        string goName = instance.name;
                        this.listCompUnits.InsertArrayElementAtIndex(this.listCompUnits.arraySize);
                        SerializedProperty compUnit = this.listCompUnits.GetArrayElementAtIndex(this.listCompUnits.arraySize - 1);
                        compUnit.FindPropertyRelative("fieldName").stringValue = goName;
                        compUnit.FindPropertyRelative("gameObject").objectReferenceValue = instance;
                        compUnit.FindPropertyRelative("component").objectReferenceValue = instance.transform;

                        EditorUtility.SetDirty(this.target);
                        this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        this.serializedObject.UpdateIfRequiredOrScript();
                    }
                }
            }
        }

        private string FormatFieldNameWithCompType(string fieldName, Type component)
        {
            var fieldPrefix = new Dictionary<Type, string> {
                    { typeof(Text), "txt"},
                    { typeof(Button), "btn"},
                    { typeof(Image), "img"},
                    { typeof(RectTransform), "rect"},
                    { typeof(GameObject), "go"},
                    { typeof(UIEntityController), "ui"},
                };
            var prefixRegex = "txt|btn|img|rect|go|ui";
            if (Regex.IsMatch(fieldName, prefixRegex))
            {
                var prefixMatch = Regex.Match(fieldName, prefixRegex);
                fieldName = fieldName.TrimPrefix(prefixMatch.Groups[0].Value);
            }
            if (fieldPrefix.ContainsKey(component))
                fieldName = fieldPrefix[component] + fieldName;
            return fieldName;
        }

        private void UpdateLuaUnityComponent()
        {
            var activeGO = Selection.activeGameObject;
            if (!EditorHelper.IsAssetGameObjectIsInPrefabScene(activeGO, out var assetPath)) return;

            string uiPrefabRootPath = Path.Combine(ResMgr.LOAD_PATH_EDITOR, UIManager.UI_PREFAB_PATH).PathFormat();
            assetPath = assetPath.Substring(uiPrefabRootPath.Length + 1);

            var scriptPath = this.luaUIEntityRefPath.stringValue;
            if (scriptPath.IsNullOrEmpty()) return;

            var luaTxt = File.ReadAllText(scriptPath);

            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(scriptPath))
            {
                bool inFill = false;
                while (sr.Peek() != -1)
                {
                    string line = sr.ReadLine();
                    if (inFill && line.Contains(TAG1))
                    {
                        inFill = false;
                    }
                    if (inFill) continue;
                    sb.AppendLine(line);
                    if (!inFill && line.Contains(TAG0))
                    {
                        inFill = true;

                        StringBuilder fields = new StringBuilder();
                        for (int j = 0; j < this.listCompUnits.arraySize; ++j)
                        {
                            SerializedProperty fieldNameProperty = this.listCompUnits.GetArrayElementAtIndex(j).FindPropertyRelative("fieldName");
                            SerializedProperty componentProperty = this.listCompUnits.GetArrayElementAtIndex(j).FindPropertyRelative("component");
                            Component component = componentProperty.objectReferenceValue as Component;
                            string fieldType = component.GetType().ToString().Suffix(".");
                            string fieldName = fieldNameProperty.stringValue;

                            fields.AppendLine("{0} = {1}, --{2}".Format(fieldName, j, fieldType).TabFormat());
                        }
                        sb.Append(fields);
                        sb.AppendLine("slug = \"{0}\",".Format(assetPath).TabFormat());
                    }
                }
            }
            luaTxt = sb.ToString();
            File.WriteAllText(scriptPath, luaTxt);
        }
    }
}