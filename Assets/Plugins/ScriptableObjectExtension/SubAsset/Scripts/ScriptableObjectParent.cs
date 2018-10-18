using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
namespace ClaudeFehlen.ScriptableObjectExtension.SubAsset {
    public class ScriptableObjectParent : ScriptableObject {
        [HideInInspector]
        internal List<ScriptableObject> subAssets;
        [HideInInspector]
        internal List<bool> hides;
        [HideInInspector]
        internal List<bool> warnings;
        //[HideInInspector]
        internal List<string> warningMessages;
        [HideInInspector]
        internal List<int> indent;
        public static List<ScriptableObject> copyStack;
        public bool IsHiding(int i) {
            if (i >= hides.Count)
                hides = new List<bool>(subAssets.Count);
            return hides[i];
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
        public virtual int GetIndent(ScriptableObject subAsset) {
            return 0;
        }
        public virtual bool GetWarning(ScriptableObject subAsset) {
            return false;
        }
        public virtual string GetWarningMessage(ScriptableObject subAsset) {
            return "";
        }
        protected virtual void OnValidate() {
            for (int i = 0; i < subAssets.Count; i++) {
                indent[i] = GetIndent(subAssets[i]);
                warnings[i] = GetWarning(subAssets[i]);
                warningMessages[i] = GetWarningMessage(subAssets[i]);
            }
        }
    }
}