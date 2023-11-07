using UnityEngine;

public class Spikes : MonoBehaviour
{
    public int damage = 10;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.TryGetComponent(out Player player)) {
                player.TakeDamage(damage);
            }
        }
    }
}
