using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour, EventInterface
{
	public GameObject popupText;
	public Transform spawnPoint;
	public Image flashImage;
	public Color baseColour;
	public Color flashColour;
	public AnimationCurve flashCurve;
	public float flashTime;
	public float flashThreshold;

	float m_flashProg;
	bool m_flashing;

	// Use this for initialization
	void Awake () 
	{
		EventSystem.RegisterDelegate(gameObject, this, EVENTTYPE.INCREASESCORE);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(gameObject, this, EVENTTYPE.INCREASESCORE);
	}

	// Update is called once per frame
	void Update () 
	{
		if(!m_flashing)
			return;

		m_flashProg += Time.deltaTime / flashTime;

		if(m_flashProg >= 1.0f)
			m_flashProg = 1.0f;

		float flashResult = flashCurve.Evaluate(m_flashProg);

		flashImage.color = Color.Lerp(baseColour, flashColour, flashResult);

		if(m_flashProg >= 1.0f)
		{
			m_flashProg = 0.0f;
			m_flashing = false;
		}
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.INCREASESCORE))
		{
			IncreaseScoreEvent eventData = data as IncreaseScoreEvent;

			string popupText = "";

			if(eventData.rewardText.Length > 0)
				popupText = eventData.rewardText + ":" + eventData.score.ToString();
			else
				popupText = eventData.score.ToString();

			CreatePopupText(popupText);

			if(!m_flashing)
			{
				m_flashing = true;
				return;
			}

			if(m_flashProg > flashThreshold)
			{
				m_flashProg = 0.0f;
				m_flashing = true;
			}
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
