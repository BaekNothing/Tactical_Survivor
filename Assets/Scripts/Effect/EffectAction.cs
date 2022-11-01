using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAction : MonoBehaviour
{
    public System.Action stopAction = null;


    public void OnParticleSystemStopped()
    {
        stopAction?.Invoke();
    }
}
