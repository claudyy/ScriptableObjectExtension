using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StackControllerType {
	If,
	Or,
	Else
}
public class Stack_Controller : StackElement {
	public override StackElementType Type() {
		return StackElementType.Operator;
	}
	public StackControllerType condition;
}
