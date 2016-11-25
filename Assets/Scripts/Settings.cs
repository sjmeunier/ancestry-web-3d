using Assets;
using System;
using System.IO;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static string RootIndividualId = "I0001";
    public static int MaxDepth = 50;
    public static bool ShowDescentLines = true;
    public static bool ShowMarriageLines = false;
    public static bool ShowNames = true;
    public static float ScaleFactor = 1f;

    public static string CurrentFolder = "";
    public static DateTime LastImportDate = new DateTime(0);
    public static string LastImportFilename = "";
    public static string WebURL = "https://sjmeunier.github.io/AncestryWeb3D_Web/sample.ged";
    private static string saveFileName = "settings.dat";

    public static void SaveSettings()
    {
        if (Application.isWebPlayer)
            return;

        using (BinaryWriter writer = new BinaryWriter(new FileStream(saveFileName, FileMode.Create)))
        {
            writer.Write(RootIndividualId);
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
        if (Application.isWebPlayer)
            return;

        if (File.Exists(saveFileName))
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(saveFileName, FileMode.Open)))
            {
                RootIndividualId = reader.ReadString();
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