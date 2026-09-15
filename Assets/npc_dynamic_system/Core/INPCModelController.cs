namespace NPCSystem
{
    /* 
    <summary>
    Interface that all NPC model controllers must implement.
    Each controller manages its own Unity Update loop; the master only
    needs to call <see cref="InitializeModel"/> once on activation.
    </summary> 
    */
    public interface INPCModelController
    {
        /* 
        <summary>
        Called once when the model becomes active (from NPCMasterController.Start).
        Use this to cache component references and rebuild internal state lists.
        </summary> 
        */
        void InitializeModel();
    }
}
