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
    public static Dictionary<string, AncestorIndividual> ancestors = new Dictionary<string, AncestorIndividual>();
	
    private static Dictionary<int, List<AncestorIndividual>> optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
    private static Dictionary<int, int> ancestorGenerationCount = new Dictionary<int, int>();
	
	private static int highestDepth = 0;
	
	private static string GedcomDataFilename = "GedcomData.dat";
	private static string ProcessedDataFilename = "GameData.dat";
		
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
            GedcomIndividual gedcomIndividual = AncestryGameData.gedcomIndividuals[individualId];

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

			individual.CountryCode = AncestryUtil.GetCountryCodeForIndividual(individualId);
			
            foreach (GedcomFamily family in AncestryGameData.gedcomFamilies.Values)
            {
                if (family.Children.Contains(individualId))
                {
                    individual.FatherId = family.HusbandId;
                    individual.MotherId = family.WifeId;
                    break;
                }
            }

            ancestors.Add(individualId, individual);
            if (depth < Settings.MaxDepth)
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

            individual.SummaryRelationship = AncestryUtil.CalculateRelationship(individual.LowestGeneration, individual.Sex.ToUpper() == "M");

            string born = AncestryUtil.ProcessDate(individual.BirthDate, false);
            if (born != "?" || !string.IsNullOrEmpty(individual.BirthPlace.Trim()))
                individual.SummaryBirthDate = string.Format("b. {0} {1}", born, individual.BirthPlace).Trim();
            string died = AncestryUtil.ProcessDate(individual.DiedDate, false);
            if (died != "?" || !string.IsNullOrEmpty(individual.DiedPlace.Trim()))
                individual.SummaryDeathDate = string.Format("d. {0} {1}", died, individual.DiedPlace).Trim();

            if (!string.IsNullOrEmpty(individual.FatherId) && AncestryGameData.gedcomIndividuals.ContainsKey(individual.FatherId))
            {
                GedcomIndividual father = AncestryGameData.gedcomIndividuals[individual.FatherId];
                individual.SummaryFatherName = father.GivenName;
                if (!string.IsNullOrEmpty(father.Prefix))
                    individual.SummaryFatherName += " " + father.Prefix;
                if (!string.IsNullOrEmpty(father.Surname))
                    individual.SummaryFatherName += " " + father.Surname;
                if (!string.IsNullOrEmpty(father.Suffix))
                    individual.SummaryFatherName += " (" + father.Suffix + ")";
                individual.SummaryFatherName += " " + AncestryUtil.GenerateBirthDeathDate(father, true);
            } else
            {
                individual.SummaryFatherName = "Unknown";
            }

            if (!string.IsNullOrEmpty(individual.MotherId) && AncestryGameData.gedcomIndividuals.ContainsKey(individual.MotherId))
            {
                GedcomIndividual mother = AncestryGameData.gedcomIndividuals[individual.MotherId];
                individual.SummaryMotherName = mother.GivenName;
                if (!string.IsNullOrEmpty(mother.Prefix))
                    individual.SummaryMotherName += " " + mother.Prefix;
                if (!string.IsNullOrEmpty(mother.Surname))
                    individual.SummaryMotherName += " " + mother.Surname;
                if (!string.IsNullOrEmpty(mother.Suffix))
                    individual.SummaryMotherName += " (" + mother.Suffix + ")";
                individual.SummaryMotherName += " " + AncestryUtil.GenerateBirthDeathDate(mother, true);
            }
            else
            {
                individual.SummaryMotherName = "Unknown";
            }

            foreach (GedcomFamily family in AncestryGameData.gedcomFamilies.Values.Where(x => x.WifeId == individual.Id || x.HusbandId == individual.Id))
            {
                string spouseId = (family.WifeId == individual.Id) ? family.HusbandId : family.WifeId;
                if (AncestryGameData.gedcomIndividuals.ContainsKey(spouseId)) {
                    GedcomIndividual spouse = AncestryGameData.gedcomIndividuals[spouseId];
                    string summary = spouse.GivenName;
                    if (!string.IsNullOrEmpty(spouse.Prefix))
                        summary += " " + spouse.Prefix;
                    if (!string.IsNullOrEmpty(spouse.Surname))
                        summary += " " + spouse.Surname;
                    if (!string.IsNullOrEmpty(spouse.Suffix))
                        summary += " (" + spouse.Suffix + ")";
                    summary += " " + AncestryUtil.GenerateBirthDeathDate(spouse, true);
                    individual.SummarySpouse.Add(family.Id, summary);
                } else
                {
                    individual.SummarySpouse.Add(family.Id, "Unknown");
                }

                string married = AncestryUtil.ProcessDate(family.MarriageDate, false);
                if (married != "?" || !string.IsNullOrEmpty(family.MarriagePlace.Trim()))
                    individual.SummaryMarriage.Add(family.Id, string.Format("m. {0} {1}", married, family.MarriagePlace).Trim());

                HashSet<string> childSummaries = new HashSet<string>();
                foreach (string childId in family.Children)
                {
                    if (!AncestryGameData.gedcomIndividuals.ContainsKey(childId))
                        continue;

                    GedcomIndividual child = AncestryGameData.gedcomIndividuals[childId];
                    string summary = child.GivenName;
                    if (!string.IsNullOrEmpty(child.Prefix))
                        summary += " " + child.Prefix;
                    if (!string.IsNullOrEmpty(child.Surname))
                        summary += " " + child.Surname;
                    if (!string.IsNullOrEmpty(child.Suffix))
                        summary += " (" + child.Suffix + ")";
                    summary += " " + AncestryUtil.GenerateBirthDeathDate(child, true);
                    childSummaries.Add(summary);
                }
                individual.SummaryChildren.Add(family.Id, childSummaries);
            }

            StringBuilder sb = new StringBuilder();
			sb.Append(individual.Id.Replace("@", "") + "\r\n");
			sb.Append("Ahnentafel Number: " + individual.AhnentafelNumber.ToString() + "\r\n");
			sb.Append(individual.SummaryName);
            sb.Append("\r\n" + individual.SummaryRelationship + "\r\n");
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
	
	public static void ImportGedcom(string gedcomFilename)
    {
        GedcomParser parser = new GedcomParser();
        parser.Parse(gedcomFilename);
        AncestryGameData.gedcomFamilies = parser.gedcomFamilies;
        AncestryGameData.gedcomIndividuals = parser.gedcomIndividuals;
    }

    public static void ImportResourceGedcom()
    {
        GedcomParser parser = new GedcomParser();
        TextAsset gedcomFile = (TextAsset)Resources.Load("GedcomData");
        parser.ParseText(gedcomFile.text);
        AncestryGameData.gedcomFamilies = parser.gedcomFamilies;
        AncestryGameData.gedcomIndividuals = parser.gedcomIndividuals;
    }

    public static void InitialiseAncestors()
    {
        ancestors = new Dictionary<string, AncestorIndividual>();
        AncestryGameData.ancestorGameData = new Dictionary<string, IndividualSphereData>();
        optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
        ancestorGenerationCount = new Dictionary<int, int>();

        ProcessAncestor("@" + Settings.RootIndividualId + "@", string.Empty, 1, 0);
        CalculateSummaryData();
        CalculateAncestorCountPerGenerationDictionary();
		CalculateAncestorGameObjectData();
    }
	
	public static void CalculateAncestorGameObjectData()
    {
        AncestryGameData.descentMaleLineVectors = new List<Vector3[]>();
        AncestryGameData.descentFemaleLineVectors = new List<Vector3[]>();
        AncestryGameData.marriageLineVectors = new List<Vector3[]>();

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

				name += "\r\n" + AncestryUtil.GenerateBirthDeathDate(individual, true);
				data.Text = name;
                data.Summary = individual.FullSummary;
                data.SphereTexture = string.Format("{0}_{1}", individual.CountryCode, individual.Sex.ToLower() == "male" ? "m" : "f");
                if (Settings.ShowFlags && !string.IsNullOrEmpty(data.SphereTexture))
                {
                    Texture2D texture = (Texture2D)Resources.Load("Flags/" + data.SphereTexture);
                    if (texture != null)
                        data.MaterialColor = Color.white;
                }
                AncestryGameData.ancestorGameData.Add(data.Id, data);

                angle += angleDelta;
                individualCount++;
            }

        }

        //Update lines

        foreach (AncestorIndividual individual in ancestors.Values)
        {
            if (individual.FatherId != null && ancestors.ContainsKey(individual.FatherId))
            {
                AncestryGameData.descentMaleLineVectors.Add(new Vector3[2] { AncestryGameData.ancestorGameData[individual.Id].Position, AncestryGameData.ancestorGameData[individual.FatherId].Position });
            }
            if (individual.MotherId != null && ancestors.ContainsKey(individual.MotherId))
            {
                AncestryGameData.descentFemaleLineVectors.Add(new Vector3[2] { AncestryGameData.ancestorGameData[individual.Id].Position, AncestryGameData.ancestorGameData[individual.MotherId].Position });
            }
        };

        foreach (GedcomFamily family in AncestryGameData.gedcomFamilies.Values)
        {
            if (!string.IsNullOrEmpty(family.HusbandId) && ancestors.ContainsKey(family.HusbandId) && !string.IsNullOrEmpty(family.WifeId) && ancestors.ContainsKey(family.WifeId))
            {
                AncestryGameData.marriageLineVectors.Add(new Vector3[2] { AncestryGameData.ancestorGameData[family.HusbandId].Position, AncestryGameData.ancestorGameData[family.WifeId].Position });
            }
        }
    }
	
	public static void SaveGedcomData()
	{
        if (Settings.webMode)
            return;

        if (File.Exists(GedcomDataFilename))
            File.Delete(GedcomDataFilename);
        using (BinaryWriter writer = new BinaryWriter(new FileStream(GedcomDataFilename, FileMode.OpenOrCreate)))
        {
            writer.Write(AncestryGameData.gedcomIndividuals.Values.Count);
            foreach (GedcomIndividual individual in AncestryGameData.gedcomIndividuals.Values)
            {
                writer.Write(individual.Id);
                individual.WriteToStream(writer);
            }
            writer.Write(AncestryGameData.gedcomFamilies.Values.Count);
            foreach (GedcomFamily family in AncestryGameData.gedcomFamilies.Values)
            {
                writer.Write(family.Id);
                family.WriteToStream(writer);
            }
        }
        
	}

    public static void LoadGedcomData()
    {
        AncestryGameData.gedcomFamilies = new Dictionary<string, GedcomFamily>();
        AncestryGameData.gedcomIndividuals = new Dictionary<string, GedcomIndividual>();
        if (Settings.webMode)
        {
            ImportResourceGedcom();
            return;
        }

        if (!File.Exists(GedcomDataFilename))
            return;

        using (BinaryReader reader = new BinaryReader(new FileStream(GedcomDataFilename, FileMode.OpenOrCreate)))
        {
            int recordCount = reader.ReadInt32();
            for(int i = 0; i < recordCount; i++)
            {
                AncestryGameData.gedcomIndividuals.Add(reader.ReadString(), new GedcomIndividual(reader));
            }

            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                AncestryGameData.gedcomFamilies.Add(reader.ReadString(), new GedcomFamily(reader));
            }
        }
    }

    public static void SaveProcessedDataFile()
    {
        if (Settings.webMode)
            return;

        if (File.Exists(ProcessedDataFilename))
            File.Delete(ProcessedDataFilename);
        using (BinaryWriter writer = new BinaryWriter(new FileStream(ProcessedDataFilename, FileMode.OpenOrCreate)))
        {
            writer.Write(highestDepth);
            writer.Write(ancestors.Values.Count);
            foreach (AncestorIndividual individual in ancestors.Values)
            {
                writer.Write(individual.Id);
                individual.WriteToStream(writer);
            }

            writer.Write(AncestryGameData.ancestorGameData.Values.Count);
            foreach (IndividualSphereData individual in AncestryGameData.ancestorGameData.Values)
            {
                writer.Write(individual.Id);
                individual.WriteToStream(writer);
            }

            writer.Write(AncestryGameData.descentMaleLineVectors.Count);
            foreach (Vector3[] line in AncestryGameData.descentMaleLineVectors)
            {
                writer.Write(line[0].x);
                writer.Write(line[0].y);
                writer.Write(line[0].z);
                writer.Write(line[1].x);
                writer.Write(line[1].y);
                writer.Write(line[1].z);
            }

            writer.Write(AncestryGameData.descentFemaleLineVectors.Count);
            foreach (Vector3[] line in AncestryGameData.descentFemaleLineVectors)
            {
                writer.Write(line[0].x);
                writer.Write(line[0].y);
                writer.Write(line[0].z);
                writer.Write(line[1].x);
                writer.Write(line[1].y);
                writer.Write(line[1].z);
            }

            writer.Write(AncestryGameData.marriageLineVectors.Count);
            foreach (Vector3[] line in AncestryGameData.marriageLineVectors)
            {
                writer.Write(line[0].x);
                writer.Write(line[0].y);
                writer.Write(line[0].z);
                writer.Write(line[1].x);
                writer.Write(line[1].y);
                writer.Write(line[1].z);
            }
        }
    }

    public static void LoadProcessedData()
    {
        if (Settings.webMode)
            return;

        ancestors = new Dictionary<string, AncestorIndividual>();
        AncestryGameData.ancestorGameData = new Dictionary<string, IndividualSphereData>();
        AncestryGameData.descentMaleLineVectors = new List<Vector3[]>();
        AncestryGameData.descentFemaleLineVectors = new List<Vector3[]>();
        AncestryGameData.marriageLineVectors = new List<Vector3[]>();

        if (!File.Exists(ProcessedDataFilename))
            return;

        using (BinaryReader reader = new BinaryReader(new FileStream(ProcessedDataFilename, FileMode.OpenOrCreate)))
        {
            highestDepth = reader.ReadInt32();

            int recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                ancestors.Add(reader.ReadString(), new AncestorIndividual(reader));
            }

            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                AncestryGameData.ancestorGameData.Add(reader.ReadString(), new IndividualSphereData(reader));
            }

            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                AncestryGameData.descentMaleLineVectors.Add(new Vector3[2] {new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) });
            }

            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                AncestryGameData.descentFemaleLineVectors.Add(new Vector3[2] { new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) });
            }

            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                AncestryGameData.marriageLineVectors.Add(new Vector3[2] { new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) });
            }
        }
    }
}