using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
[CustomEditor(typeof(ScriptableObjectParent), true)]
public class ScriptableObjectParent_Editor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		GUILayout.BeginVertical(EditorStyles.helpBox);
		var data = target as ScriptableObjectParent;
		//if (GUILayout.Button("Hide All"))
		//	HideAll(data);
		//if (GUILayout.Button("Show All"))
		//	ShowAll(data);
		GUILayout.Space(12);
		GUILayout.BeginVertical(EditorStyles.helpBox);

		if (data.subAssets.Count == 0)
			GUILayout.Label("No sub scriptable objects");


		for (int i = 0; i < data.subAssets.Count; i++) {
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(data.subAssets[i].name))
				data.hide[i] = !data.hide[i];
			if (GUILayout.Button("Copy"))
				Copy(data.subAssets[i]);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			if (data.IsHiding(i) == false) { 
				//data.effects[i].OnInspectorGUI();
				Editor editor = Editor.CreateEditor(data.subAssets[i]);
				editor.DrawDefaultInspector();
			}
			

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Duplicate"))
				Double(data, i);
			if (GUILayout.Button("Up"))
				MoveUp(data, i);
			if (GUILayout.Button("Down"))
				MoveDown(data, i);
			if (GUILayout.Button("Delete"))
				Delete(data, i);
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();
		if (GUILayout.Button("Create SubAsset"))
			DoCreateScriptableObject(data);
		GUILayout.Space(12);
		if (GUILayout.Button("Copy stack"))
			CopyStack(data);
		if (GUILayout.Button(ScriptableObjectParent.copyStack != null ? "Past Stack ("+ScriptableObjectParent.copyStack.Count+")" : "need to copy somthing first"))
			PastStack(data);
		GUILayout.Space(12);
		if (GUILayout.Button("Delet stack"))
			DeleteAll(data);

		GUILayout.EndVertical();
	}

	private void Copy(ScriptableObject scriptableObject) {
		ScriptableObjectParent.copyStack = new List<ScriptableObject>() { scriptableObject };
	}

	private void HideAll(ScriptableObjectParent data) {
		for (int i = 0; i < data.subAssets.Count; i++) {
			data.hide[i] = true;
		}
	}
	private void ShowAll(ScriptableObjectParent data) {
		for (int i = 0; i < data.subAssets.Count; i++) {
			data.hide[i] = false;
		}
	}
	private void CopyStack(ScriptableObjectParent data) {
		ScriptableObjectParent.copyStack = new List<ScriptableObject>(data.subAssets);

	}
	private void PastStack(ScriptableObjectParent data) {
		if (ScriptableObjectParent.copyStack == null)
			return;
		var list = ScriptableObjectParent.copyStack;
		for (int i = 0; i < list.Count; i++) {
			var newD = Object.Instantiate(list[i]) as ScriptableObject;
			AddChild(data, newD);
		}
		ScriptableObjectParent.copyStack = null;

	}
	private void SetSubAssetHideFlags(HideFlags flag) {
		UnityEngine.Object[] os = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
		foreach (UnityEngine.Object o in os) {
			if (o != target) {
				o.hideFlags = flag;
				Undo.RecordObject(o, "ShowSubAsset");
			}
		}
		AssetDatabase.SaveAssets();
	}
	private void Double(ScriptableObjectParent data, int i) {
		var d = data.subAssets[i];
		var newD = Object.Instantiate(d) as ScriptableObject;
		AddChild(data,newD);
	}

	private void Delete(ScriptableObjectParent data, int i) {
		var sub = data.subAssets[i];

		data.subAssets.RemoveAt(i);
		data.hide.RemoveAt(i);
		DestroyImmediate(sub, true);
		Undo.RecordObject(data, "DeletAsset");
		AssetDatabase.SaveAssets();
	}
	private void DeleteAll(ScriptableObjectParent data) {
		for (int i = data.subAssets.Count - 1; i >= 0 ; i--) {
			Delete(data, i);
		}
	}
	private void MoveUp(ScriptableObjectParent data, int i) {
		if (i - 1 < 0)
			return;
		Switch(data, i, i - 1);
	}

	private void MoveDown(ScriptableObjectParent data, int i) {
		if (i + 1 >= data.subAssets.Count)
			return;
		Switch(data, i,i+1);
	}

	private static void Switch(ScriptableObjectParent data, int position,int switchWith) {
		var other = data.subAssets[switchWith];
		var h = data.hide[switchWith];
		data.subAssets[switchWith] = data.subAssets[position];
		data.hide[switchWith] = data.hide[position];
		data.subAssets[position] = other;
		data.hide[position] = h;
	}

	/// <summary>
	/// Creating specific class menu items.
	/// </summary>
	/// <returns>The created ScriptableObject.</returns>
	/// <typeparam name="T">Type of ScriptableObject to create.</typeparam>
	public static T CreateAsset<T>() where T : ScriptableObject {
		T asset = ScriptableObject.CreateInstance<T>();

		string path = GetAssetPath();
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

		BuildAsset(asset, assetPathAndName);

		return asset;
	}

	/// <summary>
	/// Gets the target path for the asset to create.
	/// </summary>
	/// <returns>The asset path.</returns>
	public static string GetAssetPath() {
		string path;

		path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path == "") {
			path = "Assets";
		} else if (Path.GetExtension(path) != "") {
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
		}

		return path;
	}

	/// <summary>
	/// Builds the asset and does neccessary AssetDatabase things.
	/// </summary>
	/// <param name="assetType">Asset type.</param>
	/// <param name="path">Path.</param>
	public static void BuildAsset(ScriptableObject asset, string assetPathAndName) {
		AssetDatabase.CreateAsset(asset, assetPathAndName);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}

	void DoCreateScriptableObject(ScriptableObjectParent data) {
		string targetScriptPath;
		MonoScript targetScript;

		// Open a File Panel to search for the script
		targetScriptPath = EditorUtility.OpenFilePanel("Select", "Assets/"+ data.SubFolder(), "cs");

		if (targetScriptPath.StartsWith(Application.dataPath)) {
			targetScriptPath = "Assets" + targetScriptPath.Substring(Application.dataPath.Length);
		}

		// Get the target script
		targetScript = AssetDatabase.LoadAssetAtPath<MonoScript>(targetScriptPath);

		if (targetScript == null) {
			return;
		}

		// Check if we are a ScriptableObject
		if (typeof(ScriptableObject).IsAssignableFrom(targetScript.GetClass())) {
			var newChild = ScriptableObject.CreateInstance(targetScript.GetClass());
			AddChild(data, newChild);
		} else {
			Debug.LogWarning("Create ScriptableObject Asset failed: Selected Class does not inherit from ScriptableObject");
		}
	}

	private static void AddChild(ScriptableObjectParent data, ScriptableObject newChild) {
		string path = GetAssetPath();
		//string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + newChild.GetType().ToString() + ".asset");
		newChild.name = newChild.GetType().ToString();

		AssetDatabase.AddObjectToAsset(newChild, data);
		data.subAssets.Add(newChild);
		data.hide.Add(false);
		data.hideFlags = HideFlags.None;
		newChild.hideFlags = HideFlags.None;
		Undo.RecordObject(newChild, "AddSubAsset");
		Undo.RecordObject(data, "AddSubAsset");
		AssetDatabase.SaveAssets();
	}
}
#endif
public class ScriptableObjectParent : ScriptableObject  {
	[HideInInspector]
	public List<ScriptableObject> subAssets;
	[HideInInspector]
	public List<bool> hide;
	public static List<ScriptableObject> copyStack;
	public bool IsHiding(int i) {
		if (i >= hide.Count)
			hide = new List<bool>(subAssets.Count);
		return hide[i];
	}
	public List<T> GetAssets<T>() where T : ScriptableObject {
		var list = new List<T>();
		for (int i = 0; i < subAssets.Count; i++) {
			if (subAssets[i] is T)
				list.Add((T)subAssets[i]);
		}
		return list;
	}
	public virtual string SubFolder() {
		return "";
	}
}
