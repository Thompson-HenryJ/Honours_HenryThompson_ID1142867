using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(FolderListLoader))]

public class HFSM_NPC_Master_Controller : MonoBehaviour
{
    public State_Manager state;
    [SerializeField] string stateName;
    States startingState;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"class name {stateName} ");
        Type type = Type.GetType(stateName);

        if (type != null && type.IsSubclassOf(typeof(MonoBehaviour)))
        {
            // Add the component dynamically
            gameObject.AddComponent(type);
            Debug.Log($"Class reference {type}");
        }

        //state.SetState(type);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
