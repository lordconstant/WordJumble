using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetCurrentNavEvent : EventBase
{
	public GameObject targetObj;
	
	public SetCurrentNavEvent()
	{
		eventType = EVENTTYPE.SETCURRENTNAV;
		targetObj = null;
	}
	
	public SetCurrentNavEvent(GameObject obj)
	{
		eventType = EVENTTYPE.SETCURRENTNAV;
		targetObj = obj;
	}
}

public class UndoNavSelectEvent : EventBase
{
	public UndoNavSelectEvent()
	{
		eventType = EVENTTYPE.UNDONAVSELECT;
	}
}

public class NavSelectionBeginEvent : EventBase
{
	public bool overridenEffects;
	
	public NavSelectionBeginEvent()
	{
		eventType = EVENTTYPE.NAVSELECTBEGIN;
		overridenEffects = false;
	}
}

public class NavSelectionEndEvent : EventBase
{
	public bool overridenEffects;
	
	public NavSelectionEndEvent()
	{
		eventType = EVENTTYPE.NAVSELECTEND;
		overridenEffects = false;
	}
}

public class NavBlockUpdateEvent : EventBase
{
	public float timer;
	
	public NavBlockUpdateEvent()
	{
		eventType = EVENTTYPE.NAVBLOCKUPDATE;
		timer = 0.0f;
	}
}

public class InputNavigation : MonoBehaviour, EventInterface
{
	public UnityEngine.EventSystems.EventSystem navigationEvents;
	public List<string> inputNames;
	public GameObject defaultObj;
	public List<GameObject> defaultObjects;
	public float selectScale;
	public float highlightSize = 8.0f;
	public Color highlightColour;
	public int historyLength = 20;
	
	GameObject m_lastSelected;
	GameObject m_curSelected;
	List<GameObject> m_selectHistory;
	List<GameObject> m_neighbourOptions;
	
	Vector3 m_curScale;
	float m_blockUpdateTimer;
	
	bool m_undoNavSelect;
	
	void Awake()
	{
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.SETCURRENTNAV);
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.NAVBLOCKUPDATE);
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.UNDONAVSELECT);

		navigationEvents = UnityEngine.EventSystems.EventSystem.current;
		
		m_undoNavSelect = false;
		m_blockUpdateTimer = 0.0f;
		m_neighbourOptions = new List<GameObject>();
		m_selectHistory = new List<GameObject>();
	}
	
	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.SETCURRENTNAV);
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.NAVBLOCKUPDATE);
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.UNDONAVSELECT);

	}
	
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.SETCURRENTNAV))
		{
			SetCurrentNavEvent setData = data as SetCurrentNavEvent;
			
			if(m_lastSelected == null)
				m_lastSelected = defaultObj;
			
			//SetCurrentSelection(setData.targetObj);
			navigationEvents.SetSelectedGameObject(setData.targetObj);
			SetCurrentSelection(setData.targetObj);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.NAVBLOCKUPDATE))
		{
			NavBlockUpdateEvent blockData = data as NavBlockUpdateEvent;
			m_blockUpdateTimer = blockData.timer > m_blockUpdateTimer ? blockData.timer : m_blockUpdateTimer;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.UNDONAVSELECT))
		{
			m_undoNavSelect = true;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(m_blockUpdateTimer > 0.0f)
		{
			m_blockUpdateTimer -= Time.deltaTime;
			return;
		}

		if(!defaultObj || !defaultObj.activeInHierarchy)
			FindBestDefaultObject();

		if(m_curSelected == m_lastSelected && navigationEvents.currentSelectedGameObject != m_curSelected)
		{
			SetCurrentSelection(navigationEvents.currentSelectedGameObject);
			return;
		}
		else if(m_curSelected != m_lastSelected && m_curSelected != navigationEvents.currentSelectedGameObject && navigationEvents.currentSelectedGameObject)
		{
			m_lastSelected = m_curSelected;
			SetCurrentSelection(navigationEvents.currentSelectedGameObject);
			return;
		}
		else if(((navigationEvents.currentSelectedGameObject && !navigationEvents.currentSelectedGameObject.activeInHierarchy) || m_undoNavSelect) && m_lastSelected && m_lastSelected.activeInHierarchy)
		{
			bool useNeighbour = m_neighbourOptions.Count > 0;
			
			GameObject newSelection = useNeighbour ? m_neighbourOptions[0] : m_lastSelected;
			
			if(m_selectHistory.Count > 0)
				m_selectHistory.RemoveAt(m_selectHistory.Count-1);
			m_lastSelected = GetLastFromHistory();
			
			if(m_lastSelected == null)
				m_lastSelected = defaultObj;
			
			navigationEvents.SetSelectedGameObject(newSelection);
			SetCurrentSelection(newSelection);
			if(m_selectHistory.Count > 0)
				m_selectHistory.RemoveAt(m_selectHistory.Count-1);
			return;
		}
		else if(navigationEvents.currentSelectedGameObject && navigationEvents.currentSelectedGameObject.activeInHierarchy && m_lastSelected && m_lastSelected.activeInHierarchy)
		{
			return;
		}
		else if(navigationEvents.currentSelectedGameObject && !navigationEvents.currentSelectedGameObject.activeInHierarchy)
		{
			m_lastSelected = defaultObj;
			SetCurrentSelection(m_lastSelected);
			navigationEvents.SetSelectedGameObject(m_lastSelected);
			return;
		}

		for(int i = 0; i < inputNames.Count; i++)
		{
			if(Input.GetAxis(inputNames[i]) != 0.0f)
			{
				//				GetHighlightInputEvent highlightEvent = new GetHighlightInputEvent();
				//				EventSystem.BroadcastEvent(gameObject, highlightEvent);
				//				m_lastSelected = highlightEvent.highlightObj;
				//
				m_lastSelected = defaultObj;
				SetCurrentSelection(m_lastSelected);
				navigationEvents.SetSelectedGameObject(m_lastSelected);
				break;
			}
		}

	}
	
	void SetCurrentSelection(GameObject newSelection)
	{
		if(m_curSelected == newSelection)
			return;
		
		m_undoNavSelect = false;
		
		if(m_curSelected)
		{
			if(m_selectHistory.Count >= historyLength)
			{
				m_selectHistory.RemoveAt(0);
			}
			m_selectHistory.Add(m_curSelected);
			
			NavSelectionEndEvent endEvent = new NavSelectionEndEvent();
			EventSystem.BroadcastEvent(gameObject, m_curSelected, endEvent);
			
			if(!endEvent.overridenEffects)
			{
				m_curSelected.transform.localScale = m_curScale;
				
				GameObject highlightObj = m_curSelected;
				
				GetOutlineOverrideEvent overrideEvent = new GetOutlineOverrideEvent();
				EventSystem.BroadcastEvent(gameObject, m_curSelected, overrideEvent);
				
				if(overrideEvent.overrideObj)
					highlightObj = overrideEvent.overrideObj;
				
				
				Destroy(highlightObj.GetComponent<SolidOutline>());
			}
		}
		
		m_neighbourOptions.Clear();
		m_curSelected = newSelection;
		
		if(m_curSelected)
		{
			Selectable selectComp = m_curSelected.GetComponent<Selectable>();
			
			if(selectComp != null)
			{
				Selectable curSelect = selectComp.FindSelectableOnRight();
				if(curSelect != null)
					m_neighbourOptions.Add(curSelect.gameObject);
				curSelect = selectComp.FindSelectableOnLeft();
				if(curSelect != null)
					m_neighbourOptions.Add(curSelect.gameObject);
				curSelect = selectComp.FindSelectableOnUp();
				if(curSelect != null)
					m_neighbourOptions.Add(curSelect.gameObject);
				curSelect = selectComp.FindSelectableOnDown();
				if(curSelect != null)
					m_neighbourOptions.Add(curSelect.gameObject);
			}
			
			NavSelectionBeginEvent beginEvent = new NavSelectionBeginEvent();
			EventSystem.BroadcastEvent(gameObject, m_curSelected, beginEvent);
			
			if(!beginEvent.overridenEffects)
			{
				Vector3 curScale = m_curSelected.transform.localScale;
				m_curScale = curScale;
				curScale *= selectScale;
				m_curSelected.transform.localScale = curScale;
				
				
				GameObject highlightObj = m_curSelected;
				
				GetOutlineOverrideEvent overrideEvent = new GetOutlineOverrideEvent();
				EventSystem.BroadcastEvent(gameObject, m_curSelected, overrideEvent);
				if(overrideEvent.overrideObj)
					highlightObj = overrideEvent.overrideObj;
				
				SolidOutline newOutline = highlightObj.AddComponent<SolidOutline>();
				newOutline.EffectDistance = highlightSize;
				newOutline.effectColor = highlightColour;
			}
		}
	}
	
	GameObject GetLastFromHistory()
	{
		int curId = m_selectHistory.Count-1;
		
		while(curId >= 0)
		{
			if(m_selectHistory[curId] && m_selectHistory[curId].activeInHierarchy)
				return m_selectHistory[curId];
			
			m_selectHistory.RemoveAt(curId);
			curId--;
		}
		
		return null;
	}

	void FindBestDefaultObject()
	{
		for(int i = 0; i < defaultObjects.Count; i++)
		{
			if(!defaultObjects[i])
				continue;

			if(!defaultObjects[i].activeInHierarchy)
				continue;

			defaultObj = defaultObjects[i];

			break;
		}
	}
}
