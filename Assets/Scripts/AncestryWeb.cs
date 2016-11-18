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

    Mesh descentMaleLineMesh;
    public Material descentMaleLineMaterial;
    Mesh descentFemaleLineMesh;
    public Material descentFemaleLineMaterial;
    Mesh marriageLineMesh;
    public Material marriageLineMaterial;

    private float lineWidth = 0.06f;

    public enum AncestryState
    {
        Settings,
		ImportScreen,
		ImportingData,
        InitialisingData,
        InitialisingObjects,
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

        descentMaleLineMesh = new Mesh();
        foreach (var line in AncestryData.descentMaleLineVectors)
        {
            AddLine(descentMaleLineMesh, MakeQuad(line[0], line[1], lineWidth), false);
        };

        descentFemaleLineMesh = new Mesh();
        foreach (var line in AncestryData.descentFemaleLineVectors)
        {
            AddLine(descentFemaleLineMesh, MakeQuad(line[0], line[1], lineWidth), false);
        };

        marriageLineMesh = new Mesh();
        foreach (var line in AncestryData.marriageLineVectors)
        {
            AddLine(marriageLineMesh, MakeQuad(line[0], line[1], lineWidth), false);
        };

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
        if (ancestryState == AncestryState.Main && loadedObjects == true)
        {
            if (Settings.ShowDescentLines)
            {
                Graphics.DrawMesh(descentMaleLineMesh, transform.localToWorldMatrix, descentMaleLineMaterial, 0);
                Graphics.DrawMesh(descentFemaleLineMesh, transform.localToWorldMatrix, descentFemaleLineMaterial, 0);
            }
            if (Settings.ShowMarriageLines)
                Graphics.DrawMesh(marriageLineMesh, transform.localToWorldMatrix, marriageLineMaterial, 0);
        }
    }

    void Start()
    {
        AncestryData.gedcomFamilies = new Dictionary<string, GedcomFamily>();
        AncestryData.gedcomIndividuals = new Dictionary<string, GedcomIndividual>();
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

        descentMaleLineMesh = new Mesh();
        descentFemaleLineMesh = new Mesh();
        marriageLineMesh = new Mesh();

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
        ancestryState = AncestryState.Main;
        loadedObjects = true;
        StopCoroutine("InitObjects");
    }

    public Vector3[] MakeQuad(Vector3 s, Vector3 e, float w)
    {
        w = w / 2;
        Vector3[] q = new Vector3[4];

        Vector3 n = Vector3.Cross(s, e);
        Vector3 l = Vector3.Cross(n, e - s);
        l.Normalize();

        q[0] = transform.InverseTransformPoint(s + l * w);
        q[1] = transform.InverseTransformPoint(s + l * -w);
        q[2] = transform.InverseTransformPoint(e + l * w);
        q[3] = transform.InverseTransformPoint(e + l * -w);

        return q;
    }

    void AddLine(Mesh m, Vector3[] quad, bool tmp)
    {
        int vl = m.vertices.Length;

        Vector3[] vs = m.vertices;
        if (!tmp || vl == 0) vs = resizeVertices(vs, 4);
        else vl -= 4;

        vs[vl] = quad[0];
        vs[vl + 1] = quad[1];
        vs[vl + 2] = quad[2];
        vs[vl + 3] = quad[3];

        int tl = m.triangles.Length;

        int[] ts = m.triangles;
        if (!tmp || tl == 0) ts = resizeTriangles(ts, 6);
        else tl -= 6;
        ts[tl] = vl;
        ts[tl + 1] = vl + 1;
        ts[tl + 2] = vl + 2;
        ts[tl + 3] = vl + 1;
        ts[tl + 4] = vl + 3;
        ts[tl + 5] = vl + 2;

        m.vertices = vs;
        m.triangles = ts;
        m.RecalculateBounds();
    }

    Vector3[] resizeVertices(Vector3[] ovs, int ns)
    {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    int[] resizeTriangles(int[] ovs, int ns)
    {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
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