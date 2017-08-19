using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootInit : MonoBehaviour {

    public static OctreeNode OctreeRoot;

    // Use this for initialization
    void Start()
    {
        OctreeRoot = OctreeNode.octreeRoot;
    }
	
	// Update is called once per frame
	void Update () {

	}
}
