$p=Get-Content Assets/Survivor/Scripts/SurvivorProgression.cs -Raw
$p=$p.Replace('Sword, Bow, Lightning }','Sword, Bow, Lightning, Chunkers, Flamewalker, Bone, Firestaff, Aura, Shotgun }')
$p=$p.Replace('"Movement Tome"','"Movement Tome", "Kiertokivet / Chunkers", "Tulijälki / Flamewalker", "Pomppuluu / Bone", "Tulipallo / Firestaff", "Aura", "Haulikko / Shotgun", "Duration Tome", "Projectile Speed Tome"')
$p=$p.Replace('"+12 % liikkumisnopeutta."','"+12 % liikkumisnopeutta.",'+"`n"+'            "Kivet kiertävät Pottua ja murskaavat. Quantity: lisää kiviä. Size: koko ja kiertorata.",'+"`n"+'            "Jättää polttavia alueita. Damage: vahinko. Size: alue. Duration: kesto.",'+"`n"+'            "Luu kimpoaa seuraavaan viholliseen. Asetaso lisää vahinkoa ja kimpoiluja. Quantity: lisää luita.",'+"`n"+'            "Tulipallo räjähtää aluevahingoksi. Quantity: lisää palloja. Size: suurempi räjähdys.",'+"`n"+'            "Vahinkokehä Potun ympärillä. Size: säde. Cooldown: nopeampi syke. Quantity ei vaikuta.",'+"`n"+'            "Lyhyen kantaman hauliparvi. Quantity: lisää hauleja. Asetaso kasvattaa vahinkoa.",'+"`n"+'            "+25 % tulijäljen kestoa.", "+20 % ammusten ja kiertokivien nopeutta."')
$p=$p.Replace('"+1 nuoli, salamakohde tai pannun lisäisku (enintään 6)."','"+1 ammus, kiertokivi, salamakohde tai pannun lisäisku. Max 6. Ei vaikuta auraan tai tulijälkeen."')
$p=$p.Replace('"+20 % pannun kantamaa ja nuolten osumakokoa."','"+20 % aseiden osuma- ja aluekokoa; kiertokivien kiertorata kasvaa."')
$p=$p.Replace('            landmarks.Clear(); flashes.Clear(); weaponLevels.Clear(); choices.Clear();','            ClearArsenal(true);'+"`n"+'            landmarks.Clear(); flashes.Clear(); weaponLevels.Clear(); choices.Clear();')
$p=$p.Replace('index > 2','index > 8')
$p=$p.Replace('            if (id < 3)', '            if (id < 0 || id >= UpgradeNames.Length) return;'+"`n"+'            if (id >= 10 && id <= 15) { var w=(Weapon)(id-7); weaponLevels[w]=WeaponLevel(w)+1; }'+"`n"+'            else if (id < 3)')
$p=$p.Replace('case 9: moveSpeed *= 1.12f; break;', 'case 9: moveSpeed *= 1.12f; break; case 16: EffectDuration *= 1.25f; break; case 17: ProjectileSpeed *= 1.2f; break;')
$p=$p.Replace('            Area++; areaStarted', '            ClearArsenal(false);'+"`n"+'            Area++; areaStarted')
$p=$p.Replace('            GUI.Label(new Rect(90,90,1100,55), Selecting ?', '            if (Selecting) { DrawStarterPages(); return; }'+"`n"+'            GUI.Label(new Rect(90,90,1100,55), Selecting ?')
$p=$p.Replace('(w.Key == Weapon.Sword ? "Pannu" : w.Key.ToString())', 'WeaponLabel(w.Key)')
$p=$p.Replace('new Rect(24,192,780,32)', 'new Rect(24,192,790,60)').Replace('new Rect(24,226,820,30)','new Rect(24,254,900,30)')
Set-Content PrototypeTools/SurvivorProgression.arsenal.cs $p -Encoding utf8
$g=Get-Content Assets/Survivor/Scripts/SurvivorGame.cs -Raw
$g=$g.Replace('            UpdateBolts(dt);','            TickArsenal(dt); UpdateBolts(dt);')
$g=$g.Replace('b.direction * (24 * dt)','b.direction * (24 * ProjectileSpeed * dt)')
Set-Content PrototypeTools/SurvivorGame.arsenal.cs $g -Encoding utf8
