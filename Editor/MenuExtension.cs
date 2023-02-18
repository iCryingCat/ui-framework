using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

namespace Com.BaiZe.UIFramework.Editor
{
    public class MenuExtension
    {
        private const string ROOT_MENU = "U2/";

        [MenuItem(ROOT_MENU + "Add Lua Script %L")]
        private static void DisplayLuaScriptsList()
        {
            AddLuaComponentWindow.ShowWindow();
        }
    }
}