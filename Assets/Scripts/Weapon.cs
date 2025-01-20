using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 10.0f;
    public float bulletPrefabLifetime = 5.0f;

    [Header("Camera")]
    public Camera playerCamera;

    void Update()
    {
        // set weapon position and rotation to follow camera
        transform.position = playerCamera.transform.position + playerCamera.transform.forward * 0.5f + playerCamera.transform.right * 0.5f;
        transform.rotation = playerCamera.transform.rotation;
    }

    private void OnAttack(InputValue value)
    {
        Debug.Log("fire");
        Fire();
    }

    private void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.VelocityChange);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifetime));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(bullet);
    }
}
