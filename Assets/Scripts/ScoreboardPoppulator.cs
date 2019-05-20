using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel
{
	public GameObject obj;
	public Text scoreText;
	public Text nameText;
	public GameObject background;
}

public class ScoreData
{
	public string id = "";
	public int score = 0;
	public bool highlight = false;
}

public class ScoreboardPoppulator : MonoBehaviour 
{
	public GameObject panelHolder;
	public GameObject panelPrefab;
	public GameObject splitterPrefab;
	public int maxPanelCount;
	public string placeHolderText;
	public Color normalColour;
	public int normalTextSize = 40;
	public float normalHeight = 60;
	public Color highlightColour;
	public int highlightTextSize = 48;
	public float highlightHeight = 66;

	List<ScoreData> m_scores;
	List<ScorePanel> m_scorePanels;
	bool m_updateScores;

	GameObject[] m_splitters = new GameObject[2];

	void Awake()
	{
		m_updateScores = false;
		m_scores = new List<ScoreData>();

		DatabaseManager.SetupServerConnection();
		StartScoreListener();
	}

	// Use this for initialization
	void Start ()
	{
		m_scorePanels = new List<ScorePanel>();

		for(int i = 0; i < maxPanelCount; i++)
		{
			ScorePanel newScorePanel = new ScorePanel();
			newScorePanel.obj = Instantiate(panelPrefab, panelHolder.transform);
			Text[] textComps = newScorePanel.obj.GetComponentsInChildren<Text>();
			newScorePanel.background = newScorePanel.obj.transform.Find("Background").gameObject;
			newScorePanel.nameText = textComps[0];
			newScorePanel.scoreText = textComps[1];
			newScorePanel.nameText.text = (i+1).ToString() + "." + " " + placeHolderText;
			newScorePanel.scoreText.text = "0";

			m_scorePanels.Add(newScorePanel);
		}

		m_splitters[0] = Instantiate(splitterPrefab, panelHolder.transform);
		m_splitters[0].transform.SetSiblingIndex(0);
		m_splitters[1] = Instantiate(splitterPrefab, panelHolder.transform);
		m_splitters[1].transform.SetSiblingIndex(1);
	}
	
	protected void StartScoreListener() 
	{
		FirebaseDatabase.DefaultInstance
			.GetReference("PressedForWordsScores").OrderByChild("Score")
			.ValueChanged += (object sender2, ValueChangedEventArgs e2) => 
		{
			int lastScore = PlayerPrefs.GetInt("LastScore");
			string lastId = PlayerPrefs.GetString("LastId");

			if (e2.DatabaseError != null) 
			{
				Debug.LogError(e2.DatabaseError.Message);
				return;
			}
				
			m_scores.Clear();
			Debug.Log("Received values for PressedForWordsScores.");
			if (e2.Snapshot == null || e2.Snapshot.ChildrenCount <= 0)
			{
				return;
			}

			foreach (var childSnapshot in e2.Snapshot.Children) 
			{
				if (childSnapshot.Child("Score") == null || childSnapshot.Child("Score").Value == null) 
				{
					Debug.LogError("Bad data in sample.  Did you forget to call SetEditorDatabaseUrl with your project id?");
					break;
				}

				Debug.Log("PressedForWordsScores entry : " +
					childSnapshot.Child("Id").Value.ToString() + " - " +
					childSnapshot.Child("Score").Value.ToString());

				ScoreData scoreData = new ScoreData();

				scoreData.id = childSnapshot.Child("Id").Value.ToString();
				scoreData.score = int.Parse(childSnapshot.Child("Score").Value.ToString());
				scoreData.highlight = scoreData.score == lastScore && scoreData.id == lastId;

				m_scores.Add(scoreData);
			}
			m_scores.Sort((x, y) => y.score.CompareTo(x.score));
			m_updateScores = true;
		};
	}

	// Update is called once per frame
	void Update () 
	{
		if(!m_updateScores)
			return;
		
		int id = m_scores.FindIndex(x => x.highlight);
		int endIdStart = 0;

		if(id >= m_scorePanels.Count)
		{
			endIdStart = id - 1;
		}
		else
		{
			endIdStart = m_scorePanels.Count-3;
		}

		for(int i = 0; i < m_scorePanels.Count; i++)
		{
			ScoreData curScoreData = null;
			int scoreId = i;

			if(i >= m_scorePanels.Count-3)
				scoreId = endIdStart - ((m_scorePanels.Count-3) - i);
			
			if(m_scores.Count > i)
				curScoreData = m_scores[scoreId];

			m_scorePanels[i].nameText.text = scoreId+1 + "." + " " + (curScoreData != null ? curScoreData.id : "---");
			m_scorePanels[i].scoreText.text = curScoreData != null ? curScoreData.score.ToString() : "0";	

			if(curScoreData == null || !curScoreData.highlight)
			{
				m_scorePanels[i].nameText.color = normalColour;
				m_scorePanels[i].scoreText.color = normalColour;
				m_scorePanels[i].nameText.fontSize = normalTextSize;
				m_scorePanels[i].scoreText.fontSize = normalTextSize;
				RectTransform panelRect = m_scorePanels[i].obj.GetComponent<RectTransform>();
				Vector2 panelSize = panelRect.sizeDelta;
				panelSize.y = normalHeight;
				panelRect.sizeDelta = panelSize;
			}
			else
			{
				m_scorePanels[i].nameText.color = highlightColour;
				m_scorePanels[i].scoreText.color = highlightColour;
				m_scorePanels[i].nameText.fontSize = highlightTextSize;
				m_scorePanels[i].scoreText.fontSize = highlightTextSize;
				RectTransform panelRect = m_scorePanels[i].obj.GetComponent<RectTransform>();
				Vector2 panelSize = panelRect.sizeDelta;
				panelSize.y = highlightHeight;
				panelRect.sizeDelta = panelSize;


				m_splitters[0].transform.SetAsLastSibling();
				m_splitters[1].transform.SetAsLastSibling();

				m_splitters[0].transform.SetSiblingIndex(i+2);
				m_splitters[1].transform.SetSiblingIndex(i-1);
			}
				
			m_scorePanels[i].background.SetActive(scoreId == id || scoreId == id - 1 || scoreId == id + 1);
		}

		m_updateScores = true;
	}
}
