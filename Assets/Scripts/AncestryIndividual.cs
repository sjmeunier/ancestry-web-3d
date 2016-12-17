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
        public long AppearanceCount;
		public long AhnentafelNumber;

        public string SummaryName;
        public string SummaryBirthDate;
        public string SummaryDeathDate;
        public Dictionary<string, string> SummarySpouse;
        public Dictionary<string, string> SummaryMarriage;
        public Dictionary<string, HashSet<string>> SummaryChildren;
        public string SummaryFatherName;
        public string SummaryMotherName;
        public string SummaryRelationship;
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
            SummaryRelationship = "";
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
            writer.Write(FatherId);
            writer.Write(MotherId);
            writer.Write(LowestGeneration);
            writer.Write(HighestGeneration);
            writer.Write(AppearanceCount);
            writer.Write(AhnentafelNumber);
            writer.Write(SummaryName);
            writer.Write(SummaryFatherName);
            writer.Write(SummaryMotherName);
            writer.Write(SummaryBirthDate);
            writer.Write(SummaryDeathDate);
            writer.Write(SummaryRelationship);
            writer.Write(FullSummary);

            writer.Write(SummarySpouse.Count);
            foreach(KeyValuePair<string, string> values in SummarySpouse)
            {
                writer.Write(values.Key);
                writer.Write(values.Value);
            }

            writer.Write(SummaryMarriage.Count);
            foreach (KeyValuePair<string, string> values in SummaryMarriage)
            {
                writer.Write(values.Key);
                writer.Write(values.Value);
            }

            writer.Write(SummaryChildren.Count);
            foreach (KeyValuePair<string, HashSet<string>> values in SummaryChildren)
            {
                writer.Write(values.Key);
                writer.Write(values.Value.Count);
                foreach (string value in values.Value)
                    writer.Write(value);
            }
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
            FatherId = reader.ReadString();
            MotherId = reader.ReadString();
            LowestGeneration = reader.ReadInt32();
            HighestGeneration = reader.ReadInt32();
            AppearanceCount = reader.ReadInt64();

            AhnentafelNumber = reader.ReadInt64();

            SummaryName = reader.ReadString();
            SummaryFatherName = reader.ReadString();
            SummaryMotherName = reader.ReadString();
            SummaryBirthDate = reader.ReadString();
            SummaryDeathDate = reader.ReadString();
            SummaryRelationship = reader.ReadString();
            FullSummary = reader.ReadString();

            SummarySpouse = new Dictionary<string, string>();
            int recordCount = reader.ReadInt32();
            for(int i = 0; i < recordCount; i++)
            {
                SummarySpouse.Add(reader.ReadString(), reader.ReadString());
            }

            SummaryMarriage = new Dictionary<string, string>();
            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                SummaryMarriage.Add(reader.ReadString(), reader.ReadString());
            }

            SummaryChildren = new Dictionary<string, HashSet<string>>();
            recordCount = reader.ReadInt32();
            for (int i = 0; i < recordCount; i++)
            {
                string id = reader.ReadString();
                HashSet<string> children = new HashSet<string>();
                int childrenCount = reader.ReadInt32();
                for (int j = 0; j < childrenCount; j++)
                    children.Add(reader.ReadString());

                SummaryChildren.Add(id, children);
            }
        }
    }

}