using System.Collections.Generic;

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
    }

}