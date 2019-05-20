using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionSolver : MonoBehaviour 
{
	void Awake()
	{
		#if !UNITY_ANDROID
		Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
		#endif
	}

	// Use this for initialization
	void Update () 
	{
		#if !UNITY_ANDROID
		if(!Screen.fullScreen)
			return;

		if(Display.main.systemWidth == Screen.currentResolution.width && Display.main.systemHeight == Screen.currentResolution.height)
			return;

		Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
		#endif
	}
}
