using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClaudeFehlen.ScriptableObjectExtension.SubAsset;
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
    int GetIndentFromHelper(StackHelper_Controller helper,ScriptableObject newChild,int indent) {
        var temp =-1;
        if (helper.myOperator == newChild)
            return indent;
        if (newChild is Stack_Condition && helper.listCondition.Contains(newChild as Stack_Condition))
            return indent + 1;
        if (newChild is Stack_Effect && helper.listEffects.Contains(newChild as Stack_Effect))
            return indent + 1;


        if (helper.neighbour != null) {
            temp = GetIndentFromHelper(helper.neighbour,newChild,indent);
            indent = temp != -1 ? temp : indent;
        }
        if (helper.child != null) {
            temp = GetIndentFromHelper(helper.child, newChild, indent + 1);
            indent = temp != -1 ? temp : indent;
        }
        return indent;
    }
	public override int GetIndent(ScriptableObject newChild) {

        var assets = GetAssets<StackElement>();
        var indent = 0;
        StackHelper_Controller helper = null;
        for (int i = 0; i < assets.Count; i++) {
            var a = assets[i];
            if (a is Stack_Effect && a == newChild)
                return indent;
            if (a is Stack_Controller) {
                helper = CreateHelper(assets.GetRange(i, assets.Count));
                break;
            }
            if (a is Stack_Condition) {
                Debug.LogError(name + " : Hier shouldn't be a condition");
            }
        }
        return GetIndentFromHelper(helper, newChild, indent);
	}
    //return 1 when subAsset has a warning
    int GetWarningFromHelper(StackHelper_Controller helper, ScriptableObject subAsset) {
        int temp = -1;
        if (subAsset is Stack_Controller  && helper.myOperator == subAsset) {
            if ((subAsset as Stack_Controller).condition == StackControllerType.If && helper.listCondition.Count == 0)
                return 1;
            if ((subAsset as Stack_Controller).condition == StackControllerType.Else && helper.listCondition.Count != 0)
                return 1;
            if (helper.listEffects.Count != 0)
                return 0;
            return 0;
        }
        if (subAsset is Stack_Condition && helper.listCondition.Contains(subAsset as Stack_Condition))
            return 0;
        if (subAsset is Stack_Effect && helper.listEffects.Contains(subAsset as Stack_Effect))
            return 0;

        if (helper.neighbour != null) {
            temp = GetWarningFromHelper(helper.neighbour, subAsset);
            if (temp != -1)
                return temp;
        }
        if (helper.child != null) {
            temp = GetWarningFromHelper(helper.child, subAsset);
            if (temp != -1)
                return temp;
        }

        return -1;
    }
    public override bool GetWarning(ScriptableObject subAsset) {
        var assets = GetAssets<StackElement>();
        StackHelper_Controller helper = null;
        for (int i = 0; i < assets.Count; i++) {
            var a = assets[i];
            if (a is Stack_Controller) {
                helper = CreateHelper(assets.GetRange(i, assets.Count));
                break;
            }
            if (a is Stack_Condition) {
                Debug.LogError(name + " : Hier shouldn't be a condition");
            }
        }
        if (GetWarningFromHelper(helper, subAsset) == 1)
            return true;
        return false;
    }
    string GetWarningMessageFromHelper(StackHelper_Controller helper, ScriptableObject subAsset) {
        string temp = "-1";
        if (subAsset is Stack_Controller && helper.myOperator == subAsset) {
            if ((subAsset as Stack_Controller).condition == StackControllerType.If && helper.listCondition.Count == 0)
                return "When the controller is an if controller, the controller needs conditions below it";
            if ((subAsset as Stack_Controller).condition == StackControllerType.Else && helper.listCondition.Count != 0)
                return "When the controller is an else controller, the controller can't have conditions below it";
            if (helper.listEffects.Count != 0)
                return "";
            return "";
        }
        if (subAsset is Stack_Condition && helper.listCondition.Contains(subAsset as Stack_Condition))
            return "";
        if (subAsset is Stack_Effect && helper.listEffects.Contains(subAsset as Stack_Effect))
            return "";

        if (helper.neighbour != null) {
            temp = GetWarningMessageFromHelper(helper.neighbour, subAsset);
            if (temp != "-1")
                return temp;
        }
        if (helper.child != null) {
            temp = GetWarningMessageFromHelper(helper.child, subAsset);
            if (temp != "-1")
                return temp;
        }

        return "-1";
    }
    public override string GetWarningMessage(ScriptableObject subAsset) {
        var assets = GetAssets<StackElement>();
        StackHelper_Controller helper = null;
        for (int i = 0; i < assets.Count; i++) {
            var a = assets[i];
            if (a is Stack_Controller) {
                helper = CreateHelper(assets.GetRange(i, assets.Count));
                break;
            }
            if (a is Stack_Condition) {
                Debug.LogError(name + " : Hier shouldn't be a condition");
            }
        }
        return GetWarningMessageFromHelper(helper, subAsset);
    }
}
