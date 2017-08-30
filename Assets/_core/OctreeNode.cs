using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //allows to elegantly manipulate and extract data from arrays and lists.  This way we don't have to worry
//about huge foreach loops.

// This is an entity of the OctreeNode.  Represents one square.
public class OctreeNode
{
    public static float rootRadius = 4;   //Use ^2 to keep compatible with 1 game unit (meters)
    public static int maxObjectLimit = 1; //How many items can the octree node contain before splitting into 8 new children
    static OctreeNode _octreeRoot;        //internal variable (store the link to the root of the entire octree)
    public static OctreeNode octreeRoot   //Can read info about the root, but read only
    {
        get
        {
            if(_octreeRoot == null)
            {
                _octreeRoot = new OctreeNode(null, new Vector3(0f, 0f, 0f), rootRadius, new List<OctreeItem>());
            }
            return _octreeRoot;
        }
    }

    // below to visualize the node.
    GameObject octantGO;              // the gameObject in charge of displaying the boundaries of THIS PARTICULAR node
    LineRenderer octantLineRenderer;  // to be initialized upon the creating of THIS node

    public float halfDimentionLength; //length from the center of node Node to one of its "walls"
    private Vector3 pos;              //Center of this node.

    public OctreeNode parent;         // which OctreeNode is the parent of THIS OctreeNode?
    public List<OctreeItem> containedItems = new List<OctreeItem>(); //keeps track of the little cubes contained in this node

    OctreeNode[] _childrenNodes = new OctreeNode[8]; //will be available to store links to any future children (always 8)

    GameObject PosText;

    public OctreeNode[] childrenNodes
    {
        get { return _childrenNodes; }
    }

    public void EraseChildrenNodes()
    {
        _childrenNodes = new OctreeNode[8];
    }

    //constructor
    public OctreeNode(OctreeNode parent, Vector3 thisChild_pos, float thisChild_halfLength, List<OctreeItem> potential_items)
    {
        this.parent = parent;                       //tell this particular node who is its parent.  (parent would call this 
                                                    //constructor when creating child)
        halfDimentionLength = thisChild_halfLength; //how far from center of this node to its alls?
        pos = thisChild_pos;                        //the coordinates of the center of this new node
        //Debug.Log(pos.ToString());
        //GameObject newTestThing = (GameObject)GameObject.Instantiate(Resources.Load("TestThing"));
        //newTestThing.transform.position = pos;


        octantGO = new GameObject();
        octantGO.hideFlags = HideFlags.HideInHierarchy;
        octantLineRenderer = octantGO.AddComponent<LineRenderer>();

        //PosText = (GameObject)GameObject.Instantiate(Resources.Load("TextMesh"));
        //PosText.transform.position = pos;
        //PosText.GetComponent<TextMesh>().text = pos.ToString();


        FillCube_VisualizeCoords(); //fill the coordinates for the line renderer


        foreach (OctreeItem item in potential_items)
        {
            ProcessItem(item);      //check if the item really belongs to THIS PARTICULAR node... ignore if not.
        }
    }

    // see if a given item should be sotred in containedItems List of items for THIS PARTICULAR NODE
    public bool ProcessItem(OctreeItem item) //https://youtu.be/TwZH-aoSzJk?t=5h4s
    {
        if (ContainsItemPos(item.transform.position))   //check if THIS NODE contains the position
        {
            if(ReferenceEquals(childrenNodes[0], null)) //check if THIS NODE doesn't have children
            {
                PushItem(item);
                return true;
            }
            else
            {
                foreach(OctreeNode childNode in childrenNodes)
                {
                    if (childNode.ProcessItem(item))
                    {
                        return true;
                    }
                }
            }
        } //end if the item is inside of THIS NODE

        return false;
    }

    private void PushItem(OctreeItem item)  //we know that an item should be acquired and contained by theis node.  Work on
                                            //adding it
    {
        if (!containedItems.Contains(item)) //only add it to our list of contained items if its not mentioned yet within the
                                            //list
        {
            containedItems.Add(item);
            item.my_ownerNodes.Add(this);
        }

        if(containedItems.Count > maxObjectLimit)
        {
            oldSplit();
        }
    }

    private void Split()
    {
        foreach(OctreeItem oi in containedItems)
        {
            oi.my_ownerNodes.Remove(this);
        }

        
    }

    private void oldSplit()
    {
        foreach(OctreeItem oi in containedItems) //for every item which was contained in this splitting node:
        {

            oi.my_ownerNodes.Remove(this);       //make the item forget about THIS NODE (since it has to split into smaller
                                                 //children)
        }

        //point the vector towards the TOP RIGHT future's child center, we are doing it relative to the position of
        //THIS PARENT node
        Vector3 positionVector = new Vector3(halfDimentionLength / 2, halfDimentionLength / 2, halfDimentionLength / 2);

        for (int i = 0; i < 4; i++)
        {
            _childrenNodes[i] = new OctreeNode(this, pos + positionVector, halfDimentionLength / 2, containedItems);

            GameObject NodeObjCounter = (GameObject)GameObject.Instantiate(Resources.Load("TextMesh"));
            NodeObjCounter.transform.position = positionVector;
            NodeObjCounter.GetComponent<TextMesh>().text = Mover.nodeCount.ToString();
            Mover.nodeCount++;

            positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;


        }

        //point the vector to the BOTTOM RIGHT future's child center
        positionVector = new Vector3(halfDimentionLength / 2, -halfDimentionLength / 2, halfDimentionLength / 2);

        for (int i = 4; i < 8; i++) //populate the next 4 children of THIS NODE
        {
            _childrenNodes[i] = new OctreeNode(this, pos + positionVector, halfDimentionLength / 2, containedItems);

            GameObject NodeObjCounter = (GameObject)GameObject.Instantiate(Resources.Load("TextMesh"));
            NodeObjCounter.transform.position = positionVector;
            NodeObjCounter.GetComponent<TextMesh>().text = Mover.nodeCount.ToString();
            Mover.nodeCount++;

            //rotate the vector around the world's top axis, by negative 90 degrees (counter clock-wise if looking from above). 
            //NOTE THAT WE ROTATE  the placement vector as if it was originating from world zero (since it is actually at the 
            //world's origin.  only after it's rotated do we displace it to the correct position with the pos vector with a 
            //child's constructor.
            positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;

        }

        containedItems.Clear();
    }

    public void Attempt_ReduceSubdivisions(OctreeItem escapedItem)
    {
        if (!ReferenceEquals(this, octreeRoot) && !Siblings_ChildrenNodesPresent_too_manyItems())
        {
            foreach (OctreeNode on in parent.childrenNodes) //iterate through this node and its 7 siblings.  Then kills them
            {
                //Debug.Log("on.pos in p.cn: " + on.pos.ToString());
                //on.KillNode(parent.childrenNodes.Where(i => !ReferenceEquals(i, this)).ToArray());
                OctreeNode[] arr = parent.childrenNodes.Where(i => !ReferenceEquals(i, this)).ToArray();
                foreach (OctreeNode i in arr)
                {
                    Debug.Log("currently iterating node: " + on.pos.ToString() + "   this node's sibling: " + i.pos.ToString());
                }
                on.KillNode(arr);

            }
            parent.EraseChildrenNodes();
        }
        else //otherwise, if there are children in siblings, or there are too many items for the parent to potentially hold, 
             //then...
        {
            containedItems.Remove(escapedItem); //remove the item from the contained items of this particular node
                                                //since such item no longer falls into the domain of this node.
            escapedItem.my_ownerNodes.Remove(this);
        }
    }

    private void KillNode(OctreeNode[] obsoleteSiblingNodes)
    {
        foreach (OctreeItem oi in containedItems) //for every item in this (about to be deleted) obsolete node, do:
        {
            //from such item's owner node extract a list excluding all the siblings of this obsolete node.  Then re-assign 
            //such list to the owner nodes of that item.
            oi.my_ownerNodes = oi.my_ownerNodes.Except(obsoleteSiblingNodes).ToList(); 
            oi.my_ownerNodes.Remove(this); //and remove this node as well, after removing its 7 siblings.
            oi.my_ownerNodes.Add(parent);
            parent.containedItems.Add(oi);
            
            
           
        }
        GameObject.Destroy(octantGO);  //"woops! make sure you delete the octantGO of every sibling node too"
        GameObject.Destroy(PosText);
    }

    //true if the children nodes are present in siblings of THIS PARTICULAR OBSOLETE NODE or if their total number of items is 
    //way too much for the parent to accept.
    private bool Siblings_ChildrenNodesPresent_too_manyItems()    
    {
        List<OctreeItem> legacy_items = new List<OctreeItem>();   //items contained in this obsolete node and the siblings

        foreach (OctreeNode sibling in parent.childrenNodes)      //iterate through siblings and see if they have any children.
        {
            if (!ReferenceEquals(sibling.childrenNodes[0], null)) //if they DO have children the nreturn true (this obsolete
                                                                  //node and its siblings won't get deleted later)
            {
                return true;
            }

            // using Linq. add all the items from the currently inspected sibling. Add only the items not already contained in 
            //our legacy items list.
            legacy_items.AddRange(sibling.containedItems.Where(i => !legacy_items.Contains(i))); 
        }

        if (legacy_items.Count > maxObjectLimit + 1)
        {
            return true; //too many items for the parent to hold.  Don't get rid of siblings and this particular obsolete node.
        }

        return false;    //having looked a tall the siblings and none of them contain child nodes, their items altogether could
                         //be held by the parent.  So get rid of this particular node and those sibling nodes.
    }

    public bool ContainsItemPos(Vector3 itemPos)
    {
        if (itemPos.x > pos.x + halfDimentionLength || itemPos.x < pos.x - halfDimentionLength)
            return false;
        if (itemPos.y > pos.y + halfDimentionLength || itemPos.y < pos.y - halfDimentionLength)
            return false;
        if (itemPos.z > pos.z + halfDimentionLength || itemPos.z < pos.z - halfDimentionLength)
            return false;

        return true;
    }

    void FillCube_VisualizeCoords()            //fill the coordinates for the line renderer
    {
        Vector3[] cubeCoords = new Vector3[8]; //coods of our Node
        Vector3 corner = new Vector3(halfDimentionLength, halfDimentionLength, halfDimentionLength); //top top right corner

        for(int x = 0; x < 4; x++)             //populate the first half of cube coords(point towards all 4 top corners)
        {
            cubeCoords[x] = corner + pos;
            corner = Quaternion.Euler(0f, 90f, 0f) * corner;
        }

        corner = new Vector3(halfDimentionLength, -halfDimentionLength, halfDimentionLength); // bottom top right corner

        for (int x = 4; x < 8; x++)       //point towards all 4 bot corners
        {
            cubeCoords[x] = corner + pos; //relative to the position of THIS PARTICULAR NODE and not the world's zero
            corner = Quaternion.Euler(0f, 90f, 0f) * corner; //rotate around the vertical axis, pointing to the remaining 
                                                             //corners of the node.
        }

        octantLineRenderer.useWorldSpace = true;
        octantLineRenderer.SetVertexCount(16);
        octantLineRenderer.SetWidth(0.03f, 0.03f);
        octantLineRenderer.SetPosition(  0, cubeCoords[0]);
        octantLineRenderer.SetPosition(  1, cubeCoords[1]);
        octantLineRenderer.SetPosition(  2, cubeCoords[2]);
        octantLineRenderer.SetPosition(  3, cubeCoords[3]);
        octantLineRenderer.SetPosition(  4, cubeCoords[0]);
        octantLineRenderer.SetPosition(  5, cubeCoords[4]);
        octantLineRenderer.SetPosition(  6, cubeCoords[5]);
        octantLineRenderer.SetPosition(  7, cubeCoords[1]);
        octantLineRenderer.SetPosition(  8, cubeCoords[5]);
        octantLineRenderer.SetPosition(  9, cubeCoords[6]);
        octantLineRenderer.SetPosition( 10, cubeCoords[2]);
        octantLineRenderer.SetPosition( 11, cubeCoords[6]);
        octantLineRenderer.SetPosition( 12, cubeCoords[7]);
        octantLineRenderer.SetPosition( 13, cubeCoords[3]);
        octantLineRenderer.SetPosition( 14, cubeCoords[7]);
        octantLineRenderer.SetPosition( 15, cubeCoords[4]);


    }
}