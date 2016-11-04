using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public struct AncestorIndividual
    {
        public string Id;
        public string GivenName;
        public string Surname;
        public string Suffix;
        public string Prefix;
        public string Sex;
        public string BirthDate;
        public string DiedDate;
        public string FatherId;
        public string MotherId;
        public HashSet<string> SpouseIds;
        public Dictionary<string, HashSet<string>> ChildrenIds;
        public string BirthPlace;
        public string DiedPlace;

        public int LowestGeneration;
        public int HighestGeneration;
        public int AppearanceCount;
		public long AhnentafelNumber;

        public string SummaryName;
        public string SummaryBirthdate;
        public string SummaryDeathDate;
        public Dictionary<string, string> SummarySpouse;
        public Dictionary<string, HashSet<string>> SummaryChildren;
        public string SummaryFatherName;
        public string SummaryMotherName;

        public AncestorIndividual(string id)
        {
            Id = id;
            GivenName = "";
            Surname = "";
            Suffix = "";
            Prefix = "";
            Sex = "";
            BirthDate = "";
            DiedDate = "";
            BirthPlace = "";
            DiedPlace = "";
            FatherId = "";
            MotherId = "";
            SpouseIds = new HashSet<string>();
            LowestGeneration = 0;
            HighestGeneration = 0;
            AppearanceCount = 0;
            ChildrenIds = new Dictionary<string, HashSet<string>>();

            AhnentafelNumber = 0;

            SummaryName = "";
            SummaryFatherName = "";
            SummaryMotherName = "";
            SummaryBirthdate = "";
            SummaryDeathDate = "";
            SummarySpouse = new Dictionary<string, string>();
            SummaryChildren = new Dictionary<string, HashSet<string>>();
        }
    }

}