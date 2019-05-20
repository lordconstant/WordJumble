using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void TransitionUpdate(float prog);

public class StartTranistionEvent : EventBase
{
	public float time;

	public StartTranistionEvent()
	{
		eventType = EVENTTYPE.STARTTRANSITION;
		time = 1.0f;
	}
}

public class TransitionBegunEvent : EventBase
{
	public TransitionUpdate transitionDelegates;

	public TransitionBegunEvent()
	{
		eventType = EVENTTYPE.TRANSITIONBEGUN;
	}
}

public class TransitionHandler : MonoBehaviour, EventInterface
{
	public Image transitionImage;
	public AnimationCurve transitionCurve;

	bool m_transitioning;
	float m_transitionProg;
	float m_transitionTime;

	TransitionUpdate m_transitionDelegates;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		EventSystem.RegisterDelegate(this, EVENTTYPE.STARTTRANSITION);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.STARTTRANSITION);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.STARTTRANSITION))
		{
			StartTranistionEvent eventData = data as StartTranistionEvent;

			StartTransition(eventData.time);
		}
	}

	public void StartTransition(float time)
	{
		if(m_transitioning)
			return;
		
		m_transitioning = true;
		m_transitionProg = 0.0f;
		m_transitionTime = time;

		TransitionBegunEvent begunEvent = new TransitionBegunEvent();
		EventSystem.BroadcastEvent(gameObject, begunEvent);
		m_transitionDelegates = begunEvent.transitionDelegates;

		StartCoroutine(TranstionCoroutine());
	}

	IEnumerator TranstionCoroutine()
	{
		while(m_transitioning)
		{
			m_transitionProg += Time.deltaTime / m_transitionTime;

			if(m_transitionProg > 1.0f)
			{
				m_transitionProg = 1.0f;
				m_transitioning = false;
			}

			if(m_transitionDelegates != null)
				m_transitionDelegates(m_transitionProg);

			if(transitionImage)
			{
				Color tranColour = transitionImage.color;
				tranColour.a = transitionCurve.Evaluate(m_transitionProg);
				transitionImage.color = tranColour;
			}

			yield return new WaitForEndOfFrame();
		}

		yield return null;
	}
}
