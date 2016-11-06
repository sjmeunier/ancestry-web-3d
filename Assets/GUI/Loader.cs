using UnityEngine;
using System.IO;

public class Loader {
//public 
	//Optional Parameters
	public string name = "Loader"; 
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
	public Loader(){
        guiSize = new Rect(Screen.width * 0.125f, Screen.height * 0.125f, Screen.width * 0.75f, Screen.height * 0.75f);
    }
	public Loader(Rect guiRect):this(){	guiSize = guiRect;	}

	
	public void setGUIRect(Rect r){	guiSize=r;	}
	
	
	public void draw(string text){
		if(guiSkin){
			oldSkin = GUI.skin;
			GUI.skin = guiSkin;
		}
		GUILayout.BeginArea(guiSize);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.Label(text);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
        visible = true;
		if(guiSkin){GUI.skin = oldSkin;}
	}
	


	//to string
	public override string ToString(){
		return "Name: "+name+"\nVisible: "+isVisible.ToString()+"\nGUI Size: "+guiSize.ToString();
	}
}

