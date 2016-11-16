using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GedcomLib;
using System;
using Assets;

public class AncestryWeb : MonoBehaviour
{
    public static string GedcomFilename;
    public static AncestryState ancestryState = AncestryState.Settings;

    private GameObject[] individualSpheres;

    public static bool loadedObjects = false;
    private string loadingText = "Loading...";

    Loader loader = new Loader();
    SettingsScreen settingsScreen = new SettingsScreen();

	public GUISkin[] skins;
	public Texture2D file,folder,back,drive;
	
	//initialize file browser
	FileBrowser fb;


    public enum AncestryState
    {
        Settings,
		ImportScreen,
		ImportingData,
        InitialisingData,
        Main
    }

    private void CreateGameObjects()
    {
        individualSpheres = new GameObject[AncestryData.ancestorGameData.Values.Count()];
		
		int i = 0;
		foreach(IndividualSphereData data in AncestryData.ancestorGameData.Values)
        {
			individualSpheres[i] = (GameObject)Instantiate(Resources.Load(data.ObjectType), data.Position, Quaternion.identity);
            individualSpheres[i].transform.GetChild(0).GetComponent<Renderer>().material.color = data.MaterialColor;
            individualSpheres[i].transform.localScale = new Vector3(data.SphereRadius, data.SphereRadius, data.SphereRadius);

			if (Settings.ShowNames)
				individualSpheres[i].transform.GetChild(1).GetComponent<TextMesh>().text = data.Text;
			else
				individualSpheres[i].transform.GetChild(1).GetComponent<TextMesh>().text = "";
			
			if (data.SphereRadius > 1)
				individualSpheres[i].transform.GetChild(1).transform.localScale = new Vector3(1f / data.SphereRadius, 1f / data.SphereRadius, 1f / data.SphereRadius);
            
			individualSpheres[i].tag = "Individual";
            individualSpheres[i].GetComponent<IndividualSphere>().individualId = data.Id;

        }
    }

	private void DeleteGameObjects()
	{
		foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Individual"))
			GameObject.DestroyImmediate(individualSphere);
		foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Highlighted"))
			GameObject.DestroyImmediate(individualSphere);
	}

    public static void ShowSettings()
    {
       
        ancestryState = AncestryState.Settings;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Start()
    {
        Settings.LoadSettings();
        ancestryState = AncestryState.Settings;

		if (string.IsNullOrEmpty(Settings.CurrentFolder))
            fb = new FileBrowser();
        else
            fb = new FileBrowser(Settings.CurrentFolder);

        fb.fileTexture = file; 
		fb.directoryTexture = folder;
		fb.backTexture = back;
		fb.driveTexture = drive;
		fb.showSearch = true;
		fb.searchRecursively = true;
		
    }


    private IEnumerator InitMain()
    {
        loadingText = "Initialising ancestors...";
        yield return new WaitForSeconds(0.25f);
        loadedObjects = false;
		DeleteGameObjects();
        AncestryData.InitialiseAncestors();
        CreateGameObjects();
        ancestryState = AncestryState.Main;
        loadedObjects = true;
        StopCoroutine("InitMain");
    }
	
	private IEnumerator ImportData()
    {
        loadingText = "Importing Gedcom...";
        yield return new WaitForSeconds(0.25f);
        loadedObjects = false;
		DeleteGameObjects();
		AncestryData.ImportGedcom(GedcomFilename);
        ancestryState = AncestryState.Settings;
        StopCoroutine("ImportData");
    }
	        

    void OnGUI()
    {
        if (ancestryState == AncestryState.Settings)
        {
            settingsScreen.draw();
        }
		else if (ancestryState == AncestryState.ImportScreen)
        {
            if (fb.draw())
            {
                if (fb.outputFile == null)
                {
                    ancestryState = AncestryState.Settings
                }
                else
                {
                    AncestryWeb.GedcomFilename = fb.outputFile.FullName;
                    Settings.CurrentFolder = fb.outputFile.Directory.FullName;
                    Settings.SaveSettings();
                    ancestryState = AncestryState.ImportingData;
                }
            }
        }
		else if (ancestryState == AncestryState.ImportingData)
        {
            loader.draw(loadingText);
            StartCoroutine("ImportData");
        }
		else if (ancestryState == AncestryState.InitialisingData)
        {
            loader.draw(loadingText);
            StartCoroutine("InitMain");
        }
        else if (ancestryState == AncestryState.Main)
        {
            if (AncestryData.selectedIndividualId != null && AncestryData.selectedIndividual.HasValue)
            {
                GUILayout.BeginArea(new Rect(10f, 10f, Screen.width * 0.3f, Screen.height - 20f));
                GUILayout.BeginVertical("box");
                GUILayout.Label(AncestryData.selectedIndividual.Value.FullSummary);
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

    }
}