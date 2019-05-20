using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndRoundEvent : EventBase
{
	public EndRoundEvent()
	{
		eventType = EVENTTYPE.ENDROUND;
	}
}

public class NextGameEvent : EventBase
{
	public NextGameEvent()
	{
		eventType = EVENTTYPE.NEXTGAME;
	}
}

public class IncreaseScoreEvent : EventBase
{
	public string rewardText;
	public int score;

	public IncreaseScoreEvent()
	{
		eventType = EVENTTYPE.INCREASESCORE;
		score = 0;
		rewardText = "";
	}

	public IncreaseScoreEvent(int newScore)
	{
		eventType = EVENTTYPE.INCREASESCORE;
		score = newScore;
		rewardText = "";
	}

	public IncreaseScoreEvent(int newScore, string newRewardText)
	{
		eventType = EVENTTYPE.INCREASESCORE;
		score = newScore;
		rewardText = newRewardText;
	}
}

public class NormalGameMode : MonoBehaviour, EventInterface
{
	public AudioSource audioPlayer;
	public GameObject gameInfoPanel;
	public GameObject endRoundInfoPanel;

	public Text scoreText;
	public Text otherScoreText;
	public Text timeText;

	int m_score;
	float m_gameTime;
	float m_gameTimer;

	bool m_roundOver;

	void Awake()
	{
		EventSystem.RegisterDelegate(this, EVENTTYPE.INCREASESCORE);
		EventSystem.RegisterDelegate(this, EVENTTYPE.ENDROUND);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.INCREASESCORE);
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.ENDROUND);
	}

	// Use this for initialization
	void Start () 
	{
		gameInfoPanel.SetActive(true);
		endRoundInfoPanel.SetActive(false);

		m_roundOver = false;

		m_gameTime = 300.0f;
		m_gameTimer = 0.0f;

		GetSettingEvent setting = new GetSettingEvent("GameTime");
		EventSystem.BroadcastEvent(gameObject, setting);
		m_gameTime = int.Parse(setting.data.GetFirstElement(setting.data.data[0]));
	}

	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyUp(KeyCode.Escape))
		{
			ExitClick();
			return;
		}

		scoreText.text = "Score " + m_score;
		otherScoreText.text = "Score " + m_score;

		if(m_roundOver)
			return;
		
		m_gameTimer += Time.deltaTime;

		if(m_gameTimer >= m_gameTime)
		{
			m_gameTimer = m_gameTime;
			EventSystem.BroadcastEvent(gameObject, new EndRoundEvent());
		}

		float remTime = m_gameTime - m_gameTimer;
		if(remTime < 0.0f)
			remTime = 0.0f;
		int seconds = Mathf.FloorToInt(remTime % 60.0f);
		int mins = Mathf.RoundToInt((remTime - seconds) / 60.0f);
		timeText.text = "Time " + mins.ToString() + ":" + seconds.ToString("D2");
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.INCREASESCORE))
		{
			IncreaseScoreEvent increaseData = data as IncreaseScoreEvent;

			m_score += increaseData.score;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ENDROUND))
		{
			m_roundOver = true;
			gameInfoPanel.SetActive(false);
			endRoundInfoPanel.SetActive(true);

			PlayerPrefs.SetInt("Score", m_score);
		}
	}

	public void NewGameClick()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void ExitClick()
	{
		Application.Quit();
	}

	public void EndRoundClick()
	{
		EventSystem.BroadcastEvent(gameObject, new EndRoundEvent());
	}

	public void PlaySfx(AudioClip sfxClip)
	{
		audioPlayer.PlayOneShot(sfxClip);
	}
}
