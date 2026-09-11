using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    public enum NPCModel
    {
        BehaviorBasedFSM   = 0,
        FiniteStateMachine = 1,
        GOAP               = 2,
        HTN                = 3,
        BDI                = 4
    }

    [AddComponentMenu("NPC System/Core/NPC Master Controller")]
    [RequireComponent(typeof(NPCBlackboard))]
    public class NPCMasterController : MonoBehaviour
    {
        [Header("Model Selection")]
        public NPCModel SelectedModel = NPCModel.BehaviorBasedFSM;

        [HideInInspector]
        public NPCModel PreviousModel = NPCModel.BehaviorBasedFSM;

        [Header("Debug")]
        public bool VerboseLogging = false;

        // Static lookup: NPCModel → concrete controller Type
        private static readonly Dictionary<NPCModel, System.Type> ModelControllerTypes =
            new Dictionary<NPCModel, System.Type>
            {
                { NPCModel.BehaviorBasedFSM,   typeof(BFSMController)  },
                { NPCModel.FiniteStateMachine,  typeof(FSMController)   },
                { NPCModel.GOAP,                typeof(GOAPController)  },
                { NPCModel.HTN,                 typeof(HTNController)   },
                { NPCModel.BDI,                 typeof(BDIController)   },
            };

        private void Start()
        {
            ActivateModel(SelectedModel);
        }

        /* 
        <summary>
        Finds the controller component for <paramref name="model"/> that is already
        attached to this GameObject (added via the Editor) and calls InitializeModel().
        </summary> 
        */
        public void ActivateModel(NPCModel model)
        {
            if (!ModelControllerTypes.TryGetValue(model, out System.Type ctrlType))
            {
                if (VerboseLogging)
                    Debug.LogWarning($"[NPCMasterController] No controller type registered for model '{model}'.", this);
                return;
            }

            Component ctrl = GetComponent(ctrlType);
            if (ctrl == null)
            {
                if (VerboseLogging)
                    Debug.LogWarning($"[NPCMasterController] Controller component '{ctrlType.Name}' not found on '{gameObject.name}'. " +
                                     "Add it via the NPC Master Controller inspector.", this);
                return;
            }

            INPCModelController npcCtrl = ctrl as INPCModelController;
            if (npcCtrl == null)
            {
                Debug.LogError($"[NPCMasterController] '{ctrlType.Name}' does not implement INPCModelController.", this);
                return;
            }

            npcCtrl.InitializeModel();

            if (VerboseLogging)
                Debug.Log($"[NPCMasterController] Activated model: {model} via {ctrlType.Name}.", this);
        }
    }
}
