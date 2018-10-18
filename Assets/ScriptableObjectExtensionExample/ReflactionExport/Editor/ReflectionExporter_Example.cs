using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ClaudeFehlen.ScriptableObjectExtension.ReflectionExporter;
public class ReflectionExporter_Example : BaseReflectionExporter<ExampleData> {
    public override string targetFolder() {
        return "Assets/ScriptableObjectExtensionExample/ReflactionExport/Resources/";
    }

    protected override string WindowName() {
        return "Example";
    }

    [MenuItem("System/ReflectionExporter/Example", false, 100)]
    static void Init() {
        CreateWindow<ReflectionExporter_Example>();
    }

}
