using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GedcomLib;
using System;
using Assets;

public class AncestryWeb : MonoBehaviour {

    public static Dictionary<string, AncestorIndividual> ancestors = new Dictionary<string, AncestorIndividual>();
	public static Dictionary<string, Vector3> ancestorPositions = new Dictionary<string, Vector3>();
    public static List<Vector3[]> decentLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> marriageLineVectors = new List<Vector3[]>();

    private Dictionary<int, List<AncestorIndividual>> optimizedAncestors = new Dictionary<int, List<AncestorIndividual>>();
    private Dictionary<int, int> ancestorGenerationCount = new Dictionary<int, int>();
    private GameObject[] individualSpheres;
    private Dictionary<string, GedcomIndividual> gedcomIndividuals;
    private Dictionary<string, GedcomFamily> gedcomFamilies;
    
    private int maxDepth = 50;
	private int highestDepth = 0;

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
            if (depth <= maxDepth)
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
			if (depth > individual.HighestGeneration){
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
		for(int i = 0; i <= highestDepth; i++) {
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

        ProcessAncestor("@I7952@", string.Empty, string.Empty, 1, 0);
		CalculateAncestorCountPerGenerationDictionary();
		
    }

    private void CreateAncestorObjects()
    {
		individualSpheres = new GameObject[ancestors.Count()];
		float angle = 0, angleDelta;

        int individualCount = 0;

        
		//Draw spheres
		for(int i = 0; i <= highestDepth; i++)
        {
			angleDelta = (float)((Math.PI * 2) / optimizedAncestors[i].Count());
			angle = angleDelta / 2;

            int ancestorCount = ancestorGenerationCount[i];
            float radius = (4f * (float)ancestorCount) / (2f * (float)Math.PI);
            if (i == 0)
                radius = 0;

			foreach(AncestorIndividual individual in optimizedAncestors[i].OrderBy(x => x.AhnentafelNumber)) {
				ancestorPositions.Add(individual.Id, new Vector3((float)(radius * Math.Cos(angle)), individual.HighestGeneration * 8, (float)(radius * Math.Sin(angle))));
				individualSpheres[individualCount] = (GameObject)Instantiate(Resources.Load("IndividualSphere"), new Vector3((float)(radius * Math.Cos(angle)), individual.HighestGeneration * 8, (float)(radius * Math.Sin(angle))), Quaternion.identity);
                if (individual.Sex == "M")
                    individualSpheres[individualCount].transform.GetChild(0).GetComponent<Renderer>().material.color = Color.blue;
                else
                    individualSpheres[individualCount].transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                angle += angleDelta;
			}
            individualCount++;
        }

        //Update lines
        
        foreach (AncestorIndividual individual in ancestors.Values)
        {
            if (individual.FatherId != null && ancestors.ContainsKey(individual.FatherId))
            {
                decentLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[individual.Id], AncestryWeb.ancestorPositions[individual.FatherId] });
            }
            if (individual.MotherId != null && ancestors.ContainsKey(individual.MotherId))
            {
                decentLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[individual.Id], AncestryWeb.ancestorPositions[individual.MotherId] });
            }
            if (individual.SpouseId != null && ancestors.ContainsKey(individual.SpouseId))
            {
                marriageLineVectors.Add(new Vector3[2] { AncestryWeb.ancestorPositions[individual.Id], AncestryWeb.ancestorPositions[individual.SpouseId] });
            }
        };
    }
	
    // Use this for initialization
    void Start () {
        InitialiseAncestors("C:\\Genealogy\\Meunier-20160924.ged");

        CreateAncestorObjects();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}