// ============================================================
// NPCSpawner.cs  (optional utility)
// Demonstrates how to spawn multiple NPC bots simultaneously —
// each consuming minimal CPU, as in the Half-Life Ledgewalker
// benchmark (Khoo et al., 2002).
// ============================================================
using UnityEngine;

namespace HalfLifeNPC
{
    public class NPCSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public GameObject NPCPrefab;

        [Tooltip("Number of bots to spawn. Half-Life ran many simultaneously on one machine.")]
        public int BotCount = 8;

        public float SpawnRadius = 10f;

        void Start()
        {
            for (int i = 0; i < BotCount; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-SpawnRadius, SpawnRadius),
                    0f,
                    Random.Range(-SpawnRadius, SpawnRadius));

                Instantiate(NPCPrefab, transform.position + offset, Quaternion.identity);
            }
        }
    }
}
