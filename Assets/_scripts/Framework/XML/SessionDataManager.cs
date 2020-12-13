using UnityEngine;
using System;
using System.Collections;
using System.Xml;
using System.IO;

public class SessionDataManager {
	private const bool DISABLE_XML = true;

	private const string EVENT_SESSION_DATA_NAME = "SubjectSessionData.xml";
	private const string SESSION_DATA_PATH = "SessionData";
	
	private XmlWriter sessionDataWriter;
	
	private string subjectID = "DEBUG";
	private string sessionDataDirectory = "";
	
	private static SessionDataManager instance;
	public static SessionDataManager Instance
	{
		get 
		{ 
			if(instance == null)
				instance = new SessionDataManager();

			return instance;
		}
	}
	
	private DateTime sessionStart;
	
	public void StartSessionTimer()
	{
		sessionStart = DateTime.UtcNow;
	}
	
	public float GetSessionTime()
	{
		DateTime now = DateTime.UtcNow;
		TimeSpan elapsedTime = now.Subtract(sessionStart);
		return (float) Math.Round(elapsedTime.TotalSeconds, 2);
	}
	
	public void BeginSession(SessionManager.Candidate currentSubject) {
		CreateSessionData(currentSubject);
	}	
	
	public static string GetSessionPath() {
		if(instance == null) {
			Debug.LogError("Subject Data NOT Instantiated!!");
			return "/";
		}
		
		return instance.sessionDataDirectory;
	}
	
	public void CloseSession() {
		if(sessionDataWriter != null) {
			sessionDataWriter.WriteEndElement();
			sessionDataWriter.WriteEndDocument();
			sessionDataWriter.Flush();
			sessionDataWriter.Close();
			
			sessionDataWriter = null;
		}
	}
	
	public XmlWriter GetXMLWriter() {
		return sessionDataWriter;
	}
	
	//Instantiate
	
	private static SessionDataManager Instantiate() {
		return new SessionDataManager();
	}
	
	private void CreateSessionData(SessionManager.Candidate currentSubject) {
		if(DISABLE_XML == true)
			return;

		subjectID = currentSubject.subjectID;
		
		if(IsSessionActive()) {
			Debug.LogError("Begin session attempted with session active!!");
			return;
		}		
		
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = ("\t");
		settings.OmitXmlDeclaration = true;
		
		CreatePath();
		
		string sessionFileName = sessionDataDirectory + "/" + EVENT_SESSION_DATA_NAME;
		sessionDataWriter = XmlWriter.Create(sessionFileName, settings);
		
		if(sessionDataWriter == null) {
			Debug.LogError("Unable to create file '" + sessionFileName + ", no logging will occur.");
			return;
		}
		
		sessionDataWriter.WriteStartElement("SessionData");
	}
	
	private void CreatePath() {
		if(!Directory.Exists(SESSION_DATA_PATH))
			Directory.CreateDirectory(SESSION_DATA_PATH);

		bool bFoundDir = false;
		int tries = -1;
		
		while(!bFoundDir) {
			string playerSuffix = subjectID;
			
			if(tries != -1 )
				playerSuffix += "_" + tries;
			
			string finalLocalDirName = SESSION_DATA_PATH + "/" + playerSuffix;
			
			if(Directory.Exists(finalLocalDirName)) {
				++tries;
			} else {
				Directory.CreateDirectory(finalLocalDirName);
				sessionDataDirectory = finalLocalDirName;
				bFoundDir = true;
			}
		}		
	}
	
	private bool IsSessionActive() {
		if(sessionDataWriter == null)
			return false;
		
		return true;
	}
}
