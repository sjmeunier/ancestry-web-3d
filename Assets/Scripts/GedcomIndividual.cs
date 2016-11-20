using System.Collections.Generic;
using System.IO;

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

        public GedcomIndividual(BinaryReader reader)
        {
            ReadFromStream(reader);
        }

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
            writer.Write(Occupation);
            writer.Write(Description);
            writer.Write(Nationality);
            writer.Write(DiedDate);
            writer.Write(DiedPlace);
            writer.Write(DiedCause);
            writer.Write(ParentFamilyId);
            writer.Write(SpouseFamilyId);
            writer.Write(Notes.Count);
            foreach (string note in Notes)
                writer.Write(note);
            writer.Write(Sources.Count);
            foreach (string source in Sources)
                writer.Write(source);
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
            Occupation = reader.ReadString();
            Description = reader.ReadString();
            Nationality = reader.ReadString();
            DiedDate = reader.ReadString();
            DiedPlace = reader.ReadString();
            DiedCause = reader.ReadString();
            ParentFamilyId = reader.ReadString();
            SpouseFamilyId = reader.ReadString();
            int recordCount = reader.ReadInt32();
            Notes = new List<string>();
            for (int i = 0; i < recordCount; i++)
                Notes.Add(reader.ReadString());
            recordCount = reader.ReadInt32();
            Sources = new List<string>();
            for (int i = 0; i < recordCount; i++)
                Sources.Add(reader.ReadString());
        }
    }

}