using System;
using System.Collections.Generic;
using System.IO;
using Com.BaiZe.GameBase;
using UnityEngine;

/// <summary>
/// UI管理类
/// 面板层级管理
/// 进出UI栈显示隐藏面板
/// </summary>
namespace Com.BaiZe.UIFramework
{
    public class UIManager
    {
        public const string UI_PREFAB_PATH = "Prefabs/UI";
        private static readonly Dictionary<Type, IUIEntity> singleEntityMap = new Dictionary<Type, IUIEntity>();
        private static readonly Stack<IUIEntity> allEntityStack = new Stack<IUIEntity>();
        private static readonly Dictionary<EnumUILayer, Stack<IUIEntity>> layerEntityMap = new Dictionary<EnumUILayer, Stack<IUIEntity>>();

        public static T GetSingleUnit<T>() where T : IUIEntity
        {
            if (!singleEntityMap.ContainsKey(typeof(T)))
            {
                throw new Exception(string.Format("The panel {0} does not exist", typeof(T)));
            }
            return (T)singleEntityMap[typeof(T)];
        }

        public static T1 ShowUI<T1>() where T1 : BaseUIEntity, new()
        {
            Type entityType = typeof(T1);
            T1 uiEntity = null;
            if (singleEntityMap.ContainsKey(entityType)) uiEntity = (T1)singleEntityMap[entityType];
            if (uiEntity == null)
            {
                uiEntity = NewUIUnit<T1>();
                LoadUI<T1>(uiEntity);
            }
            allEntityStack.Push(uiEntity);
            uiEntity.Show();
            return uiEntity;
        }

        public static T1 NewUIUnit<T1>() where T1 : BaseUIUnit, new()
        {
            T1 uiUnit = new T1();
            Type type = typeof(T1);
            object[] tags = (UIUnitTag[])type.GetCustomAttributes(typeof(UIUnitTag), false);
            UIUnitTag uiUnitTag = tags.Length > 0 ? tags[0] as UIUnitTag : null;
            if (uiUnitTag != null)
            {
                if (uiUnitTag.isSingleton) singleEntityMap[type] = (IUIEntity)uiUnit;
            }
            return uiUnit;
        }

        private static void LoadUI<T1>(T1 uiUnit) where T1 : BaseUIUnit, new()
        {
            string assetPath = uiUnit.LinkAssetPath();
            GameObject uiAsset = LoadUIAsset(assetPath);
            GameObject uiInstance = ResMgr.Instantiate(uiAsset);
            Debug.Assert(uiInstance);
            uiUnit.BindEntityController(uiInstance);
        }

        public static T1 Instantiate<T1>(GameObject uiAsset) where T1 : BaseUIUnit, new()
        {
            T1 uiUnit = NewUIUnit<T1>();
            GameObject uiInstance = ResMgr.Instantiate(uiAsset);
            uiUnit.BindEntityController(uiInstance);
            return uiUnit;
        }

        private static GameObject LoadUIAsset(string slug)
        {
            string path = Path.Combine(UI_PREFAB_PATH, slug);
            return ResMgr.LoadPrefab(path);
        }
    }
}