using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingData
{
	public string name;
	public List<string> data;

	public SettingData()
	{
		name = "";
		data = new List<string>();
	}

	public string GetFirstElement(string fromString)
	{
		if(data.Count <= 0)
			return "";

		string[] words = (fromString.Trim()).Split(',');
		return words[0];
	}
}

public class GetSettingEvent : EventBase
{
	public string name;
	public SettingData data;

	public GetSettingEvent()
	{
		name = "";
		data = null;
		eventType = EVENTTYPE.GETSETTING;
	}

	public GetSettingEvent(string settingName)
	{
		name = settingName;
		data = null;
		eventType = EVENTTYPE.GETSETTING;
	}
}

public class SettingsLoader : MonoBehaviour, EventInterface
{
	List<SettingData> m_settings;

	// Use this for initialization
	void Awake () 
	{
		m_settings = new List<SettingData>();

		string[] settingsData = Serializer.LoadCSVData("WordJumbleSettings");

		SettingData curSetting = null;

		for(int i = 0; i < settingsData.Length; i++)
		{
			string[] words = (settingsData[i].Trim()).Split(',');

			if(words.Length <= 0)
				continue;

			if(curSetting == null)
			{
				if(words[0].StartsWith("//"))
				{
					curSetting = new SettingData();

					curSetting.name = words[0].Substring(2);
				}

				continue;
			}
			else
			{
				if(words[0] == "//")
				{
					m_settings.Add(curSetting);
					curSetting = null;
					continue;
				}

				curSetting.data.Add(settingsData[i]);
			}
		}

		if(curSetting != null)
		{
			m_settings.Add(curSetting);
		}

		EventSystem.RegisterDelegate(this, EVENTTYPE.GETSETTING);
	}

	void OnDestroy()
	{
		EventSystem.UnRegisterDelegate(this, EVENTTYPE.GETSETTING);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.GETSETTING))
		{
			GetSettingEvent getSettingData = data as GetSettingEvent;

			for(int i = 0; i < m_settings.Count; i++)
			{
				if(m_settings[i].name == getSettingData.name)
				{
					getSettingData.data = m_settings[i];
					break;
				}
			}
		}
	}
}
