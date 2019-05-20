using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputKeyboard : MonoBehaviour 
{
	public InputField textField;

	// Use this for initialization
	void Start () 
	{
		if(textField == null)
			Debug.Log("Missing InputField!");
	}

	public void OnKeyPress(Text textComp)
	{
		textField.text += textComp.text;
	}

	public void OnUndoPress()
	{
		if(textField.text.Length <= 0)
			return;

		textField.text = textField.text.Remove(textField.text.Length-1);
	}
}
