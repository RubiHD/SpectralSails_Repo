using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyShoot : MonoBehaviour
{
    public Transform shootController;

    public float laneDistance;

    public LayerMask playerLayer;

    public bool inRange;

    public float timeToShoot;

    public float timeLastShoot;

    public GameObject enemyBullet;

    public float timeWaitShoot;

    public Animator animator;


    private void Update()
    {
        inRange = Physics2D.Raycast(shootController.position, transform.right, laneDistance, playerLayer);

        if (inRange)
        {
            if (Time.time > timeToShoot + timeLastShoot)
            {
                timeLastShoot = Time.time;
                
                Invoke(nameof(Shoot), timeWaitShoot);
            }
        }

    }

    private void Shoot()
    {
        Instantiate(enemyBullet, shootController.position, shootController.rotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(shootController.position, shootController.position + transform.right * laneDistance);
    }
}
