using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    //probably needs to go onto the checkpoints
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<AIAgent>().CheckpointChecker(gameObject);//wants to be TryGet
    }
}
