#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

[InitializeOnLoad]
public class EscapeFromGuardSceneBuilder
{
    static EscapeFromGuardSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (Directory.Exists("Assets/Scripts") &&
                (!File.Exists("Assets/Scenes/EscapeFromGuard_Horror.unity") || !File.Exists("Assets/Scenes/MainMenu.unity")))
            {
                CreateScene(false);
            }
        };
    }

    [MenuItem("Tools/Escape From Guard/0. CREATE FULL PROJECT: MENU + GAME")]
    public static void CreateFullProjectFromMenu() => CreateScene(true);

    [MenuItem("Tools/Escape From Guard/1. Create HORROR Game Scene")]
    public static void CreateSceneFromMenu() => CreateScene(true);

    [MenuItem("Tools/Escape From Guard/2. Create MAIN MENU Scene")]
    public static void CreateMainMenuFromMenu() => CreateMainMenuScene(true);

    public static void CreateScene(bool showDialog)
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Audio");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientLight = new Color(.11f,.13f,.16f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.018f,.026f,.035f);
        RenderSettings.fogDensity = .016f;

        var wall = Mat("V20 wall different gray", new Color(.18f,.21f,.23f));
        var wall2 = Mat("V20 wall green gray", new Color(.12f,.19f,.17f));
        var floor = Mat("V20 floor dark warm", new Color(.075f,.07f,.065f));
        var ceil = Mat("V20 black ceiling", new Color(.012f,.014f,.018f));
        var door = Mat("V20 NORMAL BROWN DOOR", new Color(.36f,.15f,.065f));
        var frame = Mat("V20 tight door frame", new Color(.85f,.48f,.13f), true);
        var red = Mat("V20 ROBOT RED EMISSIVE", new Color(1f,.02f,.01f), true);
        var robotMat = Mat("V20 ROBOT BODY BLACK", new Color(.05f,.06f,.065f));
        var yellow = Mat("V20 yellow", new Color(1f,.68f,.08f), true);
        var blue = Mat("V20 blue teleport", new Color(.1f,.45f,1f), true);
        var hand = Mat("V20 hands", new Color(.62f,.48f,.36f));
        var gunmetal = Mat("V20 gun metal", new Color(.025f,.025f,.03f));
        var danger = Mat("V21 danger red glow", new Color(.85f,.04f,.025f), true);
        var stripe = Mat("V21 dirty hazard stripe", new Color(1f,.58f,.05f), true);
        var cable = Mat("V21 black cable", new Color(.01f,.011f,.012f));
        var ammoMat = Mat("V21 ammo box green", new Color(.08f,.34f,.16f));
        var batteryMat = Mat("V21 battery blue", new Color(.08f,.42f,.95f), true);
        var fileMat = Mat("V21 secret file white", new Color(.82f,.78f,.62f));

        // 3 separate floors: each has closed outer walls, ceiling, corridor, rooms with tight doors
        BuildFloor(1, 0, wall, wall2, floor, ceil, door, frame);
        BuildFloor(2, 5, wall2, wall, floor, ceil, door, frame);
        BuildFloor(3, 10, wall, wall2, floor, ceil, door, frame);
        AddHorrorPolish(danger, stripe, cable);

        var spawn2 = Empty("Teleport Target Floor 2", new Vector3(0, 5.95f, -24));
        var spawn3 = Empty("Teleport Target Floor 3", new Vector3(0, 10.95f, -24));
        MakeTeleporter("ТЕЛЕПОРТ НА 2 ЭТАЖ", new Vector3(0,.08f,23), spawn2.transform, blue, "Ты на втором этаже: здесь 2 робота");
        MakeTeleporter("ТЕЛЕПОРТ НА 3 ЭТАЖ", new Vector3(0,5.08f,23), spawn3.transform, blue, "Ты на третьем этаже: здесь 3 робота и джетпак");

        MakeKeycard(new Vector3(-4.8f,.75f,-10), Mat("V20 keycard green", new Color(.05f,.9f,.25f), true), "Keycard", "ключ-карта", "Ключ-карта взята");
        MakeKeycard(new Vector3(4.8f,10.75f,16), yellow, "Jetpack", "джетпак", "Джетпак взят. Иди на финальную платформу на 3 этаже — она выбросит тебя наружу.");
        MakeRoomLoot(new Vector3(-6.2f,.35f,-20), ammoMat, "Ammo", "Патроны", "Коробка патронов: +6", 6, 0f, "");
        MakeRoomLoot(new Vector3(6.2f,.35f,-4), batteryMat, "Battery", "Батарейка", "Батарейка фонарика: +35%", 0, 35f, "");
        MakeRoomLoot(new Vector3(-6.2f,5.35f,4), ammoMat, "Ammo", "Патроны", "Коробка патронов: +8", 8, 0f, "");
        MakeRoomLoot(new Vector3(6.2f,5.35f,20), fileMat, "SecretFile", "Секретная папка", "Ты нашел документы охраны.", 0, 0f, "В документах написано: наблюдатели перезагружаются после попадания. У тебя есть примерно 15 секунд, чтобы проскочить.");
        MakeRoomLoot(new Vector3(-6.2f,10.35f,-8), batteryMat, "Battery", "Батарейка", "Батарейка фонарика: +45%", 0, 45f, "");
        MakeRoomLoot(new Vector3(6.2f,10.35f,8), ammoMat, "Ammo", "Патроны", "Коробка патронов: +10", 10, 0f, "");
        Transform outsideSpawn = MakeOutsideFinale(yellow, blue, floor, wall, ceil);
        MakeEscapePlatform(new Vector3(0,10.15f,26), yellow, false, outsideSpawn);

        var player = new GameObject("Player"); player.tag="Player"; player.transform.position = new Vector3(0,1,-25);
        var cc = player.AddComponent<CharacterController>(); cc.height=1.8f; cc.radius=.35f; cc.stepOffset=.45f; cc.slopeLimit=50;
        var pc = player.AddComponent<SimplePlayerController>(); pc.walkSpeed=3.4f; pc.runSpeed=5.8f; pc.jumpHeight=1.1f;
        var inv = player.AddComponent<InventorySystem>();
        var inter = player.AddComponent<PlayerInteractor>();
        var cam = new GameObject("PlayerCamera"); cam.transform.SetParent(player.transform); cam.transform.localPosition = new Vector3(0,.65f,0); cam.transform.localRotation=Quaternion.identity;
        var camera = cam.AddComponent<Camera>(); camera.fieldOfView=72; cam.AddComponent<AudioListener>(); pc.cameraRoot=cam.transform; inter.cameraRoot=cam.transform;
        var flObj = new GameObject("Flashlight"); flObj.transform.SetParent(cam.transform); flObj.transform.localPosition=Vector3.zero;
        var fl = flObj.AddComponent<Light>(); fl.type=LightType.Spot; fl.range=28; fl.spotAngle=62; fl.intensity=4.2f; fl.shadows=LightShadows.Soft;
        var flCtrl = player.AddComponent<FlashlightController>(); flCtrl.flashlight=fl;
        var gun = player.AddComponent<GunController>(); gun.cameraRoot=cam.transform; gun.ammo=12; gun.maxAmmo=12; gun.robotDisableSeconds=15f;
        var jet = player.AddComponent<JetpackFlightController>(); jet.cameraRoot = cam.transform;
        jet.jetpackLoopSound = player.AddComponent<AudioSource>(); jet.jetpackLoopSound.clip = Clip("JETPACK_LOOP", .35f, 170); jet.jetpackLoopSound.loop = true; jet.jetpackLoopSound.volume = .28f;
        gun.shootSound = player.AddComponent<AudioSource>(); gun.shootSound.clip = AssetClip("Assets/Audio/shot.wav", "LOUD_SHOT", .13f, 120); gun.shootSound.volume=.85f; gun.shootSound.spatialBlend=.15f;
        gun.emptySound = player.AddComponent<AudioSource>(); gun.emptySound.clip = Clip("EMPTY_CLICK", .08f, 600); gun.emptySound.volume=.5f;
        BuildHands(cam.transform, hand, gunmetal, yellow, gun);

        var gmObj = new GameObject("GameManager"); var gm=gmObj.AddComponent<GameManager>(); gm.flashlight=flCtrl;
        gm.caughtSound = gmObj.AddComponent<AudioSource>(); gm.caughtSound.clip=Clip("SCREAM_CAUGHT", .7f, 45); gm.caughtSound.volume=.8f;
        gm.keySound = gmObj.AddComponent<AudioSource>(); gm.keySound.clip=Clip("PICKUP", .18f, 600); gm.keySound.volume=.5f;
        var pm = gmObj.AddComponent<PauseMenu>();
        gmObj.AddComponent<RuntimeRobotSpawner>();
        CreateUI(gm, pm, inv, gun, inter);

        // ROBOTS: placed in front of player routes, huge red eye, red spotlight. No NavMesh needed.
        MakeGuard("ROBOT FLOOR 1 - VISIBLE", new Vector3(0, .05f, -8), player.transform, new Vector3[]{new Vector3(0,.05f,-12), new Vector3(0,.05f,10), new Vector3(0,.05f,20)}, robotMat, red);
        MakeGuard("ROBOT FLOOR 2 A", new Vector3(-2.4f, 5.05f, -10), player.transform, new Vector3[]{new Vector3(-2.4f,5.05f,-15), new Vector3(-2.4f,5.05f,12)}, robotMat, red);
        MakeGuard("ROBOT FLOOR 2 B", new Vector3(2.4f, 5.05f, 12), player.transform, new Vector3[]{new Vector3(2.4f,5.05f,8), new Vector3(2.4f,5.05f,21)}, robotMat, red);
        MakeGuard("ROBOT FLOOR 3 A", new Vector3(-2.8f, 10.05f, -12), player.transform, new Vector3[]{new Vector3(-2.8f,10.05f,-18), new Vector3(-2.8f,10.05f,0)}, robotMat, red);
        MakeGuard("ROBOT FLOOR 3 B", new Vector3(2.8f, 10.05f, 4), player.transform, new Vector3[]{new Vector3(2.8f,10.05f,0), new Vector3(2.8f,10.05f,14)}, robotMat, red);
        MakeGuard("ROBOT FLOOR 3 C", new Vector3(0, 10.05f, 20), player.transform, new Vector3[]{new Vector3(0,10.05f,14), new Vector3(0,10.05f,24)}, robotMat, red);

        var amb = new GameObject("Background Horror Music"); var au=amb.AddComponent<AudioSource>(); au.clip=AssetClip("Assets/Audio/background.wav", "HUM", 3f, 50); au.loop=true; au.spatialBlend=0f; au.volume=.45f; au.Play();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/EscapeFromGuard_Horror.unity");
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/Scenes/EscapeFromGuard_Horror.unity", true)
        };
        if (showDialog) EditorUtility.DisplayDialog("Escape From Guard", "Готово: создана игровая сцена. Старт теперь сразу в игре.", "OK");
    }

    static void BuildFloor(int n, float y, Material wallA, Material wallB, Material floor, Material ceil, Material door, Material frame)
    {
        string p = "F"+n+" ";
        Cube(p+"floor", new Vector3(0,y-0.05f,0), new Vector3(16,.1f,60), floor, true);
        // Floor 3 is the final jetpack area: leave the roof open so the player can fly upward/outside.
        if (n != 3) Cube(p+"ceiling", new Vector3(0,y+3.05f,0), new Vector3(16,.22f,60), ceil, true);
        else
        {
            Cube(p+"partial ceiling back", new Vector3(0,y+3.05f,-17), new Vector3(16,.22f,26), ceil, true);
            Cube(p+"partial ceiling middle", new Vector3(0,y+3.05f,2), new Vector3(16,.22f,8), ceil, true);
            // no ceiling over the final exit / jetpack zone (z 10..30)
        }
        Cube(p+"left outer", new Vector3(-8,y+1.5f,0), new Vector3(.35f,3,60), wallA, true);
        Cube(p+"right outer", new Vector3(8,y+1.5f,0), new Vector3(.35f,3,60), wallA, true);
        Cube(p+"back", new Vector3(0,y+1.5f,-30), new Vector3(16,3,.35f), wallA, true);
        Cube(p+"front", new Vector3(0,y+1.5f,30), new Vector3(16,3,.35f), wallA, true);
        // corridor side walls with tight door segments
        foreach(float z in new float[]{-20,-8,4,16})
        {
            Room(p+"L"+z, -4.2f, -6.2f, z, y, wallA, wallB, floor, ceil, door, frame, n==2 && z==4);
            Room(p+"R"+z, 4.2f, 6.2f, z+4, y, wallB, wallA, floor, ceil, door, frame, n==3 && z==16);
        }
        for(float z=-25; z<=25; z+=10) Lamp(new Vector3(0,y+2.7f,z));
        FloorSign(p+"floor label", "ЭТАЖ " + n, new Vector3(0,y+0.07f,-26.2f), Color.white, .28f);
    }

    // corridorWallX is where the wall/door sits. roomCenterX is outside corridor. Door is exactly in the opening.
    static void Room(string name, float corridorWallX, float roomCenterX, float z, float y, Material wall1, Material wall2, Material floor, Material ceil, Material doorMat, Material frameMat, bool locked)
    {
        int side = roomCenterX < 0 ? -1 : 1;
        float roomW = 5.2f; float roomD = 3.6f; float open = 1.8f;
        Cube(name+" floor", new Vector3(roomCenterX,y,z), new Vector3(roomD,.08f,roomW), floor, true);
        Cube(name+" ceiling", new Vector3(roomCenterX,y+3,z), new Vector3(roomD,.12f,roomW), ceil, true);
        Cube(name+" outer wall", new Vector3(roomCenterX + side*roomD/2,y+1.5f,z), new Vector3(.25f,3,roomW), wall2, true);
        Cube(name+" back z wall", new Vector3(roomCenterX,y+1.5f,z-roomW/2), new Vector3(roomD,3,.25f), wall2, true);
        Cube(name+" front z wall", new Vector3(roomCenterX,y+1.5f,z+roomW/2), new Vector3(roomD,3,.25f), wall2, true);
        float seg = (roomW-open)/2f;
        Cube(name+" corridor wall A", new Vector3(corridorWallX,y+1.5f,z-roomW/2+seg/2), new Vector3(.32f,3,seg), wall1, true);
        Cube(name+" corridor wall B", new Vector3(corridorWallX,y+1.5f,z+roomW/2-seg/2), new Vector3(.32f,3,seg), wall1, true);
        Cube(name+" lintel", new Vector3(corridorWallX,y+2.65f,z), new Vector3(.38f,.7f,open), wall1, true);
        Cube(name+" frame left", new Vector3(corridorWallX,y+1.2f,z-open/2-.08f), new Vector3(.44f,2.4f,.12f), frameMat, true);
        Cube(name+" frame right", new Vector3(corridorWallX,y+1.2f,z+open/2+.08f), new Vector3(.44f,2.4f,.12f), frameMat, true);
        var d = Cube(name+" DOOR", new Vector3(corridorWallX + side*.04f,y+1.05f,z), new Vector3(.16f,2.1f,1.65f), doorMat, false);
        var od = d.AddComponent<OpenableDoor>(); od.locked=locked; od.requiredItemId="Keycard"; od.openAngle = side < 0 ? -95 : 95; od.lockedMessage = locked ? "Дверь закрыта. Нужна ключ-карта" : "";
    }

    static void MakeTeleporter(string label, Vector3 pos, Transform target, Material mat, string msg)
    {
        var pad = Cube(label, pos, new Vector3(4,.18f,4), mat, false); pad.GetComponent<Collider>().isTrigger=true;
        var tp = pad.AddComponent<FloorTeleporter>(); tp.targetPoint=target; tp.message=msg;
        var l = new GameObject(label+" light"); l.transform.position=pos+Vector3.up*1.1f; var li=l.AddComponent<Light>(); li.type=LightType.Point; li.color=Color.cyan; li.range=9; li.intensity=3.2f;
        FloorSign(label+" floor text", label, pos + new Vector3(0,.12f,-2.8f), Color.cyan, .23f);
    }

    static void MakeKeycard(Vector3 pos, Material mat, string itemId, string display, string pickupMessage)
    {
        GameObject o;
        if (itemId == "Jetpack")
        {
            o = new GameObject(display);
            o.transform.position = pos;
            var trigger = o.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.4f, 1.4f, 1.0f);
            trigger.center = new Vector3(0, .55f, 0);
            CylinderChild(o, "left fuel tank", new Vector3(-.32f,.65f,0), new Vector3(.28f,.75f,.28f), mat);
            CylinderChild(o, "right fuel tank", new Vector3(.32f,.65f,0), new Vector3(.28f,.75f,.28f), mat);
            CubeChild(o, "center backpack body", new Vector3(0,.65f,0), new Vector3(.46f,.72f,.25f), mat);
            CubeChild(o, "top handle", new Vector3(0,1.08f,0), new Vector3(.85f,.08f,.15f), mat);
            CubeChild(o, "left nozzle", new Vector3(-.32f,.18f,0), new Vector3(.20f,.20f,.20f), mat);
            CubeChild(o, "right nozzle", new Vector3(.32f,.18f,0), new Vector3(.20f,.20f,.20f), mat);
        }
        else
        {
            o=Cube(display, pos, new Vector3(.8f,.12f,.5f), mat, false);
            o.GetComponent<Collider>().isTrigger=true;
        }
        var item=o.AddComponent<InteractableItem>(); item.itemId=itemId; item.displayName=display; item.pickupMessage=pickupMessage;
        var l=new GameObject(display+" light"); l.transform.position=pos+Vector3.up*.9f; var li=l.AddComponent<Light>(); li.type=LightType.Point; li.color=mat.color; li.range=6; li.intensity=2.5f;
        FloorSign(display+" floor text", display.ToUpper(), pos + new Vector3(0,.08f,-1.15f), Color.white, .18f);
    }

    static void MakeRoomLoot(Vector3 pos, Material mat, string itemId, string display, string pickupMessage, int ammoAmount, float batteryAmount, string noteText)
    {
        GameObject o;
        if (itemId == "Ammo")
        {
            o = Cube(display, pos, new Vector3(.9f,.34f,.55f), mat, false);
            CubeChild(o, "ammo lid", new Vector3(0,.22f,0), new Vector3(.95f,.08f,.58f), mat);
        }
        else if (itemId == "Battery")
        {
            o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            o.name = display;
            o.transform.position = pos + Vector3.up * .18f;
            o.transform.localScale = new Vector3(.25f,.38f,.25f);
            o.GetComponent<Renderer>().sharedMaterial = mat;
        }
        else
        {
            o = Cube(display, pos, new Vector3(.7f,.06f,.9f), mat, false);
        }

        var col = o.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        var item = o.AddComponent<InteractableItem>();
        item.itemId = itemId;
        item.displayName = display;
        item.pickupMessage = pickupMessage;
        item.ammoAmount = ammoAmount;
        item.batteryAmount = batteryAmount;
        item.noteText = noteText;
        item.addToInventory = itemId == "SecretFile";

        var l = new GameObject(display + " loot light");
        l.transform.position = pos + Vector3.up * .75f;
        var li = l.AddComponent<Light>();
        li.type = LightType.Point;
        li.color = mat.color;
        li.range = 4.5f;
        li.intensity = 1.8f;
    }

    static Transform MakeOutsideFinale(Material yellow, Material blue, Material floor, Material wall, Material ceil)
    {
        // Outside area is far from the building so the player clearly leaves the level.
        Cube("OUTSIDE ground", new Vector3(55, 8.9f, 26), new Vector3(42, .18f, 42), floor, true);
        Cube("OUTSIDE back wall", new Vector3(55, 10.4f, 47), new Vector3(42, 3, .35f), wall, true);
        Cube("OUTSIDE left wall", new Vector3(34, 10.4f, 26), new Vector3(.35f, 3, 42), wall, true);
        Cube("OUTSIDE right wall", new Vector3(76, 10.4f, 26), new Vector3(.35f, 3, 42), wall, true);
        // No outside roof: this is the open-sky final jetpack flight arena.
        var spawn = Empty("OUTSIDE JETPACK SPAWN", new Vector3(43, 10.1f, 26));
        MakeEscapePlatform(new Vector3(68, 12.2f, 26), yellow, true, null);
        var marker=Cube("BLUE OUTSIDE START MARKER", new Vector3(43, 9.05f, 26), new Vector3(4,.25f,4), blue, false); marker.GetComponent<Collider>().enabled=false;
        var sun=new GameObject("Outside soft moon light"); sun.transform.position=new Vector3(55,17,26); var li=sun.AddComponent<Light>(); li.type=LightType.Point; li.color=new Color(.55f,.7f,1f); li.range=45; li.intensity=4.5f;
        FloorSign("Outside objective floor text", "ЛЕТИ НА ЖЁЛТУЮ ПЛАТФОРМУ", new Vector3(55,9.06f,17), Color.cyan, .22f);
        return spawn.transform;
    }

    static void MakeEscapePlatform(Vector3 pos, Material mat, bool outsideLanding, Transform outsideSpawn)
    {
        var p=Cube(outsideLanding ? "OUTSIDE WIN LANDING PLATFORM" : "FINAL EXIT TO OUTSIDE PLATFORM", pos, new Vector3(5,.25f,5), mat, false);
        p.GetComponent<Collider>().isTrigger=true;
        var ep = p.AddComponent<EscapePlatform>(); ep.isOutsideLandingPlatform = outsideLanding; ep.outsideSpawnPoint = outsideSpawn;
        var l=new GameObject((outsideLanding ? "Outside win" : "Final") + " light"); l.transform.position=pos+Vector3.up*1.5f; var li=l.AddComponent<Light>(); li.type=LightType.Point; li.color=new Color(1,.7f,.15f); li.range=12; li.intensity=4;
        FloorSign((outsideLanding ? "Outside win" : "Final") + " floor text", outsideLanding ? "ПОСАДОЧНАЯ ПЛАТФОРМА" : "ВЫХОД НАРУЖУ", pos + new Vector3(0,.18f,-3.1f), Color.yellow, .20f);
    }

    static void MakeGuard(string name, Vector3 basePos, Transform player, Vector3[] patrol, Material body, Material eye)
    {
        var g = new GameObject(name); g.transform.position=basePos;
        CubeChild(g,"body",new Vector3(0,1.0f,0),new Vector3(1.25f,1.8f,.9f),body);
        CubeChild(g,"head",new Vector3(0,2.1f,.05f),new Vector3(1.0f,.65f,.75f),body);
        CubeChild(g,"RED EYE",new Vector3(0,2.12f,.48f),new Vector3(.75f,.22f,.08f),eye);
        CubeChild(g,"left arm",new Vector3(-.9f,1.0f,.05f),new Vector3(.2f,1.3f,.2f),body);
        CubeChild(g,"right arm",new Vector3(.9f,1.0f,.05f),new Vector3(.2f,1.3f,.2f),body);
        var col=g.AddComponent<CapsuleCollider>(); col.center=new Vector3(0,1.1f,0); col.height=2.4f; col.radius=.65f;
        var ai=g.AddComponent<GuardAI>(); ai.player=player; ai.detectionDistance=14; ai.instantDetectionDistance=2.1f; ai.fieldOfView=105; ai.patrolSpeed=1.9f; ai.chaseSpeed=4.4f; ai.catchDistance=2.2f;
        Transform[] pts = new Transform[patrol.Length]; for(int i=0;i<patrol.Length;i++){ var p=Empty(name+" patrol "+i, patrol[i]); pts[i]=p.transform; } ai.patrolPoints=pts;
        var l=new GameObject(name+" red light"); l.transform.SetParent(g.transform); l.transform.localPosition=new Vector3(0,2.2f,.65f); var li=l.AddComponent<Light>(); li.type=LightType.Point; li.color=Color.red; li.range=9; li.intensity=5.5f;
        FloorSign(name+" floor warning", "ОПАСНО: РОБОТ", basePos+new Vector3(0,.07f,-2.2f), Color.red, .18f);
    }

    static void BuildHands(Transform cam, Material hand, Material metal, Material flashMat, GunController gun)
    {
        var root = new GameObject("GunRoot visible recoil"); root.transform.SetParent(cam); root.transform.localPosition=new Vector3(.16f,-.45f,.68f); gun.gunRoot=root.transform;
        CubeChild(root,"left hand",new Vector3(-.18f,-.04f,.12f),new Vector3(.08f,.09f,.36f),hand);
        CubeChild(root,"right hand",new Vector3(.12f,-.04f,.12f),new Vector3(.08f,.09f,.36f),hand);
        CubeChild(root,"gun grip",new Vector3(.05f,-.10f,.36f),new Vector3(.08f,.16f,.07f),metal);
        CubeChild(root,"gun body",new Vector3(.05f,.02f,.50f),new Vector3(.16f,.08f,.28f),metal);
        CubeChild(root,"gun barrel",new Vector3(.05f,.025f,.73f),new Vector3(.07f,.06f,.23f),metal);
        var flash=Cube("BIG MUZZLE FLASH", Vector3.zero, new Vector3(.32f,.32f,.32f), flashMat, false); flash.transform.SetParent(root.transform); flash.transform.localPosition=new Vector3(.05f,.03f,.95f); flash.SetActive(false); gun.muzzleFlashObject=flash;
        var light=new GameObject("Muzzle flash light"); light.transform.SetParent(root.transform); light.transform.localPosition=new Vector3(.05f,.03f,.95f); var li=light.AddComponent<Light>(); li.type=LightType.Point; li.range=7; li.intensity=0; gun.muzzleFlash=li; li.enabled=false;
    }

    static void AddHorrorPolish(Material danger, Material stripe, Material cable)
    {
        var moon = new GameObject("Cold moon shaft light");
        moon.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        var dl = moon.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.color = new Color(.55f,.68f,1f);
        dl.intensity = .42f;
        dl.shadows = LightShadows.Soft;

        for (int floorIndex = 0; floorIndex < 3; floorIndex++)
        {
            float y = floorIndex * 5f;
            for (float z = -22f; z <= 22f; z += 11f)
            {
                var alarm = new GameObject("Red emergency strip F" + (floorIndex + 1));
                alarm.transform.position = new Vector3(0, y + 2.86f, z);
                var light = alarm.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f,.05f,.03f);
                light.range = 8f;
                light.intensity = 1.65f;

                Cube("Ceiling red glow " + floorIndex + " " + z, new Vector3(0, y + 2.93f, z), new Vector3(3.4f,.045f,.16f), danger, false);
                Cube("Floor hazard stripe L " + floorIndex + " " + z, new Vector3(-1.45f, y + .011f, z), new Vector3(1.25f,.025f,.1f), stripe, false);
                Cube("Floor hazard stripe R " + floorIndex + " " + z, new Vector3(1.45f, y + .011f, z), new Vector3(1.25f,.025f,.1f), stripe, false);
            }

            for (float z = -25f; z <= 25f; z += 10f)
            {
                Cube("Ceiling cable A " + floorIndex + " " + z, new Vector3(-3.2f, y + 2.89f, z), new Vector3(.08f,.06f,7.5f), cable, false);
                Cube("Ceiling cable B " + floorIndex + " " + z, new Vector3(3.2f, y + 2.89f, z), new Vector3(.08f,.06f,7.5f), cable, false);
            }
        }
    }

    static void CreateUI(GameManager gm, PauseMenu pm, InventorySystem inv, GunController gun, PlayerInteractor inter)
    {
        var cObj=new GameObject("Canvas"); var c=cObj.AddComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceOverlay; cObj.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; cObj.AddComponent<GraphicRaycaster>();
        gun.ammoText = TextUI("AMMO CLEAR", cObj.transform, "ПАТРОНЫ: 12 / 12", 22, new Vector2(0,1), new Vector2(0,1), new Vector2(168,-35), new Vector2(300,34), TextAnchor.MiddleLeft);
        inv.inventoryText = TextUI("Inventory", cObj.transform, "КЛЮЧ-КАРТА: НЕТ\nДЖЕТПАК: НЕТ", 18, new Vector2(0,1), new Vector2(0,1), new Vector2(178,-85), new Vector2(320,54), TextAnchor.MiddleLeft);
        gm.batteryText = TextUI("Battery", cObj.transform, "ФОНАРИК: 100%", 18, new Vector2(0,1), new Vector2(0,1), new Vector2(158,-127), new Vector2(280,30), TextAnchor.MiddleLeft);
        gm.statusText = TextUI("Status", cObj.transform, "", 22, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-30), new Vector2(850,38), TextAnchor.MiddleCenter);
        inter.promptText = TextUI("Prompt", cObj.transform, "", 28, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,-170), new Vector2(850,65), TextAnchor.MiddleCenter);
        TextUI("Crosshair", cObj.transform, "+", 30, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(50,50));
        gm.caughtPanel=Panel(cObj.transform,new Color(.28f,0,0,.90f)); TextUI("CaughtText", gm.caughtPanel.transform, "ТЫ ПРОИГРАЛ\nТебя поймал наблюдатель", 48, Vector2.zero, Vector2.one, new Vector2(0,65), new Vector2(900,130)); ButtonUI("RestartCaught",gm.caughtPanel.transform,"Рестарт",new Vector2(0,-70)).onClick.AddListener(()=>gm.RestartLevel()); gm.caughtPanel.SetActive(false);
        gm.winPanel=Panel(cObj.transform,new Color(0,.10f,.04f,.90f)); TextUI("WinText", gm.winPanel.transform, "ТЫ ВЫБРАЛСЯ\nПосле третьего этажа ты улетел из комплекса", 42, Vector2.zero, Vector2.one, new Vector2(0,65), new Vector2(1000,135)); ButtonUI("RestartWin",gm.winPanel.transform,"Играть заново",new Vector2(0,-75)).onClick.AddListener(()=>gm.RestartLevel()); gm.winPanel.SetActive(false);
        gm.notePanel=Panel(cObj.transform,new Color(0,0,0,.82f)); gm.noteText=TextUI("NoteText", gm.notePanel.transform, "", 26, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,0), new Vector2(760,220)); gm.notePanel.SetActive(false);
        var pause=Panel(cObj.transform,new Color(0,0,0,.78f)); TextUI("PauseTitle",pause.transform,"ПАУЗА",42,Vector2.zero,Vector2.one,new Vector2(0,120),new Vector2(500,90)); ButtonUI("Resume",pause.transform,"Продолжить",new Vector2(0,30)).onClick.AddListener(()=>pm.Resume()); ButtonUI("Restart",pause.transform,"Рестарт",new Vector2(0,-35)).onClick.AddListener(()=>gm.RestartLevel()); pm.pausePanel=pause; pause.SetActive(false);
        var es=new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
    }

    static void CreateMainMenuScene(bool showDialog)
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Audio");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientLight = new Color(.08f,.09f,.11f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.01f,.012f,.018f);
        RenderSettings.fogDensity = .02f;

        var dark = Mat("Menu dark floor", new Color(.04f,.04f,.045f));
        var red = Mat("Menu red glow", new Color(.75f,.05f,.02f), true);
        var brown = Mat("Menu brown doors", new Color(.42f,.20f,.08f));
        Cube("Menu floor", new Vector3(0,-.1f,3), new Vector3(16,.2f,18), dark, true);
        Cube("Menu back wall", new Vector3(0,2.5f,10), new Vector3(16,5,.3f), dark, true);
        Cube("Menu left wall", new Vector3(-8,2.5f,3), new Vector3(.3f,5,18), dark, true);
        Cube("Menu right wall", new Vector3(8,2.5f,3), new Vector3(.3f,5,18), dark, true);
        Cube("Menu horror door", new Vector3(0,1.4f,8.7f), new Vector3(2.4f,2.8f,.18f), brown, true);
        Cube("Menu red eye", new Vector3(0,2.2f,8.55f), new Vector3(.7f,.18f,.08f), red, false);
        var lamp = new GameObject("Menu red light"); lamp.transform.position = new Vector3(0,3.2f,6.5f); var li=lamp.AddComponent<Light>(); li.type=LightType.Point; li.color=Color.red; li.range=12; li.intensity=3.2f;
        var camObj = new GameObject("Menu Camera"); camObj.transform.position = new Vector3(0,1.8f,-4); camObj.transform.rotation = Quaternion.Euler(8,0,0); camObj.AddComponent<Camera>(); camObj.AddComponent<AudioListener>();
        var menuMusic = new GameObject("Menu Background Horror Music"); var menuAu=menuMusic.AddComponent<AudioSource>(); menuAu.clip=AssetClip("Assets/Audio/background.wav", "MENU_HUM", 3f, 42); menuAu.loop=true; menuAu.spatialBlend=0f; menuAu.volume=.38f; menuAu.Play();

        var cObj=new GameObject("MainMenuCanvas"); var c=cObj.AddComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceOverlay; cObj.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; cObj.AddComponent<GraphicRaycaster>();
        var controller = cObj.AddComponent<MainMenuController>();
        TextUI("Title", cObj.transform, "ESCAPE FROM GUARD", 54, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-95), new Vector2(900,90));
        TextUI("Subtitle", cObj.transform, "Хоррор-проект Unity: меню, настройки, AI, NavMesh, оружие, джетпак", 22, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-155), new Vector2(1000,45));
        var start = ButtonUI("StartButton", cObj.transform, "НАЧАТЬ ИГРУ", new Vector2(0,70)); start.onClick.AddListener(controller.PlayGame);
        var settings = ButtonUI("SettingsButton", cObj.transform, "НАСТРОЙКИ", new Vector2(0,5)); settings.onClick.AddListener(controller.ToggleSettings);
        var quit = ButtonUI("QuitButton", cObj.transform, "ВЫХОД", new Vector2(0,-60)); quit.onClick.AddListener(controller.QuitGame);
        controller.settingsPanel = Panel(cObj.transform, new Color(0,0,0,.82f)); controller.settingsPanel.name = "SettingsPanel";
        TextUI("SettingsTitle", controller.settingsPanel.transform, "НАСТРОЙКИ", 36, Vector2.zero, Vector2.one, new Vector2(0,120), new Vector2(600,70));
        TextUI("VolumeLabel", controller.settingsPanel.transform, "Громкость", 24, Vector2.zero, Vector2.one, new Vector2(-170,45), new Vector2(220,50), TextAnchor.MiddleLeft);
        var sliderObj = new GameObject("VolumeSlider"); sliderObj.transform.SetParent(controller.settingsPanel.transform); var slider = sliderObj.AddComponent<Slider>(); slider.minValue=0; slider.maxValue=1; slider.value=AudioListener.volume; var rt=sliderObj.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f); rt.anchoredPosition=new Vector2(105,45); rt.sizeDelta=new Vector2(260,30); controller.volumeSlider=slider; slider.onValueChanged.AddListener(controller.SetVolume);
        var back = ButtonUI("BackButton", controller.settingsPanel.transform, "НАЗАД", new Vector2(0,-70)); back.onClick.AddListener(controller.ToggleSettings);
        controller.settingsPanel.SetActive(false);
        var es=new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        if (showDialog) EditorUtility.DisplayDialog("Main Menu", "Главное меню создано: Assets/Scenes/MainMenu.unity", "OK");
    }

    static GameObject Empty(string name, Vector3 pos){ var g=new GameObject(name); g.transform.position=pos; return g; }
    static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat, bool stat){ var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=pos; go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=mat; go.isStatic=stat; return go; }
    static void CubeChild(GameObject parent,string name,Vector3 local,Vector3 scale,Material mat){ var c=Cube(name,Vector3.zero,scale,mat,false); c.transform.SetParent(parent.transform); c.transform.localPosition=local; c.transform.localRotation=Quaternion.identity; }
    static void CylinderChild(GameObject parent,string name,Vector3 local,Vector3 scale,Material mat){ var c=GameObject.CreatePrimitive(PrimitiveType.Cylinder); c.name=name; c.transform.SetParent(parent.transform); c.transform.localPosition=local; c.transform.localRotation=Quaternion.identity; c.transform.localScale=scale; c.GetComponent<Renderer>().sharedMaterial=mat; var col=c.GetComponent<Collider>(); if(col!=null) col.enabled=false; }
    static void FloorSign(string name, string val, Vector3 pos, Color color, float size)
    {
        // v21.6: world labels and floor markers are completely disabled. HUD only.
    }
    static Material Mat(string name, Color color, bool emission=false){ string path="Assets/Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path); if(!mat){ mat=new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat,path);} mat.color=color; mat.SetFloat("_Glossiness", emission ? .55f : .22f); if(emission){ mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color*1.85f); } else { mat.DisableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Color.black); } EditorUtility.SetDirty(mat); return mat; }
    static void Lamp(Vector3 pos){ var o=new GameObject("Cold flicker lamp"); o.transform.position=pos; var l=o.AddComponent<Light>(); l.type=LightType.Point; l.range=13; l.intensity=2.1f; l.color=new Color(.72f,.86f,1f); var flicker=o.AddComponent<FlickerLight>(); flicker.minIntensity=.75f; flicker.maxIntensity=2.4f; flicker.speed=9f; }
    static GameObject Panel(Transform parent, Color color){ var p=new GameObject("Panel"); p.transform.SetParent(parent); var img=p.AddComponent<Image>(); img.color=color; var rt=p.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; return p; }
    static Text TextUI(string name, Transform parent, string text, int size, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 sd, TextAnchor align=TextAnchor.MiddleCenter){ var go=new GameObject(name); go.transform.SetParent(parent); var t=go.AddComponent<Text>(); t.text=text; t.fontSize=size; t.color=Color.white; t.alignment=align; t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); var rt=go.GetComponent<RectTransform>(); rt.anchorMin=amin; rt.anchorMax=amax; rt.anchoredPosition=pos; rt.sizeDelta=sd; return t; }
    static Button ButtonUI(string name, Transform parent, string label, Vector2 pos){ var go=new GameObject(name); go.transform.SetParent(parent); var img=go.AddComponent<Image>(); img.color=new Color(.09f,.03f,.03f,.95f); var b=go.AddComponent<Button>(); var rt=go.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(260,50); TextUI(label,go.transform,label,22,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero); return b; }
    static void Text3D(string name, string val, Vector3 pos, Quaternion rot, Color color, float size){ var go=new GameObject(name); go.transform.position=pos; go.transform.rotation=rot; var tm=go.AddComponent<TextMesh>(); tm.text=val; tm.fontSize=64; tm.characterSize=size; tm.color=color; tm.anchor=TextAnchor.MiddleCenter; tm.alignment=TextAlignment.Center; }
    static AudioClip AssetClip(string path, string fallbackName, float fallbackLength, float fallbackFreq){ if(File.Exists(path)) AssetDatabase.ImportAsset(path); var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path); return clip != null ? clip : Clip(fallbackName, fallbackLength, fallbackFreq); }
    static AudioClip Clip(string name, float length, float freq){ int rate=44100; int samples=Mathf.CeilToInt(rate*length); float[] data=new float[samples]; for(int i=0;i<samples;i++){ float t=(float)i/rate; data[i]=Mathf.Sin(2*Mathf.PI*freq*t)*Mathf.Exp(-t*8f); } var clip=AudioClip.Create(name,samples,1,rate,false); clip.SetData(data,0); return clip; }
}
#endif
