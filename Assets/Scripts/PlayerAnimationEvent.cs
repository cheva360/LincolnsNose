using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    // Called by animation event "PlayCircle"
    public void PlayCircle()
    {
        if (player != null)
        {
            player.PlayCircle();
        }
    }


    public void FinishTransformation()
    {
        if (player != null)
        {
            player.FinishTransformation();
        }
    }
}