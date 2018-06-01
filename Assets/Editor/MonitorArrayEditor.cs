using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorMan;
using UnityEngine.UI;

[CustomEditor(typeof(MonitorArray))]
[CanEditMultipleObjects]
public class MonitorArrayEditor : Editor
{
	SerializedProperty m_arrayShape;
	SerializedProperty m_widthInUnits;
	SerializedProperty m_arrayWidth;
	SerializedProperty m_arrayHeight;
	SerializedProperty m_monitorSizeFactor;
	SerializedProperty m_borderSize;
	SerializedProperty m_monitorPrefab;

	private void OnEnable()
	{
		m_arrayShape = serializedObject.FindProperty("m_arrayShape");
		m_widthInUnits = serializedObject.FindProperty("m_widthInUnits");
		m_arrayWidth = serializedObject.FindProperty("m_arrayWidth");
		m_arrayHeight = serializedObject.FindProperty("m_arrayHeight");
		m_monitorSizeFactor = serializedObject.FindProperty("m_monitorSizeFactor");
		m_borderSize = serializedObject.FindProperty("m_borderSize");
		m_monitorPrefab = serializedObject.FindProperty("m_monitorPrefab");
}

	public override void OnInspectorGUI()
	{
		//serializedObject.Update(); // "updates serialized object's representation"...
		var array = target as MonitorArray;
		//var vp = array.GetComponent<UnityEngine.Video.VideoPlayer>();
		/*if (GUILayout.Button("Demo"))
		{
			array.Create();
		}*/
		//base.OnInspectorGUI();
		var form = (MonitorArray.ArrayShapes)m_arrayShape.enumValueIndex;
		
		EditorGUILayout.PropertyField(m_monitorPrefab);
		EditorGUILayout.PropertyField(m_borderSize);
		EditorGUILayout.PropertyField(m_monitorSizeFactor);

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(m_widthInUnits);

		EditorGUILayout.PropertyField(m_arrayShape);

		++EditorGUI.indentLevel;
		{ // for formatting v nice
			EditorGUILayout.PropertyField(m_arrayWidth);
			EditorGUILayout.PropertyField(m_arrayHeight);
		}

		serializedObject.ApplyModifiedProperties();

		if (EditorGUI.EndChangeCheck())
		{
			array.Start();
		}
	}
}
