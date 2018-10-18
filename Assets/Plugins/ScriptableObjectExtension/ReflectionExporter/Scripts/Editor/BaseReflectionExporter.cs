using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using System.Reflection;
using System;
namespace ClaudeFehlen.ScriptableObjectExtension.ReflectionExporter {
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
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Import"))
                Import();
            if (GUILayout.Button("Export"))
                Export();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        protected static void CreateWindow<W>() where W : BaseReflectionExporter<T> {
            W window = (W)EditorWindow.GetWindow(typeof(W));
            window.minSize = new Vector2(100f, 200f);
            window.Show(true);
        }

        void Import() {
            string[] lines = importString.Split(new string[] { "\r", "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, T> dict = LoadAllData();

            string[] propertyNames = new string[1];
            foreach (var line in lines) {

                string[] cells = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (cells[0] == "Type") {
                    propertyNames = cells;
                } else {
                    ImportLine(dict, propertyNames, cells);

                }
            }
        }

        private void ImportLine(Dictionary<string, T> dict, string[] propertyNames, string[] cells) {
            var typeName = cells[0];
            var name = cells[1];
            //already Exist
            if (dict.ContainsKey(name)) {
                if (changeExisting) {
                    SetData(dict[name], cells, propertyNames);
                    EditorUtility.SetDirty(dict[name]);
                }
            } else {

                Type type = GetTypeFromString(typeName);

                T data = ScriptableObject.CreateInstance(type) as T;

                SetData(data, cells, propertyNames);
                if(AssetDatabase.IsValidFolder(targetFolder())) {
                    AssetDatabase.CreateAsset(data, targetFolder() + data.name + ".asset");
                } else {
                    Debug.LogWarning("target folder : " + targetFolder() + "   doesnt exist");
                }
            }
        }

        private static Type GetTypeFromString(string type) {
            Type textType = null;
            string typeName = type;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                textType = assembly.GetType(typeName);
                if (textType != null)
                    break;
            }

            return textType;
        }

        private static Dictionary<string, T> LoadAllData() {
            var allData = Resources.LoadAll<T>("").ToList();

            var dict = new Dictionary<string, T>(allData.Count);

            for (int i = 0; i < allData.Count; i++)
                if (dict.ContainsKey(allData[i].name) == false)
                    dict.Add(allData[i].name, allData[i]);
            return dict;
        }

        void SetData(T data,string[] values, string[] propertyNames) {
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
        protected virtual object GetValueGetValueFromString(string s,Type type){
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

            stringBuilder = new StringBuilder();

            Dictionary<Type, List<T>> dic = CreateDictionaryFromType();

            foreach (var k in dic.Keys) {
                ExporterAddHeader(dic[k]);

                for (int i = 0; i < dic[k].Count; i++) 
                    ExporterAddValues(dic[k], i);

            }

            importString = stringBuilder.ToString();
            Debug.Log(stringBuilder.ToString());

        }

        private static void ExporterAddValues(List<T> list, int i) {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            FieldInfo[] fields = list[i].GetType().GetFields(flags);
            stringBuilder.Append(list[i].GetType() + "\t");
            stringBuilder.Append(list[i].name + "\t");
            foreach (var f in fields) {
                if (f.GetValue(list[i]) != null)
                    stringBuilder.Append(f.GetValue(list[i]).ToString() + "\t");
            }
            stringBuilder.Append("\n");
        }

        private static void ExporterAddHeader(List<T> list) {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            FieldInfo[] fields = list[0].GetType().GetFields(flags);
            stringBuilder.Append("Type" + "\t");
            stringBuilder.Append("Name" + "\t");

            foreach (var f in fields) {
                stringBuilder.Append(f.Name + "\t");
            }
            stringBuilder.Append("\n");
        }

        private static Dictionary<Type, List<T>> CreateDictionaryFromType() {
            List<T> allData = Resources.LoadAll<T>("").ToList();

            Dictionary<Type, List<T>> dic = new Dictionary<Type, List<T>>();
            List<T> temp;
            for (int i = 0; i < allData.Count; i++) {
                if (dic.TryGetValue(allData[i].GetType(), out temp)) {
                    temp.Add(allData[i]);
                } else {
                    dic.Add(allData[i].GetType(), new List<T>(1) { allData[i] });
                }
            }

            return dic;
        }

    }
}