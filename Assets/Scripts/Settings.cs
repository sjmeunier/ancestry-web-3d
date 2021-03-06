using Assets;
using System;
using System.IO;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static string RootIndividualId = "I0001";
	public static string RootIndividualId2 = "I7951";
	public static bool IsDualMode = false;
	public static bool ShowFlags = true;
    public static int MaxDepth = 50;
    public static bool ShowDescentLines = true;
    public static bool ShowMarriageLines = false;
    public static bool ShowNames = true;
    public static float ScaleFactor = 1f;

    public static string CurrentFolder = "";
    public static DateTime LastImportDate = new DateTime(0);
    public static string LastImportFilename = "";
    private static string saveFileName = "settings.dat";

    public static bool webMode = false;

    public static void SaveSettings()
    {
        if (webMode)
            return;

        using (BinaryWriter writer = new BinaryWriter(new FileStream(saveFileName, FileMode.Create)))
        {
            writer.Write(RootIndividualId);
			writer.Write(RootIndividualId2);
			writer.Write(IsDualMode);
			writer.Write(ShowFlags);
            writer.Write(MaxDepth);
            writer.Write(ShowDescentLines);
            writer.Write(ShowMarriageLines);
            writer.Write(ShowNames);
            writer.Write(CurrentFolder);
            writer.Write(LastImportDate.Ticks);
            writer.Write(LastImportFilename);
        }
    }

    public static void LoadSettings()
    {
        if (webMode)
        {
            MaxDepth = 8;
            return;
        }

        if (File.Exists(saveFileName))
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(saveFileName, FileMode.Open)))
            {
                RootIndividualId = reader.ReadString();
				RootIndividualId2 = reader.ReadString();
				IsDualMode = reader.ReadBoolean();
				ShowFlags = reader.ReadBoolean();
                MaxDepth = reader.ReadInt32();
                ShowDescentLines = reader.ReadBoolean();
                ShowMarriageLines = reader.ReadBoolean();
                ShowNames = reader.ReadBoolean();
                CurrentFolder = reader.ReadString();
                LastImportDate = new DateTime(reader.ReadInt64());
                LastImportFilename = reader.ReadString();
            }
        }
    }
}