using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public int BulletID = -1;
    public string BulletTag = "DefaultBullet";

    [HideInInspector]
    public float BulletSpeed = 12.5f, Damage = 30, BulletLifeTime = 5f;
    [HideInInspector]
    public int BulletRicochets = 0, ShotByPlayer = -1;

    private Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        BulletID = NetworkingIDManager.SetIDForObjectWithTag(BulletTag);
    }

    public void InitializeBullet(float bulletSpeed, float damage, float lifeTime, int ricochets, int shotBy, float angle, Vector2 spawnPos)
    {
        //Send event to the server to spawn the bullet
        object[] data = { BulletID, BulletTag, bulletSpeed, damage, lifeTime, ricochets, shotBy, angle, spawnPos };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(Tags.SpawnBullet, data, options, SendOptions.SendReliable);
    }

    public void InitializeBulletFromServerValues(float bulletSpeed, float damage, float lifeTime, int ricochets, int shotBy, float angle, Vector2 spawnPos)
    {
        //Apply the bullet changes recieved from the server
        CancelInvoke();
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.position = spawnPos;
        BulletSpeed = bulletSpeed;
        Damage = damage;
        BulletLifeTime = lifeTime;
        BulletRicochets = ricochets;
        ShotByPlayer = shotBy;
        Invoke("DisableBullet", BulletLifeTime);
        //Only enable the bullet after the server says to enable
        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        //Apply speed on spawn
        if (rb2D.velocity == Vector2.zero)
            rb2D.velocity = transform.right * BulletSpeed;
        //Keep bullet speed if it slows after colliding with objects
        if (rb2D.velocity.magnitude < BulletSpeed)
            rb2D.velocity *= 1.1f;
    }

    //Debug option to disable bullets lifetime
    public void DisableLifeTime() => CancelInvoke();

    private void OnTriggerEnter2D(Collider2D col)
    {
        //If shot by same player or other objects which don't communicate with bullets
        if ((col.CompareTag("Player") && col.GetComponent<PhotonView>().ViewID == ShotByPlayer) || col.CompareTag("Door"))
            return;

        //Possible collisions
        if (col.CompareTag("Bullet"))
        {
            if (col.GetComponent<Bullet>().ShotByPlayer == ShotByPlayer)
                return;
            else
                BulletRicochets--;
        }
        else if (col.transform.root.GetComponent<IHealth>() != null)
        {
            col.transform.root.GetComponent<IHealth>().TakeDamage(Damage);
            BulletRicochets--;
        }
        else if (col.transform.parent.GetComponent<IHealth>() != null)
        {
            col.transform.parent.GetComponent<IHealth>().TakeDamage(Damage);
            BulletRicochets--;
        }
        else
            BulletRicochets--;

        if (BulletRicochets < 0)
            DisableBullet();
    }

    //Disable bullet, automatically returns to object pool on disable
    private void DisableBullet() => gameObject.SetActive(false);
}
