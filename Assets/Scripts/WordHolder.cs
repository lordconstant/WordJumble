using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextWordStoreEvent : EventBase
{
	public WordStore wordStore;

	public NextWordStoreEvent()
	{
		eventType = EVENTTYPE.NEXTWORDSTORE;
	}

	public NextWordStoreEvent(WordStore store)
	{
		eventType = EVENTTYPE.NEXTWORDSTORE;
		wordStore = store;
	}
}

public class GetShuffledWordEvent : EventBase
{
	public string shuffledWord;

	public GetShuffledWordEvent()
	{
		eventType = EVENTTYPE.GETSHUFFLEDWORD;
	}
}

public class GetWordsEvent : EventBase
{
	public List<string> words;

	public GetWordsEvent()
	{
		eventType = EVENTTYPE.GETWORDS;
		words = new List<string>();
	}
}

public class WordStore
{
	public string shuffledLetters;
	public List<string> words;

	public WordStore()
	{
		words = new List<string>();
		shuffledLetters = "";
	}
}

public class WordHolder : MonoBehaviour, EventInterface
{
	List<WordStore> m_wordStores;
	List<WordStore> m_usedStores;

	WordStore m_curStore;

	void Awake()
	{
		EventSystem.RegisterDelegate(this, EVENTTYPE.GETSHUFFLEDWORD);
		EventSystem.RegisterDelegate(this, EVENTTYPE.GETWORDS);
		EventSystem.RegisterDelegate(this, EVENTTYPE.NEXTGAME);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.GETSHUFFLEDWORD);
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.GETWORDS);
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.NEXTGAME);
	}

	// Use this for initialization
	void Start () 
	{
		m_wordStores = new List<WordStore>();
		m_usedStores = new List<WordStore>();

		GetSettingEvent setting = new GetSettingEvent("Words");
		EventSystem.BroadcastEvent(gameObject, setting);

		if(setting.data == null)
			return;

		List<string> wordData = setting.data.data;

		for(int i = 0; i < wordData.Count; i++)
		{
			char[] splitChars = new char[1];
			splitChars[0] = ',';
			string[] words = (wordData[i].Trim()).Split(splitChars, System.StringSplitOptions.RemoveEmptyEntries);

			if(words.Length <= 1)
				continue;

			WordStore newWordStore = new WordStore();
			newWordStore.shuffledLetters = words[0];

			for(int j = 1; j < words.Length; j++)
			{
				newWordStore.words.Add(words[j]);
			}

			newWordStore.words.Sort((x, y) => x.Length.CompareTo(y.Length));

			m_wordStores.Add(newWordStore);
		}

		GetNewStore();
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.GETSHUFFLEDWORD))
		{
			GetShuffledWordEvent eventData = data as GetShuffledWordEvent;

			eventData.shuffledWord = m_curStore.shuffledLetters;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.GETWORDS))
		{
			GetWordsEvent eventData = data as GetWordsEvent;

			eventData.words = m_curStore.words;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.NEXTGAME))
		{
			GetNewStore();
		}
	}

	void GetNewStore()
	{
		if(m_wordStores.Count <= 0)
		{
			for(int i = 0; i < m_usedStores.Count; i++)
			{
				m_wordStores.Add(m_usedStores[i]);
			}

			m_usedStores.Clear();
		}

		int storeId = Random.Range(0, m_wordStores.Count);

		m_curStore = m_wordStores[storeId];
		m_usedStores.Add(m_curStore);
		m_wordStores.Remove(m_curStore);

		NextWordStoreEvent nextStoreEvent = new NextWordStoreEvent(m_curStore);
		EventSystem.BroadcastEvent(gameObject, nextStoreEvent);
	}
}
