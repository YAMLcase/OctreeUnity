using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet;

public class DebugPanel : MonoBehaviour {

    private static GameObject[] octCubes;
    //private static int octCubeCount = 0;

    private void Update()
    {
        //GameObject[] octCubes = GameObject.FindGameObjectsWithTag("OctCube");
        //octCubeCount = octCubes.Length;
    }

    private void OnGUI()
    {
        GameObject[] octCubes = GameObject.FindGameObjectsWithTag("OctCube");

        //Each Line of the debug panel
        string[] debugPanelLines = {
            "octCubes: " + octCubes.Length.ToString(),
            OctreeNode.octreeRoot.childrenNodes.ToString(),
        };

        int debugPos = 0;
        GUI.color = Color.black;
        GUI.Label(new Rect((Screen.width / 2) - 5, (Screen.height / 2) - 5, 10, 20), "+");
        foreach (string debugLine in debugPanelLines)
        {
            debugPos += 10;
            GUI.Label(new Rect(10, debugPos, 500, 20), debugLine);
        }

        foreach (GameObject o in octCubes)
        {
            debugPos += 20;

            GUI.Label(new Rect(10, debugPos, 500, 20), o.transform.position.ToString());
        }
    }
}
