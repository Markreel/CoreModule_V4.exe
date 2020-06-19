using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBehaviour : MonoBehaviour
{
    [SerializeField] float swayAmount;
    private Vector3 startRot;

    private Vector3 newRot;

    private void Awake()
    {
        startRot = transform.localEulerAngles;
        SetNewRot();
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Lerp(Quaternion.Euler(transform.localEulerAngles), Quaternion.Euler(newRot), Time.deltaTime);

        if(transform.localEulerAngles == newRot)
        {
            SetNewRot();
        }
    }

    private void SetNewRot()
    {
        Vector3 newRot = startRot + new Vector3(Random.Range(-swayAmount, swayAmount), Random.Range(-swayAmount, swayAmount), Random.Range(-swayAmount, swayAmount));
    }
}
