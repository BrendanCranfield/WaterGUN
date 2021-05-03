using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Camera fpsCam;

    // Start is called before the first frame update
    void Start()
    {
        fpsCam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(fpsCam.transform.position, transform.forward,out hit, Mathf.Infinity))
        {
            Debug.DrawRay(fpsCam.transform.position, transform.forward, Color.red);
            Debug.Log("Object: " + hit.transform.name);
        }
    }
}
