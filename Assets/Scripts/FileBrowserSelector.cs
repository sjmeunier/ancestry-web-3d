using UnityEngine;
using UnityEngine.SceneManagement;

public class FileBrowserSelector : MonoBehaviour {
	//skins and textures
	public GUISkin[] skins;
	public Texture2D file,folder,back,drive;
	
	//initialize file browser
	FileBrowser fb;


    bool fileSelected = false;

    void Start () {
        Settings.LoadSettings();

        if (string.IsNullOrEmpty(Settings.CurrentFolder))
            fb = new FileBrowser();
        else
            fb = new FileBrowser(Settings.CurrentFolder);

        fb.fileTexture = file; 
		fb.directoryTexture = folder;
		fb.backTexture = back;
		fb.driveTexture = drive;
		fb.showSearch = true;
		fb.searchRecursively = true;
    }
	
	void OnGUI(){
        if (fileSelected)
        {
            SceneManager.LoadScene("AncestryScene");
        }
        else
        {
            if (fb.draw())
            {
                if (fb.outputFile == null)
                {
                    Debug.Log("Cancel hit");
                }
                else
                {
                    AncestryWeb.GedcomFilename = fb.outputFile.FullName;
                    Settings.CurrentFolder = fb.outputFile.Directory.FullName;
                    Settings.SaveSettings();
                    fileSelected = true;
                }
            }
        }
	}

}