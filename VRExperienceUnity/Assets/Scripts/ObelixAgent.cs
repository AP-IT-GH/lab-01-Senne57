using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class ObelixAgent : Agent
{
    // -------------------------------------------------------------------------
    // Inspector velden
    // -------------------------------------------------------------------------

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 120f;

    [Header("Scene Setup")]
    public Transform environment;
    public GameObject menhirPrefab;
    public GameObject destinationPrefab;

    [Header("Spawn Settings")]
    public int menhirCount = 1;
    public float spawnRadius = 7f;
    public float minAngleBetweenObjects = 30f;

    // -------------------------------------------------------------------------
    // Private velden
    // -------------------------------------------------------------------------

    private Rigidbody rb;
    private bool hasMenhir;

    private readonly List<GameObject> menhirs = new List<GameObject>();
    private readonly List<GameObject> destinations = new List<GameObject>();

    private GameObject nearestMenhir;
    private GameObject nearestDestination;
    private float previousDistanceToTarget;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        menhirCount = Mathf.RoundToInt(
            Academy.Instance.EnvironmentParameters.GetWithDefault("menhir_count", menhirCount));

        ResetAgentState();
        SpawnAllObjects();
        UpdateNearestTargets();
        previousDistanceToTarget = nearestMenhir != null ? DistanceTo(nearestMenhir) : 0f;
    }

    // -------------------------------------------------------------------------
    // Observaties — 10 floats
    // Space Size in Behavior Parameters moet exact 10 zijn.
    //
    // [0]   hasMenhir (0 of 1)                                    1
    // [1-3] richting naar dichtstbijzijnde MENHIR (0 als draagt)  3
    // [4-6] richting naar dichtstbijzijnde DEST (0 als niet draagt) 3
    // [7-9] huidige snelheid (x, y, z)                            3
    //                                                        totaal 10
    //
    // FIX: Observaties zijn nu context-afhankelijk. Als Obelix een menhir
    // draagt, is de richting naar andere menhirs niet relevant en wordt
    // Vector3.zero doorgegeven (en omgekeerd voor de bestemming).
    // Dit voorkomt dat het netwerk verwarrende/tegenstrijdige signalen krijgt.
    // -------------------------------------------------------------------------

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hasMenhir ? 1f : 0f);                              // 1

        // FIX: Toon enkel de richting die relevant is voor de huidige toestand.
        // Draagt menhir   menhir-richting verbergen, bestemmingsrichting tonen.
        // Draagt geen m.  menhir-richting tonen, bestemmingsrichting verbergen.
        sensor.AddObservation(hasMenhir ? Vector3.zero : DirectionTo(nearestMenhir));        // 3
        sensor.AddObservation(hasMenhir ? DirectionTo(nearestDestination) : Vector3.zero);   // 3

        sensor.AddObservation(rb.linearVelocity);                                // 3
    }

    // -------------------------------------------------------------------------
    // Acties
    // -------------------------------------------------------------------------

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = actions.ContinuousActions[0];
        float rotateInput = actions.ContinuousActions[1];

        Vector3 velocity = transform.forward * moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        transform.Rotate(Vector3.up, rotateInput * rotateSpeed * Time.deltaTime);

        // Kleine tijdstraf voor efficiëntie
        AddReward(-0.0001f);

        UpdateNearestTargets();

        // -----------------------------------------------------------------------
        // Beloning/straf op basis van afstandsverandering naar het juiste doel
        // -----------------------------------------------------------------------
        GameObject doel = hasMenhir ? nearestDestination : nearestMenhir;

        if (doel != null)
        {
            float huidigeAfstand = DistanceTo(doel);
            AddReward((previousDistanceToTarget - huidigeAfstand) * 0.01f);
            previousDistanceToTarget = huidigeAfstand;
        }

        // Episode beëindigen als Obelix van het platform valt
        if (transform.localPosition.y < -0.5f)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
    }

    // -------------------------------------------------------------------------
    // Botsingsdetectie
    // Dubbele isolatiecheck zodat agents van naburige environments
    // elkaar NOOIT kunnen beïnvloeden.
    // -------------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.IsChildOf(environment))
            return;

        GameObject obj = other.gameObject;

        if (other.CompareTag("Menhir") && !hasMenhir && menhirs.Contains(obj))
        {
            PickUpMenhir(obj);
        }
        else if (other.CompareTag("Menhir") && hasMenhir && menhirs.Contains(obj))
        {
            // Zware straf: groter dan de som van alle mogelijke positieve
            // beloningen in één episode, zodat reward hacking nooit loont.
            AddReward(-3f);
            EndEpisode();
        }
        else if (other.CompareTag("Destination") && hasMenhir && destinations.Contains(obj))
        {
            DeliverMenhir(obj);
        }
        else if (other.CompareTag("Destination") && !hasMenhir && destinations.Contains(obj))
        {
            // Kleine straf: Obelix raakt een bestemming aan zonder menhir.
            AddReward(-0.1f);
        }
    }

    // -------------------------------------------------------------------------
    // Spawnen
    // -------------------------------------------------------------------------

    private void SpawnAllObjects()
    {
        List<float> usedAngles = new List<float>();
        Vector3 centre = environment.position;
        const float spawnY = 1f;

        for (int i = 0; i < menhirCount; i++)
        {
            float angle = FindFreeAngle(usedAngles);
            usedAngles.Add(angle);
            GameObject menhir = Instantiate(menhirPrefab,
                                            AngleToPosition(centre, angle, spawnY),
                                            Quaternion.identity,
                                            environment);
            LockInPlace(menhir);
            menhirs.Add(menhir);
        }

        for (int i = 0; i < menhirCount; i++)
        {
            float angle = FindFreeAngle(usedAngles);
            usedAngles.Add(angle);
            GameObject dest = Instantiate(destinationPrefab,
                                           AngleToPosition(centre, angle, spawnY),
                                           Quaternion.identity,
                                           environment);
            LockInPlace(dest);
            destinations.Add(dest);
        }
    }

    private Vector3 AngleToPosition(Vector3 centre, float angleDegrees, float y)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(
            centre.x + spawnRadius * Mathf.Cos(rad),
            y,
            centre.z + spawnRadius * Mathf.Sin(rad));
    }

    private float FindFreeAngle(List<float> usedAngles)
    {
        const int maxAttempts = 300;
        for (int i = 0; i < maxAttempts; i++)
        {
            float candidate = Random.Range(0f, 360f);
            if (IsFreeAngle(candidate, usedAngles))
                return candidate;
        }
        Debug.LogWarning("[ObelixAgent] Geen vrije hoek gevonden. Verlaag minAngleBetweenObjects.");
        return Random.Range(0f, 360f);
    }

    private bool IsFreeAngle(float candidate, List<float> usedAngles)
    {
        foreach (float used in usedAngles)
            if (Mathf.Abs(Mathf.DeltaAngle(candidate, used)) < minAngleBetweenObjects)
                return false;
        return true;
    }

    // -------------------------------------------------------------------------
    // Hulpfuncties
    // -------------------------------------------------------------------------

    private void ResetAgentState()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        transform.localRotation = Quaternion.identity;
        hasMenhir = false;

        foreach (GameObject m in menhirs) if (m != null) Destroy(m);
        foreach (GameObject d in destinations) if (d != null) Destroy(d);

        menhirs.Clear();
        destinations.Clear();

        nearestMenhir = null;
        nearestDestination = null;
    }

    private void UpdateNearestTargets()
    {
        nearestMenhir = GetNearest(menhirs);
        nearestDestination = GetNearest(destinations);
    }

    private GameObject GetNearest(List<GameObject> objects)
    {
        GameObject nearest = null;
        float best = float.MaxValue;

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;
            float d = DistanceTo(obj);
            if (d < best) { best = d; nearest = obj; }
        }
        return nearest;
    }

    private void LockInPlace(GameObject obj)
    {
        Rigidbody r = obj.GetComponent<Rigidbody>();
        if (r != null) r.isKinematic = true;
    }

    private void PickUpMenhir(GameObject menhir)
    {
        menhirs.Remove(menhir);
        Destroy(menhir);
        hasMenhir = true;
        AddReward(0.5f);

        UpdateNearestTargets();
        previousDistanceToTarget = nearestDestination != null ? DistanceTo(nearestDestination) : 0f;
    }

    private void DeliverMenhir(GameObject destination)
    {
        destinations.Remove(destination);
        Destroy(destination);
        hasMenhir = false;
        AddReward(1f);

        if (menhirs.Count == 0 && destinations.Count == 0)
        {
            AddReward(2f);
            EndEpisode();
        }
        else
        {
            UpdateNearestTargets();
            previousDistanceToTarget = nearestMenhir != null ? DistanceTo(nearestMenhir) : 0f;
        }
    }

    private Vector3 DirectionTo(GameObject target)
    {
        if (target == null) return Vector3.zero;
        return (target.transform.position - transform.position).normalized;
    }

    private float DistanceTo(GameObject target)
    {
        if (target == null) return 0f;
        return Vector3.Distance(transform.position, target.transform.position);
    }
}