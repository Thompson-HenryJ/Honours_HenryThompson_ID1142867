using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : States
{

    public override States RunCurrentState()
    {
        Debug.Log("Patrol Officer");
        return this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
