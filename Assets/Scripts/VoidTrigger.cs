using UnityEngine;

public class VoidTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (player.IsDead) return;
            player.Die(isVoidFall: true);
        }
    }
}