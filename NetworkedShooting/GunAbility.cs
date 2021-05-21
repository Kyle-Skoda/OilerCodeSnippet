using UnityEngine;

public class GunAbility : AUseAbility
{
    private Weapon localWeapon;
    private GameObject localPlayer;
    private bool gunActionInProgress = false;

    private void Awake()
    {
        localPlayer = GameManager.Instance.LocalPlayerInstance;
        GetWeapon();
    }

    //Create an instance of the scriptable object
    private void GetWeapon()
    {
        if (weapon != null)
            localWeapon = Instantiate(weapon);
        else
            localWeapon = Instantiate(GetComponent<InventorySlot>().GetItem().weapon);
    }

    public override void UseAbility()
    {
        //Fallback incase weapon didnt create an instance
        if (localWeapon == null)
            GetWeapon();

        //Prevent shooting if reloading or firerate is on cooldown
        if (gunActionInProgress == true || localWeapon.ClipAmmo <= 0)
            return;

        //Shoot
        StartCoroutine(CameraManager.Instance.ChangeCameraForLength(CameraTags.BulletShoot, CameraTags.PlayerFollow, .15f));
        for (int i = 0; i < localWeapon.BulletsPerShot; i++)
        {
            //Get needed parameters to send to server.
            Bullet b = ObjectPool.Instance.GetPooledObject(localWeapon.BulletTag).GetComponent<Bullet>();
            Vector3 spawnAngle = localPlayer.transform.GetChild(2).transform.localRotation.eulerAngles;
            spawnAngle.z += Random.Range(-localWeapon.BulletSpread, localWeapon.BulletSpread);
            Vector2 spawnPos = localPlayer.transform.GetChild(2).GetChild(1).GetChild(0).transform.position;
            //Send event to server to spawn a bullet with needed parameters.
            b.InitializeBullet(localWeapon.BulletSpeed, localWeapon.Damage, localWeapon.BulletLifeTime, localWeapon.BulletRicochets,
                localPlayer.GetComponent<PhotonView>().ViewID, spawnAngle.z, spawnPos);
        }
        localWeapon.ClipAmmo--;
        StartCoroutine(FireRateCoolDown());
    }

    private IEnumerator FireRateCoolDown()
    {
        gunActionInProgress = true;
        yield return new WaitForSeconds(localWeapon.FireRate);
        gunActionInProgress = false;
    }
}
