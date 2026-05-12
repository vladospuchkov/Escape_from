using UnityEngine;

public class GuardAI : MonoBehaviour
{
    public enum GuardState { Patrol, Chase, Search, Disabled }

    public Transform[] patrolPoints;
    public Transform player;
    public float patrolSpeed = 2.2f;
    public float chaseSpeed = 4.2f;
    public float detectionDistance = 7f;
    public float instantDetectionDistance = 2.2f;
    public float catchDistance = 2.0f;
    public float fieldOfView = 70f;
    public float searchTime = 4.5f;
    public float detectionBuildTime = 0.7f;
    public LayerMask obstacleMask = ~0;

    private int currentPoint;
    private Renderer[] renderers;
    private Collider[] guardColliders;
    private Vector3 lastKnownPosition;
    private float searchTimer;
    private float catchTimer;
    private float detectionMeter;
    private GuardState state = GuardState.Patrol;
    private float disabledTimer;
    private bool disabledByShot;
    private readonly Color normalColor = new Color(0.1f, 0.5f, 1f);
    private readonly Color alertColor = new Color(1f, 0.12f, 0.05f);
    private readonly Color searchColor = new Color(1f, 0.7f, 0.05f);

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        guardColliders = GetComponentsInChildren<Collider>();
        SetColor(normalColor);
        lastKnownPosition = transform.position;
    }

    void Update()
    {
        if (PauseMenu.IsPaused || Time.timeScale == 0f) return;
        if (disabledByShot)
        {
            disabledTimer -= Time.deltaTime;
            SetColor(new Color(0.08f, 0.08f, 0.08f));
            if (disabledTimer <= 0f)
            {
                disabledByShot = false;
                SetGuardColliders(true);
                state = GuardState.Search;
                searchTimer = searchTime;
            }
            return;
        }
        if (player == null) return;
        TryCatchPlayer();
        if (Time.timeScale == 0f) return;
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        bool seesPlayer = CanSeePlayer();
        if (seesPlayer)
        {
            float d = Vector3.Distance(Flat(transform.position), Flat(player.position));
            float build = d <= instantDetectionDistance ? detectionBuildTime + 1f : Time.deltaTime;
            detectionMeter += build;
            lastKnownPosition = player.position;
            searchTimer = searchTime;
            if (d <= instantDetectionDistance || detectionMeter >= detectionBuildTime)
                state = GuardState.Chase;
        }
        else
        {
            detectionMeter = Mathf.Max(0f, detectionMeter - Time.deltaTime * 1.5f);
            if (state == GuardState.Chase)
            {
                state = GuardState.Search;
                searchTimer = searchTime;
            }
        }

        switch (state)
        {
            case GuardState.Chase:
                SetColor(alertColor);
                MoveTowards(player.position, chaseSpeed);
                break;

            case GuardState.Search:
                SetColor(searchColor);
                MoveTowards(lastKnownPosition, patrolSpeed * 1.15f);
                searchTimer -= Time.deltaTime;
                if (Vector3.Distance(transform.position, lastKnownPosition) < 0.45f || searchTimer <= 0f)
                    state = GuardState.Patrol;
                break;

            default:
                SetColor(normalColor);
                Transform target = patrolPoints[currentPoint];
                MoveTowards(target.position, patrolSpeed);
                if (Vector3.Distance(Flat(transform.position), Flat(target.position)) < 0.45f)
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                break;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * 1.45f;
        Vector3 target = player.position + Vector3.up * 0.9f;
        Vector3 dir = target - eye;
        float distance = dir.magnitude;
        if (distance > detectionDistance) return false;
        if (distance > instantDetectionDistance && IsPlayerCrouching()) distance *= 1.45f;
        if (distance > detectionDistance) return false;
        if (Vector3.Angle(transform.forward, dir.normalized) > fieldOfView * 0.5f) return false;

        RaycastHit[] hits = Physics.RaycastAll(eye, dir.normalized, distance, obstacleMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            if (hit.transform == player || hit.transform.IsChildOf(player)) return true;
            return false;
        }
        return true;
    }

    void TryCatchPlayer()
    {
        if (disabledByShot || state == GuardState.Disabled || player == null)
        {
            catchTimer = 0f;
            return;
        }

        if (Mathf.Abs(player.position.y - transform.position.y) > 2.4f)
        {
            catchTimer = 0f;
            return;
        }

        if (Vector3.Distance(Flat(transform.position), Flat(player.position)) <= catchDistance)
        {
            catchTimer += Time.deltaTime;
            if (catchTimer >= 0.35f && GameManager.Instance != null)
                GameManager.Instance.PlayerCaught();
        }
        else catchTimer = 0f;
    }

    public void DisableForSeconds(float seconds)
    {
        disabledByShot = true;
        disabledTimer = Mathf.Max(1f, seconds);
        state = GuardState.Disabled;
        detectionMeter = 0f;
        catchTimer = 0f;
        SetGuardColliders(false);
        SetColor(new Color(0.08f, 0.08f, 0.08f));
        if (GameManager.Instance != null) GameManager.Instance.SetStatus("Робот отключён: " + Mathf.RoundToInt(disabledTimer) + " сек.");
    }

    public void Investigate(Vector3 position)
    {
        if (state == GuardState.Chase) return;
        detectionMeter = Mathf.Max(detectionMeter, detectionBuildTime * 0.65f);
        lastKnownPosition = position;
        searchTimer = searchTime;
        state = GuardState.Search;
    }

    bool IsPlayerCrouching()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 flatTarget = Flat(target);
        Vector3 pos = Flat(transform.position);
        Vector3 dir = flatTarget - pos;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), 10f * Time.deltaTime);
    }

    Vector3 Flat(Vector3 v) => new Vector3(v.x, transform.position.y, v.z);

    void SetColor(Color c)
    {
        foreach (var r in renderers)
            if (r != null && r.material != null) r.material.color = c;
    }

    void SetGuardColliders(bool enabled)
    {
        if (guardColliders == null) return;
        foreach (var col in guardColliders)
            if (col != null) col.enabled = enabled;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}
