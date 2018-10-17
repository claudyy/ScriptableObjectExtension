using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack_Condition : StackElement {
	public override StackElementType Type() {
		return StackElementType.Condition;
	}
	public virtual bool Condition(StackConditionInfo info) {
		return true;
	}
}
