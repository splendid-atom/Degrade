using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minimalist.Utility;

namespace Minimalist.Quantity
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class QuantitySubscriberCanvasBhv : UIElementBhv
    {
        // Public properties
        public RenderMode RenderMode => Canvas.renderMode;
        public Camera Camera => Canvas.worldCamera;
        public Transform AnchorTransform { get => _anchorTransform; set => _anchorTransform = value; }

        // Private properties
        private Canvas Canvas => _canvas == null || !Application.isPlaying ? GetComponent<Canvas>() : _canvas;
        private QuantitySubscriber[] Subscribers => _subscribers == null || _subscribers.Length == 0 || !Application.isPlaying ? GetComponentsInChildren<QuantitySubscriber>() : _subscribers;

        // Private serialized fields
        [SerializeField] private Transform _anchorTransform;
        [SerializeField] private bool _lookAtCamera = true;

        // Private fields
        private Canvas _canvas;
        private QuantitySubscriber[] _subscribers;

        private void Awake()
        {
            _canvas = Canvas;

            _subscribers = Subscribers;
        }

        private void Start()
        {
            Canvas.worldCamera = Canvas.worldCamera == null ? Camera.main : Canvas.worldCamera;
        }

        public void OnEnable()
        {
            RectTransform.hideFlags = HideFlags.NotEditable;

            UpdateName();
        }

        private void OnTransformChildrenChanged()
        {
            UpdateName();

            ResetSubscriberScales();
        }

        private void LateUpdate()
        {
            MoveWithAnchor();

            if (Application.isPlaying)
            {

                LookAtCameraIf(_lookAtCamera);
            }

            this.SizeDelta = Vector2.zero;
        }

        private void MoveWithAnchor()
        {
            if (AnchorTransform != null)
            {
                this.Position = AnchorTransform.position;
            }
        }

        public void LookAtCameraIf(bool condition)
        {
            if (this.RenderMode != RenderMode.WorldSpace)
            {
                return;
            }

            Vector3 alignment = !condition || this.Camera == null ? Vector3.forward : this.Camera.transform.forward;

            RectTransform.LookAt(RectTransform.position + alignment);
        }

        public void UpdateScale()
        {
            switch (this.RenderMode)
            {
                case RenderMode.WorldSpace:

                    RectTransform.localScale = Vector3.one / 100f;

                    break;

                case RenderMode.ScreenSpaceCamera:

                case RenderMode.ScreenSpaceOverlay:

                    RectTransform.localScale = Vector3.one;

                    break;
            }

            ResetSubscriberScales();
        }

        public void UpdateName()
        {
            if (MinimalistSystemsManager.Instance.automaticObjectNaming)
            {
                name = "Canvas (" + this.RenderMode.ToString() + ")";

                if (AnchorTransform != null)
                {
                    name = AnchorTransform.name + " " + name;
                }

                foreach (QuantitySubscriber subscriber in Subscribers)
                {
                    subscriber.UpdateName();
                }
            }
        }

        private void ResetSubscriberScales()
        {
            foreach (QuantitySubscriber subscriber in Subscribers)
            {
                subscriber.LocalScale = Vector3.one;
            }
        }
    }
}
