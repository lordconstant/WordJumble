using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetOutlineOverrideEvent : EventBase
{
	public GameObject overrideObj;

	public GetOutlineOverrideEvent()
	{
		eventType = EVENTTYPE.OUTLINEOVERRIDE;
		overrideObj = null;
	}
}
public class OutlineOverride : MonoBehaviour, EventInterface
{
	public GameObject outlineObj;

	void Awake()
	{
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.OUTLINEOVERRIDE);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.OUTLINEOVERRIDE);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.OUTLINEOVERRIDE))
		{
			GetOutlineOverrideEvent overrideData = data as GetOutlineOverrideEvent;
			overrideData.overrideObj = outlineObj;
		}
	}
}
