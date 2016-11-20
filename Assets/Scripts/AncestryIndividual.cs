using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets
{
    public class AncestorIndividual
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

        public AncestorIndividual(BinaryReader reader)
        {
            ReadFromStream(reader);
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(GivenName);
            writer.Write(Surname);
            writer.Write(Suffix);
            writer.Write(Prefix);
            writer.Write(Sex);
            writer.Write(BirthDate);
            writer.Write(BirthPlace);
            writer.Write(DiedDate);
            writer.Write(DiedPlace);

        }

        private void ReadFromStream(BinaryReader reader)
        {
            Id = reader.ReadString();
            GivenName = reader.ReadString();
            Surname = reader.ReadString();
            Suffix = reader.ReadString();
            Prefix = reader.ReadString();
            Sex = reader.ReadString();
            BirthDate = reader.ReadString();
            BirthPlace = reader.ReadString();
            DiedDate = reader.ReadString();
            DiedPlace = reader.ReadString();
        }
    }

}