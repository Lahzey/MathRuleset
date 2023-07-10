using System.IO;
using System.Net;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace.TreeGenerator.Inspector {
[CustomEditor(typeof(TreeGenConfig))]
public class TreeGenConfigInspector : Editor {
	
	private Editor meshEditor;
	
	public override void OnInspectorGUI() {
		// render the default inspector
		base.OnInspectorGUI();
		
		// acquire controlId and state
		int controlId = EditorGUIUtility.GetControlID(FocusType.Passive);
		State state = (State) GUIUtility.GetStateObject(typeof(State), controlId);
		
		// seed input for preview field
		TreeGenConfig config = (TreeGenConfig) target;
		state.Seed = EditorGUILayout.IntField("Seed", state.Seed);

		if (state.Seed != state.MeshSeed) {
			state.MeshSeed = state.Seed;
			state.Mesh = null;
			state.Tree = null;
			state.MeshData = null;
			meshEditor = null;
			new Thread(() => {
				state.Tree = new Tree(config, state.Seed);
				state.MeshData = state.Tree.PrepareMeshData();
			}).Start();
		} else if (state.Mesh == null && state.MeshData != null) {
			state.Mesh = state.Tree.GenerateMesh(state.MeshData.Value);
			meshEditor = Editor.CreateEditor(state.Mesh);
		}
		
		// preview field
		GUIStyle bgColor = new GUIStyle();
		bgColor.normal.background = EditorGUIUtility.whiteTexture;
		if (meshEditor != null) {
			// draw mesh preview
			meshEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
		} else {
			// draw loading preview
			EditorGUILayout.LabelField("Loading...", GUILayout.Height(256));
		}
		
		// export mesh button
		if (EditorGUILayout.DropdownButton(new GUIContent("Export Mesh"), FocusType.Keyboard)) {
			string path = EditorUtility.SaveFilePanel("Export Mesh", "", "tree", "asset");
			// convert absolute path to relative path
			if (path.StartsWith(Application.dataPath)) {
				path = "Assets" + path.Substring(Application.dataPath.Length);
			}
			if (path.Length != 0 && state.Mesh != null) {
				Debug.Log("Saving asset to " + path);
				AssetDatabase.CreateAsset(state.Mesh, path);
				AssetDatabase.SaveAssets();
			}
		}
	}

	public class State {
		public int Seed { get; set; }
		public Mesh Mesh { get; set; }
		public int MeshSeed { get; set; }

		public Tree Tree { get; set; }
		public PreparedMeshData? MeshData { get; set; }
	}
}
}