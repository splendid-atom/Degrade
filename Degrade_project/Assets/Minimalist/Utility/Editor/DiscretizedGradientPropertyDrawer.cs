using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Minimalist.Utility.Editor
{
    [CustomPropertyDrawer(typeof(DiscretizedGradient))]
    public class DiscretizedGradientPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty gradient = property.FindPropertyRelative("_gradient");

            EditorGUI.PropertyField(position, gradient, new GUIContent(label.text));

            EditorGUI.EndProperty();
        }
    }
}