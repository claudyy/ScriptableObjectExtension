using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StackElementType {
	Operator,
	Condition,
	Effect
}
public abstract class StackElement : ScriptableObject {

	public abstract StackElementType Type();
}
