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
	
	//Constructors
	public SettingsScreen(){
        guiSize = new Rect(Screen.width * 0.125f, Screen.height * 0.125f, Screen.width * 0.75f, Screen.height * 0.75f);
    }
	public SettingsScreen(Rect guiRect):this(){	guiSize = guiRect;	}

	
	public void setGUIRect(Rect r){	guiSize=r;	}
	
	
	public bool draw(){
		if(guiSkin){
			oldSkin = GUI.skin;
			GUI.skin = guiSkin;
		}
		GUILayout.BeginArea(guiSize);
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Root Individual");
        Settings.RootIndividualId = GUILayout.TextField(Settings.RootIndividualId);
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
        bool buttonClicked = GUILayout.Button("Load");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        visible = true;
		if(guiSkin){GUI.skin = oldSkin;}

        return buttonClicked;
	}
	


	//to string
	public override string ToString(){
		return "Name: "+name+"\nVisible: "+isVisible.ToString()+"\nGUI Size: "+guiSize.ToString();
	}
}

