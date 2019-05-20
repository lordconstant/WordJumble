using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUILayoutEvent : EventBase
{
	public UpdateUILayoutEvent()
	{
		eventType = EVENTTYPE.UPDATEUILAYOUT;
	}
}

public class UiExpand : MonoBehaviour, EventInterface
{
	public bool vertical = true;

	int m_updateForFrames;
	RectTransform m_rectTrans;
	HorizontalOrVerticalLayoutGroup m_parentLayout;

	void Awake()
	{
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.UPDATEUILAYOUT);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.UPDATEUILAYOUT);
	}

	// Use this for initialization
	void Start () {
		m_updateForFrames = 3;
		m_rectTrans = GetComponent<RectTransform>();
		m_parentLayout = GetComponentInParent<HorizontalOrVerticalLayoutGroup>();

		//Invoke("UpdateUI", 0.1f);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.UPDATEUILAYOUT))
		{
			m_updateForFrames = 1;
		}
	}

	void Update()
	{
		if(m_updateForFrames <= 0)
			return;

		UpdateUI();

		m_updateForFrames--;
	}

	// Update is called once per frame
	void UpdateUI () 
	{
		if(!m_rectTrans)
			return;

		float newHeight = 0.0f;
		float newWidth = 0.0f;

		int childCount = 0;

		foreach(Transform child in transform)
		{
			childCount++;

			RectTransform rectTrans = child.GetComponent<RectTransform>();

			float childMinX = rectTrans.position.x + rectTrans.rect.xMin;
			float childMaxX = rectTrans.position.x + rectTrans.rect.xMax;
			float childMinY = rectTrans.position.y + rectTrans.rect.yMin;
			float childMaxY = rectTrans.position.y + rectTrans.rect.yMax;

			if(vertical)
			{
				newHeight += childMaxY - childMinY;
				float curWidth = childMaxX - childMinX;
				newWidth = curWidth > newWidth ? curWidth : newWidth;
			}
			else
			{
				newWidth += childMaxX - childMinX;
				float curHeight = childMaxY - childMinY;
				newHeight = curHeight > newHeight ? curHeight : newHeight;
			}

		}

		if(m_parentLayout)
		{
			newWidth += m_parentLayout.padding.left + m_parentLayout.padding.right;
			newHeight += m_parentLayout.padding.top + m_parentLayout.padding.bottom;

			if(vertical)
				newHeight += m_parentLayout.spacing * (childCount-1);
			else
				newWidth += m_parentLayout.spacing * (childCount-1);
		}

		Vector2 deltaSize = m_rectTrans.sizeDelta;
		deltaSize.x = newWidth;
		deltaSize.y = newHeight;
		m_rectTrans.sizeDelta = deltaSize;
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectTrans);
	}
}
