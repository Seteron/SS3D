using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Engine.Substances;
public class Spills : MonoBehaviour
{
    SubstanceContainer substanceContainer;
    public GameObject lid;
    public ParticleSystem displayParticles;
    public ParticleSystem transferParticles;
    float timer;
    bool spilling = false;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        substanceContainer = GetComponent<SubstanceContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        bool lidded = lid.activeSelf;
        float relativeVolume = (substanceContainer.CurrentVolume / substanceContainer.Volume);
        if(!lidded && relativeVolume > 0 && Vector3.Angle(transform.up, Vector3.up) > 80 * (1 - relativeVolume))
        {
            if(!spilling) StartSpilling();
            else Spill();
        }
        else if(spilling)
        {
            EndSpilling();
        }
    }

    void StartSpilling()
    {
        spilling = true;
        timer = Time.time + 0.001f;
        displayParticles.Play();

    }
    void Spill()
    {
        float relativeVolume = (substanceContainer.CurrentVolume / substanceContainer.Volume);
        float spillRate = (Vector3.Angle(transform.up, Vector3.up) - (80 * (1 - relativeVolume))) * 0.01f;
        //float molesToRemove = (Vector3.Angle(transform.up, Vector3.up) - 100 * (1 - relativeVolume)) * 0.01f;
        var disParticles = displayParticles.main;
        disParticles.startSpeed = spillRate;

        if(Time.time > timer)
        {
            timer = Time.time + (1 - spillRate * 5);
            var emitParams = new ParticleSystem.EmitParams();
            emitParams.velocity = new Vector3(0, 0, spillRate);
            transferParticles.Emit(emitParams, 1);
        }
    }

    void EndSpilling()
    {
        spilling = false;
        displayParticles.Stop();
    }


}
