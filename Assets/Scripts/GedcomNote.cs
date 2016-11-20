using System.Collections.Generic;

namespace GedcomLib
{
    public class GedcomNote
    {
        public string Id;
        public string Text;

        public GedcomNote(string id)
        {
            Id = id;
            Text = "";
        }
    }
}