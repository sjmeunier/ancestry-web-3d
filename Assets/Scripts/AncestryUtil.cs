using GedcomLib;
using System;
using Assets;
using System.Globalization;
using System.Collections.Generic;

public class AncestryUtil
{
    public static string CalculateRelationship(int generations, bool isMale)
    {
        if (generations == 0)
            return String.Empty;

        string relationship = "";
        if (isMale)
        {
            if (generations == 1)
                relationship = "Father";
            else if (generations == 2)
                relationship = "Grandfather";
            else if (generations == 3)
                relationship = "Great-grandfather";
            else
                relationship = string.Format("Great({0})-grandfather", generations - 2);
        }
        else
        {
            if (generations == 1)
                relationship = "Mother";
            else if (generations == 2)
                relationship = "Grandmother";
            else if (generations == 3)
                relationship = "Great-grandmother";
            else
                relationship = string.Format("Great({0})-grandmother", generations - 2);
        }
        return relationship;
    }

	public static string ProcessDate(string date, bool onlyYear)
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
	
    public static string GenerateBirthDeathDate(AncestorIndividual individual, bool onlyYear)
    {
        string born = AncestryUtil.ProcessDate(individual.BirthDate, onlyYear);
        string died = AncestryUtil.ProcessDate(individual.DiedDate, onlyYear);
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

    public static string GenerateBirthDeathDate(GedcomIndividual individual, bool onlyYear)
    {
        string born = AncestryUtil.ProcessDate(individual.BirthDate, onlyYear);
        string died = AncestryUtil.ProcessDate(individual.DiedDate, onlyYear);
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
        if (!AncestryGameData.gedcomIndividuals.ContainsKey(individualId))
            return string.Empty;

        GedcomIndividual individual = AncestryGameData.gedcomIndividuals[individualId];
        string name = individual.GivenName;
        if (!string.IsNullOrEmpty(individual.Prefix))
            name += " " + individual.Prefix;
        if (!string.IsNullOrEmpty(individual.Surname))
            name += " " + individual.Surname;
        if (!string.IsNullOrEmpty(individual.Suffix))
            name += " (" + individual.Suffix + ")";
        return name;
    }	
	
	public static string GetCountryCodeForIndividual(string individualId)
	{
        if (!AncestryGameData.gedcomIndividuals.ContainsKey(individualId))
            return string.Empty;
		GedcomIndividual individual = AncestryGameData.gedcomIndividuals[individualId];
		
		string country = GetCountryFromPlace(individual.BirthPlace);
        if (string.IsNullOrEmpty(country))
			country = GetCountryFromPlace(individual.DiedPlace);
		string countryCode = GetCountryCode(country);
		
		return countryCode;
	}
	
	public static string GetCountryFromPlace(string place)
	{
		var placeArr = place.Split(new char[] { ',' });
        return placeArr[placeArr.Length - 1].Trim();
	}
	
	private static CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
	
	public static string GetCountryCode (string countryName)
	{
        if (string.IsNullOrEmpty(countryName))
            return string.Empty;

        foreach(CultureInfo culture in cultures)
        {
            RegionInfo region = new RegionInfo(culture.LCID);
            if (region.EnglishName.ToLower().Equals(countryName.ToLower()))
                return region.TwoLetterISORegionName.ToLower();
        }
        foreach (CultureInfo culture in cultures)
        {
            RegionInfo region = new RegionInfo(culture.LCID);
            if (region.EnglishName.ToLower().Contains(countryName.ToLower()))
                return region.TwoLetterISORegionName.ToLower();
        }
        return string.Empty;
	}
}
