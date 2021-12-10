using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guns : MonoBehaviour
{
    public bool gunReset = false;
    public float resetTime = 5f;
    public GameObject gunSmoke;

    public void GunsReset()
    {
        Invoke(nameof(ResetComplite), resetTime);
    }

    void ResetComplite()
    {
        gunReset = false;
    }

    void GunSmoke()
    {
        Instantiate(gunSmoke, transform.position, Quaternion.identity);
    }
}
