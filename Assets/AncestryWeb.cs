using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GedcomLib;
using System;
using Assets;

public class AncestryWeb : MonoBehaviour {

    private Dictionary<string, AncestorIndividual> ancestors = new Dictionary<string, AncestorIndividual>();
    private Dictionary<string, GameObject> individualSpheres = new Dictionary<string, GameObject>();
    private Dictionary<string, GedcomIndividual> gedcomIndividuals;
    private Dictionary<string, GedcomFamily> gedcomFamilies;
    private int maxDepth = 10;

    private void ProcessAncestor(string individualId, string spouseId, string childId, float theta, int depth)
    {
        if (ancestors.ContainsKey(individualId))
        {
            IncrementAppearance(individualId, childId, depth);
        }
        else
        {
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

            individual.radius = 1;
            individual.y = individual.HighestGeneration;
            individual.r = individual.HighestGeneration;
            individual.theta = theta;

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
                    ProcessAncestor(individual.FatherId, individual.MotherId, individualId, theta + (float)((Math.PI /2.0) / Math.Pow(2, individual.y )), depth + 1);

                if (!string.IsNullOrEmpty(individual.MotherId))
                    ProcessAncestor(individual.MotherId, individual.FatherId, individualId, theta - (float)((Math.PI / 2.0) / Math.Pow(2, individual.y)), depth + 1);
            }
        }
    }

    private void IncrementAppearance(string individualId, string childId, int depth)
    {
        if (ancestors.ContainsKey(individualId))
        {
            AncestorIndividual individual = ancestors[individualId];
            individual.LowestGeneration = Math.Min(individual.LowestGeneration, depth);
            individual.HighestGeneration = Math.Min(individual.HighestGeneration, depth);
            individual.AppearanceCount++;

            if (!string.IsNullOrEmpty(childId))
                individual.ChildrenIds.Add(childId);

            ancestors[individualId] = individual;

            if (!string.IsNullOrEmpty(individual.FatherId))
                IncrementAppearance(individual.FatherId, individualId, depth + 1);

            if (!string.IsNullOrEmpty(individual.MotherId))
                IncrementAppearance(individual.MotherId, individualId, depth + 1);

        }
    }

    private void InitialiseAncestors(string filename)
    {
        GedcomParser parser = new GedcomParser();
        parser.Parse(filename);
        gedcomFamilies = parser.gedcomFamilies;
        gedcomIndividuals = parser.gedcomIndividuals;

        ProcessAncestor("@I7952@", string.Empty, string.Empty, 0, 0);
    }

    private void CreateAncestorObjects()
    {
        foreach (AncestorIndividual individual in ancestors.Values)
        {
            individualSpheres.Add(individual.Id, (GameObject)Instantiate(Resources.Load("IndividualSphere"), new Vector3((float)(individual.r * Math.Cos(individual.theta) * 4), individual.y * 6, (float)(individual.r * Math.Sin(individual.theta) * 4)), Quaternion.identity));
        }
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
