
using UnityEngine;

public class PoolableParticles : PoolableBehaviour
{
    [SerializeField]private ParticleSystem particles;

    private void Update()
    {
        if (particles.IsAlive(true))
        {
            return;
        }
        PushToPool();
    }
}
