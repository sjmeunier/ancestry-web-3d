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

    private static string oldIndividual = "";
    private static string oldIndividual2 = "";
    private static bool oldIsDualMode = false;
    private static int oldMaxGenerations = 0;

	//Constructors
	public SettingsScreen(){
        guiSize = new Rect(Screen.width * 0.125f, Screen.height * 0.125f, Screen.width * 0.75f, Screen.height * 0.75f);
    }
	public SettingsScreen(Rect guiRect):this(){	guiSize = guiRect;	}

	
	public void setGUIRect(Rect r){	guiSize=r;	}


    public void draw() {

        if (guiSkin) {
            oldSkin = GUI.skin;
            GUI.skin = guiSkin;
        }
        GUILayout.BeginArea(guiSize);
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Imported Data");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Individuals: {0}", AncestryGameData.gedcomIndividuals.Values.Count));
        GUILayout.Label(string.Format("Families: {0}", AncestryGameData.gedcomFamilies.Values.Count));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (Settings.webMode == true)
            GUILayout.Label("Web demo mode");
        else if (Settings.LastImportDate.Ticks == 0 || AncestryGameData.gedcomIndividuals.Values.Count == 0)
            GUILayout.Label("No imported data found");
        else
            GUILayout.Label(string.Format("Last imported {0}: {1}", Settings.LastImportDate.Date.ToShortDateString(), Settings.LastImportFilename));
        GUILayout.EndHorizontal();
        bool importClicked = false;
        if (Settings.webMode == false)
        {
            GUILayout.BeginHorizontal();
            importClicked = GUILayout.Button("Import New Data");
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        Settings.IsDualMode = GUILayout.Toggle(Settings.IsDualMode, "Show two root individuals");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (Settings.IsDualMode)
            GUILayout.Label("Root Individual 1");
        else
            GUILayout.Label("Root Individual");
        Settings.RootIndividualId = GUILayout.TextField(Settings.RootIndividualId);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("");
        GUILayout.Label(AncestryUtil.GenerateName("@" + Settings.RootIndividualId + "@"));
        GUILayout.EndHorizontal();
        if (Settings.IsDualMode)
        { 
            GUILayout.BeginHorizontal();
            GUILayout.Label("Root Individual 2");
            Settings.RootIndividualId2 = GUILayout.TextField(Settings.RootIndividualId2);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            GUILayout.Label(AncestryUtil.GenerateName("@" + Settings.RootIndividualId2 + "@"));
            GUILayout.EndHorizontal();
        }
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
            if (AncestryWeb.loadedObjects == false)
            {
                if (AncestryWeb.loadedData == true)
                    AncestryWeb.ancestryState = AncestryWeb.AncestryState.InitialisingObjects;
                else
                    AncestryWeb.ancestryState = AncestryWeb.AncestryState.InitialisingData;
                AncestryWeb.loadedData = false;
            }
            else if (Settings.RootIndividualId != oldIndividual || Settings.MaxDepth != oldMaxGenerations || Settings.RootIndividualId2 != oldIndividual2 || Settings.IsDualMode != oldIsDualMode)
            {
                AncestryWeb.loadedData = false;
                AncestryWeb.ancestryState = AncestryWeb.AncestryState.InitialisingData;
            }
            else
                AncestryWeb.ancestryState = AncestryWeb.AncestryState.UpdatingObjects;

            SettingsScreen.oldIndividual = Settings.RootIndividualId;
            SettingsScreen.oldIndividual2 = Settings.RootIndividualId2;
            SettingsScreen.oldIsDualMode = Settings.IsDualMode;
            SettingsScreen.oldMaxGenerations = Settings.MaxDepth;
        }
		
	}
	


	//to string
	public override string ToString(){
		return "Name: "+name+"\nVisible: "+isVisible.ToString()+"\nGUI Size: "+guiSize.ToString();
	}
}
