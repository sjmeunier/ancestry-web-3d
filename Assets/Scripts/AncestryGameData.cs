using UnityEngine;
using GedcomLib;
using System.Collections.Generic;
using Assets;

public class AncestryGameData : MonoBehaviour
{
	public static Dictionary<string, GedcomIndividual> gedcomIndividuals;
    public static Dictionary<string, GedcomFamily> gedcomFamilies;
	
	public static Dictionary<string, IndividualSphereData> ancestorGameData = new Dictionary<string, IndividualSphereData>();
    public static List<Vector3[]> descentMaleLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> descentFemaleLineVectors = new List<Vector3[]>();
    public static List<Vector3[]> marriageLineVectors = new List<Vector3[]>();
    public static string selectedIndividualId = null;

}
