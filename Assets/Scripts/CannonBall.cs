using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CannonBall : NetworkBehaviour
{
    public int damage = 100;
    public GameObject cannonDestroy, waterCollision;
    public AudioClip cannonBoom, waterCollisionSound;
    [HideInInspector] public uint owner;
    // Start is called before the first frame update
    [Server]
    void Start()
    {
        Invoke(nameof(ColliderOn), 0.5f);
    }

    void ColliderOn()
    {
        GetComponent<SphereCollider>().enabled = true;
    }

    public void Hit()
    {
        GameObject explosion = Instantiate(cannonDestroy, transform.position, Quaternion.identity);
        Destroy(explosion, 5f);
        AudioSource.PlayClipAtPoint(cannonBoom, transform.position);
        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        WaterCollaider water = other.GetComponent<WaterCollaider>();
        if (water)
        {
            GameObject waterSplash = Instantiate(waterCollision, transform.position, Quaternion.Euler(-90f,0,0));
            Destroy(waterSplash, 5f);
            AudioSource.PlayClipAtPoint(waterCollisionSound, transform.position);
        }
    }

}
