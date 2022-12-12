using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doctor : Interactable
{
    [SerializeField] private GameObject DoctorModel;
    [SerializeField] private ParticleSystem DoctorDeathParticles;
    [SerializeField] private List<SkinnedMeshRenderer> DoctorMaterials;
    [SerializeField] private Material toast;
    private bool dead = false;
    private bool killSwitchOn = false;
    private Vector3 startDoctorPosition;
    private GameObject player;

    void Start()
    {
        startDoctorPosition = DoctorModel.transform.position;
        player = GameObject.FindWithTag("Player");
    }

    
    void Update()
    {
        if (!dead)
        {
            DoctorModel.transform.position = startDoctorPosition + new Vector3(0f, 0.5f*Mathf.Sin(Time.time), 0f);
            DoctorModel.transform.LookAt(new Vector3(0, player.transform.position.y, 0), Vector3.up);
        }
        else
        {
            DoctorModel.transform.localRotation *= Quaternion.Euler(0f, 10*Time.deltaTime, 0f);
        }
    }
    public override void Unlock()
    {
        killSwitchOn = true;
        DoAction();
    }

    public override void DoAction()
    {
        if (!killSwitchOn)
        {
            MessageManager.instance.AddObjective("FINAL EXPERIMENT: Find the killswitch required to kill the doctor", Color.green);
            MessageManager.instance.DisplayMessage("Find the killswitch!", Color.yellow);
        }
        else
        {
            if (dead) return;
            MessageManager.instance.RemoveObjective("FINAL EXPERIMENT");
            GameDirector.instance.isDoctorDead = true;
            dead = true;
            DoctorDeathParticles.Play();
            StartCoroutine(changeDoctorLook());
        }
    }

    IEnumerator changeDoctorLook()
    {
        yield return new WaitForSeconds(8);

        foreach (SkinnedMeshRenderer renderer in DoctorMaterials)
        {
            renderer.material = toast;
        }

    }
}
