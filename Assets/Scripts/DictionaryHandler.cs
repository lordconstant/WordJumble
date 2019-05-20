using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryCheckEvent : EventBase
{
	public List<string> inputWords;
	public List<string> outputWords;

	public DictionaryCheckEvent()
	{
		eventType = EVENTTYPE.DICTIONARYCHECK;
		outputWords = new List<string>();
	}
}

public class DictionaryHandler : MonoBehaviour, EventInterface
{
	public TextAsset dictionaryDoc;

	HashSet<string> m_words;

	void Awake()
	{
		EventSystem.RegisterDelegate(this, EVENTTYPE.DICTIONARYCHECK);
		m_words = new HashSet<string>();
		CollectWords();
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.DICTIONARYCHECK);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.DICTIONARYCHECK))
		{
			DictionaryCheckEvent checkData =  data as DictionaryCheckEvent;

			if(checkData.inputWords == null)
				return;
			
			for(int i = 0; i < checkData.inputWords.Count; i++)
			{
				if(!m_words.Contains(checkData.inputWords[i]))
					continue;
				
				checkData.outputWords.Add(checkData.inputWords[i]);
			}
		}
	}

	void CollectWords()
	{
		string[] wordArr = dictionaryDoc.text.Split(new char[] {'\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);

		for(int i = 0; i < wordArr.Length; i++)
		{
			string word = wordArr[i].ToUpper();

			if(word.Contains("'"))
			{
				word.Remove(word.IndexOf("'"), 1);
			}

			if(word.Length < 3)
				continue;
			
			m_words.Add(word);
		}

		Debug.Log(m_words.Count);
	}
}
