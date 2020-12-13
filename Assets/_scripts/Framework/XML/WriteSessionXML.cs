using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class WriteSessionXML {
	
	public static void WriteToXML(string name, Dictionary<string, string> values) 
	{
		XmlWriter writer = SessionDataManager.Instance.GetXMLWriter();
		
		if(writer == null) {
			return;
		}

		writer.WriteStartElement("event");
		
		writer.WriteStartAttribute("name");
		writer.WriteValue(name);
		writer.WriteEndAttribute();
		
		writer.WriteStartAttribute("sessionTimeElapsed");
		writer.WriteValue(SessionDataManager.Instance.GetSessionTime());
		writer.WriteEndAttribute();
		
		foreach(KeyValuePair<string, string> pair in values) 
		{
			writer.WriteStartElement("Param");
			writer.WriteStartAttribute("name");
			writer.WriteValue(pair.Key);
			writer.WriteEndAttribute();
			
			writer.WriteStartAttribute("value");
			writer.WriteValue(pair.Value);
			writer.WriteEndAttribute();
			writer.WriteEndElement();
		}
		
		writer.WriteEndElement();
	}
	
}
