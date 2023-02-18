
using UnityEngine;
using UnityEngine.UI;

namespace Com.BaiZe.UIFramework
{
    public abstract class BaseUIUnit : IUIUnit, IMetaBindable, IAssetLinkable
    {
        protected UIEntityController entityController;

        public GameObject gameObject { get; private set; }
        public RectTransform rectTransform { get; private set; }

        public virtual void BindMeta(BaseUIMeta dataMeta) { }
        protected virtual void OnBindEvents() { }
        protected virtual void OnLoaded() { }
        protected virtual void OnShowed() { }
        protected virtual void OnHided() { }

        public void Load()
        {
            this.BindCompUnits();
            this.OnBindEvents();
            this.OnLoaded();
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
            this.PlayActiveAnim();
            this.PlayActiveSound();
            this.OnShowed();
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            this.PlayInActiveAnim();
            this.PlayActiveSound();
            this.OnHided();
        }

        protected T GetCompUnit<T>(int index) where T : Component
        {
            return this.entityController.GetComponentUnit(index).component as T;
        }

        protected T1 GetEntityController<T1>(int index) where T1 : BaseUIUnit, new()
        {
            UIComponentUnit compUnit = this.entityController.GetComponentUnit(index);
            T1 uiEntity = UIManager.NewUIUnit<T1>();
            uiEntity.BindEntityController(compUnit.gameObject);
            return uiEntity;
        }

        public void BindEntityController(GameObject go)
        {
            this.gameObject = go;
            this.rectTransform = go.GetComponent<RectTransform>();
            this.entityController = go.GetComponent<UIEntityController>();
            this.Load();
        }

        private void PlayActiveAnim()
        {
            if (null == this.entityController.director) return;
            this.entityController.director.Play();
        }

        private void PlayInActiveAnim()
        {
            if (null == this.entityController.director) return;

        }

        private void PlayActiveSound()
        {
            if (null == this.entityController.clip) return;
            // SoundManager.Instance.PlayUIEffect(this.entityController.clip);
        }

        public abstract string LinkAssetPath();
        protected virtual void BindCompUnits() { }
    }
}