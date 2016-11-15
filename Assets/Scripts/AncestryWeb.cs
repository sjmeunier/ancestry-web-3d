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
    public static string selectedIndividualId = null;
    public static AncestorIndividual? selectedIndividual = null;

    private Dictionary<int, List<AncestorIndividual>> optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
    private Dictionary<int, int> ancestorGenerationCount = new Dictionary<int, int>();
    private GameObject[] individualSpheres;
    private static Dictionary<string, GedcomIndividual> gedcomIndividuals;
    private static Dictionary<string, GedcomFamily> gedcomFamilies;

    public static bool loadedObjects = false;
    private string loadingText = "Loading...";

    Loader loader = new Loader();
    SettingsScreen settingsScreen = new SettingsScreen();

    private int highestDepth = 0;

    public enum AncestryState
    {
        Settings,
        InitialisingData,
        Main
    }

    private void ProcessAncestor(string individualId, string childId, long ahnentafelNumber, int depth)
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
            individual.BirthPlace = gedcomIndividual.BirthPlace.Trim();
            individual.DiedDate = gedcomIndividual.DiedDate.Trim();
            individual.DiedPlace = gedcomIndividual.DiedPlace.Trim();
            individual.AppearanceCount = 1;
            individual.LowestGeneration = depth;
            individual.HighestGeneration = depth;

            individual.AhnentafelNumber = ahnentafelNumber;

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

            ancestors.Add(individualId, individual);
            if (depth <= Settings.MaxDepth)
            {
                if (!string.IsNullOrEmpty(individual.FatherId))
                    ProcessAncestor(individual.FatherId, individualId, 2 * ahnentafelNumber, depth + 1);

                if (!string.IsNullOrEmpty(individual.MotherId))
                    ProcessAncestor(individual.MotherId, individualId, 2 * ahnentafelNumber + 1, depth + 1);
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

    private void CalculateSummaryData()
    {
        List<string> keys = new List<string>(ancestors.Keys);
        foreach (string individualId in keys)
        {
            AncestorIndividual individual = ancestors[individualId];
           
            individual.SummaryName = individual.GivenName;
            if (!string.IsNullOrEmpty(individual.Prefix))
                individual.SummaryName += " " + individual.Prefix;
            if (!string.IsNullOrEmpty(individual.Surname))
                individual.SummaryName += " " + individual.Surname;
            if (!string.IsNullOrEmpty(individual.Suffix))
                individual.SummaryName += " (" + individual.Suffix + ")";

            string born = ProcessDate(individual.BirthDate, false);
            if (born != "?" || !string.IsNullOrEmpty(individual.BirthPlace.Trim()))
                individual.SummaryBirthDate = string.Format("b. {0} {1}", born, individual.BirthPlace).Trim();
            string died = ProcessDate(individual.DiedDate, false);
            if (died != "?" || !string.IsNullOrEmpty(individual.DiedPlace.Trim()))
                individual.SummaryDeathDate = string.Format("d. {0} {1}", died, individual.DiedPlace).Trim();

            if (!string.IsNullOrEmpty(individual.FatherId) && gedcomIndividuals.ContainsKey(individual.FatherId))
            {
                GedcomIndividual father = gedcomIndividuals[individual.FatherId];
                individual.SummaryFatherName = father.GivenName;
                if (!string.IsNullOrEmpty(father.Prefix))
                    individual.SummaryFatherName += " " + father.Prefix;
                if (!string.IsNullOrEmpty(father.Surname))
                    individual.SummaryFatherName += " " + father.Surname;
                if (!string.IsNullOrEmpty(father.Suffix))
                    individual.SummaryFatherName += " (" + father.Suffix + ")";
                individual.SummaryFatherName += " " + GenerateBirthDeathDate(father, true);
            } else
            {
                individual.SummaryFatherName = "Unknown";
            }

            if (!string.IsNullOrEmpty(individual.MotherId) && gedcomIndividuals.ContainsKey(individual.MotherId))
            {
                GedcomIndividual mother = gedcomIndividuals[individual.MotherId];
                individual.SummaryMotherName = mother.GivenName;
                if (!string.IsNullOrEmpty(mother.Prefix))
                    individual.SummaryMotherName += " " + mother.Prefix;
                if (!string.IsNullOrEmpty(mother.Surname))
                    individual.SummaryMotherName += " " + mother.Surname;
                if (!string.IsNullOrEmpty(mother.Suffix))
                    individual.SummaryMotherName += " (" + mother.Suffix + ")";
                individual.SummaryMotherName += " " + GenerateBirthDeathDate(mother, true);
            }
            else
            {
                individual.SummaryMotherName = "Unknown";
            }

            foreach (GedcomFamily family in gedcomFamilies.Values.Where(x => x.WifeId == individual.Id || x.HusbandId == individual.Id))
            {
                string spouseId = (family.WifeId == individual.Id) ? family.HusbandId : family.WifeId;
                if (gedcomIndividuals.ContainsKey(spouseId)) {
                    GedcomIndividual spouse = gedcomIndividuals[spouseId];
                    string summary = spouse.GivenName;
                    if (!string.IsNullOrEmpty(spouse.Prefix))
                        summary += " " + spouse.Prefix;
                    if (!string.IsNullOrEmpty(spouse.Surname))
                        summary += " " + spouse.Surname;
                    if (!string.IsNullOrEmpty(spouse.Suffix))
                        summary += " (" + spouse.Suffix + ")";
                    summary += " " + GenerateBirthDeathDate(spouse, true);
                    individual.SummarySpouse.Add(family.Id, summary);
                } else
                {
                    individual.SummarySpouse.Add(family.Id, "Unknown");
                }

                string married = ProcessDate(family.MarriageDate, false);
                if (married != "?" || !string.IsNullOrEmpty(family.MarriagePlace.Trim()))
                    individual.SummaryMarriage.Add(family.Id, string.Format("m. {0} {1}", married, family.MarriagePlace).Trim());

                HashSet<string> childSummaries = new HashSet<string>();
                foreach (string childId in family.Children)
                {
                    if (!gedcomIndividuals.ContainsKey(childId))
                        continue;

                    GedcomIndividual child = gedcomIndividuals[childId];
                    string summary = child.GivenName;
                    if (!string.IsNullOrEmpty(child.Prefix))
                        summary += " " + child.Prefix;
                    if (!string.IsNullOrEmpty(child.Surname))
                        summary += " " + child.Surname;
                    if (!string.IsNullOrEmpty(child.Suffix))
                        summary += " (" + child.Suffix + ")";
                    summary += " " + GenerateBirthDeathDate(child, true);
                    childSummaries.Add(summary);
                }
                individual.SummaryChildren.Add(family.Id, childSummaries);
            }

            ancestors[individualId] = individual;
        }
    }

    private void ParseGedcom()
    {
        GedcomParser parser = new GedcomParser();
        if (Settings.WebMode)
            parser.ParseWeb(Settings.WebURL);
        else
            parser.Parse(AncestryWeb.GedcomFilename);
        gedcomFamilies = parser.gedcomFamilies;
        gedcomIndividuals = parser.gedcomIndividuals;
    }
    private void InitialiseAncestors()
    {
        ancestors = new Dictionary<string, AncestorIndividual>();
        ancestorPositions = new Dictionary<string, Vector3>();
        decentMaleLineVectors = new List<Vector3[]>();
        decentFemaleLineVectors = new List<Vector3[]>();
        marriageLineVectors = new List<Vector3[]>();
        optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
        ancestorGenerationCount = new Dictionary<int, int>();

        ProcessAncestor("@" + Settings.RootIndividualId + "@", string.Empty, 1, 0);
        CalculateSummaryData();
        CalculateAncestorCountPerGenerationDictionary();

    }

    public static string GenerateName(string individualId)
    {
        if (!gedcomIndividuals.ContainsKey(individualId))
            return string.Empty;

        GedcomIndividual individual = gedcomIndividuals[individualId];
        string name = individual.GivenName;
        if (!string.IsNullOrEmpty(individual.Prefix))
            name += " " + individual.Prefix;
        if (!string.IsNullOrEmpty(individual.Surname))
            name += " " + individual.Surname;
        if (!string.IsNullOrEmpty(individual.Suffix))
            name += " (" + individual.Suffix + ")";
        return name;
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

                if (Settings.ShowNames)
                {
                    string name = individual.GivenName;
                    if (!string.IsNullOrEmpty(individual.Prefix))
                        name += " " + individual.Prefix;
                    if (!string.IsNullOrEmpty(individual.Surname))
                        name += " " + individual.Surname;
                    if (!string.IsNullOrEmpty(individual.Suffix))
                        name += "\r\n" + individual.Suffix;

                    individualSpheres[individualCount].transform.GetChild(1).GetComponent<TextMesh>().text = name + "\r\n" + GenerateBirthDeathDate(individual, true);

                }
                else
                    individualSpheres[individualCount].transform.GetChild(1).GetComponent<TextMesh>().text = "";
                if (sphereRadius > 1)
                    individualSpheres[individualCount].transform.GetChild(1).transform.localScale = new Vector3(1f / sphereRadius, 1f / sphereRadius, 1f / sphereRadius);
                individualSpheres[individualCount].tag = "Individual";
                individualSpheres[individualCount].GetComponent<IndividualSphere>().individualId = individual.Id;

                angle += angleDelta;
                individualCount++;
            }

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
        };

        foreach (GedcomFamily family in gedcomFamilies.Values)
        {
            if (!string.IsNullOrEmpty(family.HusbandId) && ancestors.ContainsKey(family.HusbandId) && !string.IsNullOrEmpty(family.WifeId) && ancestors.ContainsKey(family.WifeId))
            {
                marriageLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[family.HusbandId], AncestryWeb.ancestorPositions[family.WifeId] });
            }
        }
    }

    private string ProcessDate(string date, bool onlyYear)
    {
        if (string.IsNullOrEmpty(date))
        {
            date = "?";
        }
        else
        {
            if (onlyYear)
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
                }
            }
            else
            {
                if (date.Contains("ABT"))
                    date = date.Replace("ABT", "c");
                else if (date.Contains("AFT"))
                    date = date.Replace("AFT", ">");
                else if (date.Contains("BEF"))
                    date = date.Replace("BEF", "<");

                date = date.Replace("JAN", "Jan").Replace("FEB", "Feb").Replace("MAR", "Mar").Replace("APR", "Apr").Replace("MAY", "May").Replace("JUN", "Jun")
                            .Replace("JUL", "Jul").Replace("AUG", "Aug").Replace("SEP", "Sep").Replace("OCT", "Oct").Replace("NOV", "Nov").Replace("DEC", "Dec");
            }
        }

        return date;
    }

    private string GenerateBirthDeathDate(AncestorIndividual individual, bool onlyYear)
    {
        string born = ProcessDate(individual.BirthDate, onlyYear);
        string died = ProcessDate(individual.DiedDate, onlyYear);
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

    private string GenerateBirthDeathDate(GedcomIndividual individual, bool onlyYear)
    {
        string born = ProcessDate(individual.BirthDate, onlyYear);
        string died = ProcessDate(individual.DiedDate, onlyYear);
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
        ancestryState = AncestryState.Settings;
        Settings.LoadSettings();
        ParseGedcom();
    }

    private IEnumerator InitMain()
    {
        loadingText = "Initialising ancestors...";
        yield return new WaitForSeconds(0.25f);
        InitialiseAncestors();
        CreateAncestorObjects();
        ancestryState = AncestryState.Main;
        loadedObjects = true;
        StopCoroutine("InitMain");
    }

    void OnGUI()
    {
        if (ancestryState == AncestryState.Settings)
        {
            settingsScreen.draw();
        }
        else if (ancestryState == AncestryState.InitialisingData)
        {
            foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Individual"))
                GameObject.DestroyImmediate(individualSphere);
            foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Highlighted"))
                GameObject.DestroyImmediate(individualSphere);

            loader.draw(loadingText);
            StartCoroutine("InitMain");
        }
        else if (ancestryState == AncestryState.Main)
        {
            if (selectedIndividualId != null && selectedIndividual.HasValue)
            {

                string summary = selectedIndividual.Value.Id.Replace("@", "") + "\r\n";
                summary += "Ahnentafel Number: " + selectedIndividual.Value.AhnentafelNumber.ToString() + "\r\n";
                summary += selectedIndividual.Value.SummaryName + "\r\n";
                if (!string.IsNullOrEmpty(selectedIndividual.Value.SummaryBirthDate))
                    summary += "\r\n" + selectedIndividual.Value.SummaryBirthDate;
                if (!string.IsNullOrEmpty(selectedIndividual.Value.SummaryDeathDate))
                    summary += "\r\n" + selectedIndividual.Value.SummaryDeathDate;

                summary += "\r\n\r\n" + "Father: " + selectedIndividual.Value.SummaryFatherName;
                summary += "\r\n" + "Mother: " + selectedIndividual.Value.SummaryMotherName;
                summary += "\r\n\r\n" + "Lines of Descent: " + selectedIndividual.Value.AppearanceCount.ToString();

                foreach (KeyValuePair<string,string> spouseSummary in selectedIndividual.Value.SummarySpouse)
                {
                    summary += "\r\n\r\nSpouse: " + spouseSummary.Value;
                    if (selectedIndividual.Value.SummaryMarriage.ContainsKey(spouseSummary.Key))
                    {
                        summary += "\r\n" + selectedIndividual.Value.SummaryMarriage[spouseSummary.Key];
                    }
                    if (selectedIndividual.Value.SummaryChildren.ContainsKey(spouseSummary.Key)) {
                        summary += "\r\n  Children:";
                        foreach (string childSummary in selectedIndividual.Value.SummaryChildren[spouseSummary.Key])
                        {
                            summary += "\r\n  - " + childSummary; 
                        }
                    }
                }

                GUILayout.BeginArea(new Rect(10f, 10f, Screen.width * 0.3f, Screen.height - 20f));
                GUILayout.BeginVertical("box");
                GUILayout.Label(summary);
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }
    }
}