using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Material))]
public class MaterialInspector : MaterialEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (!isVisible)
		{
			return;
		}

		var m = target as Material;

		if (m != null)
		{
			m.renderQueue = EditorGUILayout.IntField("Render Queue", m.renderQueue);

			if (GUILayout.Button("Set Default Render Queue By Shader"))
			{
				if (m.shader != null)
				{
					m.renderQueue = m.shader.renderQueue;
				}
			}
		}
	}
}