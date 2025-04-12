using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Minimalist.Utility.Editor;

namespace Minimalist.Quantity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(QuantitySubscriberCanvasBhv))]
    public class QuantitySubscriberCanvasEditor : UnityEditor.Editor
    {
        // Private serialized properties
        private SerializedProperty _anchorTransform;
        private SerializedProperty _lookAtCamera;

        // Private fields
        private RenderMode _previousRenderMode;
        private bool _currentButtonState;
        private bool _previousButtonState;

        private void OnEnable()
        {
            _anchorTransform = serializedObject.FindProperty("_anchorTransform");

            _lookAtCamera = serializedObject.FindProperty("_lookAtCamera");
        }

        public override void OnInspectorGUI()
        {
            QuantitySubscriberCanvasBhv quantitySubscriberCanvas = target as QuantitySubscriberCanvasBhv;

            serializedObject.Update();

            EditorExtensions.ScriptHolder(target);

            if (quantitySubscriberCanvas.RenderMode != _previousRenderMode)
            {
                serializedObject.ApplyModifiedProperties();

                quantitySubscriberCanvas.UpdateScale();

                quantitySubscriberCanvas.UpdateName();

                quantitySubscriberCanvas.LookAtCameraIf(false);

                _previousRenderMode = quantitySubscriberCanvas.RenderMode;
            }

            EditorGUI.BeginChangeCheck();

            EditorExtensions.PropertyField("Anchor", _anchorTransform, quantitySubscriberCanvas.RenderMode == RenderMode.WorldSpace);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                quantitySubscriberCanvas.UpdateScale();
            }

            GUI.enabled = quantitySubscriberCanvas.Camera != null;

            GUILayout.BeginHorizontal();

            EditorExtensions.PropertyField("Look At Camera", _lookAtCamera, quantitySubscriberCanvas.RenderMode == RenderMode.WorldSpace, GUILayout.ExpandWidth(false));

            GUI.enabled = !Application.isPlaying && quantitySubscriberCanvas.RenderMode == RenderMode.WorldSpace;

            _currentButtonState = GUILayout.RepeatButton("Edit Mode Preview", GUI.skin.button);

            if (Event.current.type == EventType.Repaint && _currentButtonState != _previousButtonState)
            {
                quantitySubscriberCanvas.LookAtCameraIf(_currentButtonState);

                _previousButtonState = _currentButtonState;
            }

            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}