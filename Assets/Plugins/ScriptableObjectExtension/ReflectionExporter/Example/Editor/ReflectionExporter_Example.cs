using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ClaudeFehlen.ReflectionExporter.Example {
    public class ReflectionExporter_Example : BaseReflectionExporter<ExampleData> {
        public override string targetFolder() {
            return "Assets/Plugins/ScriptableObjectExtension/ReflectionExporter/Example/Resources/";
        }

        protected override string WindowName() {
            return "Example";
        }

        [MenuItem("System/ReflectionExporter/Example", false, 100)]
        static void Init() {
            CreateWindow<ReflectionExporter_Example>();
        }

    }
}