using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ClaudeFehlen.ScriptableObjectExtension.ReflectionExporter {
    public class GenericReflectionExporter : BaseReflectionExporter<ScriptableObject> {
        public override string targetFolder() {
            return "Assets/Resources/";
        }

        protected override string WindowName() {
            return "Generic Exporter";
        }
        [MenuItem("System/ReflectionExporter/Generic", false, 100)]
        static void Init() {
            CreateWindow<GenericReflectionExporter>();
        }
    }
}