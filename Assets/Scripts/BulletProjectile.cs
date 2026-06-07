using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform bulletHitVFXPrefab;

    private Vector3 targetPosition;

    public void SetUp(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private void Update()
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);

        float moveSpeed = 200f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);

        if (distanceAfterMoving > distanceBeforeMoving)
        {
            transform.position = targetPosition;

            Instantiate(bulletHitVFXPrefab, targetPosition, Quaternion.identity);

            trailRenderer.transform.parent = null;

            Destroy(gameObject);
        }
    }

}
