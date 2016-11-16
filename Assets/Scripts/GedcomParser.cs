using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

namespace GedcomLib
{
    public class GedcomParser
    {
        public GedcomHeader gedcomHeader = new GedcomHeader();
        public Dictionary<string, GedcomIndividual> gedcomIndividuals = new Dictionary<string, GedcomIndividual>();
        public Dictionary<string, GedcomFamily> gedcomFamilies = new Dictionary<string, GedcomFamily>();
        public Dictionary<string, GedcomNote> gedcomNotes = new Dictionary<string, GedcomNote>();
        public Dictionary<string, GedcomSource> gedcomSources = new Dictionary<string, GedcomSource>();

        private GedcomIndividual currentGedcomIndividual;
        private GedcomFamily currentGedcomFamily;
        private GedcomNote currentGedcomNote;
        private GedcomSource currentGedcomSource;
        private GedcomRecordEnum currentRecord = GedcomRecordEnum.None;
        private GedcomSubRecordEnum currentSubRecord = GedcomSubRecordEnum.None;

        public void ParseWeb(string url)
        {
            WWW request = new WWW(url);

            while (!request.isDone)
            {
               //Waiting
            }

            string[] lines = request.text.Split(new string[] { "\n" }, StringSplitOptions.None);
            string line;
            char[] separators = new char[1] { ' ' };
            foreach(string linevar in lines)
            {
                line = linevar.Replace("'", "''");
                while (line.IndexOf("  ") > 0)
                {
                    line = line.Replace("  ", " ");
                }
                string[] lineArray = line.Split(separators, 3);
                switch (lineArray[0])
                {
                    case "0":
                        ProcessRootLevel(lineArray);
                        break;
                    case "1":
                        ProcessLevel1(lineArray);
                        break;
                    case "2":
                        ProcessLevel2(lineArray);
                        break;
                }

            }
        }
        public void Parse(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            string line = "";
            char[] separators = new char[1] {' '};
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Replace("'", "''");
                while (line.IndexOf("  ") > 0)
                {
                    line = line.Replace("  ", " ");
                }
                string[] lineArray = line.Split(separators, 3);
                switch (lineArray[0])
                {
                    case "0":
                        ProcessRootLevel(lineArray);
                        break;
                    case "1":
                        ProcessLevel1(lineArray);
                        break;
                    case "2":
                        ProcessLevel2(lineArray);
                        break;
                }

            }
        }

        public void ProcessRootLevel(string[] lineArray)
        {
            switch (currentRecord)
            {
                case GedcomRecordEnum.Individual:
                    gedcomIndividuals.Add(currentGedcomIndividual.Id, currentGedcomIndividual);
                    break;
                case GedcomRecordEnum.Family:
                    if (!gedcomFamilies.ContainsKey(currentGedcomFamily.Id))
                        gedcomFamilies.Add(currentGedcomFamily.Id, currentGedcomFamily);
                    break;
                case GedcomRecordEnum.Note:
                    if (!gedcomNotes.ContainsKey(currentGedcomNote.Id))
                        gedcomNotes.Add(currentGedcomNote.Id, currentGedcomNote);
                    break;
                case GedcomRecordEnum.Source:
                    if (!gedcomSources.ContainsKey(currentGedcomSource.Id))
                        gedcomSources.Add(currentGedcomSource.Id, currentGedcomSource);
                    break;
            }

            if (lineArray[1] == "HEAD")
            {
                    currentRecord = GedcomRecordEnum.Header;
                    currentSubRecord = GedcomSubRecordEnum.None;
            } else if (lineArray[1].IndexOf("@") >= 0) {
                string val = lineArray[2];
                if (val.Length > 4)
                    val = val.Substring(0, 4);
                switch (val)
                {
                    case "INDI":
                        currentRecord = GedcomRecordEnum.Individual;
                        currentGedcomIndividual = new GedcomIndividual(lineArray[1]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "FAM":
                        currentRecord = GedcomRecordEnum.Family;
                        currentGedcomFamily = new GedcomFamily(lineArray[1]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "NOTE":
                        currentRecord = GedcomRecordEnum.Note;
                        currentGedcomNote = new GedcomNote(lineArray[1]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "SOUR":
                        currentRecord = GedcomRecordEnum.Source;
                        currentGedcomSource = new GedcomSource(lineArray[1]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                }
            }
        }
        public void ProcessLevel1(string[] lineArray)
        {
            if (currentRecord == GedcomRecordEnum.Header)
            {
                switch (lineArray[1])
                {
                    case "SOUR":
                        gedcomHeader.Source = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.HeaderSource;
                        break;
                    case "DEST":
                        gedcomHeader.Destination = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "DATE":
                        gedcomHeader.Date = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "FILE":
                        gedcomHeader.File = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "CHAR":
                        gedcomHeader.CharacterEncoding = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "GEDC":
                        currentSubRecord = GedcomSubRecordEnum.HeaderGedcom;
                        break;
                }
            }
            else if (currentRecord == GedcomRecordEnum.Individual)
            {
                switch (lineArray[1])
                {
                    case "NAME":
                        //currentGedcomIndividual.GivenName = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.IndividualName;
                        break;
                    case "SEX":
                        currentGedcomIndividual.Sex = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "BIRT":
                        currentSubRecord = GedcomSubRecordEnum.IndividualBirth;
                        break;
                    case "DEAT":
                        currentSubRecord = GedcomSubRecordEnum.IndividualDeath;
                        break;
                    case "OCCU":
                        currentGedcomIndividual.Occupation = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "DSCR":
                        if (lineArray.Length > 2)
                            currentGedcomIndividual.Description = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "NATI":
                        currentGedcomIndividual.Nationality = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "NOTE":
                        currentGedcomIndividual.Notes.Add(lineArray[2]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "SOUR":
                        currentGedcomIndividual.Sources.Add(lineArray[2]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    default:
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                }
            }
            else if (currentRecord == GedcomRecordEnum.Family)
            {
                switch (lineArray[1])
                {
                    case "HUSB":
                        currentGedcomFamily.HusbandId = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "WIFE":
                        currentGedcomFamily.WifeId = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "CHIL":
                        currentGedcomFamily.Children.Add(lineArray[2]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "MARR":
                        currentSubRecord = GedcomSubRecordEnum.FamilyMarriage;
                        break;
                    case "NOTE":
                        currentGedcomFamily.Notes.Add(lineArray[2]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "SOUR":
                        currentGedcomFamily.Sources.Add(lineArray[2]);
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    default:
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                }
            }
            else if (currentRecord == GedcomRecordEnum.Note)
            {
                switch (lineArray[1])
                {
                    case "CONC":
                        currentGedcomNote.Text = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "CONT":
                        currentGedcomNote.Text += lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    default:
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                }
            }
            else if (currentRecord == GedcomRecordEnum.Source)
            {
                switch (lineArray[1])
                {
                    case "TITL":
                        currentGedcomSource.Text = lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    case "CONT":
                        currentGedcomSource.Text += lineArray[2];
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                    default:
                        currentSubRecord = GedcomSubRecordEnum.None;
                        break;
                }
            }
        }

        public void ProcessLevel2(string[] lineArray)
        {
            if (currentSubRecord == GedcomSubRecordEnum.HeaderSource)
            {
                switch (lineArray[1])
                {
                    case "VERS":
                        gedcomHeader.SourceVersion = lineArray[2];
                        break;
                    case "NAME":
                        gedcomHeader.SourceName = lineArray[2];
                        break;
                    case "CORP":
                        gedcomHeader.SourceCorporation = lineArray[2];
                        break;
                }
            }
            else if (currentSubRecord == GedcomSubRecordEnum.HeaderGedcom)
            {
                switch (lineArray[1])
                {
                    case "VERS":
                        gedcomHeader.GedcomVersion = lineArray[2];
                        break;
                    case "FORM":
                        gedcomHeader.GedcomForm = lineArray[2];
                        break;
                }
            }
            else if (currentSubRecord == GedcomSubRecordEnum.IndividualName)
            {
                switch (lineArray[1])
                {
                    case "GIVN":
                        if (string.IsNullOrEmpty(currentGedcomIndividual.GivenName) && string.IsNullOrEmpty(currentGedcomIndividual.Surname))
                            currentGedcomIndividual.GivenName = lineArray[2];
                        break;
                    case "SURN":
                        if (string.IsNullOrEmpty(currentGedcomIndividual.Surname))
                            currentGedcomIndividual.Surname = lineArray[2];
                        break;
                    case "NSFX":
                        currentGedcomIndividual.Suffix = lineArray[2];
                        break;
                    case "SPFX":
                        currentGedcomIndividual.Prefix = lineArray[2];
                        break;
                }
            }
            else if (currentSubRecord == GedcomSubRecordEnum.IndividualBirth)
            {
                switch (lineArray[1])
                {
                    case "DATE":
                        currentGedcomIndividual.BirthDate = lineArray[2];
                        break;
                    case "PLAC":
                        currentGedcomIndividual.BirthPlace = lineArray[2];
                        break;
                }
            }
            else if (currentSubRecord == GedcomSubRecordEnum.IndividualDeath)
            {
                switch (lineArray[1])
                {
                    case "DATE":
                        currentGedcomIndividual.DiedDate = lineArray[2];
                        break;
                    case "PLAC":
                        currentGedcomIndividual.DiedPlace = lineArray[2];
                        break;
                    case "CAUS":
                        currentGedcomIndividual.DiedCause = lineArray[2];
                        break;
                }
            }
            else if (currentSubRecord == GedcomSubRecordEnum.FamilyMarriage)
            {
                switch (lineArray[1])
                {
                    case "DATE":
                        currentGedcomFamily.MarriageDate = lineArray[2];
                        break;
                    case "PLAC":
                        currentGedcomFamily.MarriagePlace = lineArray[2];
                        break;
                }
            }
        }
    }
}