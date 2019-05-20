using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionAndSceneSwap : MonoBehaviour, EventInterface
{
	public float time = 3.0f;
	[Range (0.0f, 1.0f)]
	public float transitionAtPerc = 0.7f;
	public string nextScene;
	public bool OnStart;

	bool m_sceneChangeStarted;

	void Awake()
	{
		m_sceneChangeStarted = false;
		EventSystem.RegisterDelegate(this, EVENTTYPE.TRANSITIONBEGUN);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.TRANSITIONBEGUN);
	}

	// Use this for initialization
	void Start () 
	{
		if(OnStart)
		{
			StartTranistionEvent newTransitionEvent = new StartTranistionEvent();
			newTransitionEvent.time = time;
			EventSystem.BroadcastEvent(gameObject, newTransitionEvent);
		}
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.TRANSITIONBEGUN))
		{
			TransitionBegunEvent eventData = data as TransitionBegunEvent;
			eventData.transitionDelegates += TransitionUpdate;
		}
	}

	public void TransitionUpdate(float prog)
	{
		if(m_sceneChangeStarted)
			return;

		if(prog < transitionAtPerc)
			return;
		
		m_sceneChangeStarted = true;
		SceneManager.LoadScene(nextScene);
	}

	public void StartTransition()
	{
		if(m_sceneChangeStarted)
			return;

		StartTranistionEvent newTransitionEvent = new StartTranistionEvent();
		newTransitionEvent.time = time;
		EventSystem.BroadcastEvent(gameObject, newTransitionEvent);
	}
}
