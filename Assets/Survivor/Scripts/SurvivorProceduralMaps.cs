using System;
using System.Collections.Generic;
using UnityEngine;

namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        // Map Type: Procedural (default, unlimited variety via ProceduralMapLayout) or a
        // fixed, hand-modeled arena (Mosswood/BuildForestMap, Luuluola/BuildBoneCaveMap,
        // Tuhkaerämaa/BuildAshWastesMap - Astra's art-package delivery, README-Tuhkaeramaa.md).
        // selectedMapType is what the MAPIT tab shows/edits; activeMapType is locked in at
        // BeginMapRun() so switching tabs mid-run never changes the map underfoot.
        public enum MapType { Procedural, Mosswood, BoneCave, AshWastes }
        MapType selectedMapType = MapType.Procedural;
        int selectedMapTier = 1;
        MapType activeMapType = MapType.Procedural;
        int activeMapTier = 1;
        Transform fixedMapRoot;
        // Stacks on top of the existing per-Area enemy scaling (curse, Area multiplier below) -
        // Procedural always runs at Tier 1, so this is a 1.0x no-op unless a fixed map is active.
        float MapTierMultiplier => 1f + (activeMapTier - 1) * .35f;

        // Kohta 3: Mappi-tilan viholliskiintiö per Tier (SpawnEnemy()-kutsujen kokonaismäärä yhdellä
        // alueella, ei samanaikaisten yläraja - se on erillinen enemyCap edelleen). Nousee Tierin
        // mukaan samaan tapaan kuin MapTierMultiplier, mutta on oma erillinen luku koska kiintiö on
        // spawnien LUKUMÄÄRÄ, ei vahinko-/HP-kerroin - näitä kahta ei pidä sekoittaa samaksi kaavaksi.
        // Koskee kaikkia kolmea kiinteää karttaa (Mosswood/Luuluola/Tuhkaerämaa) yhtä lailla, koska
        // ne kaikki kulkevat saman SurvivorGame.cs/SurvivorProgression.cs-ydinsilmukan läpi eikä
        // mikään tästä eteenpäin haaraudu MapType:n mukaan - vain activeMapType != Procedural.
        static int TierEnemyQuota(int tier) => tier switch { 1 => 150, 2 => 220, 3 => 300, _ => 150 };

        // Onko tämän alueen spawnaus loppunut - Selviytymistilassa sama 90s-ajastin kuin ennenkin,
        // Mappi-tilassa uusi kiintiöehto (SpawnEnemy() lakkaa kutsumasta, ks. Update()). Tämä EI vielä
        // tarkoita että bossin voi kutsua Mappi-tilassa - ks. AreaBossReady alla, joka vaatii lisäksi
        // kentän olevan tyhjä.
        bool AreaSpawningStopped => activeMapType == MapType.Procedural
            ? Elapsed - areaStarted >= 90
            : areaKillCount >= TierEnemyQuota(activeMapTier);

        // Onko alueen bossi kutsuttavissa ("[E] Kutsu alueen bossi"). Selviytymistilassa
        // muuttumaton alkuperäinen käytös (pelkkä ajastin, viholliset saavat olla yhä kentällä).
        // Mappi-tilassa vaatii LISÄKSI ettei kentällä ole enää yhtään henkiin jäänyttä vihollista
        // (areaKillCount >= tierQuota && enemies.Count == 0 && boss == null - boss == null
        // tarkistetaan jo kutsupaikoissa erikseen, ei toisteta tässä).
        bool AreaBossReady => activeMapType == MapType.Procedural
            ? AreaSpawningStopped
            : AreaSpawningStopped && enemies.Count == 0;
        public ProceduralMapLayout MapLayout { get; private set; }
        public int CurrentMapSeed { get; private set; }
        public int RunSeed { get; private set; }
        public bool MapTransitionPending => BossDefeated && bossDefeatedAt >= 0;
        public Vector3 MapSpawn => MapLayout == null ? Vector3.zero : MapLayout.Spawn;
        Transform proceduralRoot;
        Transform mapEffectRoot;
        Transform MapEffectRoot
        {
            get { if(!mapEffectRoot) { mapEffectRoot=new GameObject("Map combat effects").transform; mapEffectRoot.SetParent(world,false); } return mapEffectRoot; }
        }
        readonly List<Material> proceduralMaterials = new List<Material>();
        [Serializable] public sealed class MapSeedRecord { public int mapNumber, seed, generatorVersion; }
        [Serializable] public sealed class MapSeedHistory { public int runSeed; public List<MapSeedRecord> maps = new List<MapSeedRecord>(); }
        MapSeedHistory seedHistory = new MapSeedHistory();
        public IReadOnlyList<MapSeedRecord> MapSeeds => seedHistory.maps;
        string mapSeedInput = "";
        float transitionLeft;
        int? requestedSeed;

        public void StartSeededRun(int seed)
        {
            if(NetworkClientMode) return;
            requestedSeed=seed; AtMainMenu=false; ResetRun();
        }

        static int NextMapSeed(int seed)
        {
            // Full-period integer step + mixing: reproducible sequence, no gameplay RNG use.
            uint n=unchecked((uint)seed+0x9E3779B9u);
            n=(n^(n>>16))*0x85EBCA6Bu; n=(n^(n>>13))*0xC2B2AE35u; n^=n>>16;
            int result=unchecked((int)n); return result==seed ? unchecked(seed+1) : result;
        }

        void BeginMapRun()
        {
            activeMapType = selectedMapType; activeMapTier = selectedMapTier;
            // Consumed here (run start), not at MAPIT selection time, so browsing doesn't spend
            // anything. If the player ran out since selecting it (e.g. last one used elsewhere),
            // fall back to the always-free procedural map rather than blocking play.
            if (activeMapType != MapType.Procedural && !ConsumeMapItem(activeMapType, activeMapTier)) { activeMapType = MapType.Procedural; activeMapTier = 1; }
            selectedMapType = activeMapType; selectedMapTier = activeMapTier; // keep the MAPIT tab in sync with what actually ran
            if (activeMapType != MapType.Procedural)
            {
                requestedSeed = null; RunSeed = 0; seedHistory = new MapSeedHistory { runSeed = 0 };
                BuildFixedMap(activeMapType);
                return;
            }
            int seed;
            if(requestedSeed.HasValue) seed=requestedSeed.Value;
            else if(!int.TryParse(mapSeedInput,out seed)) seed=BitConverter.ToInt32(Guid.NewGuid().ToByteArray(),0);
            requestedSeed=null; RunSeed=seed;
            seedHistory=new MapSeedHistory { runSeed=seed };
            GenerateProceduralMap(seed, true);
        }

        // Same teardown as GenerateProceduralMap (destroy the old root, clear meshes/materials/
        // obstacles) but builds a fixed Mosswood/Luuluola arena instead - MapLayout stays null,
        // which is exactly the legacy (pre-procedural) fallback path already used by
        // ClampToMap/ResolveMapPosition/MoveOnMap and CreateLandmarks.
        void BuildFixedMap(MapType type)
        {
            if(proceduralRoot) { proceduralRoot.gameObject.SetActive(false); Destroy(proceduralRoot.gameObject); proceduralRoot=null; }
            if(fixedMapRoot) { fixedMapRoot.gameObject.SetActive(false); Destroy(fixedMapRoot.gameObject); fixedMapRoot=null; }
            foreach(var mesh in mapMeshes) if(mesh) Destroy(mesh); mapMeshes.Clear();
            foreach(var material in proceduralMaterials) { materials.Remove(material); if(material) Destroy(material); }
            proceduralMaterials.Clear(); forestObstacles.Clear();
            MapLayout=null; CurrentMapSeed=0;
            int materialStart=materials.Count;
            if(type==MapType.BoneCave) BuildBoneCaveMap();
            else if(type==MapType.AshWastes) BuildAshWastesMap();
            else BuildForestMap();
            for(int i=materialStart;i<materials.Count;i++) proceduralMaterials.Add(materials[i]);
        }

        void DrawMapTypeTab()
        {
            GUI.Label(new Rect(240,150,800,50), "MAPIT", centerTitleStyle);
            GUI.Label(new Rect(240,204,800,44), "Kiinteät kartat kuluttavat yhden Karttaesineen PELAA-hetkellä - korkeampi Tier tekee vihollisista vaarallisempia. Bossien kaatamisesta on pieni mahdollisuus löytää niitä.", centerTextStyle);
            float y = 262;
            DrawMapTypeOption(MapType.Procedural, 1, y, "SATUNNAINEN (oletus, Tier 1, rajaton)", true); y += 56;
            foreach (var type in new[] { MapType.Mosswood, MapType.BoneCave, MapType.AshWastes })
                for (int tier = 1; tier <= 3; tier++) { DrawMapTypeOption(type, tier, y, null, false); y += 46; }
            y += 14;
            string selectedLabel = selectedMapType == MapType.Procedural ? "SATUNNAINEN" : MapTypeName(selectedMapType).ToUpperInvariant() + " T" + selectedMapTier;
            if (GUI.Button(new Rect(390,y,500,54), "PELAA VALITULLA KARTALLA  [" + selectedLabel + "]", centerButtonStyle)) StartGame();
        }

        void DrawMapTypeOption(MapType type, int tier, float y, string label, bool unlimited)
        {
            int count = unlimited ? -1 : MapItemCount(type, tier);
            bool owned = unlimited || count > 0;
            if (label == null) label = MapTypeName(type).ToUpperInvariant() + " T" + tier + (owned ? " (" + count + " kpl)" : " - Ei karttoja, tapa bosseja saadaksesi");
            bool selected = selectedMapType == type && (unlimited || selectedMapTier == tier);
            GUI.enabled = owned;
            GUI.color = selected ? new Color(.3f,.85f,1) : Color.white;
            if (GUI.Button(new Rect(390,y,500,40), label, centerButtonStyle) && owned) { selectedMapType = type; selectedMapTier = unlimited ? 1 : tier; }
            GUI.color = Color.white; GUI.enabled = true;
        }

        void GenerateProceduralMap(int seed, bool save)
        {
            var layout=new ProceduralMapLayout(seed);
            if(layout.ReachableCellCount!=layout.FloorCells.Count) throw new InvalidOperationException("Disconnected map");
            if(proceduralRoot) { proceduralRoot.gameObject.SetActive(false); Destroy(proceduralRoot.gameObject); }
            foreach(var mesh in mapMeshes) if(mesh) Destroy(mesh); mapMeshes.Clear();
            foreach(var material in proceduralMaterials) { materials.Remove(material); if(material) Destroy(material); }
            proceduralMaterials.Clear(); forestObstacles.Clear();
            MapLayout=layout; CurrentMapSeed=seed;
            int materialStart=materials.Count;
            BuildDungeonGeometry();
            for(int i=materialStart;i<materials.Count;i++) proceduralMaterials.Add(materials[i]);
            if(save)
            {
                seedHistory.maps.Add(new MapSeedRecord { mapNumber=Area, seed=seed, generatorVersion=ProceduralMapLayout.GeneratorVersion });
                PlayerPrefs.SetInt(SaveKey+"LastMapSeed",seed);
                PlayerPrefs.SetInt(SaveKey+"LastMapNumber",Area);
                PlayerPrefs.SetString(SaveKey+"MapSeedHistory",JsonUtility.ToJson(seedHistory));
                PlayerPrefs.Save();
            }
        }

        void BuildDungeonGeometry()
        {
            proceduralRoot=new GameObject("Generated map - seed "+CurrentMapSeed).transform;
            proceduralRoot.SetParent(world,false);
            var roomMat=MakeMaterial(new Color(.25f,.35f,.22f));
            var trailMat=MakeMaterial(new Color(.27f,.37f,.23f));
            var wallMat=MakeMaterial(new Color(.34f,.40f,.38f));
            var bark=MakeMaterial(new Color(.25f,.16f,.09f));
            var leaf=MakeMaterial(new Color(.15f,.31f,.22f));
            var roomV=new List<Vector3>(); var roomT=new List<int>();
            var trailV=new List<Vector3>(); var trailT=new List<int>();
            var wallV=new List<Vector3>(); var wallT=new List<int>();
            void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
            { int n=v.Count; v.Add(a);v.Add(b);v.Add(c);v.Add(d);t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3}); }
            void Wall(Vector3 center,Vector3 along)
            {
                var side=new Vector3(-along.z,0,along.x).normalized*.35f;
                var a=center-along-side;var b=center-along+side;var c=center+along+side;var d=center+along-side;
                var up=Vector3.up*1.1f;
                Quad(wallV,wallT,a+up,b+up,c+up,d+up);
                Quad(wallV,wallT,a,d,d+up,a+up); Quad(wallV,wallT,b,a,a+up,b+up);
                Quad(wallV,wallT,c,b,b+up,c+up); Quad(wallV,wallT,d,c,c+up,d+up);
            }
            var directions=new[]{Vector2Int.right,Vector2Int.up,Vector2Int.left,Vector2Int.down};
            foreach(var cell in MapLayout.FloorCells)
            {
                bool room = (cell.x * 17 + cell.y * 31 + (CurrentMapSeed & 255)) % 19 != 0;
                var p=MapLayout.ToWorld(cell); const float h=ProceduralMapLayout.CellSize*.5f;
                Quad(room ? roomV : trailV,room ? roomT : trailT,p+new Vector3(-h,0,-h),p+new Vector3(-h,0,h),p+new Vector3(h,0,h),p+new Vector3(h,0,-h));
                foreach(var dir in directions)
                    if(!MapLayout.IsFloor(cell.x+dir.x,cell.y+dir.y))
                        Wall(p+new Vector3(dir.x,0,dir.y)*h,new Vector3(-dir.y,0,dir.x)*h);
            }
            MapMesh("Open meadow",FacetedMesh("Meadow mesh",roomV.ToArray(),roomT.ToArray()),Vector3.zero,Vector3.one,roomMat,proceduralRoot);
            MapMesh("Grass variation",FacetedMesh("Grass detail mesh",trailV.ToArray(),trailT.ToArray()),Vector3.zero,Vector3.one,trailMat,proceduralRoot);
            MapMesh("Outer stone boundary",FacetedMesh("Boundary wall mesh",wallV.ToArray(),wallT.ToArray()),Vector3.zero,Vector3.one,wallMat,proceduralRoot);
            var coneV=new Vector3[8]; var coneT=new int[21]; coneV[0]=Vector3.up;
            for(int i=0;i<7;i++) { float a=i*Mathf.PI*2/7;coneV[i+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));coneT[i*3]=0;coneT[i*3+1]=(i+1)%7+1;coneT[i*3+2]=i+1; }
            var cone=FacetedMesh("Pine crown mesh",coneV,coneT);
            var rng=new System.Random(CurrentMapSeed);
            // Decorative groves stay outside walkable tiles, so every corridor remains open.
            var placed=new HashSet<Vector2Int>();
            foreach(var cell in MapLayout.FloorCells)
            {
                if(rng.Next(0,7)!=0) continue;
                foreach(var dir in directions)
                {
                    var outside=cell+dir*2;
                    if(MapLayout.IsFloor(outside.x,outside.y) || !placed.Add(outside)) continue;
                    var p=MapLayout.ToWorld(outside);
                    if(MapLayout.IsWalkable(p+Vector3.right*3) || MapLayout.IsWalkable(p-Vector3.right*3) || MapLayout.IsWalkable(p+Vector3.forward*3) || MapLayout.IsWalkable(p-Vector3.forward*3)) continue;
                    float size=.8f+(float)rng.NextDouble()*.5f;
                    Shape("Pine trunk",PrimitiveType.Cylinder,p+Vector3.up*1.4f,new Vector3(.7f,1.4f,.7f),bark,proceduralRoot);
                    for(int tier=0;tier<3;tier++) MapMesh("Pine crown",cone,p+Vector3.up*(1.5f+tier*1.1f)*size,new Vector3(2.2f-tier*.45f,2.8f,2.2f-tier*.45f)*size,leaf,proceduralRoot);
                    break;
                }
            }
        }

        void ClearMapCombat()
        {
            if(mapEffectRoot) { mapEffectRoot.gameObject.SetActive(false); Destroy(mapEffectRoot.gameObject); mapEffectRoot=null; }
            ClearArsenal(false);
            foreach(var e in enemies) if(e.body) Destroy(e.body.gameObject); enemies.Clear();
            foreach(var b in bolts) if(b.body) Destroy(b.body.gameObject); bolts.Clear();
            foreach(var d in drops) if(d.body) Destroy(d.body.gameObject); drops.Clear();
            foreach(var p in landmarks) if(p.body) Destroy(p.body.gameObject); landmarks.Clear();
            foreach(var f in flashes) if(f.body) Destroy(f.body.gameObject); flashes.Clear();
            damageLabels.Clear(); boss=null;
            slashTimer=0; if(slash) slash.gameObject.SetActive(false);
        }

        void BeginMapTransition()
        {
            transitionLeft=2f; invulnerability=3f;
            notice="Bossi voitettu! Uusi kartta generoidaan..."; noticeUntil=Elapsed+6;
        }

        void TickMapTransition(float dt)
        {
            // Runs before upgrade/shop pauses: boss death always completes the transition once.
            transitionLeft-=dt;
            if(transitionLeft<=0) AdvanceArea();
        }

        void DrawMapSeedMenu()
        {
            GUI.Label(new Rect(35,340,290,28),"KARTAN SEED (valinnainen)",textStyle);
            mapSeedInput=GUI.TextField(new Rect(35,375,265,34),mapSeedInput,11);
            GUI.Label(new Rect(35,414,290,65),"Tyhjä = uusi satunnainen kartta",textStyle);
            if(!string.IsNullOrWhiteSpace(mapSeedInput) && !int.TryParse(mapSeedInput,out _)) GUI.Label(new Rect(35,440,290,26),"Anna kokonaisluku (32 bit).",textStyle);
            GUI.enabled=PlayerPrefs.HasKey(SaveKey+"LastMapSeed");
            if(GUI.Button(new Rect(35,470,265,38),"Käytä viimeisintä seediä",buttonStyle)) mapSeedInput=PlayerPrefs.GetInt(SaveKey+"LastMapSeed").ToString();
            GUI.enabled=true;
        }

        void DrawDungeonMap()
        {
            if(MapLayout==null) return;
            if(GUI.Button(new Rect(24,284,400,28),"Seed: "+CurrentMapSeed+"  [kopioi]",buttonStyle)) GUIUtility.systemCopyBuffer=CurrentMapSeed.ToString();
            var rect=new Rect(1030,305,220,220);
            GUI.color=new Color(.035f,.05f,.06f,.9f);GUI.DrawTexture(rect,Texture2D.whiteTexture);
            float scale=rect.width/ProceduralMapLayout.GridSize;
            foreach(var c in MapLayout.FloorCells)
            {
                GUI.color=new Color(.38f,.48f,.35f);
                GUI.DrawTexture(new Rect(rect.x+c.x*scale,rect.y+(ProceduralMapLayout.GridSize-1-c.y)*scale,scale+.2f,scale+.2f),Texture2D.whiteTexture);
            }
            void Dot(Vector3 p,Color color,float size)
            { var c=MapLayout.ToCell(p);GUI.color=color;GUI.DrawTexture(new Rect(rect.x+(c.x+.5f)*scale-size/2,rect.y+(ProceduralMapLayout.GridSize-.5f-c.y)*scale-size/2,size,size),Texture2D.whiteTexture); }
            foreach(var landmark in landmarks) if(!landmark.used) Dot(landmark.body.position,landmark.kind==2 ? Color.cyan : landmark.kind==1 ? Color.red : Color.yellow,5);
            Dot(NetworkClientMode && RemotePlayerNet.Local ? RemotePlayerNet.Local.Position : player.position,Color.white,6);
            GUI.color=Color.white;
        }
    }
}
