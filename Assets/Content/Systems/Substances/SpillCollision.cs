using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Engine.Substances;

public class SpillCollision : MonoBehaviour
{
    List<ParticleCollisionEvent> collisionEvents;
    public SubstanceContainer substanceContainer;
    // Start is called before the first frame update
    void Start()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnParticleCollision(GameObject other)
    {
        int numEvents = GetComponent<ParticleSystem>().GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numEvents; i++)
        {
            if(other.GetComponent<SubstanceContainer>())
            {
                substanceContainer.TransferMoles(other.GetComponent<SubstanceContainer>(), 0.1f);
            }
            else
            {
                substanceContainer.RemoveMoles(0.1f);
            }
        }
    }
}
