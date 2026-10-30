using UnityEditor;
using UnityEngine;

public static class FolderListLoaderEditor
{
    public Object[] modules;
    
    static FolderListLoaderEditor()
    {
        //Populate Array with all modules for NPC controller
        modules = Resources.LoadAll("Modules");

        //verify assets loaded successfully
        Debug.Log($"Loaded {modules.Length} assets from the folder!");

        foreach (var state in modules)
        {
            if (state.GetType().IsSubclassOf(typeof(States)))
            {
                Debug.Log($"Loaded the State: {state.name}.");
            }
        }
    }

}
