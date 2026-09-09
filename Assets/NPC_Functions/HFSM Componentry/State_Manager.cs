using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class State_Manager : MonoBehaviour
{
    public States currentState;
    FolderListLoader folderList;
    public GameObject modules;
    public PatrolState patrolState;
    /** set each indivual state definition**/

    void Start()
    {

    }

    private void Update()
    {
        //Run current state functions
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        //defines current action for running state machine
        States nextState = currentState?.RunCurrentState();

        if (nextState != null)
        {
            SwitchToNextState(nextState);

        }
    }

    private void SwitchToNextState(States nextState)
    {
        //updates current state of NPC
        currentState = nextState;
    }

    public void SetState(States newState)
    {
        currentState = newState;
    }
}
