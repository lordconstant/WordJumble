using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Xml.Serialization;

//Wasnt using save or delete & using the resource folder makes cross platform use simpler
public static class Serializer 
{
	//Save Data
//	public static void saveData<T>(T dataToSave, string dataFileName)
//	{
//		string tempPath = Path.Combine(Application.streamingAssetsPath, "data");
//		tempPath = Path.Combine(tempPath, dataFileName + ".txt");
//
//		//Convert To Json then to bytes
//		string jsonData = JsonUtility.ToJson(dataToSave, true);
//		//byte[] jsonByte = Encoding.ASCII.GetBytes(jsonData);
//
//		//Create Directory if it does not exist
//		if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
//		{
//			Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
//		}
//		//Debug.Log(path);
//
//		try
//		{
//			File.WriteAllText(tempPath, jsonData);
//			Debug.Log("Saved Data to: " + tempPath.Replace("/", "\\"));
//		}
//		catch (Exception e)
//		{
//			Debug.LogWarning("Failed To PlayerInfo Data to: " + tempPath.Replace("/", "\\"));
//			Debug.LogWarning("Error: " + e.Message);
//		}
//	}
//
	//Load Data
	public static T loadData<T>(string dataFileName)
	{
		//string tempPath = Path.Combine(Application.streamingAssetsPath, "data/");
		//tempPath = Path.Combine(tempPath, dataFileName + ".txt");
		//string jsonData = null;

		//#if UNITY_ANDROID
		//WWW www = new WWW(tempPath);
		//while(!www.isDone){
			//if(www.error != ""){
			//	return default(T);
			//}
		//}

		//jsonData = www.text;
		//#else
		//Exit if Directory or File does not exist
//		if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
//		{
//			Debug.LogWarning("Directory does not exist");
//			return default(T);
//		}
//
//		if (!File.Exists(tempPath))
//		{
//			Debug.Log("File does not exist");
//			return default(T);
//		}
//
//		//Load saved Json
//		try
//		{
//			jsonData = File.ReadAllText(tempPath);
//			Debug.Log("Loaded Data from: " + tempPath.Replace("/", "\\"));
//		}
//		catch (Exception e)
//		{
//			Debug.LogWarning("Failed To Load Data from: " + tempPath.Replace("/", "\\"));
//			Debug.LogWarning("Error: " + e.Message);
//		}
		//#endif
		//Convert to json string
		//string jsonData = Encoding.ASCII.GetString(jsonByte);

		//Convert to Object
		TextAsset loadedText = Resources.Load("data/" + dataFileName) as TextAsset;
		object resultValue = JsonUtility.FromJson<T>(loadedText.text);
		return (T)Convert.ChangeType(resultValue, typeof(T));
	}

	public static string[] LoadCSVData(string filename)
	{
		string[] csvLines = null;

		#if UNITY_ANDROID
		string tempPath = Application.streamingAssetsPath + "/Data/" + filename + ".csv";
		WWW www = new WWW(tempPath);
		while(!www.isDone){
		}

		csvLines = www.text.Split("\n"[0]);
		#else
		string tempPath = Path.Combine(Application.streamingAssetsPath, "data/");
		tempPath = Path.Combine(tempPath, filename + ".csv");

		if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
		{
			Debug.LogWarning("Directory does not exist");
			return csvLines;
		}

		if (!File.Exists(tempPath))
		{
			Debug.Log("File does not exist");
			return csvLines;
		}

		//Load saved Json
		try
		{
			csvLines = File.ReadAllLines(tempPath);
			Debug.Log("Loaded Data from: " + tempPath.Replace("/", "\\"));
		}
		catch (Exception e)
		{
			Debug.LogWarning("Failed To Load Data from: " + tempPath.Replace("/", "\\"));
			Debug.LogWarning("Error: " + e.Message);
		}
		#endif
			
		return csvLines;

	}

//	public static bool deleteData(string dataFileName)
//	{
//		bool success = false;
//
//		//Load Data
//		string tempPath = Path.Combine(Application.streamingAssetsPath, "data");
//		tempPath = Path.Combine(tempPath, dataFileName + ".txt");
//
//		//Exit if Directory or File does not exist
//		if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
//		{
//			Debug.LogWarning("Directory does not exist");
//			return false;
//		}
//
//		if (!File.Exists(tempPath))
//		{
//			Debug.Log("File does not exist");
//			return false;
//		}
//
//		try
//		{
//			File.Delete(tempPath);
//			Debug.Log("Data deleted from: " + tempPath.Replace("/", "\\"));
//			success = true;
//		}
//		catch (Exception e)
//		{
//			Debug.LogWarning("Failed To Delete Data: " + e.Message);
//		}
//
//		return success;
//	}
}