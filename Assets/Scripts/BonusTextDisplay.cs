using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusTextDisplay : MonoBehaviour, EventInterface
{
	public GameObject popupText;
	public Transform spawnPoint;
	
	// Use this for initialization
	void Awake () 
	{
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.INCREASESCORE);
	}
	
	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.INCREASESCORE);
	}
	
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.INCREASESCORE))
		{
			IncreaseScoreEvent eventData = data as IncreaseScoreEvent;
			
			string popupText = "";
			
			if(eventData.rewardText.Length > 0)
				CreatePopupText("BONUS!");
		}
	}
	
	void CreatePopupText(string text)
	{
		if(!popupText)
			return;
		
		GameObject newObj = Instantiate(popupText, spawnPoint.position, spawnPoint.rotation, spawnPoint);
		
		Text textComp = newObj.GetComponentInChildren<Text>();
		
		if(!textComp)
			return;
		
		textComp.text = text;
	}
}
