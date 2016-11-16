using Assets;
using UnityEngine;

public class DrawLines : MonoBehaviour
{
    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnPostRender() {
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        if (AncestryWeb.ancestryState == AncestryWeb.AncestryState.Main)
        {
            GL.Begin(GL.LINES);
            if (Settings.ShowDescentLines)
            {
                GL.Color(new Color(0.25f, 0.25f, 0.75f, 1f));
                foreach (var line in AncestryData.descentMaleLineVectors)
                {
                    GL.Vertex3(line[0].x, line[0].y, line[0].z);
                    GL.Vertex3(line[1].x, line[1].y, line[1].z);
                };

                GL.Color(new Color(0.75f, 0.25f, 0.25f, 1f));
                foreach (var line in AncestryData.descentFemaleLineVectors)
                {
                    GL.Vertex3(line[0].x, line[0].y, line[0].z);
                    GL.Vertex3(line[1].x, line[1].y, line[1].z);
                };
            }
            if (Settings.ShowMarriageLines)
            {
                GL.Color(new Color(0.25f, 0.75f, 0.25f, 1f));
                foreach (var line in AncestryData.marriageLineVectors)
                {
                    GL.Vertex3(line[0].x, line[0].y, line[0].z);
                    GL.Vertex3(line[1].x, line[1].y, line[1].z);
                };
            }
            GL.End();
        }
    }
}