using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class TestAgent : Agent
{
    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("Acción recibida: " + actions.ContinuousActions[0]);
    }
}