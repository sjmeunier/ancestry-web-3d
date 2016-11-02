using UnityEngine;
using UnityEngine.SceneManagement;

public class FileBrowserSelector : MonoBehaviour {
	//skins and textures
	public GUISkin[] skins;
	public Texture2D file,folder,back,drive;
	
	//initialize file browser
	FileBrowser fb = new FileBrowser();
    Loader loader = new Loader();

    bool fileSelected = false;

    void Start () {
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
            loader.draw();
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
                    Debug.Log(fb.outputFile.FullName);
                    AncestryWeb.GedcomFilename = fb.outputFile.FullName;
                    fileSelected = true;
                }
            }
        }
	}
}
