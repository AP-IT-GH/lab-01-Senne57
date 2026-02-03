using UnityEngine;

public class FollowWaypoint : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float rotSpeed = 2f;
    public float waypointThreshold = 0.5f;
    private int currentWaypointIndex = 0;
    void Start()
    {
    }
    void Update()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        float distanceToWaypoint = direction.magnitude;
        if (distanceToWaypoint <= waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
            }
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
    }
}