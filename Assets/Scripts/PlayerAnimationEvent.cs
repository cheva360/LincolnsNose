using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public Player player;

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