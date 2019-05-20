using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnglishKeySetup : MonoBehaviour 
{
	public GameObject layoutObj;
	public GameObject keyObj;
	public string[] letters;

	// Use this for initialization
	void Start () 
	{
		for(int i = 0; i < letters.Length; i++)
		{
			GameObject newKey = Instantiate(keyObj, layoutObj.transform) as GameObject;
			Text textComp = newKey.GetComponentInChildren<Text>();

			if(textComp == null)
				continue;

			textComp.text = letters[i];
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
