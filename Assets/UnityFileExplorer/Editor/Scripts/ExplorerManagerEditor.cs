using UnityEditor;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
	[CustomEditor(typeof(ExplorerManager))]
	public class ExplorerManagerEditor : Editor
	{
		private Editor _settingsEditor = null;
		public Editor SettingsEditor
		{
			get
			{
				if (_settingsEditor == null)
					_settingsEditor = CreateEditor(((ExplorerManager)target).ExplorerConfiguration);
				return _settingsEditor;
			}
		}

		private SerializedProperty _explorerConfig;
		private SerializedProperty _canvas;

		public void OnEnable()
		{
			_explorerConfig = serializedObject.FindProperty("_explorerConfiguration");
			_canvas = serializedObject.FindProperty("_canvas");

			if (_explorerConfig.objectReferenceValue == null)
			{
				_explorerConfig.objectReferenceValue = (ExplorerConfiguration)Resources.Load("DefaultFileExplorerConfiguration");
				serializedObject.ApplyModifiedProperties();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_explorerConfig, new GUIContent("Explorer Configuration"));
			if (_explorerConfig.objectReferenceValue != null)
			{
				EditorGUI.indentLevel++;
				SettingsEditor.OnInspectorGUI();
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.PropertyField(_canvas);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
