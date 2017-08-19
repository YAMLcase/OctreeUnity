using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {

    Material recentCubeMaterial;
    Transform recentCubeTransform;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
	}

	// Update is called once per frame
	void Update () {
        transform.Translate(
            Input.GetAxis("Horizontal") * Time.deltaTime * 15f,
            0f,
            -Input.GetAxis("Vertical") * Time.deltaTime * 15f,
            Space.Self);
        transform.Rotate(0f, Input.GetAxis("Mouse X") * Time.deltaTime * 30f, 0f, Space.World);
        transform.Rotate(Input.GetAxis("Mouse Y") * Time.deltaTime * 30f, 0f, 0f, Space.Self);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject newCube = (GameObject)GameObject.Instantiate(Resources.Load("Cube"));
            newCube.transform.position = this.transform.position + transform.forward * 6;
            newCube.GetComponent<OctreeItem>().RefreshOwners();
        }

        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.forward, out hit, 100f))
        {
            if(hit.collider.tag == "OctCube")
            {
                if (recentCubeMaterial != null)
                {
                    recentCubeMaterial.color = Color.white;
                }

                GameObject caught = hit.collider.gameObject;  //cube in front of us
                Rigidbody caughtRigid = caught.GetComponent<Rigidbody>();
                recentCubeMaterial = caught.GetComponent<Renderer>().material;
                recentCubeMaterial.color = Color.cyan;

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    caughtRigid.isKinematic = true;
                    recentCubeTransform = caught.transform;
                    recentCubeTransform.parent = transform;
                }
                if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    caughtRigid.isKinematic = false;
                    if(recentCubeTransform != null)
                        recentCubeTransform.parent = null;
                }

                if (Input.GetKeyUp(KeyCode.E))
                {
                    GameObject.Destroy(caught);

                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    caughtRigid.AddForce(transform.forward * 150f);

                }
            }
            else
            {
                if (recentCubeMaterial != null)
                {
                    recentCubeMaterial.color = Color.white;
                }     
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
