using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DatabaseManager
{
	static bool m_connected = false;
	static DependencyStatus m_dependencyStatus = DependencyStatus.UnavailableOther;

	public static void SetupServerConnection()
	{
		if(!m_connected)
			return;

		m_connected = true;

		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			m_dependencyStatus = task.Result;
			if (m_dependencyStatus == DependencyStatus.Available) {
				InitializeFirebase();
			} else {
				Debug.LogError(
					"Could not resolve all Firebase dependencies: " + m_dependencyStatus);
			}
		});
	}

	static void InitializeFirebase()
	{
		FirebaseApp app = FirebaseApp.DefaultInstance;

		// NOTE: You'll need to replace this url with your Firebase App's database
		// path in order for the database connection to work correctly in editor.
		app.SetEditorDatabaseUrl("https://wordjumble-games4life.firebaseio.com");
		if (app.Options.DatabaseUrl != null)
			app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
	}
}
