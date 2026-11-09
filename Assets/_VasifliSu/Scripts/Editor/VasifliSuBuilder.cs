#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace VasifliSu.EditorTools
{
    /// <summary>
    /// Tek tıkla projeyi kurar: import ayarları, malzemeler, bölüm verileri, kamyon prefabı,
    /// tüm sahneler (Ana Menü, Bölüm Seçimi, 5 Bölüm) ve Build Settings.
    /// Menü: Vasıflı Su / 1) Projeyi Kur
    /// </summary>
    public static class VasifliSuBuilder
    {
        const string Root = "Assets/_VasifliSu";
        const string Art = Root + "/Art";

        // ---------------------------------------------------------------- MENÜ
        [MenuItem("Vasıflı Su/1) Projeyi Kur (Sahneler + Prefab)", priority = 1)]
        public static void BuildAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Vasıflı Su", "Klasörler", 0.05f);
                EnsureFolders();
                EditorUtility.DisplayProgressBar("Vasıflı Su", "Görsel import ayarları", 0.1f);
                ConfigureImports();
                EditorUtility.DisplayProgressBar("Vasıflı Su", "Varlıklar", 0.3f);
                var assets = BuildGameAssets();
                var physics = BuildPhysicsMaterials();
                var levels = BuildLevels(physics);
                EditorUtility.DisplayProgressBar("Vasıflı Su", "Kamyon prefabı", 0.45f);
                var truck = BuildTruckPrefab(physics);
                EditorUtility.DisplayProgressBar("Vasıflı Su", "Sahneler", 0.6f);
                var scenes = new List<string>();
                scenes.Add(BuildMainMenu());
                scenes.Add(BuildLevelSelect(levels));
                for (int i = 0; i < levels.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Vasıflı Su", "Bölüm " + (i + 1), 0.6f + i * 0.07f);
                    scenes.Add(BuildLevelScene(i, levels[i], truck, physics));
                }
                var list = new List<EditorBuildSettingsScene>();
                foreach (var s in scenes) list.Add(new EditorBuildSettingsScene(s, true));
                EditorBuildSettings.scenes = list.ToArray();
                ConfigurePlayer();
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(scenes[0]);
                Debug.Log("<color=#3FD0E8><b>Vasıflı Su kurulumu tamamlandı.</b></color> Play'e basarak oyunu başlatabilirsin.");
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        [MenuItem("Vasıflı Su/2) Windows Build (.exe)", priority = 20)]
        public static void BuildWindows()
        {
            var opts = new BuildPlayerOptions
            {
                scenes = SceneList(),
                locationPathName = "Builds/Windows/VasifliSu.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var r = BuildPipeline.BuildPlayer(opts);
            Debug.Log("Windows build: " + r.summary.result + "  ->  " + Path.GetFullPath("Builds/Windows"));
        }

        [MenuItem("Vasıflı Su/3) WebGL Build", priority = 21)]
        public static void BuildWebGL()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            var opts = new BuildPlayerOptions
            {
                scenes = SceneList(),
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            var r = BuildPipeline.BuildPlayer(opts);
            Debug.Log("WebGL build: " + r.summary.result + "  ->  " + Path.GetFullPath("Builds/WebGL"));
        }

        static string[] SceneList()
        {
            var l = new List<string>();
            foreach (var s in EditorBuildSettings.scenes) if (s.enabled) l.Add(s.path);
            return l.ToArray();
        }

        // ---------------------------------------------------------------- KURULUM
        static void EnsureFolders()
        {
            foreach (var f in new[] { "Data", "Prefabs", "Scenes", "Materials", "Resources", "PhysicsMaterials" })
                Directory.CreateDirectory(Root + "/" + f);
            AssetDatabase.Refresh();
        }

        static void ConfigureImports()
        {
            var nine = new Dictionary<string, Vector4>
            {
                { "button", new Vector4(44, 44, 44, 44) },
                { "button_alt", new Vector4(44, 44, 44, 44) },
                { "panel", new Vector4(60, 60, 60, 60) },
                { "panel_dark", new Vector4(60, 60, 60, 60) },
                { "bar_bg", new Vector4(22, 22, 22, 22) },
                { "bar_fill", new Vector4(18, 18, 18, 18) },
                { "card", new Vector4(48, 48, 48, 48) },
            };
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Art }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                bool ground = path.Contains("/Ground/");
                bool bg = path.Contains("/Backgrounds/");
                if (ground)
                {
                    imp.textureType = TextureImporterType.Default;
                    imp.wrapMode = TextureWrapMode.Repeat;
                    imp.mipmapEnabled = true;
                }
                else
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    imp.spritePixelsPerUnit = 100;
                    imp.mipmapEnabled = false;
                    imp.alphaIsTransparency = true;
                    var st = new TextureImporterSettings();
                    imp.ReadTextureSettings(st);
                    st.spriteMeshType = SpriteMeshType.FullRect;
                    st.spriteAlignment = (int)SpriteAlignment.Center;
                    imp.SetTextureSettings(st);
                    imp.wrapMode = bg ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                    imp.spriteBorder = nine.TryGetValue(name, out var b) ? b : Vector4.zero;
                }
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.maxTextureSize = 2048;
                imp.SaveAndReimport();
            }
        }

        static T Load<T>(string path) where T : Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) Debug.LogWarning("Bulunamadı: " + path);
            return a;
        }
        static Sprite S(string rel) => Load<Sprite>(Art + "/" + rel + ".png");
        static AudioClip Clip(string n) => Load<AudioClip>(Root + "/Audio/" + n + ".wav");

        static T CreateOrReplace<T>(string path, T obj) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                string keepName = existing.name;
                EditorUtility.CopySerialized(obj, existing);
                existing.name = string.IsNullOrEmpty(keepName) ? Path.GetFileNameWithoutExtension(path) : keepName;
                Object.DestroyImmediate(obj, true);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(obj, path);
            return obj;
        }

        static GameAssets BuildGameAssets()
        {
            var a = ScriptableObject.CreateInstance<GameAssets>();
            a.fontBold = Load<Font>(Root + "/Fonts/Inter-ExtraBold.otf");
            a.fontRegular = Load<Font>(Root + "/Fonts/Inter-Medium.otf");
            a.logo = S("UI/logo"); a.menuBackground = S("UI/menu_bg");
            a.button = S("UI/button"); a.buttonAlt = S("UI/button_alt"); a.buttonRound = S("UI/button_round");
            a.panel = S("UI/panel"); a.panelDark = S("UI/panel_dark");
            a.barBg = S("UI/bar_bg"); a.barFill = S("UI/bar_fill"); a.card = S("UI/card");
            a.iconWater = S("Icons/water"); a.iconSpeed = S("Icons/speed"); a.iconTimer = S("Icons/timer");
            a.iconStar = S("Icons/star"); a.iconStarEmpty = S("Icons/star_empty"); a.iconLock = S("Icons/lock");
            a.iconPause = S("Icons/pause"); a.iconGas = S("Icons/gas"); a.iconBrake = S("Icons/brake");
            a.iconSettings = S("Icons/settings"); a.iconHome = S("Icons/home"); a.iconRetry = S("Icons/retry");
            a.iconNext = S("Icons/next"); a.iconPlay = S("Icons/play"); a.iconDoc = S("Icons/doc");
            a.iconExit = S("Icons/exit"); a.iconFlag = S("Environment/flag");
            a.rock = S("Environment/rock"); a.depot = S("Environment/depot");
            a.deliveryPoint = S("Environment/delivery"); a.splash = S("Environment/splash"); a.coin = S("Environment/coin");
            a.music = Clip("music"); a.click = Clip("click"); a.splashSfx = Clip("splash");
            a.engine = Clip("engine"); a.win = Clip("win"); a.lose = Clip("lose"); a.beep = Clip("beep"); a.go = Clip("go"); a.coinSfx = Clip("coin");
            return CreateOrReplace(Root + "/Resources/GameAssets.asset", a);
        }

        struct Phys { public PhysicsMaterial2D ground, wheel, cargo, body; }

        static PhysicsMaterial2D PM(string name, float friction, float bounce)
        {
            var m = new PhysicsMaterial2D(name) { friction = friction, bounciness = bounce };
            return CreateOrReplace(Root + "/PhysicsMaterials/" + name + ".physicsMaterial2D", m);
        }

        static Phys BuildPhysicsMaterials()
        {
            return new Phys
            {
                ground = PM("Zemin", 1f, 0f),
                wheel = PM("Lastik", 1.3f, 0.05f),
                cargo = PM("Damacana", 1.1f, 0f),
                body = PM("Govde", 0.4f, 0f),
            };
        }

        static Material Mat(string name, Texture tex)
        {
            var m = new Material(Shader.Find("Sprites/Default")) { name = name };
            if (tex != null) m.mainTexture = tex;
            return CreateOrReplace(Root + "/Materials/" + name + ".mat", m);
        }

        static LevelData[] BuildLevels(Phys phys)
        {
            var grass = Mat("Cim", null);
            var defs = new[]
            {
                new { bld=new[]{"volcano_observatory","volcano_hut","billboard","volcano_vent"}, obs=new[]{"obs_basalt","obs_barrel","obs_basalt"}, sig=new[]{"sign_caution","sign_brand","sign_slope","sign_delivery"}, snow=false, n="Volkan Eteği", cu="Volkan Gözlemevi", sub="Öğretici teslimat", d="Volkan eteğindeki gözlemevine ilk teslimat. Kontrolleri öğren, lav çatlaklarına dikkat!", len=170f, amp=2.2f, fr=0.036f, ro=0.12f, cl=0f, seed=11, fric=1f, rocks=4, cnt=6, req=0.5f, par=40f, sky=new Color32(50,20,35,255), gr=new Color32(70,55,58,255), dec=new[]{"dead_tree","rock_decor","volcano_vent"}, sp=9f },
                new { bld=new[]{"city_shop","city_apartment","city_office","city_hospital","billboard"}, obs=new[]{"obs_barrier","obs_cone","obs_barrel"}, sig=new[]{"sign_caution","sign_brand","sign_slope","sign_delivery"}, snow=false, n="Şehir Merkezi", cu="Merkez Hastanesi", sub="Gece teslimatı", d="Gece vardiyasındaki şehir hastanesine acil su! Şantiye engellerine dikkat.", len=220f, amp=2.8f, fr=0.035f, ro=0.15f, cl=0f, seed=23, fric=0.95f, rocks=6, cnt=7, req=0.5f, par=52f, sky=new Color32(12,16,42,255), gr=new Color32(160,160,172,255), dec=new[]{"streetlamp","tree","streetlamp"}, sp=7f },
                new { bld=new[]{"barn","house_red","water_tower","billboard"}, obs=new[]{"obs_bale","obs_barrel","obs_crate"}, sig=new[]{"sign_slope","sign_caution","sign_brand","sign_delivery"}, snow=false, n="Gün Batımı Vadisi", cu="Vadi Çiftliği", sub="Dik yokuşlar", d="Vadideki çiftliklere giden yol dik ve kayalarla dolu. Ağırlık merkezine dikkat!", len=260f, amp=4.5f, fr=0.031f, ro=0.3f, cl=0f, seed=37, fric=1f, rocks=9, cnt=8, req=0.5f, par=65f, sky=new Color32(255,145,95,255), gr=new Color32(200,150,60,255), dec=new[]{"autumn_tree","rock_decor","autumn_tree"}, sp=8f },
                new { bld=new[]{"house_snow","lodge_snow","house_snow","billboard"}, obs=new[]{"obs_ice","obs_cone","obs_barrel"}, sig=new[]{"sign_slippery","sign_brand","sign_slope","sign_delivery"}, snow=true, n="Kartopu Kasabası", cu="Kayak Oteli", sub="Kaygan zemin", d="Karlı kasabanın kayak oteline kışlık su. Kar kaygandır, gazı yavaş ver!", len=280f, amp=5.0f, fr=0.03f, ro=0.28f, cl=0.008f, seed=53, fric=0.45f, rocks=6, cnt=8, req=0.5f, par=75f, sky=new Color32(40,60,120,255), gr=new Color32(245,250,255,255), dec=new[]{"pine_snow","streetlamp","pine_snow","snowman","bush_snow"}, sp=5f },
                new { bld=new[]{"port_containers","port_crane","port_warehouse","billboard","port_containers"}, obs=new[]{"obs_crate","obs_barrel","obs_tires"}, sig=new[]{"sign_caution","sign_slope","sign_brand","sign_delivery"}, snow=false, n="Mavi Liman", cu="Liman Fabrikası", sub="Final teslimatı", d="Limandaki fabrika işçileri susuz! Konteynerler ve vinçler arasında en uzun ve en sert rota.", len=320f, amp=4.8f, fr=0.026f, ro=0.28f, cl=0f, seed=71, fric=0.9f, rocks=10, cnt=8, req=0.45f, par=90f, sky=new Color32(120,175,225,255), gr=new Color32(200,200,205,255), dec=new[]{"streetlamp","bush","streetlamp"}, sp=8f },
            };
            var levels = new LevelData[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                var ld = ScriptableObject.CreateInstance<LevelData>();
                ld.levelName = d.n; ld.customer = d.cu; ld.subtitle = d.sub; ld.description = d.d;
                ld.length = d.len; ld.amplitude = d.amp; ld.frequency = d.fr; ld.roughness = d.ro; ld.climb = d.cl;
                ld.seed = d.seed; ld.groundFriction = d.fric; ld.rockCount = d.rocks;
                ld.damacanaCount = d.cnt; ld.requiredRatio = d.req; ld.parTime = d.par;
                ld.skyColor = d.sky; ld.grassColor = d.gr;
                ld.groundMaterial = Mat("Zemin_" + (i + 1), Load<Texture2D>(Art + "/Ground/ground_" + (i + 1) + ".png"));
                ld.grassMaterial = grass;
                ld.backgroundFar = S("Backgrounds/bg_far_" + (i + 1));
                ld.backgroundNear = S("Backgrounds/bg_near_" + (i + 1));
                var dec = new List<Sprite>();
                foreach (var n in d.dec) { var s = S("Environment/" + n); if (s) dec.Add(s); }
                ld.decorSprites = dec.ToArray();
                ld.decorSpacing = d.sp;
                var bs = new List<Sprite>();
                foreach (var n in d.bld) { var sp_ = S("Environment/" + n); if (sp_) bs.Add(sp_); }
                ld.buildingSprites = bs.ToArray();
                ld.buildingSpacing = 30f;
                ld.snowfall = d.snow;
                var os = new List<Sprite>();
                foreach (var n in d.obs) { var sp_ = S("Environment/" + n); if (sp_) os.Add(sp_); }
                ld.obstacleSprites = os.ToArray();
                var ss = new List<Sprite>();
                foreach (var n in d.sig) { var sp_ = S("Environment/" + n); if (sp_) ss.Add(sp_); }
                ld.signSprites = ss.ToArray();
                levels[i] = CreateOrReplace(Root + "/Data/Level_" + (i + 1) + ".asset", ld);
            }
            var db = ScriptableObject.CreateInstance<LevelDatabase>();
            db.levels = levels;
            CreateOrReplace(Root + "/Resources/LevelDatabase.asset", db);
            return levels;
        }

        // ---------------------------------------------------------------- KAMYON
        static GameObject BuildTruckPrefab(Phys phys)
        {
            var truck = new GameObject("VasifliSu_Kamyon");
            var body = truck.AddComponent<Rigidbody2D>();
            body.mass = 3f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.angularDamping = 0.3f;
            var sr = truck.AddComponent<SpriteRenderer>();
            sr.sprite = S("Vehicle/truck_body");
            sr.sortingOrder = 10;

            void Box(Vector2 size, Vector2 off)
            {
                var b = truck.AddComponent<BoxCollider2D>();
                b.size = size; b.offset = off; b.sharedMaterial = phys.body;
            }
            Box(new Vector2(4.24f, 0.43f), new Vector2(0f, -0.335f));    // şasi + kasa tabanı
            Box(new Vector2(1.42f, 0.88f), new Vector2(0.41f, 0.22f));    // su tankı (kasanın ön duvarı)
            Box(new Vector2(0.97f, 1.05f), new Vector2(1.635f, 0.275f));  // kabin
            Box(new Vector2(0.14f, 1.12f), new Vector2(-2.05f, 0.44f));   // arka kasa duvarı

            // Çatıdaki su simgesi tabelası
            var sign = new GameObject("SuTabelasi");
            sign.transform.SetParent(truck.transform, false);
            sign.transform.localPosition = new Vector3(1.55f, 1.25f, 0f);
            var signSr = sign.AddComponent<SpriteRenderer>();
            signSr.sprite = S("Vehicle/truck_sign");
            signSr.sortingOrder = 10;

            // Devrilme sensörü (kabin tavanı)
            var head = new GameObject("DevrilmeSensoru");
            head.transform.SetParent(truck.transform, false);
            head.transform.localPosition = new Vector3(1.6f, 0.82f, 0f);
            var hc = head.AddComponent<CircleCollider2D>();
            hc.isTrigger = true; hc.radius = 0.22f;
            head.AddComponent<HeadSensor>();

            // Tekerlekler + WheelJoint2D
            Rigidbody2D Wheel(string name, Vector2 pos, out WheelJoint2D joint)
            {
                var w = new GameObject(name);
                w.transform.SetParent(truck.transform, false);
                w.transform.localPosition = pos;
                var wsr = w.AddComponent<SpriteRenderer>();
                wsr.sprite = S("Vehicle/wheel");
                wsr.sortingOrder = 12;
                var wrb = w.AddComponent<Rigidbody2D>();
                wrb.mass = 0.5f;
                wrb.interpolation = RigidbodyInterpolation2D.Interpolate;
                wrb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var cc = w.AddComponent<CircleCollider2D>();
                cc.radius = 0.42f;
                cc.sharedMaterial = phys.wheel;
                joint = truck.AddComponent<WheelJoint2D>();
                joint.connectedBody = wrb;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = pos;
                joint.connectedAnchor = Vector2.zero;
                joint.enableCollision = false;
                var susp = joint.suspension;
                susp.frequency = 4f; susp.dampingRatio = 0.75f; susp.angle = 90f;
                joint.suspension = susp;
                joint.useMotor = false;
                return wrb;
            }
            var rearRb = Wheel("ArkaTeker", new Vector2(-1.35f, -0.7f), out var rearJ);
            var frontRb = Wheel("OnTeker", new Vector2(1.55f, -0.7f), out var frontJ);

            // Kasa: damacanalar (maks. 8, iki sıra x 4). Sayı sahnede bölüme göre ayarlanır.
            var cargoRoot = new GameObject("Kasa");
            cargoRoot.transform.SetParent(truck.transform, false);
            var dSprite = S("Vehicle/damacana");
            for (int i = 0; i < 8; i++)
            {
                int row = i / 4, col = i % 4;
                var d = new GameObject("Damacana_" + (i + 1));
                d.transform.SetParent(cargoRoot.transform, false);
                d.transform.localPosition = new Vector3(-1.77f + col * 0.4f + row * 0.05f, 0.19f + row * 0.6f, 0f);
                var dsr = d.AddComponent<SpriteRenderer>();
                dsr.sprite = dSprite;
                dsr.sortingOrder = 9;
                var drb = d.AddComponent<Rigidbody2D>();
                drb.mass = 0.3f;
                drb.linearDamping = 0.2f;
                drb.angularDamping = 0.5f;
                drb.interpolation = RigidbodyInterpolation2D.Interpolate;
                var cap = d.AddComponent<CapsuleCollider2D>();
                cap.size = new Vector2(0.34f, 0.58f);
                cap.direction = CapsuleDirection2D.Vertical;
                cap.sharedMaterial = phys.cargo;
                d.AddComponent<Damacana>();
            }

            var ctrl = truck.AddComponent<TruckController>();
            ctrl.rearJoint = rearJ; ctrl.frontJoint = frontJ;
            ctrl.rearWheel = rearRb; ctrl.frontWheel = frontRb;
            var engine = truck.AddComponent<AudioSource>();
            engine.loop = true; engine.playOnAwake = false; engine.clip = Clip("engine");
            ctrl.engineSource = engine;
            var cargo = truck.AddComponent<WaterCargo>();
            cargo.truckRoot = truck.transform;

            var prefab = PrefabUtility.SaveAsPrefabAsset(truck, Root + "/Prefabs/VasifliSu_Kamyon.prefab");
            Object.DestroyImmediate(truck);
            return prefab;
        }

        // ---------------------------------------------------------------- ORTAK SAHNE
        static Camera NewScene(Color bg, string name)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            camGo.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();
            return cam;
        }

        static string Save(string name)
        {
            string path = Root + "/Scenes/" + name + ".unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
            return path;
        }

        static void FullBackground(Transform canvas, float darken = 0f)
        {
            var bg = UIFactory.NewImage("Arkaplan", canvas, GameAssets.I.menuBackground, Color.white);
            bg.preserveAspect = false;
            UIFactory.Stretch(bg.rectTransform);
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 16f / 9f;
            if (darken > 0f) UIFactory.Dim("Karartma", canvas, darken);
        }

        static Text Label(Transform parent, string text, int size, Color c, Vector2 anchor, Vector2 pos, Vector2 sz, TextAnchor al = TextAnchor.MiddleCenter, bool bold = true)
        {
            var t = DynLabel(parent, text, size, c, anchor, pos, sz, al, bold);
            t.gameObject.AddComponent<LocalizedText>().key = text;
            return t;
        }

        /// <summary>Kodla güncellenen (dinamik) etiket — LocalizedText eklenmez.</summary>
        static Text DynLabel(Transform parent, string text, int size, Color c, Vector2 anchor, Vector2 pos, Vector2 sz, TextAnchor al = TextAnchor.MiddleCenter, bool bold = true)
        {
            var t = UIFactory.NewText(text.Length > 12 ? text.Substring(0, 12) : text, parent, text, size, c, al, bold);
            UIFactory.Place(t.rectTransform, anchor, pos, sz);
            return t;
        }

        static readonly Vector2 C = new Vector2(0.5f, 0.5f);

        // ---------------------------------------------------------------- ANA MENÜ
        static string BuildMainMenu()
        {
            NewScene(Brand.Blue, "MainMenu");
            var canvas = UIFactory.NewCanvas("Canvas").transform;
            FullBackground(canvas);

            var logo = UIFactory.NewImage("Logo", canvas, GameAssets.I.logo, Color.white);
            UIFactory.Place(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(880, 340));

            var col = UIFactory.Rect("Butonlar", canvas);
            UIFactory.Place(col, C, new Vector2(0, -60), new Vector2(560, 520));
            var layout = col.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 22; layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false; layout.childControlWidth = false;
            var a = GameAssets.I;
            var play = UIFactory.NewButton("Oyna", col, "OYNA", new Vector2(560, 116), true, a.iconPlay, 48);
            var set = UIFactory.NewButton("Ayarlar", col, "AYARLAR", new Vector2(520, 96), false, a.iconSettings);
            var gdd = UIFactory.NewButton("GDD", col, "GDD", new Vector2(520, 96), false, a.iconDoc);
            var quit = UIFactory.NewButton("Cikis", col, "ÇIKIŞ", new Vector2(520, 96), false, a.iconExit);

            // Rütbe paneli (oyunlaştırma)
            var rank = UIFactory.Panel("RutbePaneli", canvas, new Vector2(820, 150), true);
            UIFactory.Place(rank.rectTransform, new Vector2(0, 0), new Vector2(36, 36), new Vector2(820, 150));
            var rankText = DynLabel(rank.transform, "Rütbe: Stajyer Dağıtıcı", 34, Color.white, new Vector2(0, 1), new Vector2(40, -18), new Vector2(740, 46), TextAnchor.MiddleLeft);
            var rankFill = UIFactory.Bar("RutbeBar", rank.transform, new Vector2(740, 26), Brand.Orange, out var rankBg);
            UIFactory.Place(rankBg.rectTransform, new Vector2(0, 1), new Vector2(40, -72), new Vector2(740, 26));
            var stats = DynLabel(rank.transform, "Toplam teslim: 0 L", 24, new Color(0.8f, 0.92f, 1f), new Vector2(0, 0), new Vector2(40, 14), new Vector2(760, 36), TextAnchor.MiddleLeft, false);

            Label(canvas, "v1.0  •  Vasıflı Su Taşımacılık (kurgusal marka)", 22, new Color(1, 1, 1, 0.8f), new Vector2(1, 0), new Vector2(-30, 20), new Vector2(700, 40), TextAnchor.MiddleRight, false);

            // Ayarlar paneli
            var setRoot = UIFactory.Dim("AyarlarPaneli", canvas, 0.65f);
            var sp = UIFactory.Panel("Panel", setRoot.transform, new Vector2(960, 860));
            Label(sp.transform, "AYARLAR", 56, Brand.TextDark, new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(800, 80));
            Slider Row(string label, float y, out Text value)
            {
                Label(sp.transform, label, 32, Brand.TextDark, new Vector2(0, 1), new Vector2(70, y), new Vector2(260, 56), TextAnchor.MiddleLeft);
                var sl = UIFactory.NewSlider(label + "Slider", sp.transform, new Vector2(420, 38));
                UIFactory.Place((RectTransform)sl.transform, new Vector2(0, 1), new Vector2(340, y - 9), new Vector2(420, 38));
                value = DynLabel(sp.transform, "%100", 28, Brand.Blue, new Vector2(1, 1), new Vector2(-60, y), new Vector2(120, 56), TextAnchor.MiddleRight);
                return sl;
            }
            var master = Row("Ana Ses", -140, out var masterV);
            var music = Row("Müzik", -225, out var musicV);
            var sfx = Row("Efektler", -310, out var sfxV);
            Label(sp.transform, "Dil", 32, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -395), new Vector2(260, 56), TextAnchor.MiddleLeft);
            var tr = UIFactory.NewButton("Turkce", sp.transform, "", new Vector2(220, 76), true, null, 30);
            UIFactory.Place((RectTransform)tr.transform, new Vector2(0, 1), new Vector2(340, -385), new Vector2(220, 76));
            var trT = UIFactory.NewText("Label", tr.transform, "Türkçe", 30, Color.white); UIFactory.Stretch(trT.rectTransform);
            var en = UIFactory.NewButton("English", sp.transform, "", new Vector2(220, 76), true, null, 30);
            UIFactory.Place((RectTransform)en.transform, new Vector2(0, 1), new Vector2(580, -385), new Vector2(220, 76));
            var enT = UIFactory.NewText("Label", en.transform, "English", 30, Color.white); UIFactory.Stretch(enT.rectTransform);
            Label(sp.transform, "Tam Ekran", 32, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -490), new Vector2(260, 56), TextAnchor.MiddleLeft);
            var fs = UIFactory.NewToggle("TamEkran", sp.transform, 64);
            UIFactory.Place((RectTransform)fs.transform, new Vector2(0, 1), new Vector2(340, -486), new Vector2(64, 64));
            var reset = UIFactory.NewButton("Sifirla", sp.transform, "İlerlemeyi Sıfırla", new Vector2(460, 86), false, a.iconRetry, 30);
            UIFactory.Place((RectTransform)reset.transform, new Vector2(0.5f, 0), new Vector2(-245, 46), new Vector2(460, 86));
            var close = UIFactory.NewButton("Kapat", sp.transform, "TAMAM", new Vector2(320, 86), true, null, 36);
            UIFactory.Place((RectTransform)close.transform, new Vector2(0.5f, 0), new Vector2(240, 46), new Vector2(320, 86));
            var settings = new GameObject("AyarlarKontrol").AddComponent<SettingsPanel>();
            settings.root = setRoot.gameObject;
            settings.masterSlider = master; settings.musicSlider = music; settings.sfxSlider = sfx;
            settings.masterValue = masterV; settings.musicValue = musicV; settings.sfxValue = sfxV;
            settings.turkishButton = tr; settings.englishButton = en;
            settings.fullscreenToggle = fs;
            settings.resetButton = reset; settings.closeButton = close;
            settings.resetText = reset.transform.Find("Label").GetComponent<Text>();
            Object.DestroyImmediate(settings.resetText.GetComponent<LocalizedText>());

            // GDD paneli
            var gddRoot = UIFactory.Dim("GDDPaneli", canvas, 0.65f);
            var gp = UIFactory.Panel("Panel", gddRoot.transform, new Vector2(1320, 900));
            Label(gp.transform, "OYUN TASARIM DOKÜMANI (GDD)", 46, Brand.TextDark, new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(1200, 70));
            var scroll = UIFactory.ScrollText("Metin", gp.transform, new Vector2(1200, 640), GddPanel.Summary, 28);
            UIFactory.Place((RectTransform)scroll.transform.parent.parent, new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(1200, 640));
            var pdf = UIFactory.NewButton("PdfAc", gp.transform, "PDF'İ AÇ", new Vector2(380, 86), true, a.iconDoc, 34);
            UIFactory.Place((RectTransform)pdf.transform, new Vector2(0.5f, 0), new Vector2(-210, 36), new Vector2(380, 86));
            var gclose = UIFactory.NewButton("Kapat", gp.transform, "KAPAT", new Vector2(320, 86), false, null, 34);
            UIFactory.Place((RectTransform)gclose.transform, new Vector2(0.5f, 0), new Vector2(200, 36), new Vector2(320, 86));
            var gddCtrl = new GameObject("GDDKontrol").AddComponent<GddPanel>();
            gddCtrl.root = gddRoot.gameObject; gddCtrl.openPdfButton = pdf; gddCtrl.closeButton = gclose;
            gddCtrl.summaryText = scroll;

            var menu = new GameObject("AnaMenu").AddComponent<MainMenuController>();
            menu.playButton = play; menu.settingsButton = set; menu.gddButton = gdd; menu.quitButton = quit;
            menu.settings = settings; menu.gdd = gddCtrl; menu.logo = logo.rectTransform;
            menu.rankText = rankText; menu.rankFill = rankFill; menu.statsText = stats;
            settings.menu = menu;

            UIFactory.NewEventSystem();
            return Save("MainMenu");
        }

        // ---------------------------------------------------------------- DEPO KAPILARI (BÖLÜM SEÇİMİ)
        static string BuildLevelSelect(LevelData[] levels)
        {
            NewScene(new Color32(150, 70, 55, 255), "LevelSelect");
            var canvas = UIFactory.NewCanvas("Canvas").transform;
            var a = GameAssets.I;
            var ctrl = new GameObject("DepoKapilari").AddComponent<LevelSelectController>();

            var wall = UIFactory.NewImage("DuvarZemin", canvas, null, new Color32(150, 70, 55, 255));
            UIFactory.Stretch(wall.rectTransform);
            var bg = UIFactory.NewImage("DepoCephesi", canvas, S("UI/garage_bg"), Color.white);
            bg.preserveAspect = false;
            UIFactory.Place(bg.rectTransform, C, Vector2.zero, new Vector2(1920, 1080));
            bg.rectTransform.pivot = C;

            ctrl.doors = new LevelSelectController.Door[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                float x = (i - 2) * 360f;
                var lamp = UIFactory.NewImage("Lamba_" + (i + 1), canvas, a.buttonRound, new Color(1f, 0.85f, 0.4f, 0.3f));
                UIFactory.Place(lamp.rectTransform, C, new Vector2(x, 200), new Vector2(46, 46));
                lamp.rectTransform.pivot = C; lamp.raycastTarget = false;

                var frame = UIFactory.NewImage("Kapi_" + (i + 1), canvas, a.panelDark, new Color(0.12f, 0.14f, 0.2f, 1f), true);
                UIFactory.Place(frame.rectTransform, C, new Vector2(x, -45), new Vector2(320, 440));
                frame.rectTransform.pivot = C;
                frame.raycastTarget = true;
                var btn = frame.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                frame.gameObject.AddComponent<RectMask2D>();

                var inside = UIFactory.NewImage("Ic", frame.transform, levels[i].backgroundFar, Color.white);
                inside.preserveAspect = false;
                UIFactory.Stretch(inside.rectTransform, 12f);
                inside.raycastTarget = false;
                var truck = UIFactory.NewImage("Kamyon", frame.transform, S("Vehicle/truck_icon"), Color.white);
                UIFactory.Place(truck.rectTransform, new Vector2(0.5f, 0f), new Vector2(-360, 14), new Vector2(270, 160));
                truck.rectTransform.pivot = new Vector2(0.5f, 0f);
                truck.raycastTarget = false;
                var shutter = UIFactory.NewImage("Kepenk", frame.transform, S("UI/door_roller"), Color.white);
                shutter.preserveAspect = false;
                UIFactory.Stretch(shutter.rectTransform, 12f);
                shutter.type = Image.Type.Filled;
                shutter.fillMethod = Image.FillMethod.Vertical;
                shutter.fillOrigin = (int)Image.OriginVertical.Top;
                shutter.raycastTarget = false;

                var lockG = UIFactory.Rect("Kilit", frame.transform);
                UIFactory.Stretch(lockG);
                var chain = UIFactory.NewImage("Zincir", lockG, S("UI/chain"), Color.white);
                UIFactory.Place(chain.rectTransform, C, new Vector2(0, -40), new Vector2(300, 60));
                chain.rectTransform.pivot = C; chain.rectTransform.localRotation = Quaternion.Euler(0, 0, 12);
                var chain2 = UIFactory.NewImage("Zincir2", lockG, S("UI/chain"), Color.white);
                UIFactory.Place(chain2.rectTransform, C, new Vector2(0, -40), new Vector2(300, 60));
                chain2.rectTransform.pivot = C; chain2.rectTransform.localRotation = Quaternion.Euler(0, 0, -12);
                var lockBg = UIFactory.NewImage("KilitArka", lockG, a.buttonRound, new Color(0.95f, 0.75f, 0.2f));
                UIFactory.Place(lockBg.rectTransform, C, new Vector2(0, -40), new Vector2(110, 110));
                lockBg.rectTransform.pivot = C;
                var lockI = UIFactory.NewImage("KilitIkon", lockBg.transform, a.iconLock, new Color(0.25f, 0.2f, 0.1f));
                UIFactory.Stretch(lockI.rectTransform, 24f);

                var plate = UIFactory.NewImage("Tabela", frame.transform, a.barFill, Brand.Blue, true);
                UIFactory.Place(plate.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(286, 92));
                plate.rectTransform.pivot = new Vector2(0.5f, 1f);
                var num = UIFactory.NewImage("No", plate.transform, a.buttonRound, Brand.Orange);
                UIFactory.Place(num.rectTransform, new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(66, 66));
                num.rectTransform.pivot = new Vector2(0, 0.5f);
                var numT = UIFactory.NewText("Sayi", num.transform, (i + 1).ToString(), 38, Color.white);
                UIFactory.Stretch(numT.rectTransform);
                var nm = UIFactory.NewText("Ad", plate.transform, levels[i].levelName, 24, Color.white);
                UIFactory.Place(nm.rectTransform, new Vector2(0, 1), new Vector2(84, -8), new Vector2(196, 40));
                nm.alignment = TextAnchor.MiddleLeft;
                nm.resizeTextForBestFit = true; nm.resizeTextMinSize = 16; nm.resizeTextMaxSize = 24;
                nm.gameObject.AddComponent<LocalizedText>().key = levels[i].levelName;
                var stars = new Image[3];
                for (int k = 0; k < 3; k++)
                {
                    stars[k] = UIFactory.NewImage("Yildiz", plate.transform, a.iconStarEmpty, Color.white);
                    UIFactory.Place(stars[k].rectTransform, new Vector2(0, 0), new Vector2(86 + k * 34, 8), new Vector2(32, 32));
                }
                ctrl.doors[i] = new LevelSelectController.Door { button = btn, shutter = shutter, truck = truck.rectTransform, lockGroup = lockG.gameObject, stars = stars, lamp = lamp };
            }

            // Üst köşeler
            var back = UIFactory.NewButton("Geri", canvas, "ANA MENÜ", new Vector2(320, 84), false, a.iconHome, 30);
            UIFactory.Place((RectTransform)back.transform, new Vector2(0, 1), new Vector2(24, -14), new Vector2(320, 84));
            ctrl.backButton = back;
            var totBg = UIFactory.Panel("ToplamPanel", canvas, new Vector2(520, 80), true);
            UIFactory.Place(totBg.rectTransform, new Vector2(1, 1), new Vector2(-24, -16), new Vector2(520, 80));
            ctrl.totalText = DynLabel(totBg.transform, "", 26, Color.white, C, Vector2.zero, new Vector2(490, 64));
            ctrl.totalText.rectTransform.pivot = C;

            // Alt bilgi şeridi
            var info = UIFactory.Panel("BilgiSeridi", canvas, new Vector2(1600, 240));
            UIFactory.Place(info.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1600, 240));
            var I = info.transform;
            ctrl.titleText = DynLabel(I, "", 44, Brand.TextDark, new Vector2(0, 1), new Vector2(34, -14), new Vector2(760, 56), TextAnchor.MiddleLeft);
            ctrl.subtitleText = DynLabel(I, "", 24, Brand.Blue, new Vector2(0, 1), new Vector2(36, -68), new Vector2(500, 32), TextAnchor.MiddleLeft);
            ctrl.customerText = DynLabel(I, "", 26, Brand.TextDark, new Vector2(0, 1), new Vector2(36, -100), new Vector2(760, 36), TextAnchor.MiddleLeft, false);
            ctrl.descText = DynLabel(I, "", 22, new Color(0.3f, 0.4f, 0.52f), new Vector2(0, 1), new Vector2(36, -138), new Vector2(1000, 44), TextAnchor.UpperLeft, false);
            ctrl.infoText = DynLabel(I, "", 24, Brand.TextDark, new Vector2(0, 0), new Vector2(36, 16), new Vector2(1060, 40), TextAnchor.MiddleLeft, false);
            var starRow = UIFactory.Rect("Yildizlar", I);
            UIFactory.Place(starRow, new Vector2(1, 1), new Vector2(-330, -22), new Vector2(180, 56));
            ctrl.detailStars = new Image[3];
            for (int k = 0; k < 3; k++)
            {
                ctrl.detailStars[k] = UIFactory.NewImage("Yildiz", starRow, a.iconStarEmpty, Color.white);
                UIFactory.Place(ctrl.detailStars[k].rectTransform, new Vector2(0, 0.5f), new Vector2(k * 60, 0), new Vector2(54, 54));
                ctrl.detailStars[k].rectTransform.pivot = new Vector2(0, 0.5f);
            }
            ctrl.bestText = DynLabel(I, "", 22, new Color(0.35f, 0.45f, 0.55f), new Vector2(1, 1), new Vector2(-34, -84), new Vector2(460, 36), TextAnchor.MiddleRight, false);
            var play = UIFactory.NewButton("TeslimataCik", I, "TESLİMATA ÇIK", new Vector2(460, 100), true, a.iconPlay, 38);
            UIFactory.Place((RectTransform)play.transform, new Vector2(1, 0), new Vector2(-30, 22), new Vector2(460, 100));
            ctrl.playButton = play;
            ctrl.playLabel = play.transform.Find("Label").GetComponent<Text>();
            Object.DestroyImmediate(ctrl.playLabel.GetComponent<LocalizedText>());
            UIFactory.NewEventSystem();
            return Save("LevelSelect");
        }

        // ---------------------------------------------------------------- OYNANIŞ
        static string BuildLevelScene(int index, LevelData ld, GameObject truckPrefab, Phys phys)
        {
            var cam = NewScene(ld.skyColor, "Level_" + (index + 1));
            var follow = cam.gameObject.AddComponent<CameraFollow>();

            // Parallax arka planlar
            void Layer(string name, Sprite sprite, float factor, float yFollow, float yOffset, int order, float scale)
            {
                if (sprite == null) return;
                var go = new GameObject(name);
                go.transform.localScale = Vector3.one * scale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(sprite.bounds.size.x * 3f, sprite.bounds.size.y);
                sr.sortingOrder = order;
                var p = go.AddComponent<ParallaxLayer>();
                p.cam = cam.transform; p.factor = factor; p.yFollow = yFollow; p.yOffset = yOffset;
                go.transform.position = new Vector3(0, yOffset, 5);
            }
            Layer("Arkaplan_Uzak", ld.backgroundFar, 0.92f, 1f, 1.5f, -30, 2f);
            Layer("Arkaplan_Yakin", ld.backgroundNear, 0.65f, 0.85f, -2.5f, -20, 1.6f);

            // Arazi
            var terrainGo = new GameObject("Arazi");
            var terrain = terrainGo.AddComponent<TerrainGenerator>();
            terrain.data = ld;
            var levelPhys = new PhysicsMaterial2D("Zemin_" + (index + 1)) { friction = ld.groundFriction, bounciness = 0f };
            terrain.groundPhysics = CreateOrReplace(Root + "/PhysicsMaterials/Zemin_" + (index + 1) + ".physicsMaterial2D", levelPhys);
            var grassGo = new GameObject("CimSeridi");
            grassGo.transform.SetParent(terrainGo.transform, false);
            var lr = grassGo.AddComponent<LineRenderer>();
            lr.sharedMaterial = ld.grassMaterial;
            terrain.grassLine = lr;
            terrain.Generate();

            // Kamyon
            var truckGo = (GameObject)PrefabUtility.InstantiatePrefab(truckPrefab);
            truckGo.transform.position = new Vector3(terrain.SpawnX, 1.25f, 0f);
            var dams = truckGo.GetComponentsInChildren<Damacana>(true);
            for (int i = 0; i < dams.Length; i++) dams[i].gameObject.SetActive(i < ld.damacanaCount);
            // Pasif damacanalar sahnede tamamen kaldırılır (sayıma girmesin)
            if (dams.Length > ld.damacanaCount)
                PrefabUtility.UnpackPrefabInstance(truckGo, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            for (int i = dams.Length - 1; i >= ld.damacanaCount; i--)
                Object.DestroyImmediate(dams[i].gameObject);
            follow.target = truckGo.transform;
            if (ld.snowfall) AddSnowfall(cam.transform);

            var lm = new GameObject("LevelManager").AddComponent<LevelManager>();
            lm.levelIndex = index;
            lm.truck = truckGo.GetComponent<TruckController>();
            lm.cargo = truckGo.GetComponent<WaterCargo>();
            lm.terrain = terrain;

            BuildHud(lm);
            BuildPause(lm);
            BuildResult(lm);
            UIFactory.NewEventSystem();
            return Save("Level_" + (index + 1));
        }

        static void AddSnowfall(Transform cam)
        {
            var go = new GameObject("KarYagisi");
            go.transform.SetParent(cam, false);
            go.transform.localPosition = new Vector3(0f, 11f, 10f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 8f; main.startSpeed = 1.2f; main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.maxParticles = 600; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.05f; main.startColor = new Color(1f, 1f, 1f, 0.9f);
            var em = ps.emission; em.rateOverTime = 70f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(40f, 1f, 1f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var nz = ps.noise; nz.enabled = true; nz.strength = 0.4f; nz.frequency = 0.3f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            r.sortingOrder = 20;
        }

        static void BuildHud(LevelManager lm)
        {
            var a = GameAssets.I;
            var canvas = UIFactory.NewCanvas("HUD", 0).transform;
            var hud = canvas.gameObject.AddComponent<HUDController>();
            hud.level = lm; lm.hud = hud;

            hud.levelNameText = DynLabel(canvas, "1. BÖLÜM", 34, Color.white, new Vector2(0, 1), new Vector2(36, -24), new Vector2(900, 56), TextAnchor.MiddleLeft);
            UIFactory.AddShadow(hud.levelNameText, 3f, 0.6f);

            // Su paneli
            var wp = UIFactory.Panel("SuPaneli", canvas, new Vector2(560, 120), true);
            UIFactory.Place(wp.rectTransform, new Vector2(0, 1), new Vector2(28, -92), new Vector2(560, 120));
            var wi = UIFactory.NewImage("SuIkon", wp.transform, a.iconWater, Brand.Aqua);
            UIFactory.Place(wi.rectTransform, new Vector2(0, 0.5f), new Vector2(26, 0), new Vector2(76, 76));
            wi.rectTransform.pivot = new Vector2(0, 0.5f);
            hud.waterText = DynLabel(wp.transform, "114 / 114 L", 32, Color.white, new Vector2(0, 1), new Vector2(120, -12), new Vector2(410, 46), TextAnchor.MiddleLeft);
            hud.waterFill = UIFactory.Bar("SuBar", wp.transform, new Vector2(410, 34), Brand.Aqua, out var wbg);
            UIFactory.Place(wbg.rectTransform, new Vector2(0, 0), new Vector2(120, 18), new Vector2(410, 34));
            var marker = UIFactory.NewImage("MinIsaret", wbg.transform, null, Brand.Red);
            marker.rectTransform.sizeDelta = new Vector2(6, 10);
            hud.requiredMarker = marker.rectTransform;

            // Coin sayacı
            var cp = UIFactory.Panel("CoinPaneli", canvas, new Vector2(250, 84), true);
            UIFactory.Place(cp.rectTransform, new Vector2(0, 1), new Vector2(28, -224), new Vector2(250, 84));
            var ci = UIFactory.NewImage("CoinIkon", cp.transform, a.coin, Color.white);
            UIFactory.Place(ci.rectTransform, new Vector2(0, 0.5f), new Vector2(18, 0), new Vector2(60, 60));
            ci.rectTransform.pivot = new Vector2(0, 0.5f);
            hud.coinText = DynLabel(cp.transform, "0", 40, new Color32(255, 190, 40, 255), new Vector2(1, 0.5f), new Vector2(-22, 0), new Vector2(150, 70), TextAnchor.MiddleRight);
            hud.coinText.rectTransform.pivot = new Vector2(1, 0.5f);
            hud.coinIcon = ci.rectTransform;

            // İlerleme
            hud.progressFill = UIFactory.Bar("Ilerleme", canvas, new Vector2(700, 36), Brand.Green, out var pbg);
            UIFactory.Place(pbg.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(700, 36));
            hud.progressFill.fillAmount = 0f;
            var flag = UIFactory.NewImage("Bayrak", pbg.transform, a.iconFlag, Color.white);
            UIFactory.Place(flag.rectTransform, new Vector2(1, 0.5f), new Vector2(40, 10), new Vector2(60, 70));
            var marker2 = UIFactory.NewImage("KamyonIsaret", pbg.transform, a.buttonRound, Brand.Orange);
            marker2.rectTransform.sizeDelta = new Vector2(44, 44);
            hud.progressTruck = marker2.rectTransform;
            hud.distanceText = DynLabel(canvas, "0 m", 28, Color.white, new Vector2(0.5f, 1), new Vector2(0, -78), new Vector2(300, 44));
            UIFactory.AddShadow(hud.distanceText, 2f, 0.6f);

            // Süre
            var tp = UIFactory.Panel("SurePaneli", canvas, new Vector2(240, 90), true);
            UIFactory.Place(tp.rectTransform, new Vector2(1, 1), new Vector2(-160, -28), new Vector2(240, 90));
            var ti = UIFactory.NewImage("SureIkon", tp.transform, a.iconTimer, Color.white);
            UIFactory.Place(ti.rectTransform, new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(54, 54));
            ti.rectTransform.pivot = new Vector2(0, 0.5f);
            hud.timerText = DynLabel(tp.transform, "0:00", 40, Color.white, new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(150, 70), TextAnchor.MiddleRight);
            hud.timerText.rectTransform.pivot = new Vector2(1, 0.5f);

            var pause = UIFactory.IconButton("Duraklat", canvas, a.iconPause, 104, Brand.Blue);
            UIFactory.Place((RectTransform)pause.transform, new Vector2(1, 1), new Vector2(-30, -22), new Vector2(104, 104));
            hud.pauseButton = pause;

            // Hız göstergesi
            var sp = UIFactory.Panel("HizPaneli", canvas, new Vector2(340, 110), true);
            UIFactory.Place(sp.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(340, 110));
            var si = UIFactory.NewImage("HizIkon", sp.transform, a.iconSpeed, Brand.Aqua);
            UIFactory.Place(si.rectTransform, new Vector2(0, 0.5f), new Vector2(26, 0), new Vector2(70, 70));
            si.rectTransform.pivot = new Vector2(0, 0.5f);
            hud.speedText = DynLabel(sp.transform, "0 km/s", 54, Color.white, new Vector2(1, 0.5f), new Vector2(-26, 0), new Vector2(220, 90), TextAnchor.MiddleRight);
            hud.speedText.rectTransform.pivot = new Vector2(1, 0.5f);

            // Pedallar
            // Geri sayım
            var cd = UIFactory.NewText("GeriSayim", canvas, "3", 180, Color.white);
            UIFactory.Place(cd.rectTransform, C, new Vector2(0, 80), new Vector2(1200, 260));
            cd.rectTransform.pivot = C;
            var ol = cd.gameObject.AddComponent<Outline>();
            ol.effectColor = Brand.DeepBlue; ol.effectDistance = new Vector2(5, -5);
            hud.countdownText = cd;
        }

        static void BuildPause(LevelManager lm)
        {
            var a = GameAssets.I;
            var canvas = UIFactory.NewCanvas("DuraklatmaMenusu", 10).transform;
            var pm = canvas.gameObject.AddComponent<PauseMenu>();
            pm.level = lm; lm.pauseMenu = pm;
            var root = UIFactory.Dim("Kok", canvas, 0.62f);
            pm.root = root.gameObject;

            // Arka planda süzülen su damlaları
            var rnd = new System.Random(5);
            pm.drops = new RectTransform[14];
            for (int i = 0; i < pm.drops.Length; i++)
            {
                var dr = UIFactory.NewImage("Damla", root.transform, a.iconWater, new Color(0.25f, 0.82f, 0.92f, 0.18f + (float)rnd.NextDouble() * 0.2f));
                float sz = 40 + rnd.Next(0, 70);
                UIFactory.Place(dr.rectTransform, C, new Vector2(-900 + i * 140 + rnd.Next(-40, 40), rnd.Next(-600, 600)), new Vector2(sz, sz));
                dr.rectTransform.pivot = C;
                dr.raycastTarget = false;
                pm.drops[i] = dr.rectTransform;
            }

            var p = UIFactory.Panel("Panel", root.transform, new Vector2(1240, 830));
            pm.panel = p.rectTransform;

            // Başlık bandı
            var head = UIFactory.NewImage("Baslik", p.transform, a.barFill, Brand.Blue, true);
            UIFactory.Place(head.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1180, 140));
            var pi = UIFactory.NewImage("PauseIkon", head.transform, a.buttonRound, Brand.Orange);
            UIFactory.Place(pi.rectTransform, new Vector2(0, 0.5f), new Vector2(26, 0), new Vector2(100, 100));
            pi.rectTransform.pivot = new Vector2(0, 0.5f);
            var pii = UIFactory.NewImage("Ikon", pi.transform, a.iconPause, Color.white);
            UIFactory.Stretch(pii.rectTransform, 24f);
            var title = Label(head.transform, "DURAKLATILDI", 60, Color.white, new Vector2(0, 1), new Vector2(150, -10), new Vector2(900, 76), TextAnchor.MiddleLeft);
            UIFactory.AddShadow(title, 3f, 0.4f);
            pm.levelText = DynLabel(head.transform, "", 26, new Color(0.85f, 0.95f, 1f), new Vector2(0, 0), new Vector2(152, 14), new Vector2(1000, 40), TextAnchor.MiddleLeft, false);
            var logo = UIFactory.NewImage("Logo", head.transform, a.logo, Color.white);
            UIFactory.Place(logo.rectTransform, new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(300, 104));
            logo.rectTransform.pivot = new Vector2(1, 0.5f);

            // Sol sütun: kamyon + teslimat durumu
            var truck = UIFactory.NewImage("Kamyon", p.transform, S("Vehicle/truck_icon"), Color.white);
            UIFactory.Place(truck.rectTransform, new Vector2(0, 1), new Vector2(60, -180), new Vector2(460, 272));
            pm.truck = truck.rectTransform;
            var card = UIFactory.Panel("Durum", p.transform, new Vector2(520, 220), true);
            UIFactory.Place(card.rectTransform, new Vector2(0, 1), new Vector2(40, -462), new Vector2(520, 220));
            Text Row(Sprite icon, Color tint, string label, float y)
            {
                var ic = UIFactory.NewImage("Ikon", card.transform, icon, tint);
                UIFactory.Place(ic.rectTransform, new Vector2(0, 1), new Vector2(26, y), new Vector2(46, 46));
                Label(card.transform, label, 26, new Color(0.75f, 0.88f, 1f), new Vector2(0, 1), new Vector2(86, y + 2), new Vector2(220, 50), TextAnchor.MiddleLeft, false);
                return DynLabel(card.transform, "", 32, Color.white, new Vector2(1, 1), new Vector2(-28, y + 2), new Vector2(240, 50), TextAnchor.MiddleRight);
            }
            pm.waterText = Row(a.iconWater, Brand.Aqua, "Su miktarı", -22);
            pm.timeText = Row(a.iconTimer, Color.white, "Süre", -84);
            pm.distanceText = Row(a.iconFlag, Color.white, "Kalan yol", -146);
            pm.waterFill = UIFactory.Bar("SuBar", card.transform, new Vector2(470, 18), Brand.Aqua, out var wbg);
            UIFactory.Place(wbg.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(470, 18));

            // Sağ sütun: butonlar + ses
            Button B(string n, string label, bool primary, Sprite icon, float y)
            {
                var b = UIFactory.NewButton(n, p.transform, label, new Vector2(520, 92), primary, icon, 34);
                UIFactory.Place((RectTransform)b.transform, new Vector2(1, 1), new Vector2(-50, y), new Vector2(520, 92));
                return b;
            }
            pm.resumeButton = B("Devam", "DEVAM ET", true, a.iconPlay, -190);
            pm.restartButton = B("Yeniden", "YENİDEN BAŞLAT", false, a.iconRetry, -296);
            pm.levelSelectButton = B("Siparisler", "SİPARİŞLER", false, a.iconFlag, -402);
            pm.mainMenuButton = B("AnaMenu", "ANA MENÜ", false, a.iconHome, -508);
            Label(p.transform, "Müzik", 26, Brand.TextDark, new Vector2(1, 1), new Vector2(-410, -612), new Vector2(160, 44), TextAnchor.MiddleLeft);
            pm.musicSlider = UIFactory.NewSlider("Muzik", p.transform, new Vector2(330, 32));
            UIFactory.Place((RectTransform)pm.musicSlider.transform, new Vector2(1, 1), new Vector2(-60, -618), new Vector2(330, 32));
            Label(p.transform, "Efektler", 26, Brand.TextDark, new Vector2(1, 1), new Vector2(-410, -662), new Vector2(160, 44), TextAnchor.MiddleLeft);
            pm.sfxSlider = UIFactory.NewSlider("Efekt", p.transform, new Vector2(330, 32));
            UIFactory.Place((RectTransform)pm.sfxSlider.transform, new Vector2(1, 1), new Vector2(-60, -668), new Vector2(330, 32));

            // Alt şerit: ipucu
            var tip = UIFactory.NewImage("Ipucu", p.transform, a.barFill, new Color(1f, 0.93f, 0.8f), true);
            UIFactory.Place(tip.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(1180, 74));
            var ti = UIFactory.NewImage("Yildiz", tip.transform, a.iconStar, Color.white);
            UIFactory.Place(ti.rectTransform, new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(48, 48));
            ti.rectTransform.pivot = new Vector2(0, 0.5f);
            pm.tipText = DynLabel(tip.transform, "", 27, new Color(0.45f, 0.28f, 0.05f), new Vector2(0, 0.5f), new Vector2(86, 0), new Vector2(800, 60), TextAnchor.MiddleLeft, false);
            pm.tipText.rectTransform.pivot = new Vector2(0, 0.5f);
            var hint = Label(tip.transform, "ESC: devam  •  R: yeniden başlat", 22, new Color(0.55f, 0.4f, 0.2f), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(380, 50), TextAnchor.MiddleRight, false);
            hint.rectTransform.pivot = new Vector2(1, 0.5f);
        }

        static void BuildResult(LevelManager lm)
        {
            var a = GameAssets.I;
            var canvas = UIFactory.NewCanvas("SonucEkrani", 20).transform;
            var rp = canvas.gameObject.AddComponent<ResultPanel>();
            rp.level = lm; lm.resultPanel = rp;
            var root = UIFactory.Dim("Kok", canvas, 0.7f);
            rp.root = root.gameObject;
            var p = UIFactory.Panel("Panel", root.transform, new Vector2(1060, 960));

            rp.header = UIFactory.NewImage("Baslik", p.transform, a.barFill, Brand.Green, true);
            UIFactory.Place(rp.header.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(980, 110));
            rp.titleText = UIFactory.NewText("BaslikYazi", rp.header.transform, "TESLİMAT TAMAM!", 56, Color.white);
            UIFactory.Stretch(rp.titleText.rectTransform);
            UIFactory.AddShadow(rp.titleText, 3f, 0.4f);
            rp.reasonText = DynLabel(p.transform, "", 30, Brand.TextDark, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(940, 90), TextAnchor.MiddleCenter, false);

            var starRow = UIFactory.Rect("Yildizlar", p.transform);
            UIFactory.Place(starRow, new Vector2(0.5f, 1), new Vector2(0, -250), new Vector2(420, 120));
            rp.stars = new Image[3];
            for (int k = 0; k < 3; k++)
            {
                rp.stars[k] = UIFactory.NewImage("Yildiz", starRow, a.iconStarEmpty, Color.white);
                UIFactory.Place(rp.stars[k].rectTransform, C, new Vector2(-140 + k * 140, k == 1 ? 10 : -6), new Vector2(k == 1 ? 120 : 100, k == 1 ? 120 : 100));
                rp.stars[k].rectTransform.pivot = C;
            }

            Label(p.transform, "Teslim edilen su", 30, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -400), new Vector2(360, 50), TextAnchor.MiddleLeft);
            rp.ratioFill = UIFactory.Bar("OranBar", p.transform, new Vector2(420, 40), Brand.Aqua, out var rbg);
            UIFactory.Place(rbg.rectTransform, new Vector2(0, 1), new Vector2(400, -406), new Vector2(420, 40));
            rp.ratioText = DynLabel(p.transform, "%0", 36, Brand.Blue, new Vector2(1, 1), new Vector2(-60, -398), new Vector2(150, 54), TextAnchor.MiddleRight);
            Label(p.transform, "Su miktarı", 30, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -470), new Vector2(360, 50), TextAnchor.MiddleLeft);
            rp.litresText = DynLabel(p.transform, "", 32, Brand.TextDark, new Vector2(1, 1), new Vector2(-60, -470), new Vector2(500, 50), TextAnchor.MiddleRight);
            Label(p.transform, "Süre", 30, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -530), new Vector2(360, 50), TextAnchor.MiddleLeft);
            rp.timeText = DynLabel(p.transform, "", 32, Brand.TextDark, new Vector2(1, 1), new Vector2(-60, -530), new Vector2(560, 50), TextAnchor.MiddleRight);
            Label(p.transform, "SKOR", 34, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -600), new Vector2(360, 60), TextAnchor.MiddleLeft);
            rp.scoreText = DynLabel(p.transform, "0", 60, Brand.Orange, new Vector2(1, 1), new Vector2(-60, -592), new Vector2(500, 76), TextAnchor.MiddleRight);
            rp.bestText = DynLabel(p.transform, "", 26, new Color(0.35f, 0.45f, 0.55f), new Vector2(1, 1), new Vector2(-60, -664), new Vector2(700, 40), TextAnchor.MiddleRight, false);
            rp.criteriaText = DynLabel(p.transform, "", 26, Brand.TextDark, new Vector2(0, 1), new Vector2(70, -660), new Vector2(560, 120), TextAnchor.UpperLeft, false);

            Button B(string n, string label, bool primary, Sprite icon, float x)
            {
                var b = UIFactory.NewButton(n, p.transform, label, new Vector2(230, 90), primary, icon, 26);
                UIFactory.Place((RectTransform)b.transform, new Vector2(0.5f, 0), new Vector2(x, 34), new Vector2(230, 90));
                ((RectTransform)b.transform).pivot = new Vector2(0.5f, 0f);
                return b;
            }
            rp.retryButton = B("Tekrar", "TEKRAR", false, a.iconRetry, -372);
            rp.levelSelectButton = B("Siparisler", "SİPARİŞLER", false, a.iconFlag, -124);
            rp.menuButton = B("Menu", "MENÜ", false, a.iconHome, 124);
            rp.nextButton = B("Sonraki", "SONRAKİ", true, a.iconNext, 372);
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Vasıflı Su Studio";
            PlayerSettings.productName = "Vasıflı Su Taşımacılık";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
        }
    }
}
#endif
