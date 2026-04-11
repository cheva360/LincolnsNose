using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField] private VisualEffect orbVFX;
    public void PlayCircle()
    {
        orbVFX.transform.position = transform.position;
        orbVFX.GetComponent<VisualEffect>().Play();
    }
}
