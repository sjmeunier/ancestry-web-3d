using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GedcomLib;
using System;
using Assets;

public class AncestryWeb : MonoBehaviour
{
    public static Dictionary<string, AncestorIndividual> ancestors = new Dictionary<string, AncestorIndividual>();
    public static Dictionary<string, Vector3> ancestorPositions = new Dictionary<string, Vector3>();
    public static List<Vector3[]> decentMaleLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> decentFemaleLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> marriageLineVectors = new List<Vector3[]>();
    public static string GedcomFilename;
    public static AncestryState ancestryState = AncestryState.Settings;

    private Dictionary<int, List<AncestorIndividual>> optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
    private Dictionary<int, int> ancestorGenerationCount = new Dictionary<int, int>();
    private GameObject[] individualSpheres;
    private Dictionary<string, GedcomIndividual> gedcomIndividuals;
    private Dictionary<string, GedcomFamily> gedcomFamilies;

    Loader loader = new Loader();
    SettingsScreen settingsScreen = new SettingsScreen();

    private int highestDepth = 0;

    public enum AncestryState
    {
        Settings,
        Loading,
        Main
    }

    private void ProcessAncestor(string individualId, string spouseId, string childId, long ahnentafelNumber, int depth)
    {
        if (ancestors.ContainsKey(individualId))
        {
            IncrementAppearance(individualId, childId, ahnentafelNumber, depth);
        }
        else
        {
            highestDepth = Math.Max(depth, highestDepth);

            AncestorIndividual individual = new AncestorIndividual(individualId);
            GedcomIndividual gedcomIndividual = gedcomIndividuals[individualId];

            individual.GivenName = gedcomIndividual.GivenName.Trim();
            individual.Surname = gedcomIndividual.Surname.Trim();
            individual.Prefix = gedcomIndividual.Prefix.Trim();
            individual.Suffix = gedcomIndividual.Suffix.Trim();
            individual.Sex = gedcomIndividual.Sex.Trim();
            individual.BirthDate = gedcomIndividual.BirthDate.Trim();
            individual.DiedDate = gedcomIndividual.DiedDate.Trim();
            individual.AppearanceCount = 1;
            individual.LowestGeneration = depth;
            individual.HighestGeneration = depth;

            individual.AhnentafelNumber = ahnentafelNumber;

            if (!string.IsNullOrEmpty(childId))
                individual.ChildrenIds.Add(childId);

            GedcomFamily? gedcomFamily = null;
            foreach (GedcomFamily family in gedcomFamilies.Values)
            {
                if (family.Children.Contains(individualId))
                {
                    gedcomFamily = family;
                    break;
                }
            }
            if (gedcomFamily != null)
            {
                individual.FatherId = gedcomFamily.Value.HusbandId;
                individual.MotherId = gedcomFamily.Value.WifeId;
            }
            individual.SpouseId = spouseId;

            ancestors.Add(individualId, individual);
            if (depth <= Settings.MaxDepth)
            {
                if (!string.IsNullOrEmpty(individual.FatherId))
                    ProcessAncestor(individual.FatherId, individual.MotherId, individualId, 2 * ahnentafelNumber, depth + 1);

                if (!string.IsNullOrEmpty(individual.MotherId))
                    ProcessAncestor(individual.MotherId, individual.FatherId, individualId, 2 * ahnentafelNumber + 1, depth + 1);
            }
        }
    }

    private void IncrementAppearance(string individualId, string childId, long ahnentafelNumber, int depth)
    {
        if (ancestors.ContainsKey(individualId))
        {
            highestDepth = Math.Max(depth, highestDepth);

            AncestorIndividual individual = ancestors[individualId];
            individual.LowestGeneration = Math.Min(individual.LowestGeneration, depth);
            if (depth > individual.HighestGeneration)
            {
                individual.AhnentafelNumber = ahnentafelNumber;
            }
            individual.AppearanceCount++;

            if (!string.IsNullOrEmpty(childId))
                individual.ChildrenIds.Add(childId);

            ancestors[individualId] = individual;

            if (!string.IsNullOrEmpty(individual.FatherId))
                IncrementAppearance(individual.FatherId, individualId, 2 * ahnentafelNumber, depth + 1);

            if (!string.IsNullOrEmpty(individual.MotherId))
                IncrementAppearance(individual.MotherId, individualId, 2 * ahnentafelNumber + 1, depth + 1);

        }
    }

    private void CalculateAncestorCountPerGenerationDictionary()
    {
        for (int i = 0; i <= highestDepth; i++)
        {
            optimizedAncestors.Add(i, ancestors.Values.Where(x => x.HighestGeneration == i).ToList());
        }

        for (int i = 0; i <= highestDepth; i++)
        {
            ancestorGenerationCount.Add(i, optimizedAncestors[i].Count());
        }

    }

    private void InitialiseAncestors(string filename)
    {
        GedcomParser parser = new GedcomParser();
        parser.Parse(filename);
        gedcomFamilies = parser.gedcomFamilies;
        gedcomIndividuals = parser.gedcomIndividuals;

        ProcessAncestor("@" + Settings.RootIndividualId + "@", string.Empty, string.Empty, 1, 0);
        CalculateAncestorCountPerGenerationDictionary();

    }

    private void CreateAncestorObjects()
    {
        individualSpheres = new GameObject[ancestors.Count()];
        float angle = 0, angleDelta;

        int individualCount = 0;

        //Draw spheres
        for (int i = 0; i <= highestDepth; i++)
        {
            angleDelta = (float)((Math.PI * 2) / optimizedAncestors[i].Count());
            angle = angleDelta / 2;

            int ancestorCount = ancestorGenerationCount[i];
            float radius = (5f * (float)ancestorCount) / (2f * (float)Math.PI) * Settings.ScaleFactor;
            if (i == 0)
                radius = 0;

            foreach (AncestorIndividual individual in optimizedAncestors[i].OrderBy(x => x.AhnentafelNumber))
            {
                ancestorPositions.Add(individual.Id, new Vector3((float)(radius * Math.Cos(angle)), individual.HighestGeneration * 8 * Settings.ScaleFactor, (float)(radius * Math.Sin(angle))));
                if (string.IsNullOrEmpty(individual.FatherId) && string.IsNullOrEmpty(individual.MotherId))
                {
                    individualSpheres[individualCount] = (GameObject)Instantiate(Resources.Load("IndividualEndSphere"), new Vector3((float)(radius * Math.Cos(angle)), individual.HighestGeneration * 8 * Settings.ScaleFactor, (float)(radius * Math.Sin(angle))), Quaternion.identity);
                }
                else
                {
                    individualSpheres[individualCount] = (GameObject)Instantiate(Resources.Load("IndividualSphere"), new Vector3((float)(radius * Math.Cos(angle)), individual.HighestGeneration * 8 * Settings.ScaleFactor, (float)(radius * Math.Sin(angle))), Quaternion.identity);
                }
                if (individual.Sex == "M")
                    individualSpheres[individualCount].transform.GetChild(0).GetComponent<Renderer>().material.color = Color.blue;
                else
                    individualSpheres[individualCount].transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                float sphereRadius = (float)(Math.Log10(individual.AppearanceCount) + 1) * Settings.ScaleFactor;
                individualSpheres[individualCount].transform.localScale = new Vector3(sphereRadius, sphereRadius, sphereRadius);

                individualSpheres[individualCount].transform.GetChild(1).GetComponent<TextMesh>().text = individual.GivenName + " " + individual.Surname + (!string.IsNullOrEmpty(individual.Suffix) ? "\r\n" + individual.Suffix : "") + "\r\n" + GenerateBirthDeathDate(individual);
                if (sphereRadius > 1)
                    individualSpheres[individualCount].transform.GetChild(1).transform.localScale = new Vector3(1f / sphereRadius, 1f / sphereRadius, 1f / sphereRadius);
                individualSpheres[individualCount].tag = "Individual";
                individualSpheres[individualCount].GetComponent<IndividualSphere>().individualId = individual.Id;

                angle += angleDelta;
            }
            individualCount++;
        }

        //Update lines

        foreach (AncestorIndividual individual in ancestors.Values)
        {
            if (individual.FatherId != null && ancestors.ContainsKey(individual.FatherId))
            {
                decentMaleLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[individual.Id], AncestryWeb.ancestorPositions[individual.FatherId] });
            }
            if (individual.MotherId != null && ancestors.ContainsKey(individual.MotherId))
            {
                decentFemaleLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[individual.Id], AncestryWeb.ancestorPositions[individual.MotherId] });
            }
            if (individual.SpouseId != null && ancestors.ContainsKey(individual.SpouseId))
            {
                marriageLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[individual.Id], AncestryWeb.ancestorPositions[individual.SpouseId] });
            }
        };
    }

    private string ProcessDate(string date)
    {
        if (string.IsNullOrEmpty(date))
        {
            date = "?";
        }
        else
        {
            string[] dateArr = date.Split(new char[] { ' ' });
            if (dateArr.Length > 1)
            {
                date = "";
                if (dateArr[0] == "ABT")
                    date = "c";
                else if (dateArr[0] == "AFT")
                    date = ">";
                else if (dateArr[0] == "BEF")
                    date = "<";
                date += dateArr[dateArr.Length - 1];

                int year = 0;
                Int32.TryParse(dateArr[dateArr.Length - 1], out year);
                if (year > 2008)
                    date = "?";
            }
        }

        return date;
    }

    private string GenerateBirthDeathDate(AncestorIndividual individual)
    {
        string born = ProcessDate(individual.BirthDate);
        string died = ProcessDate(individual.DiedDate);
        if (born != "?" || died != "?")
        {
            if (born == "?")
                return string.Format("(d.{0})", died);
            else if (died == "?")
                return string.Format("(b.{0})", born);
            else
                return string.Format("(b.{0}, d.{1})", born, died);
        }
        return string.Empty;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Start()
    {

    }

    void OnGUI()
    {
        if (ancestryState == AncestryState.Settings)
        {
            if (settingsScreen.draw())
            {
                ancestryState = AncestryState.Loading;
            }

        }
        else if (ancestryState == AncestryState.Loading)
        {
            loader.draw();
            InitialiseAncestors(GedcomFilename);
            CreateAncestorObjects();
            ancestryState = AncestryState.Main;
        }
    }
}