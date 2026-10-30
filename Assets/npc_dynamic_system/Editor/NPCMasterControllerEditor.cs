#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NPCSystem
{
    /* <summary>
    Custom Inspector for NPCMasterController.
    
    Features:
      • VerboseLogging toggle.
      • SelectedModel enum dropdown — changing it calls SwapModel(), which removes the
        old controller and all its behaviour components and adds the new controller.
      • Per-behaviour checkboxes for the active model.
        Ticking adds the behaviour component; unticking removes it (both undoable).
      • Foldout per ticked behaviour reveals its serialised properties (skipping
        base/identity fields).
    </summary> */
    [CustomEditor(typeof(NPCMasterController))]
    public class NPCMasterControllerEditor : Editor
    {
        // ── Data types ─────────────────────────────────────────────────────────────

        private struct BehaviorEntry
        {
            public string Label;
            public Type   Type;
            public string Tooltip;
        }

        // ── Static behaviour/controller registries ─────────────────────────────────

        private static readonly Dictionary<NPCModel, Type> ModelControllers =
            new Dictionary<NPCModel, Type>
            {
                { NPCModel.BehaviorBasedFSM,   typeof(BFSMController)  },
                { NPCModel.FiniteStateMachine,  typeof(FSMController)   },
                { NPCModel.GOAP,                typeof(GOAPController)  },
                { NPCModel.HTN,                 typeof(HTNController)   },
                { NPCModel.BDI,                 typeof(BDIController)   },
            };

        private static readonly Dictionary<NPCModel, BehaviorEntry[]> ModelBehaviors =
            new Dictionary<NPCModel, BehaviorEntry[]>
            {
                {
                    NPCModel.BehaviorBasedFSM, new[]
                    {
                        // Resources.loadAll(resources/folder01) subtype of States
                        // Label = state.name, Type = typeof(state.Type)
                        new BehaviorEntry { Label = "Patrol",  Type = typeof(BFSM_PatrolBehavior), Tooltip = "Loops between patrol waypoints when no enemy is detected."  },
                        new BehaviorEntry { Label = "Search",  Type = typeof(BFSM_SearchBehavior), Tooltip = "Investigates a heard sound and navigates to last known position." },
                        new BehaviorEntry { Label = "Combat",  Type = typeof(BFSM_CombatBehavior), Tooltip = "Fires projectiles at a visible enemy within engage range."  },
                        new BehaviorEntry { Label = "Evade",   Type = typeof(BFSM_EvadeBehavior),  Tooltip = "Flees when health is critically low and an enemy is detected."  },
                        new BehaviorEntry { Label = "Duck",    Type = typeof(BFSM_DuckBehavior),   Tooltip = "Moves to a CoverPoint Transform when enemy is visible."  },
                    }
                },
                {
                    NPCModel.FiniteStateMachine, new[]
                    {
                        new BehaviorEntry { Label = "Idle",    Type = typeof(FSM_IdleState),    Tooltip = "Fallback state — stops the agent when nothing else applies."  },
                        new BehaviorEntry { Label = "Patrol",  Type = typeof(FSM_PatrolState),  Tooltip = "Loops between patrol waypoints when no enemy is detected."  },
                        new BehaviorEntry { Label = "Alert",   Type = typeof(FSM_AlertState),   Tooltip = "Navigates to last known position on a hearing alert."  },
                        new BehaviorEntry { Label = "Combat",  Type = typeof(FSM_CombatState),  Tooltip = "Fires at a visible enemy when health is not critical."  },
                        new BehaviorEntry { Label = "Flee",    Type = typeof(FSM_FleeState),    Tooltip = "Evacuates when health is critically low."  },
                    }
                },
                {
                    NPCModel.GOAP, new[]
                    {
                        new BehaviorEntry { Label = "Patrol",      Type = typeof(GOAP_PatrolAction),      Tooltip = "Loops patrol waypoints when no enemy is present."  },
                        new BehaviorEntry { Label = "Attack",      Type = typeof(GOAP_AttackAction),      Tooltip = "Fires at a visible in-range enemy."  },
                        new BehaviorEntry { Label = "Flee",        Type = typeof(GOAP_FleeAction),        Tooltip = "Runs to safety when health is critically low."  },
                        new BehaviorEntry { Label = "Investigate", Type = typeof(GOAP_InvestigateAction), Tooltip = "Moves to last known position on a hearing alert."  },
                    }
                },
                {
                    NPCModel.HTN, new[]
                    {
                        new BehaviorEntry { Label = "Patrol",  Type = typeof(HTN_PatrolTask),  Tooltip = "Loops between patrol waypoints when no enemy is detected."  },
                        new BehaviorEntry { Label = "Search",  Type = typeof(HTN_SearchTask),  Tooltip = "Investigates a heard sound."  },
                        new BehaviorEntry { Label = "Combat",  Type = typeof(HTN_CombatTask),  Tooltip = "Fires at a visible in-range enemy."  },
                    }
                },
                {
                    NPCModel.BDI, new[]
                    {
                        new BehaviorEntry { Label = "Patrol",  Type = typeof(BDI_PatrolDesire),  Tooltip = "Low-utility fallback patrol when no threat is present."  },
                        new BehaviorEntry { Label = "Attack",  Type = typeof(BDI_AttackDesire),  Tooltip = "High-utility engagement when enemy is visible and health is adequate."  },
                        new BehaviorEntry { Label = "Flee",    Type = typeof(BDI_FleeDesire),    Tooltip = "Overrides attack when health drops critically low."  },
                    }
                },
            };

        // Base-class fields that should not appear in the per-behaviour foldout
        private static readonly HashSet<string> SkippedProperties = new HashSet<string>
        {
            "m_Script", "StackName", "Priority", "StateName",
            "TaskName", "DesireName", "ActionName", "Cost"
        };

        // ── Instance state ─────────────────────────────────────────────────────────
        private SerializedProperty _selectedModelProp;
        private SerializedProperty _verboseLoggingProp;

        // Tracks open/closed foldout per behaviour Type
        private readonly Dictionary<Type, bool> _foldoutStates = new();

        private void OnEnable()
        {
            _selectedModelProp  = serializedObject.FindProperty("SelectedModel");
            _verboseLoggingProp = serializedObject.FindProperty("VerboseLogging");
        }

        // ── Inspector GUI ──────────────────────────────────────────────────────────

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var master = (NPCMasterController)target;

            // ── VerboseLogging ─────────────────────────────────────────────────────
            EditorGUILayout.PropertyField(_verboseLoggingProp,
                new GUIContent("Verbose Logging", "Log model activations and transitions to the Console."));

            DrawUILine(Color.grey);

            // ── Model selector ─────────────────────────────────────────────────────
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_selectedModelProp,
                new GUIContent("NPC Model", "Select the AI architecture to use for this NPC."));
            bool modelChanged = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (modelChanged && Application.isEditor)
                SwapModel(master, master.PreviousModel, master.SelectedModel);

            // ── Behaviour checkboxes ───────────────────────────────────────────────
            DrawUILine(Color.grey);
            EditorGUILayout.LabelField("Behaviours", EditorStyles.boldLabel);

            NPCModel currentModel = master.SelectedModel;
            if (ModelBehaviors.TryGetValue(currentModel, out BehaviorEntry[] entries))
            {
                foreach (var entry in entries)
                    DrawBehaviourRow(master, entry);
            }
        }

        // ── Per-behaviour row ──────────────────────────────────────────────────────

        private void DrawBehaviourRow(NPCMasterController master, BehaviorEntry entry)
        {
            Component existing = master.GetComponent(entry.Type);
            bool isActive      = existing != null;

            EditorGUILayout.BeginHorizontal();
            bool newActive = EditorGUILayout.ToggleLeft(
                new GUIContent(entry.Label, entry.Tooltip),
                isActive,
                GUILayout.Width(160f));
            EditorGUILayout.EndHorizontal();

            // Add component
            if (newActive && !isActive)
            {
                Undo.AddComponent(master.gameObject, entry.Type);
                existing = master.GetComponent(entry.Type);
            }
            // Remove component
            else if (!newActive && isActive)
            {
                Undo.DestroyObjectImmediate(existing);
                existing = null;
            }

            // Foldout for the ticked component's properties
            if (existing != null)
            {
                if (!_foldoutStates.ContainsKey(entry.Type))
                    _foldoutStates[entry.Type] = false;

                EditorGUI.indentLevel++;
                _foldoutStates[entry.Type] = EditorGUILayout.Foldout(
                    _foldoutStates[entry.Type],
                    $"{entry.Label} parameters",
                    true);

                if (_foldoutStates[entry.Type])
                    DrawComponentProperties(existing);

                EditorGUI.indentLevel--;
            }
        }

        // ── Draw component properties (skip base/identity fields) ─────────────────

        private static void DrawComponentProperties(Component component)
        {
            var so   = new SerializedObject(component);
            var iter = so.GetIterator();
            bool enterChildren = true;

            so.Update();

            while (iter.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (SkippedProperties.Contains(iter.name)) continue;

                EditorGUILayout.PropertyField(iter, true);
            }

            so.ApplyModifiedProperties();
        }

        // ── Model swap ─────────────────────────────────────────────────────────────

        private static void SwapModel(NPCMasterController master,
                                      NPCModel oldModel, NPCModel newModel)
        {
            if (oldModel == newModel) return;

            Undo.RecordObject(master, "Swap NPC Model");

            // Remove behaviours belonging to the old model
            if (ModelBehaviors.TryGetValue(oldModel, out BehaviorEntry[] oldEntries))
            {
                foreach (var entry in oldEntries)
                {
                    var comp = master.GetComponent(entry.Type);
                    if (comp != null)
                        Undo.DestroyObjectImmediate(comp);
                }
            }

            // Remove the old controller
            if (ModelControllers.TryGetValue(oldModel, out Type oldCtrlType))
            {
                var oldCtrl = master.GetComponent(oldCtrlType);
                if (oldCtrl != null)
                    Undo.DestroyObjectImmediate(oldCtrl);
            }

            // Add the new controller
            if (ModelControllers.TryGetValue(newModel, out Type newCtrlType))
                Undo.AddComponent(master.gameObject, newCtrlType);

            // Record previous model
            master.PreviousModel = newModel;
            EditorUtility.SetDirty(master);
        }

        // ── Utilities ─────────────────────────────────────────────────────────────

        private static void DrawUILine(Color color, int thickness = 1, int padding = 6)
        {
            Rect r = EditorGUILayout.GetControlRect(false, thickness + padding);
            r.height   = thickness;
            r.y       += padding / 2f;
            r.x       -= 2f;
            r.width   += 4f;
            EditorGUI.DrawRect(r, color);
        }
    }
}
#endif
