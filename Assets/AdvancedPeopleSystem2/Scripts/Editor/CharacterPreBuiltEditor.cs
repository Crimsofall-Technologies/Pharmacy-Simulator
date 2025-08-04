using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    [CustomEditor(typeof(CharacterPreBuilt))]
    public class CharacterPreBuiltEditor : Editor
    {

        private CharacterPreBuilt characterPreBuilt;

        private SerializedProperty dataProperty;
        private SerializedProperty materialsProperty;
        private SerializedProperty settingsProperty;
        public void OnEnable()
        {
            characterPreBuilt = (CharacterPreBuilt)target;

            dataProperty = serializedObject.FindProperty("preBuiltDatas");
            materialsProperty = serializedObject.FindProperty("materials");
            settingsProperty = serializedObject.FindProperty("settings");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(settingsProperty);
            for (int i = 0; i < dataProperty.arraySize; i++)
            {
                SerializedProperty dataPropertyElement = dataProperty.GetArrayElementAtIndex(i);
                SerializedProperty fieldGroupName = dataPropertyElement.FindPropertyRelative("GroupName");
                SerializedProperty fieldMeshes = dataPropertyElement.FindPropertyRelative("meshes");
                SerializedProperty materials = dataPropertyElement.FindPropertyRelative("materials");
                GUILayout.Space(20);
                GUILayout.BeginVertical(string.Format("{0} Group", fieldGroupName.stringValue), "GroupBox");
                GUILayout.Space(10);
                EditorGUI.indentLevel++;
                fieldMeshes.isExpanded = EditorGUILayout.Foldout(fieldMeshes.isExpanded, "Meshes");
                if (fieldMeshes.isExpanded)
                {
                    for (int m = 0; m < fieldMeshes.arraySize; m++)
                    {
                        SerializedProperty meshElement = fieldMeshes.GetArrayElementAtIndex(m);
                        EditorGUILayout.PropertyField(meshElement, GUIContent.none);
                    }
                }
                GUILayout.Space(10);
                materials.isExpanded = EditorGUILayout.Foldout(materials.isExpanded, "Materials");
                if (materials.isExpanded)
                {
                    for (int m = 0; m < materials.arraySize; m++)
                    {
                        SerializedProperty materialPropertyElement = materials.GetArrayElementAtIndex(m);
                        EditorGUILayout.PropertyField(materialPropertyElement, GUIContent.none);
                    }
                }
                GUILayout.Space(15);
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }
            GUILayout.Space(20);
            EditorGUI.EndDisabledGroup();
        }
    }
}