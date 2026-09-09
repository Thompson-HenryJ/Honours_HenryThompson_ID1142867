using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_ClassType : MonoBehaviour
{

    [SerializeField] List<string> ModuleStart;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        List<string> ModuleStart = new List<string>();

        ModuleStart.Add("Patroller");
        ModuleStart.Add("Commander");
        ModuleStart.Add("Civilian");

    }
}
