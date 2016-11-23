using System.Collections.Generic;
using System.IO;

namespace GedcomLib
{

    public class GedcomFamily
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

        public GedcomFamily(BinaryReader reader)
        {
            ReadFromStream(reader);
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(HusbandId);
            writer.Write(WifeId);
            writer.Write(MarriageDate);
            writer.Write(MarriagePlace);
            writer.Write(Children.Count);
            foreach (string child in Children)
                writer.Write(child);
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
            HusbandId = reader.ReadString();
            WifeId = reader.ReadString();
            MarriageDate = reader.ReadString();
            MarriagePlace = reader.ReadString();
            int recordCount = reader.ReadInt32();
            Children = new List<string>();
            for (int i = 0; i < recordCount; i++)
                Children.Add(reader.ReadString());
            recordCount = reader.ReadInt32();
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