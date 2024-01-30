using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour

{
    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("hallo welt");
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(0, 0, 5*Time.deltaTime);
    }
}
