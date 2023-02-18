using System;
using System.Collections.Generic;
using Com.BaiZe.SharpToolSet;
using UnityEngine;
using UnityEngine.UI;

namespace Com.BaiZe.UIFramework
{
    [Serializable]
    public enum EnumUILayer
    {
        Scene,
        Common,
        Touch,
        Tips,
    }

    [Serializable]
    public enum EnumUINode
    {
        Low,
        Middle,
        Top,
    }

    public class UICanvas : Singleton<UICanvas>
    {
        public const string PATH_UI_CANVAS = "UICanvas";
        private Dictionary<EnumUILayer, Canvas> layerMap = new Dictionary<EnumUILayer, Canvas>();
        private Dictionary<EnumUILayer, Dictionary<EnumUINode, RectTransform>> nodeMap = new Dictionary<EnumUILayer, Dictionary<EnumUINode, RectTransform>>();

        public void LoadCanvas()
        {
            var rootAsset = Resources.Load<GameObject>(PATH_UI_CANVAS);
            var root = GameObject.Instantiate<GameObject>(rootAsset).transform;

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            foreach (var layer in Enum.GetValues(typeof(EnumUILayer)))
            {
                var layerGO = root.Find(layer.ToString());
                var layerCanvas = layerGO.GetComponent<Canvas>();
                this.layerMap[(EnumUILayer)layer] = layerCanvas;

                foreach (var node in Enum.GetValues(typeof(EnumUINode)))
                {
                    var nodeGO = layerGO.Find(node.ToString()).GetComponent<RectTransform>();
                    this.nodeMap[(EnumUILayer)layer][(EnumUINode)node] = nodeGO;
                }
            }
            root.gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public Canvas GetLayerCanvas(EnumUILayer layer)
        {
            this.layerMap.TryGetValue(layer, out var canvas);
            return canvas;
        }

        public RectTransform GetNode(EnumUILayer layer, EnumUINode node)
        {
            this.nodeMap.TryGetValue(layer, out var nodes);
            nodes.TryGetValue(node, out var rect);
            return rect;
        }

        public void PushUI(UIUnitTag tag)
        {

        }
    }
}