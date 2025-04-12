using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Minimalist.Utility.Editor;

namespace Minimalist.Quantity.Editor
{
    [CustomPropertyDrawer(typeof(QuantityDynamics))]
    public class QuantityDynamicsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            QuantityBhv quantity = property.serializedObject.targetObject as QuantityBhv;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty type = property.FindPropertyRelative("_type");

            SerializedProperty deltaAmountType = property.FindPropertyRelative("_deltaAmountType");

            SerializedProperty deltaPercentage = property.FindPropertyRelative("_deltaPercentage");

            SerializedProperty deltaAmount = property.FindPropertyRelative("_deltaAmount");

            SerializedProperty deltaTime = property.FindPropertyRelative("_deltaTime");

            SerializedProperty enabled = property.FindPropertyRelative("_enabled");

            GUILayout.Space(-EditorGUIUtility.singleLineHeight);

            GUILayout.BeginHorizontal();

            GUILayout.Space(EditorGUIUtility.labelWidth);

            EditorExtensions.PropertyField("", type);

            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            string tooltip = "Controls whether the passive";

            if (type.intValue == (int)QuantityDynamicsType.Accumulation)
            {
                tooltip += " increment ";
            }
            else if (type.intValue == (int)QuantityDynamicsType.Depletion)
            {
                tooltip += " decrement ";
            }

            tooltip += "is defined as a flat amount - 'Absolute' - or as a percentage of this quantity's capacity - 'Relative'.";

            EditorExtensions.PropertyField("Delta Amount Type", tooltip, deltaAmountType, type.intValue != (int)QuantityDynamicsType.None);

            EditorGUI.indentLevel++;

            EditorExtensions.PropertyField("Delta Amount", deltaAmount, type.intValue != (int)QuantityDynamicsType.None && deltaAmountType.intValue == (int)QuantityDynamicsDeltaAmountType.Absolute);

            EditorExtensions.PropertyField("Delta Percentage", deltaPercentage, type.intValue != (int)QuantityDynamicsType.None && deltaAmountType.intValue == (int)QuantityDynamicsDeltaAmountType.Relative);

            if (deltaAmountType.intValue == (int)QuantityDynamicsDeltaAmountType.Relative)
            {
                deltaAmount.floatValue = deltaPercentage.floatValue / 100f * quantity.Capacity;
            }

            if (deltaAmount.floatValue > quantity.Capacity)
            {
                deltaAmount.floatValue = quantity.Capacity;
            }

            EditorGUI.indentLevel--;

            EditorExtensions.PropertyField("Delta Time", deltaTime, type.intValue != (int)QuantityDynamicsType.None);

            GUILayout.BeginHorizontal();

            EditorExtensions.PropertyField("Enabled", enabled, Application.isPlaying && type.intValue != (int)QuantityDynamicsType.None, GUILayout.ExpandWidth(false));

            bool previousEnabledState = GUI.enabled;

            GUI.enabled = !Application.isPlaying && type.intValue != (int)QuantityDynamicsType.None;

            if (GUILayout.Button("Simulate one time step", GUILayout.ExpandWidth(true)))
            {
                quantity.Amount += quantity.PassiveDynamics.SignedDeltaAmount;
            }

            GUI.enabled = previousEnabledState;

            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}