using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stack_Condition : StackElement {
	public override StackElementType Type() {
		return StackElementType.Condition;
	}
	public abstract bool Condition(StackConditionInfo info);
	
}
