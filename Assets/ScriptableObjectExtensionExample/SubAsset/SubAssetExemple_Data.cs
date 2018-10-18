using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClaudeFehlen.ScriptableObjectExtension.SubAsset;
public class SubAssetExemple_Data : ScriptableObjectParent {
    public override string SubFolder() {
        return "Assets/ScriptableObjectExtensionExample/SubAsset";
    }
}
