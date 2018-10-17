using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ScriptableObject_ConditionStack), true)]
public class ScriptableObject_ConditionStack_Editor : ScriptableObjectParent_Editor {
	StackControllerType condition;
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		var stack = target as ScriptableObject_ConditionStack;
		GUILayout.BeginVertical(EditorStyles.helpBox);
		condition = (StackControllerType)EditorGUILayout.EnumPopup("Primitive to create:", condition);
		if (GUILayout.Button("AddController")) {
			var newChild = ScriptableObject.CreateInstance("Stack_Controller");
			(newChild as Stack_Controller).condition = condition;
			AddChild(stack, newChild);
		}
		GUILayout.EndVertical();
	}
}
#endif

public class StackHelper_Controller {

	public Stack_Controller myOperator;
	public StackHelper_Controller child;
	public StackHelper_Controller neighbour;
	
	public List<Stack_Condition> listCondition = new List<Stack_Condition>();
	public List<Stack_Effect> listEffects = new List<Stack_Effect>();

	public StackHelper_Controller(Stack_Controller myOperator) {
		this.myOperator = myOperator;
	}
	bool Condition(StackConditionInfo info) {
		foreach (var c in listCondition) {
			if (c.Condition(info) == false)
				return false;
		}
		return true;
	}
	public List<Stack_Effect> GetEffect(StackConditionInfo info) {
		var list = new List<Stack_Effect>();
		if(Condition(info) == true) {
			list.AddRange(listEffects);
			if(child != null)
				list.AddRange(child.GetEffect(info));
		} else {
			if(neighbour != null && neighbour.myOperator.condition == StackControllerType.Else)
				list.AddRange(neighbour.GetEffect(info));

		}
		if (neighbour != null && neighbour.myOperator.condition == StackControllerType.Or)
			list.AddRange(neighbour.GetEffect(info));
		return list;
	}

}
public class StackConditionInfo {

}
public class ScriptableObject_ConditionStack : ScriptableObjectParent {

	StackHelper_Controller CreateHelper(List<StackElement> aList) {
		if (aList[0] is Stack_Controller == false)
			return null;
		if (aList.Count <= 1)
			return null;
		var controller = new StackHelper_Controller(aList[0] as Stack_Controller);
		

		for (int i = 1; i < aList.Count; i++) {
			switch (aList[i].Type()) {
				case StackElementType.Condition:
					if (controller.myOperator.condition == StackControllerType.Else)
						Debug.LogWarning(name + " : after else is not supose to be any conditions");
					controller.listCondition.Add(aList[i] as Stack_Condition);

					break;
				case StackElementType.Effect:
					controller.listEffects.Add(aList[i] as Stack_Effect);

					break;
				case StackElementType.Operator:
					if((aList[i] as Stack_Controller).condition == StackControllerType.If) {
						controller.child = CreateHelper(aList.GetRange(i, aList.Count - i));
					}
					if ((aList[i] as Stack_Controller).condition == StackControllerType.Or) {
						controller.neighbour = CreateHelper(aList.GetRange(i, aList.Count - i));
					}
					if ((aList[i] as Stack_Controller).condition == StackControllerType.Else) {
						controller.neighbour = CreateHelper(aList.GetRange(i, aList.Count -i));
					}
					return controller;

				default:
					break;
			}
		}
		return controller;

	}
	public List<T> GetEffects<T>(StackConditionInfo info) where T : Stack_Effect {
		var list = new List<T>();
		var assets = GetAssets<StackElement>();
		StackHelper_Controller helper = null;
		for (int i = 0; i < assets.Count; i++) {
			var a = assets[i];
			if (a is Stack_Effect)
				list.Add(a as T);
			if(a is Stack_Controller) {
				helper = CreateHelper(assets.GetRange(i,assets.Count));
				break;
			}
			if (a is Stack_Condition) {
				Debug.LogError(name +" : Hier shouldn't be a condition");
			}
		}
		if(helper != null)
			list.AddRange(helper.GetEffect(info).Cast<T>());

		return list;
	}
	public override int GetIndent(ScriptableObject newChild) {
		var assets = GetAssets<StackElement>();
		var indent = 0;
		for (int i = 0; i < assets.Count; i++) {
			var a = assets[i];
			if (a is Stack_Effect && a == newChild)
				return indent;
			if (a is Stack_Controller) {
				if (a == newChild)
					return indent;
				indent++;
			}

		}
		return base.GetIndent(newChild);
	}
}
