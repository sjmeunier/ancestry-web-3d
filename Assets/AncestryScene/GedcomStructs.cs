using System.Collections.Generic;

namespace GedcomLib
{
    public struct GedcomHeader
    {
        public string Source;
        public string SourceVersion;
        public string SourceName;
        public string SourceCorporation;
        public string Destination;
        public string Date;
        public string File;
        public string CharacterEncoding;
        public string GedcomVersion;
        public string GedcomForm;
    }

    public struct GedcomIndividual
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

    public struct GedcomFamily
    {
        public string Id;
        public string HusbandId;
        public string WifeId;
        public string MarriageDate;
        public string MarriagePlace;
        public List<string> Children;
        public List<string> Notes;
        public List<string> Sources;

        public GedcomFamily(string id)
        {
            Id = id;
            HusbandId = "";
            WifeId = "";
            MarriageDate = "";
            MarriagePlace = "";
            Children = new List<string>();
            Notes = new List<string>();
            Sources = new List<string>();
        }
    }

    public struct GedcomNote
    {
        public string Id;
        public string Text;

        public GedcomNote(string id)
        {
            Id = id;
            Text = "";
        }
    }

    public struct GedcomSource
    {
        public string Id;
        public string Text;

        public GedcomSource(string id)
        {
            Id = id;
            Text = "";
        }
    }

    public enum GedcomRecordEnum
    {
        None,
        Header,
        Individual,
        Family,
        Source,
        Note
    }

    public enum GedcomSubRecordEnum
    {
        None,
        HeaderSource,
        HeaderGedcom,
        IndividualName,
        IndividualBirth,
        IndividualDeath,
        FamilyChildren,
        FamilyMarriage
    }
}
