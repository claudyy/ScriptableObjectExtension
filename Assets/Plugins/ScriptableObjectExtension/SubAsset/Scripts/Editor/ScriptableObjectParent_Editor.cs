using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
namespace ClaudeFehlen.ScriptableObjectExtension.SubAsset {

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
                var warning = data.warnings[i];
                GUI.backgroundColor = warning ? Color.red : Color.gray;
                EditorGUI.indentLevel = data.indent[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.backgroundColor = Color.magenta;
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(data.subAssets[i].name))
                    data.hides[i] = !data.hides[i];
                if (GUILayout.Button("Copy"))
                    Copy(data.subAssets[i]);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                if (warning) {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(data.warningMessages[i]);
                    GUILayout.EndVertical();
                }

                GUI.backgroundColor = Color.gray;
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

                EditorGUILayout.EndHorizontal();

            }
            GUI.backgroundColor = Color.white;

            EditorGUI.indentLevel = 0;

            GUILayout.EndVertical();
            GUILayout.Space(12);
            if (GUILayout.Button("Sort by Name"))
                SortByName(data);
            if (GUILayout.Button("Sort by Type"))
                SortByType(data);
            GUILayout.Space(12);
            if (GUILayout.Button("Create SubAsset"))
                DoCreateScriptableObject(data);
            GUILayout.Space(12);
            if (GUILayout.Button("Copy stack"))
                CopyStack(data);
            if(ScriptableObjectParent.copyStack == null || ScriptableObjectParent.copyStack.Count < 1)
                GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(ScriptableObjectParent.copyStack != null ? "Past Stack (" + ScriptableObjectParent.copyStack.Count + ")" : "need to copy somthing first"))
                PastStack(data);
            GUI.backgroundColor = Color.white;
            GUILayout.Space(12);
            if (GUILayout.Button("Delet stack"))
                DeleteAll(data);

            GUILayout.EndVertical();
        }
        private void SortByType(ScriptableObjectParent scriptableObject) {
            scriptableObject.subAssets.Sort((a, b) => a.GetType().FullName.CompareTo(b.GetType().FullName));
        }
        private void SortByName(ScriptableObjectParent scriptableObject) {
            scriptableObject.subAssets.Sort((x, y) => string.Compare(x.name, y.name));
        }
        private void Copy(ScriptableObject scriptableObject) {
            ScriptableObjectParent.copyStack = new List<ScriptableObject>() { scriptableObject };
        }

        private void HideAll(ScriptableObjectParent data) {
            for (int i = 0; i < data.subAssets.Count; i++) {
                data.hides[i] = true;
            }
        }
        private void ShowAll(ScriptableObjectParent data) {
            for (int i = 0; i < data.subAssets.Count; i++) {
                data.hides[i] = false;
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
            AddChild(data, newD);
        }

        private void Delete(ScriptableObjectParent data, int i) {
            var sub = data.subAssets[i];

            data.subAssets.RemoveAt(i);
            data.hides.RemoveAt(i);
            DestroyImmediate(sub, true);
            Undo.RecordObject(data, "DeletAsset");
            AssetDatabase.SaveAssets();
            ChangedData(data);
        }
        private void DeleteAll(ScriptableObjectParent data) {
            for (int i = data.subAssets.Count - 1; i >= 0; i--) {
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
            Switch(data, i, i + 1);
        }

        private void Switch(ScriptableObjectParent data, int position, int switchWith) {
            var other = data.subAssets[switchWith];
            var h = data.hides[switchWith];
            data.subAssets[switchWith] = data.subAssets[position];
            data.hides[switchWith] = data.hides[position];

            data.subAssets[position] = other;
            data.hides[position] = h;

            ChangedData(data);
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
            targetScriptPath = EditorUtility.OpenFilePanel("Select", data.SubFolder(), "cs");

            if (data.SubFolder() == "")
                targetScriptPath = EditorUtility.OpenFilePanel("Select", "Assets/", "cs");

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

        protected void AddChild(ScriptableObjectParent data, ScriptableObject newChild) {
            //string path = GetAssetPath();
            //string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + newChild.GetType().ToString() + ".asset");
            newChild.name = newChild.GetType().ToString();

            AssetDatabase.AddObjectToAsset(newChild, data);
            data.subAssets.Add(newChild);
            data.hides.Add(false);
            data.indent.Add(data.GetIndent(newChild));
            data.warnings.Add(data.GetWarning(newChild));
            data.warningMessages.Add(data.GetWarningMessage(newChild));

            data.hideFlags = HideFlags.None;
            newChild.hideFlags = HideFlags.None;
            Undo.RecordObject(newChild, "AddSubAsset");
            Undo.RecordObject(data, "AddSubAsset");
            AssetDatabase.SaveAssets();
            ChangedData(data);


        }

        void ChangedData(ScriptableObjectParent data) {
            for (int i = 0; i < data.subAssets.Count; i++) {
                data.indent[i] = data.GetIndent(data.subAssets[i]);
                data.warnings[i] = data.GetWarning(data.subAssets[i]);
                data.warningMessages[i] = data.GetWarningMessage(data.subAssets[i]);
            }
        }
    }
}
