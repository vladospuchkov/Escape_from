using UnityEngine;

// Safety spawner: if the generated scene lost robots or Unity did not save them,
// this script creates 1 robot on floor 1, 2 on floor 2, 3 on floor 3 at runtime.
public class RuntimeRobotSpawner : MonoBehaviour
{
    public bool forceRecreateRobots = true;

    void Start()
    {
        SpawnRobotsIfNeeded();
    }

    void SpawnRobotsIfNeeded()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("RuntimeRobotSpawner: Player not found");
            return;
        }

        GuardAI[] existing = FindObjectsOfType<GuardAI>();
        if (existing.Length >= 6 && !forceRecreateRobots)
        {
            Debug.Log("RuntimeRobotSpawner: robots already exist: " + existing.Length);
            return;
        }

        if (forceRecreateRobots)
        {
            foreach (GuardAI g in existing)
            {
                if (g != null) Destroy(g.gameObject);
            }
        }

        Material body = MakeMat("RuntimeRobotBody_BLACK", new Color(0.02f, 0.02f, 0.025f), false);
        Material red = MakeMat("RuntimeRobotEye_RED_EMISSIVE", new Color(1f, 0.02f, 0.01f), true);

        CreateRobot("ROBOT 1F - HUNTER", new Vector3(0f, 0.05f, -8f), player,
            new Vector3[] { new Vector3(0f, 0.05f, -13f), new Vector3(0f, 0.05f, 8f), new Vector3(0f, 0.05f, 22f) }, body, red);

        CreateRobot("ROBOT 2F - HUNTER A", new Vector3(-2.3f, 5.05f, -10f), player,
            new Vector3[] { new Vector3(-2.3f, 5.05f, -18f), new Vector3(-2.3f, 5.05f, 10f), new Vector3(0f, 5.05f, 22f) }, body, red);
        CreateRobot("ROBOT 2F - HUNTER B", new Vector3(2.3f, 5.05f, 12f), player,
            new Vector3[] { new Vector3(2.3f, 5.05f, 4f), new Vector3(2.3f, 5.05f, 22f), new Vector3(0f, 5.05f, -6f) }, body, red);

        CreateRobot("ROBOT 3F - HUNTER A", new Vector3(-2.8f, 10.05f, -12f), player,
            new Vector3[] { new Vector3(-2.8f, 10.05f, -22f), new Vector3(-2.8f, 10.05f, 2f) }, body, red);
        CreateRobot("ROBOT 3F - HUNTER B", new Vector3(2.8f, 10.05f, 4f), player,
            new Vector3[] { new Vector3(2.8f, 10.05f, -3f), new Vector3(2.8f, 10.05f, 15f) }, body, red);
        CreateRobot("ROBOT 3F - HUNTER C", new Vector3(0f, 10.05f, 20f), player,
            new Vector3[] { new Vector3(0f, 10.05f, 10f), new Vector3(0f, 10.05f, 24f), new Vector3(-3f, 10.05f, 20f) }, body, red);

        Debug.Log("RuntimeRobotSpawner: created 6 visible robots");
        if (GameManager.Instance != null) GameManager.Instance.SetStatus("Создано роботов: 1 / 2 / 3 по этажам");
    }

    GameObject CreateRobot(string name, Vector3 pos, Transform player, Vector3[] patrolPositions, Material bodyMat, Material eyeMat)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        root.layer = 0;

        AddCube(root, "body", new Vector3(0, 1.1f, 0), new Vector3(1.35f, 2.0f, 1.0f), bodyMat);
        AddCube(root, "head", new Vector3(0, 2.35f, 0.02f), new Vector3(1.15f, 0.75f, 0.85f), bodyMat);
        AddCube(root, "BIG RED EYE - SHOOT HERE", new Vector3(0, 2.35f, 0.52f), new Vector3(0.95f, 0.28f, 0.12f), eyeMat);
        AddCube(root, "left claw", new Vector3(-0.95f, 1.05f, 0.15f), new Vector3(0.22f, 1.45f, 0.22f), bodyMat);
        AddCube(root, "right claw", new Vector3(0.95f, 1.05f, 0.15f), new Vector3(0.22f, 1.45f, 0.22f), bodyMat);

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1.25f, 0);
        col.height = 2.7f;
        col.radius = 0.75f;

        GuardAI ai = root.AddComponent<GuardAI>();
        ai.player = player;
        ai.detectionDistance = 18f;
        ai.instantDetectionDistance = 2.3f;
        ai.fieldOfView = 120f;
        ai.patrolSpeed = 1.8f;
        ai.chaseSpeed = 4.6f;
        ai.catchDistance = 2.2f;
        ai.detectionBuildTime = 0.35f;

        Transform[] points = new Transform[patrolPositions.Length];
        for (int i = 0; i < patrolPositions.Length; i++)
        {
            GameObject p = new GameObject(name + " Patrol Point " + i);
            p.transform.position = patrolPositions[i];
            points[i] = p.transform;
        }
        ai.patrolPoints = points;

        GameObject lightObj = new GameObject(name + " RED LIGHT");
        lightObj.transform.SetParent(root.transform);
        lightObj.transform.localPosition = new Vector3(0, 2.4f, 0.8f);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.red;
        light.range = 12f;
        light.intensity = 7f;

        return root;
    }

    void AddCube(GameObject parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent.transform);
        cube.transform.localPosition = localPos;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = scale;
        Renderer r = cube.GetComponent<Renderer>();
        if (r != null) r.material = mat;
    }

    Material MakeMat(string name, Color color, bool emission)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        if (emission)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2.5f);
        }
        return mat;
    }
}
