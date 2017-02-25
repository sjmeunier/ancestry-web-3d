using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets
{
    public class AncestorIndividualShared : AncestorIndividual
    {
        public int LowestGeneration1;
        public int LowestGeneration2;
        public int HighestGeneration1;
        public int HighestGeneration2;
        public long AppearanceCount1;
        public long AppearanceCount2;
		public long AhnentafelNumber1;
		public long AhnentafelNumber2;

        public AncestorIndividualShared(AncestorIndividual individual1, AncestorIndividual individual2)
        {
            Id = individual1.Id;
            GivenName = individual1.GivenName;
            Surname = individual1.Surname;
            Suffix = individual1.Suffix;
            Prefix = individual1.Prefix;
            Sex = individual1.Sex;
            BirthDate = individual1.BirthDate;
            DiedDate = individual1.DiedDate;
            BirthPlace = individual1.BirthPlace;
            DiedPlace = individual1.DiedPlace;
            FatherId = individual1.FatherId;
            MotherId = individual1.MotherId;
            LowestGeneration = Math.Min(individual1.LowestGeneration, individual2.LowestGeneration);
            LowestGeneration1 = individual1.LowestGeneration;
            LowestGeneration2 = individual2.LowestGeneration;

            HighestGeneration = Math.Max(individual1.HighestGeneration, individual2.HighestGeneration);
            HighestGeneration1 = individual1.HighestGeneration;
            HighestGeneration2 = individual2.HighestGeneration;

            AppearanceCount = individual1.AppearanceCount + individual2.AppearanceCount;
            AppearanceCount1 = individual1.AppearanceCount;
            AppearanceCount2 = individual2.AppearanceCount;

			AhnentafelNumber = Math.Min(individual1.AhnentafelNumber, individual2.AhnentafelNumber);
            AhnentafelNumber1 = individual1.AhnentafelNumber;
            AhnentafelNumber2 = individual2.AhnentafelNumber;

            SummaryName = individual1.SummaryName;
            SummaryFatherName = individual1.SummaryFatherName;
            SummaryMotherName = individual1.SummaryMotherName;
            SummaryBirthDate = individual1.SummaryBirthDate;
            SummaryDeathDate = individual1.SummaryDeathDate;
            SummarySpouse = new Dictionary<string, string>();
			foreach(var item in individual1.SummarySpouse)
				SummarySpouse.Add(item.Key, item.Value);
            SummaryMarriage = new Dictionary<string, string>();
			foreach(var item in individual1.SummaryMarriage)
				SummaryMarriage.Add(item.Key, item.Value);
            SummaryChildren = new Dictionary<string, HashSet<string>>();
			foreach(var item in individual1.SummaryChildren)
			{
				HashSet<string> ids = new HashSet<string>();
				foreach(string id in item.Value)
					ids.Add(id);
				SummaryChildren.Add(item.Key, ids);
			}
            SummaryRelationship = individual1.SummaryRelationship;
			FullSummary = individual1.FullSummary;
        }

        public AncestorIndividualShared(BinaryReader reader)
        {
            ReadFromStream(reader);
        }

        public new void WriteToStream(BinaryWriter writer)
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
            writer.Write(LowestGeneration1);
            writer.Write(LowestGeneration2);
            writer.Write(HighestGeneration);
            writer.Write(HighestGeneration1);
            writer.Write(HighestGeneration2);
            writer.Write(AppearanceCount);
            writer.Write(AppearanceCount1);
            writer.Write(AppearanceCount2);
            writer.Write(AhnentafelNumber);
            writer.Write(AhnentafelNumber1);
            writer.Write(AhnentafelNumber2);
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
            LowestGeneration1 = reader.ReadInt32();
            LowestGeneration2 = reader.ReadInt32();
            HighestGeneration = reader.ReadInt32();
            HighestGeneration1 = reader.ReadInt32();
            HighestGeneration2 = reader.ReadInt32();
            AppearanceCount = reader.ReadInt64();
            AppearanceCount1 = reader.ReadInt64();
            AppearanceCount2 = reader.ReadInt64();

            AhnentafelNumber = reader.ReadInt64();
            AhnentafelNumber1 = reader.ReadInt64();
            AhnentafelNumber2 = reader.ReadInt64();

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