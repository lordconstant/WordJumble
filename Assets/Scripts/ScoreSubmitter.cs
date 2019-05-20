using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSubmitter : MonoBehaviour 
{
	public Text scoreText;
	public InputField nameText;

	int m_score;
	bool m_scoreSent;

	void Awake()
	{
		DatabaseManager.SetupServerConnection();
	}

	// Use this for initialization
	void Start () 
	{
		m_scoreSent = false;
		m_score = PlayerPrefs.GetInt("Score", 0);
		PlayerPrefs.DeleteKey("Score");
		if(scoreText != null)
			scoreText.text = m_score.ToString();

		TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true, true);
	}

	// A realtime database transaction receives MutableData which can be modified
	// and returns a TransactionResult which is either TransactionResult.Success(data) with
	// modified data or TransactionResult.Abort() which stops the transaction with no changes.
	TransactionResult AddScoreTransaction(MutableData mutableData) {
		List<object> scores = mutableData.Value as List<object>;

		if (scores == null)
			scores = new List<object>();

		// Now we add the new score as a new entry that contains the email address and score.
		Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
		newScoreMap["Id"] = nameText.text;
		newScoreMap["Score"] = m_score;
		scores.Add(newScoreMap);

		// You must set the Value to indicate data at that location has changed.
		mutableData.Value = scores;
		return TransactionResult.Success(mutableData);
	}

	public void SubmitScore()
	{
		if(m_scoreSent)
			return;

		m_scoreSent = true;

		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("PressedForWordsScores");

		PlayerPrefs.SetString("LastId", nameText.text);
		PlayerPrefs.SetInt("LastScore", m_score);

		Debug.Log("Running Transaction...");
		// Use a transaction to ensure that we do not encounter issues with
		// simultaneous updates that otherwise might create more than MaxScores top scores.
		reference.RunTransaction(AddScoreTransaction)
			.ContinueWith(task => 
				{
					if (task.Exception != null) 
					{
						Debug.Log(task.Exception.ToString());
					} 
					else if (task.IsCompleted) 
					{
						Debug.Log("Transaction complete.");
					}
				});
	}
}
