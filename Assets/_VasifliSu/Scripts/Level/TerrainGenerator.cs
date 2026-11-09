using System.Collections.Generic;
using UnityEngine;

namespace VasifliSu
{
    /// <summary>
    /// LevelData parametrelerinden engebeli 2B arazi üretir:
    /// görünen zemin (Mesh), çim şeridi (LineRenderer), fiziksel çarpışma (EdgeCollider2D),
    /// kaya engelleri, dekor, başlangıç deposu ve teslimat noktası.
    /// Aynı seed her zaman aynı araziyi üretir.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
    public class TerrainGenerator : MonoBehaviour
    {
        public LevelData data;
        public PhysicsMaterial2D groundPhysics;
        public LineRenderer grassLine;
        public float step = 0.5f;
        public float startX = -20f;
        public float startFlat = 16f;
        public float endFlat = 30f;
        public float depth = 25f;
        public float textureTile = 4f;
        [Tooltip("İzin verilen en dik eğim (tan). 0.8 ≈ 39°")]
        public float maxSlope = 0.8f;

        public float FinishX => data.length - endFlat * 0.55f;
        public float SpawnX => 4f;
        public float MinHeight { get; private set; }

        const string GeneratedName = "_Generated";
        float _endHeight;
        float[] _heights;   // eğimi sınırlandırılmış nihai yükseklikler

        void Awake() { Generate(); }

        /// <summary>x konumundaki zemin yüksekliği.</summary>
        public float HeightAt(float x)
        {
            if (_heights != null && _heights.Length > 1)
            {
                float f = (x - startX) / step;
                int i = Mathf.Clamp(Mathf.FloorToInt(f), 0, _heights.Length - 2);
                return Mathf.Lerp(_heights[i], _heights[i + 1], Mathf.Clamp01(f - i));
            }
            return RawClamped(x);
        }

        float RawClamped(float x)
        {
            float endStart = data.length - endFlat;
            if (x >= endStart) return _endHeight;
            return RawHeight(x);
        }

        float RawHeight(float x)
        {
            if (x <= startFlat) return 0f;
            float t = x - startFlat;
            float s = data.seed * 13.37f;
            float h = 0f;
            h += (Mathf.PerlinNoise(t * data.frequency + s, s * 0.5f) - 0.5f) * 2f * data.amplitude;
            h += (Mathf.PerlinNoise(t * data.frequency * 2.2f + s * 1.9f, 3.3f) - 0.5f) * 2f * data.amplitude * 0.25f;
            h += (Mathf.PerlinNoise(t * 0.6f + s * 0.3f, 9.1f) - 0.5f) * 2f * data.roughness;
            h += t * data.climb;
            // İlk metrelerde yumuşak geçiş
            float blend = Mathf.SmoothStep(0f, 1f, t / 14f);
            // Perlin başlangıçta 0 olmadığı için kaydırma
            float h0 = (Mathf.PerlinNoise(s, s * 0.5f) - 0.5f) * 2f * data.amplitude
                     + (Mathf.PerlinNoise(s * 1.9f, 3.3f) - 0.5f) * 2f * data.amplitude * 0.25f
                     + (Mathf.PerlinNoise(s * 0.3f, 9.1f) - 0.5f) * 2f * data.roughness;
            return (h - h0) * blend;
        }

        public void Generate()
        {
            if (data == null) return;
            _endHeight = RawHeight(data.length - endFlat);

            // Eski üretilmiş objeleri temizle
            var old = transform.Find(GeneratedName);
            if (old != null) DestroyImmediate(old.gameObject);
            var root = new GameObject(GeneratedName).transform;
            root.SetParent(transform, false);

            float endX = data.length + 25f;
            var pts = new List<Vector2>();
            float minY = 0f;
            _heights = null;
            int count = Mathf.FloorToInt((endX - startX) / step) + 1;
            var hs = new float[count];
            float maxDy = maxSlope * step;
            for (int i = 0; i < count; i++)
            {
                float y = RawClamped(startX + i * step);
                if (i > 0) y = Mathf.Clamp(y, hs[i - 1] - maxDy, hs[i - 1] + maxDy);
                hs[i] = y;
            }
            // Bitiş düzlüğü: son kısımda sabit yükseklik
            int endIdx = Mathf.Clamp(Mathf.FloorToInt((data.length - endFlat - startX) / step), 0, count - 1);
            for (int i = endIdx; i < count; i++) hs[i] = hs[endIdx];
            _endHeight = hs[endIdx];
            _heights = hs;
            for (int i = 0; i < count; i++)
            {
                pts.Add(new Vector2(startX + i * step, hs[i]));
                if (hs[i] < minY) minY = hs[i];
            }
            MinHeight = minY;

            BuildMesh(pts, minY - depth);
            BuildCollider(pts);
            BuildGrass(pts);
            BuildWalls(root, pts);
            BuildRocks(root);
            BuildDecor(root);
            BuildBuildings(root);
            BuildSigns(root);
            BuildLandmarks(root);
        }

        void BuildMesh(List<Vector2> pts, float bottom)
        {
            var mesh = new Mesh { name = "TerrainMesh" };
            var verts = new Vector3[pts.Count * 2];
            var uvs = new Vector2[pts.Count * 2];
            var tris = new int[(pts.Count - 1) * 6];
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                verts[i * 2] = new Vector3(p.x, p.y, 0f);
                verts[i * 2 + 1] = new Vector3(p.x, bottom, 0f);
                uvs[i * 2] = new Vector2(p.x / textureTile, (p.y - bottom) / textureTile);
                uvs[i * 2 + 1] = new Vector2(p.x / textureTile, 0f);
            }
            for (int i = 0; i < pts.Count - 1; i++)
            {
                int v = i * 2, t = i * 6;
                tris[t] = v; tris[t + 1] = v + 2; tris[t + 2] = v + 1;
                tris[t + 3] = v + 1; tris[t + 4] = v + 2; tris[t + 5] = v + 3;
            }
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            var mr = GetComponent<MeshRenderer>();
            if (data.groundMaterial != null) mr.sharedMaterial = data.groundMaterial;
            mr.sortingOrder = 0;
        }

        void BuildCollider(List<Vector2> pts)
        {
            var edge = GetComponent<EdgeCollider2D>();
            edge.SetPoints(pts);
            edge.edgeRadius = 0.05f;
            if (groundPhysics != null)
            {
                groundPhysics.friction = data.groundFriction;
                edge.sharedMaterial = groundPhysics;
            }
            if (GetComponent<GroundSurface>() == null) gameObject.AddComponent<GroundSurface>();
        }

        void BuildGrass(List<Vector2> pts)
        {
            if (grassLine == null) return;
            grassLine.positionCount = pts.Count;
            for (int i = 0; i < pts.Count; i++) grassLine.SetPosition(i, new Vector3(pts[i].x, pts[i].y - 0.12f, 0f));
            grassLine.startColor = grassLine.endColor = data.grassColor;
            grassLine.widthMultiplier = 0.45f;
            grassLine.useWorldSpace = true;
            grassLine.numCornerVertices = 2;
            if (data.grassMaterial != null) grassLine.sharedMaterial = data.grassMaterial;
            grassLine.sortingOrder = 2;
        }

        void BuildWalls(Transform root, List<Vector2> pts)
        {
            // Sol tarafta görünmez duvar: geri geri düşmeyi engeller
            var wall = new GameObject("LeftWall");
            wall.transform.SetParent(root, false);
            var e = wall.AddComponent<EdgeCollider2D>();
            e.SetPoints(new List<Vector2> { new Vector2(startX + 2f, -5f), new Vector2(startX + 2f, 40f) });
        }

        /// <summary>Yola kaya ve temaya uygun engeller (kütük, sandık, koni, varil, lastik, buz, saman) koyar.</summary>
        void BuildRocks(Transform root)
        {
            var a = GameAssets.I;
            var rng = new System.Random(data.seed * 7919);
            float from = startFlat + 25f, to = data.length - endFlat - 8f;
            bool hasObs = data.obstacleSprites != null && data.obstacleSprites.Length > 0;
            for (int i = 0; i < data.rockCount; i++)
            {
                float x = Mathf.Lerp(from, to, (i + 0.5f) / Mathf.Max(1, data.rockCount)) + (float)(rng.NextDouble() - 0.5) * 8f;
                bool useObs = hasObs && (i % 2 == 1);
                if (useObs) PlaceObstacle(root, data.obstacleSprites[(i / 2) % data.obstacleSprites.Length], x, i);
                else if (a != null && a.rock != null) PlaceRock(root, a.rock, x, rng, i);
            }
        }

        void PlaceRock(Transform root, Sprite sprite, float x, System.Random rng, int i)
        {
            float r = 0.25f + (float)rng.NextDouble() * 0.2f;
            var go = new GameObject("Kaya_" + i);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(x, HeightAt(x) - r * 0.35f, 0f);
            go.transform.rotation = Quaternion.Euler(0, 0, (float)rng.NextDouble() * 360f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = 3;
            float spriteR = sprite.bounds.extents.x;
            go.transform.localScale = Vector3.one * (r / Mathf.Max(0.01f, spriteR));
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = spriteR * 0.92f;
            if (groundPhysics != null) col.sharedMaterial = groundPhysics;
            go.AddComponent<GroundSurface>();
        }

        void PlaceObstacle(Transform root, Sprite sprite, float x, int i)
        {
            if (sprite == null) return;
            float slope = Mathf.Atan2(HeightAt(x + 0.5f) - HeightAt(x - 0.5f), 1f) * Mathf.Rad2Deg;
            var go = new GameObject("Engel_" + sprite.name + "_" + i);
            go.transform.SetParent(root, false);
            go.transform.rotation = Quaternion.Euler(0, 0, slope);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = 3;
            var ext = sprite.bounds.extents;
            go.transform.position = new Vector3(x, HeightAt(x) + ext.y * 0.85f, 0f);
            string n = sprite.name;
            Collider2D col;
            if (n.Contains("tires")) { var c = go.AddComponent<CircleCollider2D>(); c.radius = ext.y * 0.9f; col = c; }
            else if (n.Contains("log")) { var c = go.AddComponent<CapsuleCollider2D>(); c.direction = CapsuleDirection2D.Horizontal; c.size = new Vector2(ext.x * 2f, ext.y * 1.7f); col = c; }
            else if (n.Contains("cone")) { var c = go.AddComponent<PolygonCollider2D>(); c.points = new[] { new Vector2(-ext.x * 0.6f, -ext.y), new Vector2(ext.x * 0.6f, -ext.y), new Vector2(0f, ext.y) }; col = c; }
            else
            {
                // Eğimli kenarlı (rampa gibi) çarpışma: kamyon dik bir duvara takılmadan üstünden geçebilir
                go.transform.position = new Vector3(x, HeightAt(x) + ext.y * 0.55f, 0f);
                var c = go.AddComponent<PolygonCollider2D>();
                c.points = new[] { new Vector2(-ext.x * 1.1f, -ext.y), new Vector2(ext.x * 1.1f, -ext.y), new Vector2(ext.x * 0.3f, ext.y * 0.85f), new Vector2(-ext.x * 0.3f, ext.y * 0.85f) };
                col = c;
            }
            if (groundPhysics != null) col.sharedMaterial = groundPhysics;
            go.AddComponent<GroundSurface>();
        }

        /// <summary>Yol kenarına uyarı ve yön tabelaları dikilir.</summary>
        void BuildSigns(Transform root)
        {
            if (data.signSprites == null || data.signSprites.Length == 0) return;
            int k = 0;
            for (float x = startFlat + 8f; x < data.length - endFlat - 4f; x += 38f)
            {
                var sprite = data.signSprites[k++ % data.signSprites.Length];
                if (sprite == null) continue;
                var go = new GameObject("Tabela_" + sprite.name);
                go.transform.SetParent(root, false);
                go.transform.position = new Vector3(x, HeightAt(x) - sprite.bounds.min.y - 0.2f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite; sr.sortingOrder = -1;
            }
            // Bitişten hemen önce "TESLİMAT →" tabelası
            var ds = System.Array.Find(data.signSprites, sp => sp != null && sp.name.Contains("delivery"));
            if (ds != null)
            {
                float x = FinishX - 12f;
                var go = new GameObject("Tabela_Teslimat");
                go.transform.SetParent(root, false);
                go.transform.position = new Vector3(x, HeightAt(x) - ds.bounds.min.y - 0.2f, 0f);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = ds; sr.sortingOrder = -1;
            }
        }

        void BuildDecor(Transform root)
        {
            if (data.decorSprites == null || data.decorSprites.Length == 0) return;
            var rng = new System.Random(data.seed * 104729);
            for (float x = startX + 4f; x < data.length + 20f; x += data.decorSpacing * (0.6f + (float)rng.NextDouble() * 0.8f))
            {
                var sprite = data.decorSprites[rng.Next(data.decorSprites.Length)];
                if (sprite == null) continue;
                var go = new GameObject("Decor");
                go.transform.SetParent(root, false);
                float s = 0.8f + (float)rng.NextDouble() * 0.5f;
                go.transform.localScale = Vector3.one * s;
                float bottom = sprite.bounds.min.y * s;
                go.transform.position = new Vector3(x, HeightAt(x) - bottom - 0.25f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = -1;
            }
        }

        /// <summary>Yol kenarına ev ve binalar yerleştirir (eğime göre zemine oturur).</summary>
        void BuildBuildings(Transform root)
        {
            if (data.buildingSprites == null || data.buildingSprites.Length == 0) return;
            var rng = new System.Random(data.seed * 31337);
            int idx = 0;
            for (float x = startFlat + 12f; x < data.length - endFlat - 6f; x += data.buildingSpacing * (0.75f + (float)rng.NextDouble() * 0.5f))
            {
                var sprite = data.buildingSprites[idx++ % data.buildingSprites.Length];
                if (sprite == null) continue;
                float s = 0.95f + (float)rng.NextDouble() * 0.25f;
                float half = sprite.bounds.extents.x * s;
                // Binanın altındaki en alçak noktaya otur (havada kalmasın)
                float y = Mathf.Min(HeightAt(x - half * 0.8f), Mathf.Min(HeightAt(x), HeightAt(x + half * 0.8f)));
                var go = new GameObject("Bina_" + sprite.name);
                go.transform.SetParent(root, false);
                go.transform.localScale = Vector3.one * s;
                go.transform.position = new Vector3(x, y - sprite.bounds.min.y * s - 0.35f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = -2;
            }
        }

        void BuildLandmarks(Transform root)
        {
            var a = GameAssets.I;
            if (a == null) return;

            if (a.depot != null)
            {
                var depot = new GameObject("VasifliSu_Depo");
                depot.transform.SetParent(root, false);
                var sr = depot.AddComponent<SpriteRenderer>();
                sr.sprite = a.depot;
                sr.sortingOrder = -1;
                depot.transform.position = new Vector3(-7f, -a.depot.bounds.min.y - 0.2f, 0f);
            }

            var finish = new GameObject("TeslimatNoktasi");
            finish.transform.SetParent(root, false);
            float fx = FinishX;
            finish.transform.position = new Vector3(fx, _endHeight, 0f);
            var box = finish.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 12f);
            box.offset = new Vector2(0f, 5f);
            finish.AddComponent<FinishLine>();

            if (a.deliveryPoint != null)
            {
                var shop = new GameObject("Market");
                shop.transform.SetParent(finish.transform, false);
                var sr = shop.AddComponent<SpriteRenderer>();
                sr.sprite = a.deliveryPoint;
                sr.sortingOrder = -1;
                shop.transform.localPosition = new Vector3(5.5f, -a.deliveryPoint.bounds.min.y - 0.2f, 0f);
            }
            if (a.iconFlag != null)
            {
                var flag = new GameObject("Bayrak");
                flag.transform.SetParent(finish.transform, false);
                var sr = flag.AddComponent<SpriteRenderer>();
                sr.sprite = a.iconFlag;
                sr.sortingOrder = 4;
                flag.transform.localPosition = new Vector3(0f, -a.iconFlag.bounds.min.y - 0.1f, 0f);
            }
        }
    }
}
