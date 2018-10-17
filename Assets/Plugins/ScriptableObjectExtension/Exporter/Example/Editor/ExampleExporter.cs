using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
public class ExampleExporter : BaseExporter<ExampleData> {

    [MenuItem("System/Exporter/Example", false, 100)]
    static void Init() {
        CreateWindow<ExampleExporter>();
    }
    public override string targetFolder() {
        return "Assets/Plugins/ScriptableObjectExtension/Import/Example/Resources/";
    }
    public override int cellsCount() {
        return 5;
    }

    protected override string WindowName() {
        return "Example";
    }
    public override void AddData(StringBuilder stringBuilder, int num, ExampleData data) {
        stringBuilder.Append(data.name + "\t");
        stringBuilder.Append(data.str + "\t");
        stringBuilder.Append(data.num + "\t");
        stringBuilder.Append(data.decimalNum + "\t");
        stringBuilder.Append(data.boolean + "\t");
        stringBuilder.Append("\n");
    }
    public override void AddDataHeader(StringBuilder stringBuilder) {
        stringBuilder.Append("name \t");
        stringBuilder.Append("string \t");
        stringBuilder.Append("number \t");
        stringBuilder.Append("Decimal Number \t");
        stringBuilder.Append("Boolean \t");
        stringBuilder.Append("\n");
    }
    public override void SetDataFromCells(ExampleData data, string[] cells) {
        data.name = cells[0];
        data.str = cells[1];
        data.num = GetIntFromString(cells[2]);
        data.decimalNum = GetFloatFromString(cells[3]);
        data.boolean = GetBoolFromString(cells[4]);
    }
 
}
