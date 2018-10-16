using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
public abstract class BaseExporter<T> : EditorWindow where T : ScriptableObject
{
	public static bool isEnabled = true;
	static StringBuilder stringBuilder;

	

	public abstract void AddDataHeader(StringBuilder stringBuilder);
	//{
		//stringBuilder.Append("\t");
		//stringBuilder.Append("\n");
	//}
	public abstract void AddData(StringBuilder stringBuilder, int num, T data);
	//{
		//stringBuilder.Append(num + "\t");
		//stringBuilder.Append("\n");
	//}
	Vector2 scrollPos;

	string importString = "";
	public abstract string targetFolder(); //= "Assets/FrameWork/SpecialSystems/BulletEngine/Resources";

	bool changeExisting = false;

	Dictionary<string, T> dict;


	protected abstract string WindowName();
	void OnGUI()
	{
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label(WindowName(), EditorStyles.boldLabel);
		GUILayout.EndVertical();
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
		importString = EditorGUILayout.TextArea(importString, GUILayout.ExpandHeight(true));
		EditorGUILayout.EndScrollView();

		GUILayout.BeginVertical(EditorStyles.helpBox);
		changeExisting = EditorGUILayout.Toggle("Change Existing", changeExisting);

		if (GUILayout.Button("Import"))
			Import();
		if (GUILayout.Button("Export"))
			Export();
		GUILayout.EndVertical();
	}
	protected static void CreateWindow<W>() where W : BaseExporter<T> {
		W window = (W)EditorWindow.GetWindow(typeof(W));
		window.minSize = new Vector2(100f, 200f);
		window.Show(true);
	}
	void Import()
	{
		BuildDict();

		string[] lines = importString.Split(new string[] { "\r", "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < lines.Length; i++)
			ConvertLine(i, lines[i]);
	}
	void Export() {
		if (!isEnabled)
			return;

		T[] allData = Resources.LoadAll<T>("");
		stringBuilder = new StringBuilder("Count " + allData.Length + " (" + System.DateTime.Now + "):\n\n");
		stringBuilder.AppendLine("Assets/Resources/" + targetFolder());
		AddDataHeader(stringBuilder);

		for (int i = 0; i < allData.Length; i++) {
			AddData(stringBuilder, i, allData[i]);
		}
		importString = stringBuilder.ToString();
		Debug.Log(stringBuilder.ToString());
	}
	void BuildDict()
	{
		var allData = Resources.LoadAll<T>("").ToList();

		dict = new Dictionary<string, T>(allData.Count);

		for (int i = 0; i < allData.Count; i++)
			if(dict.ContainsKey(allData[i].name) == false)
				dict.Add(allData[i].name, allData[i]);

	}
	public abstract int cellsCount();
	void ConvertLine(int num, string line)
	{
		string[] cells = line.Split(new string[] { "\t" }, System.StringSplitOptions.None);

		if (cells.Length != cellsCount())
			return;

		if (cells[0].Trim().Length == 0)
			return;
		if (dict.ContainsKey(cells[0]) == false)
		{
			var data = Build(cells);
			dict.Add(cells[0], data);
		}
		else
		{
			Debug.Log("Data " + cells[0] + " already existing.");

			if (changeExisting)
			{
				SetDataFromCells(dict[cells[0]], cells);
				EditorUtility.SetDirty(dict[cells[0]]);
			}
		}
	}

	public T Build(string[] cells)
	{
		T data = ScriptableObject.CreateInstance<T>();

		SetDataFromCells(data, cells);
		AssetDatabase.CreateAsset(data, targetFolder() + data.name + ".asset");

		return data;
	}
	public abstract void SetDataFromCells(T data, string[] cells);


	protected TEnum GetEnumFromString<TEnum>(string valueText)
	{
		return (TEnum)System.Enum.Parse(typeof(TEnum), valueText);
	}

	protected bool GetBoolFromString(string boolText)
	{
		return boolText == "TRUE";
	}

	protected int GetIntFromString(string inString)
	{
		return int.Parse(inString);
	}
	protected float GetFloatFromString(string inString)
	{
		return float.Parse(inString);
	}
}
	

