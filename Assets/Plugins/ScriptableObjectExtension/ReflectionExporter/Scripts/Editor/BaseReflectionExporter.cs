using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using System.Reflection;
using System;
namespace ClaudeFehlen.ReflectionExporter {
    public abstract class BaseReflectionExporter<T> : EditorWindow where T : ScriptableObject {

        Vector2 scrollPos;

        string importString = "";
        public abstract string targetFolder();
        bool changeExisting = false;
        protected abstract string WindowName();
        public static bool isEnabled = true;
        static StringBuilder stringBuilder;
        void OnGUI() {
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
        protected static void CreateWindow<W>() where W : BaseReflectionExporter<T> {
            W window = (W)EditorWindow.GetWindow(typeof(W));
            window.minSize = new Vector2(100f, 200f);
            window.Show(true);
        }

        void Import() {
            string[] lines = importString.Split(new string[] { "\r", "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            var allData = Resources.LoadAll<T>("").ToList();

            var dict = new Dictionary<string, T>(allData.Count);

            for (int i = 0; i < allData.Count; i++)
                if (dict.ContainsKey(allData[i].name) == false)
                    dict.Add(allData[i].name, allData[i]);

            string[] propertyNames = new string[1];
            foreach (var line in lines) {

                string[] cells = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);
                if(cells[0] == "Type") {
                    Debug.Log("New Type");
                    propertyNames = cells;
                } else {
                    var type = cells[0];
                    var name = cells[1];
                    //already Exist
                    if (dict.ContainsKey(name)) {
                        SetData(dict[name], cells, propertyNames);
                        EditorUtility.SetDirty(dict[name]);
                    } else {
                        var types = type.Split(new string[] { "." }, System.StringSplitOptions.None);
                        Debug.Log(types[types.Length - 1]);

                        Type textType = null;
                        string typeName = type;
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies) {
                            textType = assembly.GetType(typeName);
                            if (textType != null)
                                break;
                        }

                        Debug.Log(textType);

                        T data = ScriptableObject.CreateInstance(textType) as T;

                        SetData(data, cells,propertyNames);
                        AssetDatabase.CreateAsset(data, targetFolder() + data.name + ".asset");
                    }
                    //or create
                    Debug.Log("Object of type" + cells[0] + "Name "+cells[1]);

                }
            }
        }
        void SetData(T data,string[] values, string[] propertyNames) {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            FieldInfo[] fields = data.GetType().GetFields(flags);
            for (int i = 0; i < propertyNames.Length; i++) {
                if(propertyNames[i] =="Type" || propertyNames[i] == "Name") {
                    if (propertyNames[i] == "Name")
                        data.name = values[i];
                    continue;
                }
                var info = data.GetType().GetField(propertyNames[i]);
                var obj = GetValueGetValueFromString(values[i], info.FieldType);
                if(obj != null)
                    data.GetType().GetField(propertyNames[i]).SetValue(data, obj);
            }
        }
        object GetValueGetValueFromString(string s,Type type){
            if(type == typeof(int))
                return GetIntFromString(s);
            if (type == typeof(float))
                return GetFloatFromString(s);
            if (type == typeof(bool))
                return GetBoolFromString(s);
            return null;
        }
        protected TEnum GetEnumFromString<TEnum>(string valueText) {
            return (TEnum)System.Enum.Parse(typeof(TEnum), valueText);
        }

        protected bool GetBoolFromString(string boolText) {
            return boolText == "True";
        }

        protected int GetIntFromString(string inString) {
            return int.Parse(inString);
        }
        protected float GetFloatFromString(string inString) {
            return float.Parse(inString);
        }
        void Export() {
            if (!isEnabled)
                return;

            List<T> allData = Resources.LoadAll<T>("").ToList();
            //stringBuilder = new StringBuilder("Count " + allData.Count + " (" + System.DateTime.Now + "):\n\n");
            //stringBuilder.AppendLine("Assets/Resources/" + targetFolder());
            stringBuilder = new StringBuilder();
            Dictionary<Type, List<T>> dic = new Dictionary<Type, List<T>>();
            List<T> temp;
            for (int i = 0; i < allData.Count; i++) {
                if(dic.TryGetValue(allData[i].GetType(),out temp)) {
                    temp.Add(allData[i]);
                } else {
                    dic.Add(allData[i].GetType(), new List<T>(1) { allData[i] });
                }
            }
            foreach (var k in dic.Keys) {
                var list = dic[k];
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                FieldInfo[] fields = list[0].GetType().GetFields(flags);
                stringBuilder.Append("Type"+ "\t");
                stringBuilder.Append("Name" + "\t");

                foreach (var f in fields) {
                    stringBuilder.Append(f.Name+"\t");
                }
                stringBuilder.Append("\n");

                for (int i = 0; i < list.Count; i++) {
                    fields = list[i].GetType().GetFields(flags);
                    stringBuilder.Append(list[i].GetType() + "\t");
                    stringBuilder.Append(list[i].name + "\t");
                    foreach (var f in fields) {
                        if(f.GetValue(list[i]) != null)
                            stringBuilder.Append(f.GetValue(list[i]).ToString() + "\t");
                    }
                    stringBuilder.Append("\n");

                }

            }
            importString = stringBuilder.ToString();
            Debug.Log(stringBuilder.ToString());
        }


    }
}