using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorMan;

[CustomEditor(typeof(MonitorArray))]
[CanEditMultipleObjects]
public class MonitorArrayEditor : Editor
{
	SerializedProperty monitorArray;

	private void OnEnable()
	{
		monitorArray = serializedObject.FindProperty("monitorArray");
	}

	public override void OnInspectorGUI()
	{
		var array = target as MonitorArray;
		//var vp = array.GetComponent<UnityEngine.Video.VideoPlayer>();
		if (GUILayout.Button("Demo"))
		{
			array.Create();
		}
		base.OnInspectorGUI();
	}
}
