using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    [AddComponentMenu("NPC System/Models/Behavior FSM/BFSM Controller")]
    public class BFSMController : MonoBehaviour, INPCModelController
    {
        // One stack per stack-name; sorted ascending (lowest Priority = highest precedence)
        private readonly Dictionary<string, List<BFSMBehaviorBase>> _stacks  = new();
        private readonly Dictionary<string, BFSMBehaviorBase>       _active  = new();
        private readonly HashSet<BFSMBehaviorBase>                   _preempted = new();

        // ── INPCModelController ────────────────────────────────────────────────────
        public void InitializeModel() => RebuildStacks();

        // ── Unity ─────────────────────────────────────────────────────────────────
        private void OnEnable()  => RebuildStacks();
        private void Update()
        {
            foreach (var kvp in _stacks)
                ProcessStack(kvp.Key, kvp.Value);
        }

        // ── Private helpers ────────────────────────────────────────────────────────
        private void RebuildStacks()
        {
            _stacks.Clear();
            _active.Clear();
            _preempted.Clear();

            foreach (var b in GetComponents<BFSMBehaviorBase>())
            {
                if (!_stacks.ContainsKey(b.StackName))
                    _stacks[b.StackName] = new List<BFSMBehaviorBase>();
                _stacks[b.StackName].Add(b);
            }

            // Lower Priority number = higher urgency — sort ascending
            foreach (var stack in _stacks.Values)
                stack.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        private void ProcessStack(string stackName, List<BFSMBehaviorBase> stack)
        {
            // Find the highest-urgency (lowest Priority) behaviour that reports active
            BFSMBehaviorBase best = null;
            foreach (var b in stack)
            {
                if (b.IsActive()) { best = b; break; }
            }

            _active.TryGetValue(stackName, out var current);

            if (best == current)
            {
                current?.Execute();
                return;
            }

            // Displace current behaviour
            if (current != null)
            {
                current.OnPreempted();
                _preempted.Add(current);
            }

            // Start / resume best behaviour
            if (best != null)
            {
                if (_preempted.Contains(best))
                {
                    _preempted.Remove(best);
                    best.OnResumed();
                }
                best.Execute();
            }

            _active[stackName] = best;
        }
    }
}
