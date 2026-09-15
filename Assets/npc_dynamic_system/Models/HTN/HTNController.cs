using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// Hierarchical Task Network controller — priority-based task selector.
    ///
    /// Each frame the controller iterates all attached HTNTaskBase components (sorted
    /// ascending by Priority) and activates the first whose CheckConditions() returns true.
    ///
    /// If the active task's conditions fail mid-execution, OnAbort() is called and the
    /// controller immediately re-evaluates.  When Execute() returns true the task is
    /// considered complete and the controller re-evaluates to pick the next task.
    /// </summary>
    [AddComponentMenu("NPC System/Models/HTN/HTN Controller")]
    public class HTNController : MonoBehaviour, INPCModelController
    {
        [Header("Debug")]
        public bool VerboseLogging = false;

        // ── Internal state ─────────────────────────────────────────────────────────
        private readonly List<HTNTaskBase> _tasks = new();
        private HTNTaskBase _currentTask;
        private bool        _taskStarted;

        // ── INPCModelController ────────────────────────────────────────────────────
        public void InitializeModel() => RebuildTasks();

        // ── Unity lifecycle ────────────────────────────────────────────────────────
        private void OnEnable() => RebuildTasks();

        private void Update()
        {
            if (_tasks.Count == 0) return;

            // Abort current task if its conditions are no longer satisfied
            if (_currentTask != null && _taskStarted && !_currentTask.CheckConditions())
            {
                _currentTask.OnAbort();
                _currentTask  = null;
                _taskStarted  = false;

                if (VerboseLogging)
                    Debug.Log("[HTNController] Task aborted — re-evaluating.", this);
            }

            // Select best task if none is running
            if (_currentTask == null)
            {
                foreach (var task in _tasks)
                {
                    if (task.CheckConditions())
                    {
                        _currentTask = task;
                        _taskStarted = false;
                        break;
                    }
                }
            }

            if (_currentTask == null) return;

            // Start the task on the first tick
            if (!_taskStarted)
            {
                _currentTask.OnStart();
                _taskStarted = true;

                if (VerboseLogging)
                    Debug.Log($"[HTNController] Starting task: {_currentTask.TaskName}", this);
            }

            // Execute and check for completion
            bool complete = _currentTask.Execute();
            if (complete)
            {
                if (VerboseLogging)
                    Debug.Log($"[HTNController] Task complete: {_currentTask.TaskName}", this);

                _currentTask = null;
                _taskStarted = false;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void RebuildTasks()
        {
            _tasks.Clear();
            _tasks.AddRange(GetComponents<HTNTaskBase>());
            _tasks.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            _currentTask = null;
            _taskStarted = false;

            if (VerboseLogging)
                Debug.Log($"[HTNController] Rebuilt {_tasks.Count} tasks.", this);
        }
    }
}
