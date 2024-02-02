using UnityEditor;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
    [CustomPropertyDrawer(typeof(ExplorerConfiguration))]
    public class ExplorerConfigurationEditor : PropertyDrawer
    {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var indent = EditorGUI.indentLevel;

			GUILayout.Label($"Explorer Configuration:", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;

			EditorGUILayout.LabelField($"Required Values:", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_nodeContainer"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_folderPrefab"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_filePrefab"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_emptyFolderPrefab"));
			EditorGUILayout.Space();

			EditorGUILayout.LabelField($"Optional Values:", EditorStyles.boldLabel);
			var pathContainer = property.FindPropertyRelative("_pathContainer");
			EditorGUILayout.PropertyField(pathContainer);
			if (pathContainer.objectReferenceValue != null)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(property.FindPropertyRelative("_pathFolderPrefab"));
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_arrowBackButton"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_arrowForwardButton"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_exitButton"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_cancelButton"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("_confirmButton"));

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
		{ 
			return 0; 
		}
	}
}
