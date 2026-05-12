using UnityEngine;

public class PooledAlarmEffect : MonoBehaviour
{
    public float lifeTime = 1f;
    private float timer;

    void OnEnable() => timer = lifeTime;

    void Update()
    {
        timer -= Time.deltaTime;
        transform.localScale += Vector3.one * Time.deltaTime;
        if (timer <= 0f)
        {
            ObjectPool pool = GetComponentInParent<ObjectPool>();
            if (pool) pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
