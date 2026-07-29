using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{

    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        //setting the radial view to be white for ease of viewing
        Handles.color = Color.white;
        //Draw in radial view
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.radius);

        //setting both sides of the angle on either side of looking straight ahead
        Vector3 viewingAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.angle / 2);
        Vector3 viewingAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.angle / 2);

        //setting the field of view to be yellow in colour
        Handles.color = Color.yellow;

        //drawing in both sides of the field of view
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewingAngle01 * fov.radius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewingAngle02 * fov.radius);

        /**
        if (fov.canSeePlayer)
        {
            //Setting the sightline to be green
            Handles.color = Color.green;
            //Drawing the line to show that the enemy has spotted the player
            //Direct line to player character
            Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);
        }
        **/ //not used yet
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
