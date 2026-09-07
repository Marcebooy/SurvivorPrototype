$g=Get-Content Assets/Survivor/Scripts/SurvivorGame.cs -Raw
$a=$g.IndexOf('            player = Shape('); $b=$g.IndexOf('            slash = Shape(', $a)
$g=$g.Remove($a,$b-$a).Insert($a,"            BuildPottu();`n")
$g=$g.Replace('ResetProgression(); notice', 'ResetProgression(); ResetPottu(); notice')
$g=$g.Replace('            TickProgression(dt); if (Finished) return;', '')
$a=$g.IndexOf('            var next = player.position + move'); $b=$g.IndexOf('            invulnerability -= dt;', $a)
$g=$g.Remove($a,$b-$a).Insert($a,"            bool dodgePressed = (k != null && k.spaceKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);`n            MovePottu(move, dt, dodgePressed);`n            TickProgression(dt); if (Finished) return;`n")
$g=$g.Replace('Health = Mathf.Max(0, Health - Mathf.Max(1, (e.elite ? 18 : 9) - Armor)); invulnerability = .65f;', 'ReceiveDamage(e.elite ? 18 : 9);')
$g=$g.Replace('if (Random.value < CritChance) amount *= 2; e.health -= amount;', 'bool critical = Random.value < CritChance; if (critical) amount *= 2; HitFeedback(e, amount, critical); e.health -= amount;')
$g=$g.Replace('BONK / SURVIVOR','POTTU / BONK')
$g=$g.Replace('WASD / nuolet  Liiku     •     Hyökkäys automaattinen     •     E  Tutki     •     TAB  Kauppa / tauko','WASD  Liiku   •   SPACE  Kierähdä   •   E  Tutki   •   TAB  Kauppa')
$g=$g.Replace('            if (!Finished) DrawProgression();','            DrawPottuHUD();' + "`n            if (!Finished) DrawProgression();")
Set-Content PrototypeTools/SurvivorGame.pottu.cs $g -Encoding utf8
$p=Get-Content Assets/Survivor/Scripts/SurvivorProgression.cs -Raw
$p=$p.Replace('Sword / Miekka','Pannu / BONK').Replace('Leveä automaattinen sivallus.','Leveä pannunheilautus. Asetaso kasvattaa pannua.').Replace('miekan','pannun')
$p=$p.Replace('Health = Mathf.Max(0, Health - Mathf.Max(1, 22 - Armor)); if (Health <= 0) FinishRun();','ReceiveDamage(22);')
$p=$p.Replace('                float reach = 4 * Size;', '                float reach = 4 * Size;' + "`n                if (best < reach * reach) pottu.Swing(aim);")
$p=$p.Replace('weapons += w.Key + " " + w.Value', 'weapons += (w.Key == Weapon.Sword ? "Pannu" : w.Key.ToString()) + " " + w.Value')
Set-Content PrototypeTools/SurvivorProgression.pottu.cs $p -Encoding utf8
