using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VanishingText : MonoBehaviour 
{
	public float lifetime = 2.0f;
	public AnimationCurve fadeCurve;
	public AnimationCurve moveCurve;
	public Text textComp;
	public float moveDistance = 100.0f;

	float m_prog;
	float m_intialPos;

	// Use this for initialization
	void Awake () 
	{
		m_prog = 0.0f;
		Destroy(gameObject, lifetime);
	}

	void Start()
	{
		m_intialPos = transform.position.y;
	}

	// Update is called once per frame
	void Update () 
	{
		m_prog += Time.deltaTime / lifetime;

		Color textCol = textComp.color;
		textCol.a = fadeCurve.Evaluate(m_prog);
		textComp.color = textCol;

		Vector3 pos = transform.position;
		float moveRes = moveCurve.Evaluate(m_prog);
		float newY = Mathf.Lerp(m_intialPos, m_intialPos + moveDistance, moveRes);
		pos.y = newY;
		transform.position = pos;
	}
}
