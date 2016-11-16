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
        public string BirthPlace;
        public string DiedPlace;

        public int LowestGeneration;
        public int HighestGeneration;
        public int AppearanceCount;
		public long AhnentafelNumber;

        public string SummaryName;
        public string SummaryBirthDate;
        public string SummaryDeathDate;
        public Dictionary<string, string> SummarySpouse;
        public Dictionary<string, string> SummaryMarriage;
        public Dictionary<string, HashSet<string>> SummaryChildren;
        public string SummaryFatherName;
        public string SummaryMotherName;
		public string FullSummary;

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
            LowestGeneration = 0;
            HighestGeneration = 0;
            AppearanceCount = 0;

            AhnentafelNumber = 0;

            SummaryName = "";
            SummaryFatherName = "";
            SummaryMotherName = "";
            SummaryBirthDate = "";
            SummaryDeathDate = "";
            SummarySpouse = new Dictionary<string, string>();
            SummaryMarriage = new Dictionary<string, string>();
            SummaryChildren = new Dictionary<string, HashSet<string>>();
			FullSummary = "";
        }
    }

}