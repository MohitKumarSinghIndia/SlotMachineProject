using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class SpriteButton : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onClick;

    private void OnMouseDown()
    {
        if (onClick != null)
        {
            onClick.Invoke();
        }
    }
}