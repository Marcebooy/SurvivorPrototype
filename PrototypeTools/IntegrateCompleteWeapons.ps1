$p=Get-Content Assets/Survivor/Scripts/SurvivorProgression.cs -Raw
$p=$p.Replace('Aura, Shotgun }','Aura, Shotgun, Revolver, Aegis, Bananarang, Axe, SpaceNoodle, Sniper, Rocket, Mines, WirelessDagger, Frostwalker, Tornado, Dexecutioner, BloodMagic, BlackHole, PoisonFlask, Katana, DragonBreath, Dice, HeroSword, CorruptedSword, Scythe }')
$p=$p.Replace('static readonly string[] UpgradeNames = {','static readonly string[] UpgradeNames = WithAdvancedNames(new string[] {')
$p=$p.Replace('static readonly string[] UpgradeDetails = {','static readonly string[] UpgradeDetails = WithAdvancedDetails(new string[] {')
$a=$p.IndexOf('static readonly string[] UpgradeNames');$b=$p.IndexOf('};',$a);$p=$p.Remove($b,2).Insert($b,'});')
$a=$p.IndexOf('static readonly string[] UpgradeDetails');$b=$p.IndexOf('};',$a);$p=$p.Remove($b,2).Insert($b,'});')
$p=$p.Replace('index > 8','index >= TotalWeaponCount')
$p=$p.Replace('            if (id >= 10 && id <= 15)', '            if (id >= 18) { var w=(Weapon)(id-9); bool owned=WeaponLevel(w)>0; weaponLevels[w]=WeaponLevel(w)+1; if(owned) GrantWeaponBonus(); }'+"`n"+'            else if (id >= 10 && id <= 15)')
$p=$p.Replace('string weapons = ""; foreach (var w in weaponLevels) weapons += WeaponLabel(w.Key) + " " + w.Value + "   ";','string weapons = LoadoutSummary();')
$p=$p.Replace('"+1 % tulijäljen kestoa."','"+1 % tulijälkien, erikoisammusten, miinojen ja alueiden kestoa."')
Set-Content PrototypeTools/SurvivorProgression.complete.cs $p -Encoding utf8
$a=Get-Content Assets/Survivor/Scripts/SurvivorArsenal.cs -Raw
$a=$a.Replace('index<3 ? index : index+7','index<3 ? index : index<9 ? index+7 : index+9')
$a=$a.Replace('            foreach(var s in shots)', '            ClearAdvanced(resetStats);'+"`n"+'            foreach(var s in shots)')
$a=$a.Replace('TickOrbits(dt); TickShots(dt); TickFlames(dt);','TickOrbits(dt); TickShots(dt); TickFlames(dt); TickAdvanced(dt);')
$a=$a.Replace('" / 3"','" / " + ((TotalWeaponCount+2)/3)')
$a=$a.Replace('Kaikki yhdeksän','Kaikki 30 asetta')
$a=$a.Replace('starterPage=(starterPage+2)%3','starterPage=(starterPage+(TotalWeaponCount+2)/3-1)%((TotalWeaponCount+2)/3)')
$a=$a.Replace('starterPage=(starterPage+1)%3','starterPage=(starterPage+1)%((TotalWeaponCount+2)/3)')
Set-Content PrototypeTools/SurvivorArsenal.complete.cs $a -Encoding utf8
$g=Get-Content Assets/Survivor/Scripts/SurvivorGame.cs -Raw
$g=$g.Replace('                var e = enemies[i]; var delta = player.position', '                if(i>=enemies.Count) continue;'+"`n"+'                var e = enemies[i]; var delta = player.position')
$g=$g.Replace('delta.normalized * (e.speed * dt)','delta.normalized * (e.speed * EnemyMoveMultiplier(e) * dt)')
$g=$g.Replace('                    ReceiveDamage(e.elite ? 26 : 13);','                    ReceiveDamage(e.elite ? 26 : 13);'+"`n"+'                    if(!enemies.Contains(e)) continue;')
$g=$g.Replace('            if (e == boss) { boss = null;', '            AdvancedKill(e);'+"`n"+'            if (e == boss) { boss = null;')
Set-Content PrototypeTools/SurvivorGame.complete.cs $g -Encoding utf8
$t=Get-Content Assets/Survivor/Scripts/SurvivorPottu.cs -Raw
$t=$t.Replace('            Health=Mathf.Max(0,Health-Mathf.Max(1,amount-Armor));','            if(BlockWithAegis()) return false;'+"`n"+'            Health=Mathf.Max(0,Health-Mathf.Max(1,amount-Armor));')
Set-Content PrototypeTools/SurvivorPottu.complete.cs $t -Encoding utf8
