using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordData
{
	public string word;
	public GameObject ownerPanel;
	public GameObject letterHolder;
	public List<GameObject> letterObjs;
	public bool wordFound;

	public WordData()
	{
		letterObjs = new List<GameObject>();
	}

	public bool InUse()
	{
		if(letterObjs.Count > 0)
			return true;

		return false;
	}

	public void SetWordFound(Color colour)
	{
		for(int i = 0; i < letterObjs.Count; i++)
		{
			Image imgComp = letterObjs[i].GetComponent<Image>();

			if(imgComp)
				imgComp.color = colour;

			Text textComp = letterObjs[i].GetComponentInChildren<Text>();

			if(textComp)
				textComp.enabled = true;
		}

		wordFound = true;
	}
}

public class WordCollection
{
	public GameObject collectionPanel;
	public List<WordData> words;
	public bool activeCollection;

	public WordCollection() 
	{
		words = new List<WordData>();
		activeCollection = false;
		collectionPanel = null;
	}

	public WordData FindFreeWord()
	{
		for(int i = 0; i < words.Count; i++)
		{
			if(words[i].InUse())
				continue;

			return words[i];
		}

		return null;
	}
}

public class FindWordsUI : MonoBehaviour, EventInterface
{
	public AudioSource audioPlayer;
	public AudioClip correctSfx;
	public AudioClip incorrectSfx;
	public AudioClip foundAllSfx;

	public GameObject collectionHolder;

	public GameObject wordCollectionPrefab;
	public GameObject wordPanelPrefab;
	public GameObject letterPrefab;

	public int maxWordCollections;
	public int maxLettersPerWord;
	public int maxWordsPerCollection;

	public int minLettersPerWord;

	public Color foundColour;

	int m_maxWords;
	int m_scorePerLetter;
	int m_scorePerGame;
	List<WordCollection> m_wordCollections;

	Dictionary<string, bool> m_foundBonusWords;

	void Awake()
	{
		m_foundBonusWords = new Dictionary<string, bool>();

		EventSystem.RegisterDelegate(this, EVENTTYPE.SUBMITWORD);
		EventSystem.RegisterDelegate(this, EVENTTYPE.NEXTWORDSTORE);
		EventSystem.RegisterDelegate(this, EVENTTYPE.ENDROUND);

		m_maxWords = maxWordCollections * maxWordsPerCollection;

		m_wordCollections = new List<WordCollection>();

		for(int i = 0; i < maxWordCollections; i++)
		{
			WordCollection newCollection = new WordCollection();

			newCollection.collectionPanel = Instantiate(wordCollectionPrefab, collectionHolder.transform);
			newCollection.collectionPanel.SetActive(false);

			for(int j = 0; j < maxWordsPerCollection; j++)
			{
				WordData newWord = new WordData();
				newCollection.words.Add(newWord);
			}

			m_wordCollections.Add(newCollection);
		}
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.SUBMITWORD);
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.NEXTWORDSTORE);
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.ENDROUND);
	}

	// Use this for initialization
	void Start () 
	{
		//List<string> validWords = FindWordsFromString("TESTER");

		GetSettingEvent setting = new GetSettingEvent("ScorePerLetter");
		EventSystem.BroadcastEvent(gameObject, setting);
		m_scorePerLetter = int.Parse(setting.data.GetFirstElement(setting.data.data[0]));
		setting.name = "ScorePerGame";
		EventSystem.BroadcastEvent(gameObject, setting);
		m_scorePerGame = int.Parse(setting.data.GetFirstElement(setting.data.data[0]));
	}
	
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.SUBMITWORD))
		{
			SubmitWordEvent submitEvent = data as SubmitWordEvent;

			bool allWordsFound = true;

			bool foundWord = false;
			bool alreadyFound = false;

			for(int i = 0; i < m_wordCollections.Count; i++)
			{
				if(alreadyFound && !allWordsFound)
					break;

				for(int j = 0; j < m_wordCollections[i].words.Count; j++)
				{
					WordData wordData = m_wordCollections[i].words[j];
					
					if(wordData.wordFound)
					{
						if(wordData.word == submitEvent.word)
						{
							alreadyFound = true;
						}

						continue;
					}

					if(wordData.word != submitEvent.word)
					{
						allWordsFound = false;

						if(alreadyFound)
							break;

						continue;
					}
					
					wordData.SetWordFound(foundColour);
					
					IncreaseScoreEvent newIncreaseEvent = new IncreaseScoreEvent(m_scorePerLetter * wordData.word.Length);
					EventSystem.BroadcastEvent(gameObject, newIncreaseEvent);
					
					foundWord = true;
				}
			}

			if(!foundWord && !alreadyFound && !m_foundBonusWords.ContainsKey(submitEvent.word))
			{
				DictionaryCheckEvent checkDictionary = new DictionaryCheckEvent();
				checkDictionary.inputWords = new List<string>();
				checkDictionary.inputWords.Add(submitEvent.word);

				EventSystem.BroadcastEvent(gameObject, checkDictionary);

				if(checkDictionary.outputWords.Count > 0)
				{
					m_foundBonusWords.Add(submitEvent.word, true);
					IncreaseScoreEvent newIncreaseEvent = new IncreaseScoreEvent(m_scorePerLetter * submitEvent.word.Length, "Bonus");
					EventSystem.BroadcastEvent(gameObject, newIncreaseEvent);
					
					foundWord = true;
				}
			}

			if(allWordsFound)
			{
				IncreaseScoreEvent newIncreaseEvent = new IncreaseScoreEvent(m_scorePerGame);
				EventSystem.BroadcastEvent(gameObject, newIncreaseEvent);
				EventSystem.BroadcastEvent(gameObject, new NextGameEvent());

				audioPlayer.PlayOneShot(foundAllSfx);
			}
			else if(foundWord)
			{
				audioPlayer.PlayOneShot(correctSfx);
			}
			else
			{
				audioPlayer.PlayOneShot(incorrectSfx);
			}
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.NEXTWORDSTORE))
		{
			NextWordStoreEvent eventData = data as NextWordStoreEvent;

			SetupWords(eventData.wordStore.words);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ENDROUND))
		{
			RevealWords();
		}
	}

	void CreateWord(string word)
	{
		word = word.ToUpper();
		for(int i = 0; i < m_wordCollections.Count; i++)
		{
			WordData foundWordData = m_wordCollections[i].FindFreeWord();

			if(foundWordData == null)
				continue;

			GameObject newWordPanel = Instantiate(wordPanelPrefab, m_wordCollections[i].collectionPanel.transform);

			foundWordData.ownerPanel = newWordPanel;
			foundWordData.letterHolder = newWordPanel.transform.childCount > 0 ? newWordPanel.transform.GetChild(0).gameObject : newWordPanel;

			for(int j = 0; j < word.Length; j++)
			{
				GameObject newLetter = Instantiate(letterPrefab, foundWordData.letterHolder.transform);

				Text textComp = newLetter.GetComponentInChildren<Text>();

				if(textComp != null)
				{
					textComp.text = word[j].ToString();
					textComp.enabled = false;
				}

				foundWordData.letterObjs.Add(newLetter);
			}

			foundWordData.word = word;

			if(!m_wordCollections[i].activeCollection)
			{
				m_wordCollections[i].activeCollection = true;
				m_wordCollections[i].collectionPanel.SetActive(true);
			}

			EventSystem.BroadcastEvent(gameObject, m_wordCollections[i].collectionPanel, new UpdateUILayoutEvent());
			break;
		}
	}

	void ClearWords()
	{
		for(int i = 0; i < m_wordCollections.Count; i++)
		{
			for(int j = 0; j < m_wordCollections[i].words.Count; j++)
			{
				WordData foundWordData = m_wordCollections[i].words[j];
				foundWordData.wordFound = false;
				foundWordData.letterObjs.Clear();
				DestroyImmediate(foundWordData.ownerPanel);
				foundWordData.ownerPanel = null;
				foundWordData.letterHolder = null;
				foundWordData.word = "";
			}

			m_wordCollections[i].activeCollection = false;
			m_wordCollections[i].collectionPanel.SetActive(false);
		}
	}

	void RevealWords()
	{
		for(int i = 0; i < m_wordCollections.Count; i++)
		{
			if(!m_wordCollections[i].activeCollection)
				continue;
			
			for(int j = 0; j < m_wordCollections[i].words.Count; j++)
			{
				if(m_wordCollections[i].words[j].wordFound)
					continue;

				m_wordCollections[i].words[j].SetWordFound(foundColour);
			}
		}
	}

	void SetupWords(List<string> words)
	{
		if(words == null)
			return;

		if(words.Count <= 0)
			return;
		
		ClearWords();

		for(int i = 0; i < words.Count; i++)
		{
			CreateWord(words[i]);
		}
	}

	List<string> FindWordsFromString(string letters)
	{
		List<string> words = GetWordPermutations("", letters);

		DictionaryCheckEvent checkEvent = new DictionaryCheckEvent();
		checkEvent.inputWords = words;

		EventSystem.BroadcastEvent(gameObject, checkEvent);

		HashSet<string> existingWords = new HashSet<string>();
		for(int i = 0; i < checkEvent.outputWords.Count; i++)
		{
			if(checkEvent.outputWords[i].Length < minLettersPerWord)
			{
				checkEvent.outputWords.RemoveAt(i);
				i--;
				continue;
			}

			if(existingWords.Contains(checkEvent.outputWords[i]))
			{
				checkEvent.outputWords.RemoveAt(i);
				i--;
				continue;
			}

			existingWords.Add(checkEvent.outputWords[i]);
		}

		checkEvent.outputWords.Sort((x, y) => x.Length.CompareTo(y.Length));
		return checkEvent.outputWords;
	}

	List<string> GetWordPermutations(string letter, string addLetters)
	{
		if(addLetters.Length == 0)
			return null;
		
		List<string> words = new List<string>();

		for(int i = 0; i < addLetters.Length; i++)
		{
			string newWord = letter + addLetters[i];
			string spareLetter = addLetters.Remove(i, 1);

			words.Add(newWord);

			if(spareLetter.Length > 0)
			{
				List<string> foundWords = GetWordPermutations(newWord, spareLetter);

				if(foundWords != null)
				{
					for(int j = 0; j < foundWords.Count; j++)
					{
						words.Add(foundWords[j]);
					}
				}
			}
		}

		return words;
	}
}
