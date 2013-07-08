using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class StringEditor : EditorWindow 
{
	ManifestResource	strings;
	Manifest			manifest;
	GUIStyle	BoldLabel = new GUIStyle("Label");
	
	void initialize()
	{
		if (System.IO.File.Exists(ManifestResource.StringsFilename))
			strings = LoadResourcesFromFile(ManifestResource.StringsFilename);
		else
			strings = new ManifestResource();
		if (System.IO.File.Exists(Manifest.ManifestFile))
			manifest = AndroidManifestEditor.LoadManifestFromFile(Manifest.ManifestFile);
		else
			manifest = new Manifest();
		FindManifestStrings(manifest, strings);
		
		BoldLabel.normal.textColor = Color.white;
		BoldLabel.fontStyle = FontStyle.Bold;
	}
	
	ResFolder resources;	
	Vector2 scroll = new Vector2(0,0);
	void OnGUI()
	{
		scroll = EditorGUILayout.BeginScrollView(scroll);
		for (int i = 0; i < strings.Count; i++)
		{
			EditorGUILayout.LabelField(strings.strings[i].name, BoldLabel);
			EditorGUILayout.BeginHorizontal();
			strings.strings[i].value = EditorGUILayout.TextField(strings.strings[i].value);
			strings.strings[i].msgid = EditorGUILayout.TextField(strings.strings[i].msgid);
			strings.strings[i].translatable = EditorGUILayout.Toggle(strings.strings[i].translatable,GUILayout.Width(20));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndScrollView();
		
	}
	
#region Static helper functions
	public static void					FindManifestStrings(Manifest manifest, ManifestResource strings)
	{
		foreach (ManifestMetaData mmd in manifest.application.meta_data)
		{
			if (mmd.value.StartsWith("@string/"))
				if (!strings.hasName(mmd.value.Substring(8))) strings.addString(mmd.name, "PUT VALUE HERE", "", true);
		}
		foreach (ManifestActivity ma in manifest.application.activity)
		{
			foreach (ManifestMetaData mmd in ma.meta_data)
			{
				if (mmd.value.StartsWith("@string/"))
					if (!strings.hasName(mmd.value.Substring(8))) strings.addString(mmd.name, "PUT VALUE HERE", "", true);
			}
		}
	}
	public static ManifestResource		LoadResourcesFromFile(string path)
	{
		ManifestResource m = new ManifestResource();
		System.IO.StreamReader reader = new System.IO.StreamReader(path);
		string xmldata = reader.ReadToEnd();
		reader.Close();
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(xmldata);
		XmlNode manifest = doc.GetElementsByTagName(Manifest.Element.resources)[0];
		m.Read(manifest);
		doc.Clone();
		return m;
	}
	public static void			SaveResourcesToFile(string path, ManifestResource m)
	{
		if (!System.IO.Directory.Exists(ManifestResource.StringsFolder))
			System.IO.Directory.CreateDirectory(ManifestResource.StringsFolder);
		XmlTextWriter	writer = new XmlTextWriter(path, System.Text.Encoding.UTF8);
		writer.Indentation = 4;
		writer.Formatting = Formatting.Indented;
		writer.Settings.NewLineHandling = NewLineHandling.Entitize;
		writer.Settings.NewLineOnAttributes = true;
		writer.WriteStartDocument();
		writer.WriteComment("This file is generated by Manifest Editor (created by Peyman Abdi peyman[at]nemo-games[dot]com).");
		m.Write(writer);
		writer.Close();
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}
#endregion
	
#if UNITY_ANDROID
	[MenuItem("Nemo/Android/String Editor")]
	public static StringEditor		ShowManifestEditorWindow()
	{
		StringEditor editor = EditorWindow.GetWindow(typeof(StringEditor),
			true, "String Manifest Editor") as StringEditor;
		editor.initialize();
		editor.Show();
		return editor;
	}
#endif
}
