using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Com.BaiZe.GameBase.Editor;
using Com.BaiZe.SharpToolSet;

using UnityEditor;
using UnityEditor.IMGUI.Controls;

using UnityEngine;

namespace Com.BaiZe.UIFramework.Editor
{
    public class AddLuaComponentWindow : EditorWindow
    {
        [SerializeField] TreeViewState m_TreeViewState;


        void OnEnable()
        {
            this.UpdateTreeView();
        }

        private LuaScriptTreeView m_TreeView;
        private SearchField m_SearchField;
        private void UpdateTreeView()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            m_TreeView = new LuaScriptTreeView(m_TreeViewState);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }

        public static void ShowWindow()
        {
            var window = GetWindow<AddLuaComponentWindow>();
            window.titleContent = new GUIContent("Lua Component");
            window.minSize = new Vector2(270, 360);
            window.Show();
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            ToolbarRender();
            TreeViewRender();
            GUILayout.EndVertical();
            bool updateCode = GUILayout.Button("Update All Lua Scripts");
            if (updateCode) this.UpdateTreeView();
        }

        private void ToolbarRender()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
            GUILayout.EndHorizontal();
        }

        private void TreeViewRender()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
        }

        public class LuaScriptTreeView : TreeView
        {
            private string pathLuaScripts;
            private List<FileMeta> listLuaScripts = new List<FileMeta>();

            public LuaScriptTreeView(TreeViewState state) : base(state)
            {
                showBorder = true;

                pathLuaScripts = EditorCache.Get<string>(EnumEditorCacheIndex.PathLuaScripts) ?? Application.dataPath;
                listLuaScripts = SharpToolSet.FileUtil.FindFilesByTypeWithDepIncludeDir(pathLuaScripts, "lua");

                Reload();
            }

            public LuaScriptTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader) { }

            protected override TreeViewItem BuildRoot()
            {
                var treeRoot = new TreeViewItem(-1, -1);
                var allLuaScripts = new List<TreeViewItem>();
                int id = 0;
                foreach (var lua in this.listLuaScripts)
                {
                    allLuaScripts.Add(new TreeViewItem(id++, lua.depth, lua.isDir ? lua.name.FileName() : lua.name));
                }
                SetupParentsAndChildrenFromDepths(treeRoot, allLuaScripts);
                return treeRoot;
            }

            protected override void DoubleClickedItem(int id)
            {
                base.DoubleClickedItem(id);
                if (this.listLuaScripts[id].isDir) return;

                var activeGO = Selection.activeGameObject;
                if (activeGO == null)
                {
                    EditorTips.ShowErrorTips("请选择需要添加组件的游戏物体！！！");
                    return;
                }
                if (!EditorHelper.IsAssetGameObjectIsInPrefabScene(activeGO, out var assetPath)) return;

                this.AddUnityComponentLuaDefineTag(id);

                this.AddLuaScripts(id);
            }

            private void AddLuaScripts(int id)
            {
                var activeGO = Selection.activeGameObject;
                if (activeGO && !activeGO.TryGetComponent<UIEntityController>(out var oldUIEntityController))
                {
                    activeGO._TryRemoveComponent<UIEntityController>();
                    var controller = activeGO.AddComponent<UIEntityController>();
                    var controllerSO = new SerializedObject(controller);
                    var luaUIEntityRefPath = controllerSO.FindProperty("luaUIEntityRefPath");
                    luaUIEntityRefPath.stringValue = this.listLuaScripts[id].path;

                    EditorUtility.SetDirty(activeGO);
                    controllerSO.ApplyModifiedPropertiesWithoutUndo();
                    controllerSO.UpdateIfRequiredOrScript();

                    AssetDatabase.Refresh();
                }
            }

            private void AddUnityComponentLuaDefineTag(int id)
            {
                string tag0 = UIEntityControllerEditor.TAG0;
                string tag1 = UIEntityControllerEditor.TAG1;
                var luaPath = this.listLuaScripts[id].path;
                var luaTxt = File.ReadAllText(luaPath);
                var matchTag0 = luaTxt.Contains(tag0);
                var matchTag1 = luaTxt.Contains(tag0);
                if (!matchTag0 || !matchTag1)
                {
                    var classRegex = @"(UI[a-zA-Z0-9_]+)\s*=\s*\{([a-zA-Z0-9_.#><=+\-*/^&|~,""'\[\]\s*]*)}";
                    var isMatchClass = Regex.IsMatch(luaTxt, classRegex);
                    if (!isMatchClass)
                    {
                        EditorTips.ShowErrorTips("请先声明Lua UIEntity 类型！");
                        return;
                    }
                    var matchResult = Regex.Match(luaTxt, classRegex);
                    var luaClass = matchResult.Groups[0].Value;
                    var className = matchResult.Groups[1].Value;
                    var dataBody = matchResult.Groups[2].Value;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(className + " = {");
                    if (!dataBody.IsNullOrEmpty())
                    {
                        dataBody = dataBody.Trim();
                        if (!dataBody.IsSuffixWith(",")) dataBody += ",";
                        sb.AppendLine(dataBody.TabFormat());
                    }
                    sb.AppendLine(tag0.TabFormat());
                    sb.AppendLine(tag1.TabFormat());
                    sb.AppendLine("}");
                    string luaClassWithTag = sb.ToString();
                    luaTxt = luaTxt.Replace(luaClass, luaClassWithTag);
                    File.WriteAllText(luaPath, luaTxt);
                }
                EditorTips.ShowCommonTips("Add Lua Script Succ！");
            }
        }
    }
}