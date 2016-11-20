using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GedcomLib;
using System;
using Assets;
using System.Text;
using System.IO;

public class AncestryData
{
	public static Dictionary<string, GedcomIndividual> gedcomIndividuals;
    public static Dictionary<string, GedcomFamily> gedcomFamilies;
	
    public static Dictionary<string, AncestorIndividual> ancestors = new Dictionary<string, AncestorIndividual>();
    public static Dictionary<string, IndividualSphereData> ancestorGameData = new Dictionary<string, IndividualSphereData>();
    public static List<Vector3[]> descentMaleLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> descentFemaleLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> marriageLineVectors = new List<Vector3[]>();
    public static string selectedIndividualId = null;
    public static AncestorIndividual selectedIndividual = null;
	
    private static Dictionary<int, List<AncestorIndividual>> optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
    private static Dictionary<int, int> ancestorGenerationCount = new Dictionary<int, int>();
	
	private static int highestDepth = 0;
	
	private static string GedcomDataFilename = "GedcomData.dat";
	private static string ProcessedDataFilename = "GameData.data";
		
    private static void ProcessAncestor(string individualId, string childId, long ahnentafelNumber, int depth)
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

            foreach (GedcomFamily family in gedcomFamilies.Values)
            {
                if (family.Children.Contains(individualId))
                {
                    individual.FatherId = family.HusbandId;
                    individual.MotherId = family.WifeId;
                    break;
                }
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

    private static void IncrementAppearance(string individualId, string childId, long ahnentafelNumber, int depth)
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
	
	private static void CalculateAncestorCountPerGenerationDictionary()
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
	
    private static void CalculateSummaryData()
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

            StringBuilder sb = new StringBuilder();
			sb.Append(individual.Id.Replace("@", "") + "\r\n");
			sb.Append("Ahnentafel Number: " + individual.AhnentafelNumber.ToString() + "\r\n");
			sb.Append(individual.SummaryName + "\r\n");
			if (!string.IsNullOrEmpty(individual.SummaryBirthDate))
				sb.Append("\r\n" + individual.SummaryBirthDate);
			if (!string.IsNullOrEmpty(individual.SummaryDeathDate))
				sb.Append("\r\n" + individual.SummaryDeathDate);

			sb.Append("\r\n\r\n" + "Father: " + individual.SummaryFatherName);
			sb.Append("\r\n" + "Mother: " + individual.SummaryMotherName);
			sb.Append("\r\n\r\n" + "Lines of Descent: " + individual.AppearanceCount.ToString());

			foreach (KeyValuePair<string,string> spouseSummary in individual.SummarySpouse)
			{
				sb.Append("\r\n\r\nSpouse: " + spouseSummary.Value);
				if (individual.SummaryMarriage.ContainsKey(spouseSummary.Key))
				{
					sb.Append("\r\n" + individual.SummaryMarriage[spouseSummary.Key]);
				}
				if (individual.SummaryChildren.ContainsKey(spouseSummary.Key)) {
					sb.Append("\r\n  Children:");
					foreach (string childSummary in individual.SummaryChildren[spouseSummary.Key])
					{
						sb.Append("\r\n  - " + childSummary); 
					}
				}
			}
			individual.FullSummary = sb.ToString();
			
            ancestors[individualId] = individual;
        }
    }
	
	private static string ProcessDate(string date, bool onlyYear)
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

    private static string GenerateBirthDeathDate(AncestorIndividual individual, bool onlyYear)
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

    private static string GenerateBirthDeathDate(GedcomIndividual individual, bool onlyYear)
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
	
	public static void ImportGedcom(string gedcomFilename)
    {
        GedcomParser parser = new GedcomParser();
        parser.Parse(gedcomFilename);
        gedcomFamilies = parser.gedcomFamilies;
        gedcomIndividuals = parser.gedcomIndividuals;
    }
	
    public static void InitialiseAncestors()
    {
        ancestors = new Dictionary<string, AncestorIndividual>();
        ancestorGameData = new Dictionary<string, IndividualSphereData>();
        optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
        ancestorGenerationCount = new Dictionary<int, int>();

        ProcessAncestor("@" + Settings.RootIndividualId + "@", string.Empty, 1, 0);
        CalculateSummaryData();
        CalculateAncestorCountPerGenerationDictionary();
		CalculateAncestorGameObjectData();
    }
	
	public static void CalculateAncestorGameObjectData()
    {
        descentMaleLineVectors = new List<Vector3[]>();
        descentFemaleLineVectors = new List<Vector3[]>();
        marriageLineVectors = new List<Vector3[]>();

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
				IndividualSphereData data = new IndividualSphereData(individual.Id);
				
				data.Position = new Vector3((float)(radius * Math.Cos(angle)), individual.HighestGeneration * 8 * Settings.ScaleFactor, (float)(radius * Math.Sin(angle)));
                if (string.IsNullOrEmpty(individual.FatherId) && string.IsNullOrEmpty(individual.MotherId))
					data.ObjectType = "IndividualEndSphere";
                else
					data.ObjectType = "IndividualSphere";

                if (individual.Sex == "M")
                    data.MaterialColor = Color.blue;
                else
                    data.MaterialColor = Color.red;
				
                data.SphereRadius = (float)(Math.Log10(individual.AppearanceCount) + 1) * Settings.ScaleFactor;

				string name = individual.GivenName;
				if (!string.IsNullOrEmpty(individual.Prefix))
					name += " " + individual.Prefix;
				if (!string.IsNullOrEmpty(individual.Surname))
					name += " " + individual.Surname;
				if (!string.IsNullOrEmpty(individual.Suffix))
					name += "\r\n" + individual.Suffix;

				name += "\r\n" + GenerateBirthDeathDate(individual, true);
				data.Text = name;

                data.Id = individual.Id;
				
				ancestorGameData.Add(data.Id, data);

                angle += angleDelta;
                individualCount++;
            }

        }

        //Update lines

        foreach (AncestorIndividual individual in ancestors.Values)
        {
            if (individual.FatherId != null && ancestors.ContainsKey(individual.FatherId))
            {
                descentMaleLineVectors.Add(new Vector3[2] { ancestorGameData[individual.Id].Position, ancestorGameData[individual.FatherId].Position });
            }
            if (individual.MotherId != null && ancestors.ContainsKey(individual.MotherId))
            {
                descentFemaleLineVectors.Add(new Vector3[2] { ancestorGameData[individual.Id].Position, ancestorGameData[individual.MotherId].Position });
            }
        };

        foreach (GedcomFamily family in gedcomFamilies.Values)
        {
            if (!string.IsNullOrEmpty(family.HusbandId) && ancestors.ContainsKey(family.HusbandId) && !string.IsNullOrEmpty(family.WifeId) && ancestors.ContainsKey(family.WifeId))
            {
                marriageLineVectors.Add(new Vector3[2] { ancestorGameData[family.HusbandId].Position, ancestorGameData[family.WifeId].Position });
            }
        }
    }
	
	public static void SaveGedcomData()
	{
        if (File.Exists(GedcomDataFilename))
            File.Delete(GedcomDataFilename);
        using (BinaryWriter writer = new BinaryWriter(new FileStream(GedcomDataFilename, FileMode.OpenOrCreate)))
        {
            writer.Write(gedcomIndividuals.Values.Count);
            foreach (GedcomIndividual individual in gedcomIndividuals.Values)
            {
                writer.Write(individual.Id);
                individual.WriteToStream(writer);
            }
            writer.Write(gedcomFamilies.Values.Count);
            foreach (GedcomFamily family in gedcomFamilies.Values)
            {
                writer.Write(family.Id);
                family.WriteToStream(writer);
            }
        }
	}

    public static void LoadGedcomData()
    {
        gedcomFamilies = new Dictionary<string, GedcomFamily>();
        gedcomIndividuals = new Dictionary<string, GedcomIndividual>();

        if (!File.Exists(GedcomDataFilename))
            return;

        using (BinaryReader reader = new BinaryReader(new FileStream(GedcomDataFilename, FileMode.OpenOrCreate)))
        {
            int recordCount = reader.ReadInt32();
            for(int i = 0; i < recordCount; i++)
            {
                gedcomIndividuals.Add(reader.ReadString(), new GedcomIndividual(reader));
            }

            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                gedcomFamilies.Add(reader.ReadString(), new GedcomFamily(reader));
            }
        }
    }

    public static void SaveProcessedDataFilename()
    {
        if (File.Exists(ProcessedDataFilename))
            File.Delete(ProcessedDataFilename);
        using (BinaryWriter writer = new BinaryWriter(new FileStream(ProcessedDataFilename, FileMode.OpenOrCreate)))
        {
            writer.Write(ancestors.Values.Count);
            foreach (AncestorIndividual individual in ancestors.Values)
            {
                writer.Write(individual.Id);
                individual.WriteToStream(writer);
            }
        }
    }

    public static void LoadProcessedData()
    {
        Dictionary<string, AncestorIndividual> ancestors = new Dictionary<string, AncestorIndividual>();
        Dictionary<string, IndividualSphereData> ancestorGameData = new Dictionary<string, IndividualSphereData>();
        List<Vector3[]> descentMaleLineVectors = new List<Vector3[]>();
        List<Vector3[]> descentFemaleLineVectors = new List<Vector3[]>();
        List<Vector3[]> marriageLineVectors = new List<Vector3[]>();

        if (!File.Exists(ProcessedDataFilename))
            return;

        using (BinaryReader reader = new BinaryReader(new FileStream(ProcessedDataFilename, FileMode.OpenOrCreate)))
        {
            int recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                ancestors.Add(reader.ReadString(), new AncestorIndividual(reader));
            }
        }
    }
}