using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (WorleyTest))]
public class GenerateWorleyTextureEditor : Editor
{
    public override void OnInspectorGUI() {
		WorleyTest worleyGen = (WorleyTest)target;

		if (DrawDefaultInspector ()) {
			if (worleyGen.autoUpdate) {
				worleyGen.GenerateWorleyTexture();
			}
		}

		if (GUILayout.Button("Generate")) {
			worleyGen.GenerateWorleyTexture();
		}
	}
}
