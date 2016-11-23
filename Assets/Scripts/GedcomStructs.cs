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