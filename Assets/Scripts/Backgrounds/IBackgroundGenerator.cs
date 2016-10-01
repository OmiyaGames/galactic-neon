using UnityEngine;
using System.Collections;

public abstract class IBackgroundGenerator : MonoBehaviour
{
    GameObject objectCache = null;
    public bool Active
    {
        get
        {
            if(objectCache == null)
            {
                objectCache = gameObject;
            }
            return objectCache.activeSelf;
        }
        set
        {
            if(objectCache == null)
            {
                objectCache = gameObject;
            }
            if(objectCache.activeSelf != value)
            {
                objectCache.SetActive(value);
                if(value == true)
                {
                    Activate();
                }
                else
                {
                    Deactivate();
                }
            }
        }
    }

    public abstract void ApplyExplosionForce(Vector3 position);

    protected abstract void Activate();
    protected abstract void Deactivate();
}
