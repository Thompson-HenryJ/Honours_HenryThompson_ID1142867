using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FolderListLoader : MonoBehaviour
{
    //The list that will hold the loaded modules
    //[SerializeField] Object[] modules;
    private Object[] modules;

    // Start is called before the first frame update
    void Start()
    {
        //Populate Array with all modules for NPC controller
        modules = Resources.LoadAll("Modules");

        //verify assets loaded successfully
        Debug.Log($"Loaded {modules.Length} assets from the folder!");

        foreach (var state in modules)
            {
            States currentState = (States)state;
            Debug.Log(currentState.name);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Awake()
    {

 

    }
}
