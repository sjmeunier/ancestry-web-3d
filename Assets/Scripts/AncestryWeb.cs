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

    public static bool loadedData = false;
    public static bool loadedObjects = false;
    private string loadingText = "Loading...";

    Loader loader = new Loader();
    SettingsScreen settingsScreen = new SettingsScreen();

	public GUISkin[] skins;
	public Texture2D file,folder,back,drive;
	
	//initialize file browser
	FileBrowser fb;

    private float lineWidth = 0.06f;

    public enum AncestryState
    {
        LoadingGedcom,
        LoadingData,
        Settings,
		ImportScreen,
		ImportingData,
        InitialisingData,
        InitialisingObjects,
        UpdatingObjects,
        Main
    }

    private void UpdateVisiblity()
    {
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("DescentMaleLine"))
            line.GetComponentInChildren<MeshRenderer>().enabled = Settings.ShowDescentLines;
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("DescentFemaleLine"))
            line.GetComponentInChildren<MeshRenderer>().enabled = Settings.ShowDescentLines;
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("MarriageLine"))
            line.GetComponentInChildren<MeshRenderer>().enabled = Settings.ShowMarriageLines;

        foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Individual"))
            individualSphere.transform.GetChild(1).GetComponent<TextMesh>().GetComponentInChildren<MeshRenderer>().enabled = Settings.ShowNames;
        foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Highlighted"))
            individualSphere.transform.GetChild(1).GetComponent<TextMesh>().GetComponentInChildren<MeshRenderer>().enabled = Settings.ShowNames;
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

			individualSpheres[i].transform.GetChild(1).GetComponent<TextMesh>().text = data.Text;
			
			if (data.SphereRadius > 1)
				individualSpheres[i].transform.GetChild(1).transform.localScale = new Vector3(1f / data.SphereRadius, 1f / data.SphereRadius, 1f / data.SphereRadius);
            
			individualSpheres[i].tag = "Individual";
            individualSpheres[i].GetComponent<IndividualSphere>().individualId = data.Id;
            individualSpheres[i].transform.localRotation = transform.rotation;

        }

        foreach (var line in AncestryData.descentMaleLineVectors)
        {
            CreateCylinderBetweenPoints(line[0], line[1], lineWidth, "MaleLineCylinder", "DescentMaleLine");
        };

        foreach (var line in AncestryData.descentFemaleLineVectors)
        {
            CreateCylinderBetweenPoints(line[0], line[1], lineWidth, "FemaleLineCylinder", "DescentFemaleLine");
        };
        foreach (var line in AncestryData.marriageLineVectors)
        {
            CreateCylinderBetweenPoints(line[0], line[1], lineWidth, "MarriageLineCylinder", "MarriageLine");
        };
    }

    private void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width, string cylinderName, string tag)
    {
        var offset = end - start;
        var scale = new Vector3(width, offset.magnitude / 2.0f, width);
        var position = start + (offset / 2.0f);


        GameObject cylinder = (GameObject)Instantiate(Resources.Load(cylinderName), position, Quaternion.identity);
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;
        cylinder.tag = tag;

    }


    private void DeleteGameObjects()
	{
		foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Individual"))
			GameObject.DestroyImmediate(individualSphere);
		foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Highlighted"))
			GameObject.DestroyImmediate(individualSphere);

        foreach (GameObject line in GameObject.FindGameObjectsWithTag("DescentMaleLine"))
            GameObject.DestroyImmediate(line);
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("DescentFemaleLine"))
            GameObject.DestroyImmediate(line);
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("MarriageLine"))
            GameObject.DestroyImmediate(line);
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
        AncestryData.gedcomFamilies = new Dictionary<string, GedcomFamily>();
        AncestryData.gedcomIndividuals = new Dictionary<string, GedcomIndividual>();
        Settings.LoadSettings();
        AncestryData.LoadGedcomData();
        AncestryData.LoadProcessedData();
        if (AncestryData.ancestors.Count > 0)
            loadedData = true;
        else
            loadedData = false;

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


    private IEnumerator InitData()
    {
        loadingText = "Initialising ancestors...";
        yield return new WaitForSeconds(0.25f);
        loadedObjects = false;
		DeleteGameObjects();
        AncestryData.InitialiseAncestors();
        ancestryState = AncestryState.InitialisingObjects;
        StopCoroutine("InitData");
    }

    
    private IEnumerator InitObjects()
    {
        loadingText = "Initialising objects...";
        yield return new WaitForSeconds(0.25f);
        loadedObjects = false;
        DeleteGameObjects();
        AncestryData.InitialiseAncestors();
        CreateGameObjects();
        UpdateVisiblity();
        AncestryData.SaveProcessedDataFile();
        ancestryState = AncestryState.Main;
        loadedObjects = true;
        StopCoroutine("InitObjects");
    }

    private IEnumerator UpdateObjects()
    {
        loadingText = "Updating objects...";
        yield return new WaitForSeconds(0.25f);
        UpdateVisiblity();
        ancestryState = AncestryState.Main;
        StopCoroutine("UpdateObjects");
    }

    private IEnumerator ImportData()
    {
        loadingText = "Importing Gedcom...";
        yield return new WaitForSeconds(0.25f);
        loadedObjects = false;
		DeleteGameObjects();
		AncestryData.ImportGedcom(GedcomFilename);
        AncestryData.SaveGedcomData();
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
                    ancestryState = AncestryState.Settings;
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
            StartCoroutine("InitData");
        }
        else if (ancestryState == AncestryState.InitialisingObjects)
        {
            loader.draw(loadingText);
            StartCoroutine("InitObjects");
        }
        else if (ancestryState == AncestryState.UpdatingObjects)
        {
            loader.draw(loadingText);
            StartCoroutine("UpdateObjects");
        }
        else if (ancestryState == AncestryState.Main)
        {
            if (AncestryData.selectedIndividualId != null && AncestryData.selectedIndividual != null)
            {
                GUILayout.BeginArea(new Rect(10f, 10f, Screen.width * 0.3f, Screen.height - 20f));
                GUILayout.BeginVertical("box");
                GUILayout.Label(AncestryData.selectedIndividual.FullSummary);
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

    }
}