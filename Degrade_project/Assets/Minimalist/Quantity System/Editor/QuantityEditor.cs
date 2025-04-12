using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Minimalist.Utility.Editor;

namespace Minimalist.Quantity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(QuantityBhv))]
    public class QuantityEditor : UnityEditor.Editor
    {
        private SerializedProperty _units;
        private SerializedProperty _maximumAmount;
        private SerializedProperty _minimumAmount;
        private SerializedProperty _capacity;
        private SerializedProperty _currentAmount;
        private SerializedProperty _fillAmount;
        private SerializedProperty _passiveDynamics;
        private SerializedProperty _onAmountChanged;
        private SerializedProperty _onInvalidRequest;

        private string _previousName;

        private void OnEnable()
        {
            QuantityBhv quantity = target as QuantityBhv;

            _units = serializedObject.FindProperty("_units");

            _maximumAmount = serializedObject.FindProperty("_maximumAmount");

            _minimumAmount = serializedObject.FindProperty("_minimumAmount");

            _capacity = serializedObject.FindProperty("_capacity");

            _currentAmount = serializedObject.FindProperty("_currentAmount");

            _fillAmount = serializedObject.FindProperty("_fillAmount");

            _passiveDynamics = serializedObject.FindProperty("_passiveDynamics");

            _onAmountChanged = serializedObject.FindProperty("_onAmountChanged");

            _onInvalidRequest = serializedObject.FindProperty("_onInvalidRequest");

            Undo.undoRedoPerformed += quantity.OnUndoRedoCallback;
        }

        public override void OnInspectorGUI()
        {
            QuantityBhv quantity = target as QuantityBhv;

            serializedObject.Update();

            EditorExtensions.ScriptHolder(target);

            if (quantity.name != _previousName)
            {
                serializedObject.ApplyModifiedProperties();

                quantity.OnNameChanged.Invoke();

                _previousName = quantity.name;
            }

            EditorExtensions.Header("Definition:");

            EditorGUI.BeginChangeCheck();

            EditorExtensions.PropertyField("Maximum Amount", _maximumAmount);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(quantity, "maximumAmount");

                quantity.MaximumAmount = _maximumAmount.floatValue;

                serializedObject.Update();
            }

            EditorGUI.BeginChangeCheck();

            EditorExtensions.PropertyField("Minimum Amount", _minimumAmount);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(quantity, "minimumAmount");

                quantity.MinimumAmount = _minimumAmount.floatValue;

                serializedObject.Update();
            }

            EditorExtensions.PropertyField("Capacity", _capacity, false);

            EditorExtensions.PropertyField("Units", _units);

            EditorExtensions.Header("Current State:");

            EditorGUI.BeginChangeCheck();

            EditorExtensions.PropertyField("Fill Amount", _fillAmount);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(quantity, "fillAmount");

                quantity.FillAmount = _fillAmount.floatValue;

                serializedObject.Update();
            }

            EditorGUI.BeginChangeCheck();

            EditorExtensions.PropertyField("Current Amount", _currentAmount);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(quantity, "currentAmount");

                quantity.Amount = _currentAmount.floatValue;

                serializedObject.Update();
            }

            EditorExtensions.Header("Passive Dynamics:");

            EditorExtensions.PropertyField("Dynamics Type", "Controls whether this quantity has any passive dynamics, and if so, " +
                "whether they consist of a periodic increment - 'Accumulation' - or decrement - 'Depletion' - to its current amount.", _passiveDynamics);

            EditorExtensions.Header("Unity Events:");

            GUI.enabled = false;

            GUILayout.TextArea("Note that these are updated automatically each time a new 'Quantity Subscriber' subscribes / unsubscribes.", GUI.skin.textArea);

            GUI.enabled = true;

            EditorExtensions.PropertyField("On Amount Changed", _onAmountChanged, false);

            GUILayout.Space(-EditorGUIUtility.singleLineHeight);

            EditorExtensions.PropertyField("On Invalid Request", _onInvalidRequest, false);

            serializedObject.ApplyModifiedProperties();
        }
    }
}