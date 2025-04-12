using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Minimalist.Utility;

namespace Minimalist.Quantity
{
    public abstract class QuantitySubscriber : UIElementBhv
    {
        // Public properties
        public QuantityBhv Quantity => _quantity;

        // Protected properties
        protected UnityAction OnQuantityAmountChangedAction { get => _onQuantityAmountChangedAction; set => _onQuantityAmountChangedAction = value; }
        protected UnityAction OnQuantityInvalidRequestAction { get => _onQuantityInvalidRequestAction; set => _onQuantityInvalidRequestAction = value; }

        // Private properties
        private QuantitySubscriberCanvasBhv SubscriberCanvas => _subscriberCanvas == null || !Application.isPlaying ? GetComponentInParent<QuantitySubscriberCanvasBhv>() : _subscriberCanvas;

        // Private serialized fields
        [Header("Subscription:")]
        [Tooltip("Quantity to subscribe to.")]
        [SerializeField] private QuantityBhv _quantity;

        // Private fields
        private QuantitySubscriberCanvasBhv _subscriberCanvas;
        private QuantityBhv _previousQuantity;
        private UnityAction _onQuantityAmountChangedAction;
        private UnityAction _onQuantityInvalidRequestAction;

        protected abstract void SpecifyQuantityEventActions();

        protected virtual void OnEnable()
        {
            InstantiateCanvasIfNeeded();

            StartListeningTo(_quantity);
        }

        protected virtual void OnDisable()
        {
            StopListeningTo(_quantity);
        }

        protected void OnDestroy()
        {
            StopListeningTo(_quantity);
        }

        protected virtual void OnValidate()
        {
            SpecifyQuantityEventActions();
            
            UpdateQuantitySubscription();
        }

        private void OnTransformParentChanged()
        {
            InstantiateCanvasIfNeeded();
        }

        private void InstantiateCanvasIfNeeded()
        {
            if (SubscriberCanvas == null)
            {
                GameObject canvasObject = new GameObject();

                Transform anchorTransform = RectTransform.parent;

                _subscriberCanvas = canvasObject.AddComponent<QuantitySubscriberCanvasBhv>();

                _subscriberCanvas.RectTransform.SetParent(anchorTransform);

                _subscriberCanvas.AnchorTransform = anchorTransform;

                _subscriberCanvas.UpdateScale();

                RectTransform.SetParent(canvasObject.transform);

                this.LocalPosition = Vector3.zero;
            }
        }

        private void UpdateQuantitySubscription()
        {
            StartListeningTo(_quantity);

            if (_quantity != _previousQuantity)
            {
                StopListeningTo(_previousQuantity);
            }

            _previousQuantity = _quantity;
        }

        private void StartListeningTo(QuantityBhv quantity)
        {
            if (quantity == null)
            {
                return;
            }

            if (!IsListeningTo(quantity.OnNameChanged))
            {
                quantity.AddListener(quantity.OnNameChanged, this.UpdateName);
            }

            if (!IsListeningTo(quantity.OnAmountChanged))
            {
                quantity.AddListener(quantity.OnAmountChanged, _onQuantityAmountChangedAction);
            }

            if (!IsListeningTo(quantity.OnInvalidRequest))
            {
                quantity.AddListener(quantity.OnInvalidRequest, _onQuantityInvalidRequestAction);
            }
        }

        private void StopListeningTo(QuantityBhv quantity)
        {
            if (quantity == null)
            {
                return;
            }

            int index;

            if (IsListeningTo(quantity.OnNameChanged, out index))
            {
                quantity.RemoveListener(quantity.OnNameChanged, index);
            }

            if (IsListeningTo(quantity.OnAmountChanged, out index))
            {
                quantity.RemoveListener(quantity.OnAmountChanged, index);
            }

            if (IsListeningTo(quantity.OnInvalidRequest, out index))
            {
                quantity.RemoveListener(quantity.OnInvalidRequest, index);
            }
        }

        private bool IsListeningTo(UnityEvent unityEvent)
        {
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                if (unityEvent.GetPersistentTarget(i) == this)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsListeningTo(UnityEvent unityEvent, out int index)
        {
            index = -1;

            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                if (unityEvent.GetPersistentTarget(i) == this)
                {
                    index = i;

                    return true;
                }
            }

            return false;
        }

        public void UpdateName()
        {
            if (MinimalistSystemsManager.Instance.automaticObjectNaming)
            {
                name = (Quantity == null ? "" : Quantity.name.ToString() + " ") + this.GetType().Name.Replace("Bhv", ""); // + " (" + SubscriberCanvas.RenderMode.ToString() + ")";
            }
        }
    }
}