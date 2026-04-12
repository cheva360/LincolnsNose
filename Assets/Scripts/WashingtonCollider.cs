using UnityEngine;

/// <summary>
/// Helper script to forward Washington's collision events to the Player script.
/// Attach this to the Washington GameObject.
/// </summary>
public class WashingtonCollider : MonoBehaviour
{
    [SerializeField] private Player player;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("WashingtonCollider could not find Player component in parent!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (player != null)
        {
            player.OnWashingtonCollisionEnter(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (player != null)
        {
            player.OnWashingtonCollisionExit(collision);
        }
    }
}