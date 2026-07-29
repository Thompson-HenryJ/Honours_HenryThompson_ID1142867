using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;
    public float delay = 0.2f;
    //for later use with player added
    public bool canSeePlayer;
    public GameObject playerRef;

    //Declaration of layermasks to create obstructions
    public LayerMask targetMask;
    public LayerMask obstructionMask;


    // Start is called before the first frame update
    private void Start()
    {
        //set playerRef to be the player character
        playerRef = GameObject.FindGameObjectWithTag("Player");
        //start Coroutine for delay to not call the function every frame (Performance saving)
        StartCoroutine(FOVRoutine());

    }

    private IEnumerator FOVRoutine()
    {
        //Feeds delay through amount of seconds
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (true)
        {
            //Continue calling functionality of Coroutine
            yield return wait;
            FieldOfViewCheck();

        }
    }

    private void FieldOfViewCheck()
    {
        {
            // sets up collider array to check for overlaps
            Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

            //Checks if something has come in range
            if (rangeChecks.Length != 0)
            {
                //Checks if overlap is player
                Transform target = rangeChecks[0].transform;
                //checks direction towards target(Player)
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                //Checks angle towards target
                if (Vector3.Angle(transform.position, directionToTarget) < angle / 2)
                {
                    //distance to player is within range
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    //does a final check that plaeyr can be seen
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {

                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            else if(canSeePlayer)
            {

            }
        }
    }
}
