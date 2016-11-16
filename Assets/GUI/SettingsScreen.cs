using UnityEngine;
using System.IO;
using System;

public class SettingsScreen {
//public 
	//Optional Parameters
	public string name = "SettingsScreen"; 
	//GUI Options
	public GUISkin guiSkin; //The GUISkin to use

	public bool isVisible{	get{	return visible;	}	} 

	//GUI
	protected Color defaultColor;
	protected int layout;
	protected Rect guiSize;
	protected GUISkin oldSkin;
	protected bool visible = false;

    private string oldIndividual = "";
    private int oldMaxGenerations = 0;
    private bool oldShowNames = true;

	//Constructors
	public SettingsScreen(){
        guiSize = new Rect(Screen.width * 0.125f, Screen.height * 0.125f, Screen.width * 0.75f, Screen.height * 0.75f);
    }
	public SettingsScreen(Rect guiRect):this(){	guiSize = guiRect;	}

	
	public void setGUIRect(Rect r){	guiSize=r;	}
	
	
	public void draw(){
        oldIndividual = Settings.RootIndividualId;
        oldMaxGenerations = Settings.MaxDepth;
        oldShowNames = Settings.ShowNames;

		if(guiSkin){
			oldSkin = GUI.skin;
			GUI.skin = guiSkin;
		}
		GUILayout.BeginArea(guiSize);
		GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Imported Data");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Individuals: {0}", AncestryData.gedcomIndividuals.Values.Count) );
        GUILayout.Label(string.Format("Families: {0}", AncestryData.gedcomFamilies.Values.Count) );
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        bool importClicked = GUILayout.Button("Import New Data");
        GUILayout.EndHorizontal();		
        GUILayout.EndVertical();
		
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Root Individual");
        Settings.RootIndividualId = GUILayout.TextField(Settings.RootIndividualId);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("");
        GUILayout.Label(AncestryData.GenerateName("@" + Settings.RootIndividualId + "@"));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Generations");
        Settings.MaxDepth = Int32.Parse(GUILayout.TextField(Settings.MaxDepth.ToString()));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        Settings.ShowDescentLines = GUILayout.Toggle(Settings.ShowDescentLines, "Draw Descent Lines");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        Settings.ShowMarriageLines = GUILayout.Toggle(Settings.ShowMarriageLines, "Draw Marriage Lines");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        Settings.ShowNames = GUILayout.Toggle(Settings.ShowNames, "Draw Names");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        bool exitClicked = GUILayout.Button("Exit");
        bool loadClicked = GUILayout.Button("Load");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        visible = true;
		if(guiSkin){GUI.skin = oldSkin;}

        if (exitClicked)
            Application.Quit();

		if (importClicked)
		{
			Settings.SaveSettings();
			AncestryWeb.ancestryState = AncestryWeb.AncestryState.ImportScreen;
		}
		
        if (loadClicked)
        {
            Settings.SaveSettings();
            if (Settings.RootIndividualId != oldIndividual || Settings.ShowNames != oldShowNames || Settings.MaxDepth != oldMaxGenerations || AncestryWeb.loadedObjects == false)
                AncestryWeb.ancestryState = AncestryWeb.AncestryState.InitialisingData;
            else
                AncestryWeb.ancestryState = AncestryWeb.AncestryState.Main;
        }
		
	}
	


	//to string
	public override string ToString(){
		return "Name: "+name+"\nVisible: "+isVisible.ToString()+"\nGUI Size: "+guiSize.ToString();
	}
}
