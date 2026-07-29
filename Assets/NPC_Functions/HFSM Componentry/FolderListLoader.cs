using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FolderListLoader : MonoBehaviour
{
    //The list that will hold the loaded modules
    [SerializeField] GameObject[] modules;

    // Start is called before the first frame update
    void Start()
    {

        //Populate Array with all modules for NPC controller
        modules = Resources.LoadAll<GameObject>("Modules");

        //verify assets loaded successfully
        Debug.Log($"Loaded [modules.count] assets from the folder!");


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
