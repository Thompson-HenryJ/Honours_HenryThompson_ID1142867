using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HFSM_NPC_PatrolBase : HFSM_NPC_Master_Controller
{

    // Start is called before the first frame update
    void Start()
    {
        state.currentState = state.patrolState;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
