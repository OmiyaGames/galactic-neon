using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ShopTrigger : MonoBehaviour
{
    public ShopCell shopController;

    Transform transformCache = null;

    // Use this for initialization
    void Start ()
    {
        collider2D.isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") == true)
        {
            if(transformCache == null)
            {
                transformCache = transform;
            }
            shopController.EnterShop(transformCache.position);
        }
    }
}
