using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityStandardAssets.CrossPlatformInput;

public class Player : NetworkBehaviour
{
    public Vector3 direction;
    public float speed = 5f;
    public GameObject playerCamera;
    public GameObject[] rightGuns;
    public GameObject[] leftGuns;
    public GameObject cannonBall, gunSmoke, deathPlayer;
    public int cannonPower = 100, health = 500;
    public AudioClip fireSound, deathPlayerSound;

    private Vector3 inputValue;

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        inputValue.x = 0f;
        inputValue.y = CrossPlatformInputManager.GetAxis("Horizontal")/5;
        inputValue.z = 0f;
        transform.Rotate(inputValue);
        inputValue.x = 0f;
        inputValue.y = 0f;
        inputValue.z = CrossPlatformInputManager.GetAxis("Vertical")/50;
        transform.Translate(inputValue);
        transform.Translate(direction);
        if (!CrossPlatformInputManager.GetButton("Jump") && CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            bool shotRight = true;
            bool shotFull = false;
            Fire(shotRight, shotFull);
        }
        if (!CrossPlatformInputManager.GetButton("Jump") && CrossPlatformInputManager.GetButtonDown("Fire2"))
        {
            bool shotRight = false;
            bool shotFull = false;
            Fire(shotRight, shotFull);
        }
        if (CrossPlatformInputManager.GetButton("Jump") && CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            bool shotRight = true;
            bool shotFull = true;
            Fire(shotRight, shotFull);
        }
        if (CrossPlatformInputManager.GetButton("Jump") && CrossPlatformInputManager.GetButtonDown("Fire2"))
        {
            bool shotRight = false;
            bool shotFull = true;
            Fire(shotRight, shotFull);
        }
    }

    void Fire(bool shotRight, bool shotFull)
    {
        GameObject[] guns;
        if (shotRight == true)
        {
            guns = rightGuns;
        } else
        {
            guns = leftGuns;
        }
        foreach (GameObject gun in guns)
        {
            Guns gunsScript = gun.GetComponent<Guns>();
            if (gunsScript.gunReset == false)
            {
                if (isServer)
                {
                    SpawnCannonBall(netId, shotRight, gun);
                    GunSmoke(netId, shotRight, gun);
                }
                else
                {
                    CmdSpawnCannonBall(netId, shotRight, gun);
                    CmdGunSmoke(netId, shotRight, gun);
                }
                gunsScript.gunReset = true;
                gunsScript.GunsReset();
                if (shotFull == false)
                {
                    break; 
                }
            }
        }
    }

    Vector3 RadialAngle(bool shotRight) { //вычисляем угол для выстрела из пушки
        float angle;
        Vector3 vector;
        transform.rotation.ToAngleAxis(out angle, out vector);
        if (transform.rotation.y < 0)
        {
            angle = 360 - angle;
        }
        float radialAngle = angle * Mathf.PI / 180;

        float x = Mathf.Cos(radialAngle);
        float z = Mathf.Sin(radialAngle);

        if (shotRight)
        {
            Vector3 direction = new Vector3(x, 0.5f, -z);
        }
        else
        {
            Vector3 direction = new Vector3(-x, 0.5f, z);
        }

        return direction;
    }

    private Vector3 GunSmokeAngle(bool shotRight)
    {
        float angle;
        Vector3 vector;
        transform.rotation.ToAngleAxis(out angle, out vector);
        if (transform.rotation.y < 0)
        {
            angle = 360 - angle;
        }
        Vector3 directionSmoke;
        if (shotRight == true)
        {
            Debug.Log(angle);
            directionSmoke = new Vector3(0, angle + 90, 0);
        }
        else
        {
            directionSmoke = new Vector3(0, angle - 90, 0);
        }

        return directionSmoke;
    }
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        TerrainCollider terrain = other.GetComponent<TerrainCollider>();
        if (terrain || player)
        {
            Die();
            Debug.Log(other.name);
        }
        CannonBall missile = other.gameObject.GetComponent<CannonBall>();
        if (missile && missile.owner != netId) 
        {
            missile.Hit();
            health -= cannonPower;
            if (health <= 0)
            {
                Debug.Log("Я умер");
                Die();
            }
        }
    }

    void Die()
    {
        Vector3 offset = new Vector3(0, 3, 0);
        Instantiate(deathPlayer, transform.position + offset, Quaternion.identity);
        AudioSource.PlayClipAtPoint(deathPlayerSound, transform.position);
        NetworkServer.Destroy(gameObject);
    }

    public override void OnStartLocalPlayer()
    {
        Instantiate(playerCamera);
    }

    [Server]
    public void SpawnCannonBall(uint netId, bool shotRight, GameObject gun)
    {
        Vector3 firePos = gun.transform.position;
        GameObject cannonPool = GameObject.Find("CannonPool");
        GameObject cannon = Instantiate(cannonBall, firePos, Quaternion.identity, cannonPool.transform); //Создаем локальный объект пули на сервере
        Vector3 direction = RadialAngle(shotRight);
        cannon.GetComponent<Rigidbody>().velocity = direction * 10f;
        cannon.GetComponent<CannonBall>().owner = netId;
        NetworkServer.Spawn(cannon);

        cannon.GetComponent<CannonBall>().damage = cannonPower;
        AudioSource.PlayClipAtPoint(fireSound, rightGuns[0].transform.position);
        Destroy(cannon, 5f);
        //отправляем информацию о сетевом объекте всем игрокам.

    }
    [Command]
    public void CmdSpawnCannonBall(uint owner, bool shotRight, GameObject gun)
    {
        SpawnCannonBall(owner, shotRight, gun);
    }

    [Server]
    private void GunSmoke(uint netId, bool shotRight, GameObject gun)
    {
        Vector3 firePos = gun.transform.position;
        Vector3 directionSmoke = GunSmokeAngle(shotRight);
        GameObject smoke = Instantiate(gunSmoke, firePos, Quaternion.Euler(directionSmoke), gun.transform);
        NetworkServer.Spawn(smoke);
    }

    [Command]
    private void CmdGunSmoke(uint netId, bool shotRight, GameObject gun)
    {
        GunSmoke(netId, shotRight, gun);
    }

}
