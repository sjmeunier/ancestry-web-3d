using System.Collections.Generic;

namespace GedcomLib
{
    public class GedcomIndividual
    {
        public string Id;
        public string GivenName;
        public string Surname;
        public string Suffix;
        public string Prefix;
        public string Sex;
        public string BirthDate;
        public string BirthPlace;
        public string Occupation;
        public string Description;
        public string Nationality;
        public string DiedDate;
        public string DiedPlace;
        public string DiedCause;
        public string ParentFamilyId;
        public string SpouseFamilyId;
        public List<string> Notes;
        public List<string> Sources;

        public GedcomIndividual(string id)
        {
            Id = id;
            GivenName = "";
            Surname = "";
            Suffix = "";
            Prefix = "";
            Sex = "";
            BirthDate = "";
            BirthPlace = "";
            Occupation = "";
            Description = "";
            Nationality = "";
            DiedDate = "";
            DiedPlace = "";
            DiedCause = "";
            ParentFamilyId = "";
            SpouseFamilyId = "";
            Notes = new List<string>();
            Sources = new List<string>();
        }
    }

}