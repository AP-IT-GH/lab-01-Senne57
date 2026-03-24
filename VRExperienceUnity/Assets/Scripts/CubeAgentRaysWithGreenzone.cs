using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgentRaysWithGreenzone : Agent
{
    public Transform Enemy;
    public Transform GreenZone;
    public float speedMultiplier = 1f;
    public float rotationMultiplier = 5f;

    private bool enemyCollected = false;

    public override void OnEpisodeBegin()
    {
        enemyCollected = false;

        this.transform.localPosition = new Vector3(0, 0.5f, 0);
        this.transform.localRotation = Quaternion.identity;

-
        Enemy.gameObject.SetActive(true);
        Enemy.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(Enemy.localPosition); 
        sensor.AddObservation(GreenZone.localPosition);
        sensor.AddObservation(enemyCollected ? 1f : 0f);    

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier * Time.deltaTime);
        transform.Rotate(0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0f);

        if (!enemyCollected)
        {
            float distanceToEnemy = Vector3.Distance(this.transform.localPosition, Enemy.localPosition);
            if (distanceToEnemy < 1.42f)
            {
                enemyCollected = true;
                Enemy.gameObject.SetActive(false);
                SetReward(0.5f);

                float distanceToZone = Vector3.Distance(this.transform.localPosition, GreenZone.localPosition);
                if (distanceToZone < 2f)
                {
                    SetReward(1.0f);
                    EndEpisode();
                }
            }
        }

        AddReward(-0.001f);

        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GreenZone") && enemyCollected)
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
    }
}