using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitWordEvent : EventBase
{
	public string word;

	public SubmitWordEvent()
	{
		eventType = EVENTTYPE.SUBMITWORD;
	}

	public SubmitWordEvent(string newWord)
	{
		eventType = EVENTTYPE.SUBMITWORD;
		word = newWord;
	}
}

public class DisplayWord
{
	public List<GameObject> letters;

	public DisplayWord()
	{
		letters = new List<GameObject>();
	}

	public Text FindOpenLetter()
	{
		for(int i = 0; i < letters.Count; i++)
		{
			Text textComp = letters[i].GetComponentInChildren<Text>();

			if(!textComp)
				continue;

			if(textComp.enabled)
				continue;

			return textComp;
		}

		return null;
	}

	public bool IsWordValid(out string word)
	{
		bool foundBlank = false;
		string fullWord = "";

		word = fullWord;

		for(int i = 0; i < letters.Count; i++)
		{
			Text textComp = letters[i].GetComponentInChildren<Text>();

			if(!textComp)
				continue;

			if(!textComp.enabled)
			{
				foundBlank = true;
				continue;
			}

			fullWord += textComp.text;

			if(foundBlank)
				return false;
		}

		word = fullWord;

		return true;
	}

	public void Reset()
	{
		for(int i = 0; i < letters.Count; i++)
		{
			Text textComp = letters[i].GetComponentInChildren<Text>();

			if(!textComp)
				continue;

			textComp.enabled = false;
		}
	}
}

public class LetterLink
{
	public GameObject mainObj;
	public GameObject linkedObj;

	public LetterLink()
	{
		mainObj = null;
		linkedObj = null;
	}

	public bool IsInLink(GameObject obj)
	{
		if(mainObj == obj)
			return true;

		return linkedObj == obj;
	}
}

public class DisplayLetterHandler : MonoBehaviour, EventInterface
{
	public AudioSource audioPlayer;

	public AudioClip clickSfx;

	public GameObject letterPrefab;
	public GameObject holderObj;

	public GameObject displayPrefab;
	public GameObject displayHolder;

	public Selectable downNavObj;

	public int maxLetters;

	DisplayWord m_displayWord;
	List<GameObject> m_createdLetters;
	List<LetterLink> m_letterLinks;
	string m_letters;

	void Awake()
	{
		EventSystem.RegisterDelegate(this, EVENTTYPE.NEXTWORDSTORE);
		EventSystem.RegisterDelegate(this, EVENTTYPE.ENDROUND);

		m_createdLetters = new List<GameObject>();
		m_letterLinks = new List<LetterLink>();

		m_displayWord = new DisplayWord();
		for(int i = 0; i < maxLetters; i++)
		{
			GameObject newLetter = Instantiate(displayPrefab, displayHolder.transform);
			m_displayWord.letters.Add(newLetter);

			Button btnComp = newLetter.GetComponent<Button>();

			if(btnComp != null)
			{
				Text textComp = newLetter.GetComponentInChildren<Text>();
				textComp.enabled = false;
				btnComp.onClick.AddListener(() => OnDisplayClick(newLetter, textComp));
				btnComp.onClick.AddListener(() => TriggerUndoNavEvent());
			}
		}
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.NEXTWORDSTORE);
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.ENDROUND);
	}
	
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.NEXTWORDSTORE))
		{
			NextWordStoreEvent eventData = data as NextWordStoreEvent;

			while(m_letterLinks == null && m_letterLinks.Count > 0)
			{
				LetterLink link = m_letterLinks[0];

				Button btnComp = link.mainObj.GetComponent<Button>();

				if(btnComp)
				{
					if(!btnComp.interactable)
					{
						btnComp.interactable = true;

						Text textComp = link.linkedObj.GetComponentInChildren<Text>();

						textComp.enabled = false;
					}
				}

				m_letterLinks.RemoveAt(0);
			}

			PopulateFromLetters(eventData.wordStore.shuffledLetters);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ENDROUND))
		{
			for(int i = 0; i < m_createdLetters.Count; i++)
			{
				Button btnComp = m_createdLetters[i].GetComponent<Button>();

				if(!btnComp)
					continue;

				btnComp.interactable = false;
			}

			for(int i = 0; i < m_displayWord.letters.Count; i++)
			{
				Button btnComp = m_displayWord.letters[i].GetComponent<Button>();

				if(!btnComp)
					continue;

				btnComp.interactable = false;
			}
		}
	}

	void PopulateFromLetters(string letters)
	{
		m_letters = letters;

		if(!letterPrefab || !holderObj)
			return;
		
		for(int i = 0; i < m_createdLetters.Count; i++)
		{
			Destroy(m_createdLetters[i]);
		}

		m_createdLetters.Clear();

		for(int i = 0; i < letters.Length; i++)
		{
			GameObject newLetter = Instantiate(letterPrefab, holderObj.transform);

			m_createdLetters.Add(newLetter);

			Text textComp = newLetter.GetComponentInChildren<Text>();

			if(textComp)
			{
				textComp.text = letters[i].ToString();
			}

			Button btnComp = newLetter.GetComponent<Button>();

			if(btnComp)
			{
				btnComp.onClick.AddListener(delegate { OnLetterClick(newLetter); });
				btnComp.onClick.AddListener(() => TriggerUndoNavEvent());
			}
		}

		SetupNavigation();
	}

	public void OnDisplayClick(GameObject go, Text textComp)
	{
		audioPlayer.PlayOneShot(clickSfx);

		if(!textComp.enabled)
			return;
		
		textComp.enabled = false;

		for(int i = 0; i < m_letterLinks.Count; i++)
		{
			if(!m_letterLinks[i].IsInLink(go))
				continue;

			GameObject otherGo = m_letterLinks[i].mainObj == go ? m_letterLinks[i].linkedObj : m_letterLinks[i].mainObj;

			if(!otherGo)
				continue;

			Toggle toggleComp = otherGo.GetComponent<Toggle>();

			if(toggleComp)
			{
				toggleComp.isOn = false;
			}

			m_letterLinks.RemoveAt(i);

			break;
		}
	}

	public void OnLetterClick(GameObject go)
	{
//		Toggle toggleComp = go.GetComponent<Toggle>();
//		
//		if(toggleComp && !toggleComp.isOn)
//		{
//			for(int i = 0; i < m_letterLinks.Count; i++)
//			{
//				if(!m_letterLinks[i].IsInLink(go))
//					continue;
//				
//				GameObject otherGo = m_letterLinks[i].mainObj == go ? m_letterLinks[i].linkedObj : m_letterLinks[i].mainObj;
//				
//				if(!otherGo)
//					continue;
//				
//				Text childTextComp = otherGo.GetComponentInChildren<Text>();
//				
//				if(childTextComp)
//				{
//					childTextComp.enabled = false;
//				}
//				
//				m_letterLinks.RemoveAt(i);
//				
//				break;
//			}
//
//			return;
//		}
		audioPlayer.PlayOneShot(clickSfx);

		Text textComp = go.GetComponentInChildren<Text>();
		Text openLetter = m_displayWord.FindOpenLetter();
		Button btnComp = go.GetComponent<Button>();

		if(!openLetter)
			return;
		
		if(btnComp)
		{
			if(!btnComp.interactable)
			{
				for(int i = 0; i < m_letterLinks.Count; i++)
				{
					if(!m_letterLinks[i].IsInLink(go))
						continue;

					GameObject otherGo = m_letterLinks[i].mainObj == go ? m_letterLinks[i].linkedObj : m_letterLinks[i].mainObj;

					if(!otherGo)
						continue;

					Text otherTextComp = otherGo.GetComponentInChildren<Text>();

					if(otherTextComp)
						otherTextComp.enabled = false;

					m_letterLinks.RemoveAt(i);

					break;
				}

				btnComp.interactable = true;
				return;
			}

			btnComp.interactable = false;
		}

		LetterLink newLink = new LetterLink();
		newLink.mainObj = go;
		newLink.linkedObj = openLetter.transform.parent.gameObject;

		m_letterLinks.Add(newLink);

		openLetter.text = textComp.text;
		openLetter.enabled = true;
	}

	public void OnSubmitClick()
	{
		string word;

		if(!m_displayWord.IsWordValid(out word))
			return;

		EventSystem.BroadcastEvent(gameObject, new SubmitWordEvent(word));

		m_displayWord.Reset();
		m_letterLinks.Clear();

		for(int i = 0; i < m_createdLetters.Count; i++)
		{
			Button btnComp = m_createdLetters[i].GetComponent<Button>();

			if(!btnComp)
				continue;

			btnComp.interactable = true;
		}
	}

	public void OnUndoClick()
	{
		if(m_letterLinks.Count <= 0)
			return;

		LetterLink link = m_letterLinks[m_letterLinks.Count-1];

		Button btnComp = link.mainObj.GetComponent<Button>();

		if(btnComp)
		{
			if(!btnComp.IsInteractable())
			{
				btnComp.interactable = true;

				Text textComp = link.linkedObj.GetComponentInChildren<Text>();

				textComp.enabled = false;
			}
		}

		m_letterLinks.RemoveAt(m_letterLinks.Count-1);
	}

	void SetupNavigation()
	{
		if(m_displayWord == null)
			return;

		for(int i = 0; i < m_displayWord.letters.Count; i++)
		{
			GameObject curLetter = m_displayWord.letters[i];

			Button btnComp = curLetter.GetComponent<Button>();

			if(btnComp == null)
				continue;

			Navigation btnNav = btnComp.navigation;
			btnNav.mode = Navigation.Mode.Explicit;

			if(i == 0)
				btnNav.selectOnLeft = m_displayWord.letters[m_displayWord.letters.Count - 1].GetComponent<Selectable>();
			else
				btnNav.selectOnLeft = m_displayWord.letters[i-1].GetComponent<Selectable>();

			if(i == m_displayWord.letters.Count - 1)
				btnNav.selectOnRight = m_displayWord.letters[0].GetComponent<Selectable>();
			else
				btnNav.selectOnRight = m_displayWord.letters[i+1].GetComponent<Selectable>();

			btnNav.selectOnDown = m_createdLetters[i].GetComponent<Selectable>();
			btnNav.selectOnUp = null;

			btnComp.navigation = btnNav;
		}

		for(int i = 0; i < m_createdLetters.Count; i++)
		{
			GameObject curLetter = m_createdLetters[i];
			
			Toggle toggleComp = curLetter.GetComponent<Toggle>();
			
			if(toggleComp == null)
				continue;
			
			Navigation btnNav = toggleComp.navigation;
			btnNav.mode = Navigation.Mode.Explicit;
			
			if(i == 0)
				btnNav.selectOnLeft = m_createdLetters[m_createdLetters.Count - 1].GetComponent<Selectable>();
			else
				btnNav.selectOnLeft = m_createdLetters[i-1].GetComponent<Selectable>(); 
			
			if(i == m_createdLetters.Count - 1)
				btnNav.selectOnRight = m_createdLetters[0].GetComponent<Selectable>();
			else
				btnNav.selectOnRight = m_createdLetters[i+1].GetComponent<Selectable>();
			
			btnNav.selectOnDown = downNavObj;
			btnNav.selectOnUp = m_displayWord.letters[i].GetComponent<Selectable>();
			
			toggleComp.navigation = btnNav;
		}
	}

	public void TriggerUndoNavEvent()
	{
		UndoNavSelectEvent undoEvent = new UndoNavSelectEvent();
		EventSystem.BroadcastEvent(gameObject, undoEvent);
	}
}
