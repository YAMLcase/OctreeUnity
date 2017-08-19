using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeItem : MonoBehaviour {

    public List<OctreeNode> my_ownerNodes = new List<OctreeNode>();

    private Vector3 prevPos;

	// Use this for initialization
	void Start () {
        prevPos = transform.position; //copies the position of this cube item.
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//if(transform.position != prevPos)
        //{
            RefreshOwners();
            prevPos = transform.position; //grab the new position if not already defined in Start()
        //}
	}

    public void RefreshOwners()
    {
        OctreeNode.octreeRoot.ProcessItem(this);

        List<OctreeNode> survivedNodes = new List<OctreeNode>(); //store nodes that keep containing the item.  plural since you
                                                                 //can work with bounding volumes later
        List<OctreeNode> obsoleteNodes = new List<OctreeNode>(); //during the function store any nodes that are no longer 
                                                                 //containing the items in this list

        foreach (OctreeNode on in my_ownerNodes)
        {
            if(!on.ContainsItemPos(transform.position))
            {
                obsoleteNodes.Add(on);
            }
            else
            {
                survivedNodes.Add(on);
            }
        }

        my_ownerNodes = survivedNodes;

        foreach(OctreeNode on in obsoleteNodes)
        {
            on.Attempt_ReduceSubdivisions(this);
        }
    }

    public void OnDestroy()
    {
        foreach (OctreeNode on in my_ownerNodes)
        {
            on.Attempt_ReduceSubdivisions(this);
        }
    }
}
