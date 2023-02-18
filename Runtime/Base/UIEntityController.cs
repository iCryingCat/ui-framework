using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;

using UObject = UnityEngine.Object;

namespace Com.BaiZe.UIFramework
{
    [Serializable]
    public class UIComponentUnit
    {
        public string fieldName;
        public GameObject gameObject;
        public Component component;
    }

    public class UIEntityController : MonoBehaviour
    {
        [HideInInspector] public List<UIComponentUnit> listCompUnits = new List<UIComponentUnit>();
#if UNITY_EDITOR
        [HideInInspector] public string luaUIEntityRefPath;
#endif
        public EnumUILayer layer = EnumUILayer.Common;
        public EnumUINode node = EnumUINode.Middle;

        public AudioClip clip = null;
        public PlayableDirector director = null;

        private void Awake()
        {
            this.director = GetComponent<PlayableDirector>();
        }

        public UIComponentUnit GetComponentUnit(int index)
        {
            if (index >= this.listCompUnits.Count)
                throw new IndexOutOfRangeException();
            return this.listCompUnits[index];
        }
    }
}