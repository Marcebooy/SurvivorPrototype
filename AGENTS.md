# AGENTS.md

## Lue tämä ensin

Lue tämä tiedosto ennen jokaista tähän Unity-projektiin liittyvää tehtävää ja noudata sen ohjeita. Jos projektin alikansioissa on omia `AGENTS.md`-tiedostoja, noudata niitä kyseisen alikansion tiedostoissa tämän tiedoston lisäksi.

## Projekti

- Unity-versio: `6000.6.0f1`
- Projektin juuri: `C:\Users\marku\Documents\ChatGPT\Unity\My project`
- Unity CLI on käytettävissä projektikohtaisesti.
- Avoimen Editorin tila tarkistetaan komennolla `unity status`.

## Työskentelyohjeet

- Tee muutokset ensisijaisesti `Assets`-kansioon.
- Älä muokkaa `Library`-, `Temp`- tai `Logs`-kansioita käsin.
- Älä poista tai korvaa projektitiedostoja ilman käyttäjän nimenomaista pyyntöä.
- Säilytä nykyinen Unity-versio, ellei käyttäjä pyydä vaihtamaan sitä.
- Käytä Unity CLI:tä, kun tehtävä koskee avointa Unity Editoria, projektin tilaa, testejä tai buildausta.
- Tarkista muutosten jälkeen Unity-konsolin virheet ja aja asiaankuuluvat testit, jos se on turvallista.
- Ilmoita ennen laajoja tai mahdollisesti rikkovia muutoksia, mitä aiot muuttaa.

## Unity CLI

Kun Editor on auki, tarkista yhteys:

```powershell
unity status
```

Käytä projektin omaa Unity CLI -taitoa Unity Editorin ohjaamiseen. Jos CLI ei löydä Editoria, tarkista ensin, että oikea projekti on avattu ja että Pipeline-paketti on asennettu.

## Julkaisuprosessi

Peli jaetaan GitHubin ([Marcebooy/SurvivorPrototype](https://github.com/Marcebooy/SurvivorPrototype), public) kautta. Kaverin pysyvä latauslinkki launcherille: `https://github.com/Marcebooy/SurvivorPrototype/releases/download/launcher/SurvivorPrototypeLauncher.exe` (launcher itse tarkistaa/lataa uusimman pelin releasen automaattisesti).

Uutta versiota julkaistaessa:
1. Buildaa peli (Unity CLI, StandaloneWindows64), zippaa `Builds/`-kansioon.
2. `git add`/`commit`/`push` koodimuutokset.
3. `gh release create vX.Y.Z <zip> --title "vX.Y.Z" --notes "..."` — releasen kuvaustekstiin muutoslista.
4. **Discord-ilmoitus on lähetettävä erikseen käsin webhookilla** — GitHubin oma Discord-integraatio (poistettu 2026-09-08) näyttää vain otsikkorivin, ei koskaan release-tekstiä. Lähetä sen sijaan oma viesti suoraan webhookiin, esim.:
   ```bash
   WEBHOOK=$(cat ".secrets/discord-webhook.txt")
   curl -s -X POST "$WEBHOOK" -H "Content-Type: application/json" -d @- <<'EOF'
   {"content": "**Uusi päivitys julkaistu: vX.Y.Z**\nKäynnistä peli uudelleen — launcher lataa päivityksen automaattisesti.\n\nMuutokset:\n• ...\n• ..."}
   EOF
   ```
   Webhook-URL on tallennettu `.secrets/discord-webhook.txt`-tiedostoon (gitignoroitu — **älä koskaan committaa tätä URL:ia**, repo on public ja URL toimisi kenelle tahansa salasanana kanavalle kirjoittamiseen).

## Viestintä

- Vastaa käyttäjälle suomeksi, ellei hän pyydä muuta.
- Kerro lyhyesti, mitä muutettiin ja miten muutos varmennettiin.
- Älä väitä tehtävää valmiiksi ilman tarkistusta.
- Kirjaa tehdyt muutokset lyhyesti tiedoston lopun "Muutosloki"-osioon (uusin ylimpänä).

## MegaBonk-koonti

MegaBonk on 3D-muotoinen roguelike survival- eli bullet-heaven-peli. Pelaaja liikkuu itse, aseet hyökkäävät automaattisesti, vihollisia tulee jatkuvina aaltoina ja jokainen pelikerta rakentuu satunnaisista päivityksistä. Tavoite on selvitä mahdollisimman pitkään, voittaa alueiden bossit ja avata pysyvästi uusia hahmoja, aseita ja esineitä.

Peli sisältää 20 hahmoa, 30 asetta, yli 70 esinettä ja 240 tehtävää. Yksi pelikerta etenee näin:

1. Valitse hahmo ja aloita yleensä yhdellä aseella.
2. Liiku kartalla, tapa vihollisia ja kerää XP:tä, kultaa sekä esineitä.
3. Level up antaa satunnaisia ase-, Tome- tai stat-päivityksiä.
4. Tutki karttaa: arkut, shrinet, kauppias, salaisuudet ja bossiportaalit tarjoavat palkintoja tai riskejä.
5. Voita bossi ja jatka seuraavalle alueelle tai vaikeustasolle.
6. Kuoleman jälkeen käytä Silver-valuuttaa pysyviin avauksiin ja parannuksiin.

### Aseet

#### Aloitusaseet

- Sword: leveä lähitaistelusivallus. Hyötyy Damage-, Size-, Quantity- ja Cooldown-statseista.
- Flamewalker: jättää tulijäljen. Hyötyy Damage-, Duration-, Size- ja Cooldown-statseista.
- Lightning Staff: iskee lähellä oleviin vihollisiin ja voi ketjuttaa. Hyötyy Damage-, Quantity-, Bounces- ja Cooldown-statseista.
- Firestaff: räjähtäviä tulipalloja. Hyötyy Damage-, Projectile Count-, Size- ja Projectile Speed -statseista.
- Chunkers: hahmon ympärillä kiertäviä kiviä. Hyötyy Damage-, Quantity-, Size- ja Projectile Speed -statseista.
- Bone: vihollisista toiseen pomppiva luuammus. Hyötyy Damage-, Quantity-, Bounces- ja Projectile Speed -statseista.
- Bow: vihollisten läpi meneviä nuolia. Hyötyy Damage-, Quantity-, Crit- ja Projectile Speed -statseista.

#### Avattavat aseet

- Revolver: useita kimpoilevia luoteja.
- Aegis: torjuu vahinkoa ja vapauttaa vastaiskun.
- Bananarang: ulos lentävä ja takaisin palaava banaani.
- Aura: jatkuva vahinkokehä pelaajan ympärillä.
- Axe: pyörivät, laajasti osuvat kirveet.
- Space Noodle: linkittyy viholliseen ja vahingoittaa välissä olevia kohteita.
- Sniper Rifle: hidas mutta erittäin voimakas läpäisevä laukaus.
- Slutty Rocket: hakeutuvat räjähtävät raketit.
- Shotgun: lyhyen kantaman haulikkoparvi.
- Mines: vihollisen kosketuksesta räjähtävät miinat.
- Wireless Dagger: hakeutuvat tikarit, jotka voivat kimmota.
- Frostwalker: hidastaa ja jäädyttää vihollisia.
- Tornado: työntää ja vahingoittaa vihollisia.
- Dexecutioner: teräase, jolla on execute-mahdollisuus tavallisiin vihollisiin.
- Blood Magic: tapot voivat kasvattaa pysyvää Max HP:tä.
- Black Hole: vetää vihollisia kasaan ja vahingoittaa niitä.
- Poison Flask: luo myrkyllisiä alueita.
- Katana: nopeat automaattiset iskut lähimpään kohteeseen.
- Dragon's Breath: automaattisesti suuntautuva tulikartio.
- Dice: satunnainen vahinko; hyvät heitot kasvattavat critiä.
- Hero Sword: voimakas lähitaistelu- ja etäviilto.
- Corrupted Sword: vahvistuu pelaajan HP:n ollessa matala.
- Scythe: 360 asteen viikatehyökkäys; elite-tapot lataavat vahvemman iskun.

### Aseiden tärkeimmät statsit

- Damage: kasvattaa aseiden vahinkoa.
- Cooldown / Attack Speed: lyhentää hyökkäysten välistä aikaa.
- Quantity / Projectile Count: kasvattaa ammusten tai hyökkäysten määrää.
- Size: kasvattaa ammuksen, auran, miekan tai räjähdyksen kokoa.
- Projectile Speed: kasvattaa ammusten nopeutta; joillakin aseilla se vaikuttaa myös kiertonopeuteen.
- Duration: kasvattaa tulijälkien, myrkkyalueiden, miinojen ja muiden efektien kestoa.
- Bounces: lisää pomppukertoja.
- Crit Chance: kriittisten osumien todennäköisyys.
- Crit Damage: kriittisten osumien lisävahinko.
- Knockback: työntää vihollisia kauemmas.
- Lifesteal: antaa mahdollisuuden parantaa yksi HP hyökkäyksellä.
- Armor: vähentää saatua vahinkoa.
- Evasion: antaa mahdollisuuden väistää isku kokonaan.
- Luck: parantaa harvinaisempien päivitysten, esineiden ja aseiden todennäköisyyttä.
- Movement Speed: nopeuttaa liikkumista ja voi olla hahmokohtaisesti myös vahinkostat.

Kaikki statsit eivät vaikuta kaikkiin aseisiin samalla tavalla. Esimerkiksi Quantity ei juuri hyödytä Auraa, kun taas Bone, Revolver, Bow ja Lightning Staff hyötyvät siitä paljon. Bounces on yleensä aseen oma päivitys, jota ei saa tavallisilla Tomeilla.

### Tomes, esineet ja shrinet

Tomeja ovat esimerkiksi Damage, Cooldown, Quantity, Size, Precision, Luck, Armor, Evasion, Movement Speed, Knockback, Cursed ja Bloody Tome. Ne vahvistavat tiettyjä statseja tai mahdollistavat erityisiä build-tyylejä.

Esineet antavat erikoisbonuksia ja muodostavat synergioita. Chonkplate kasvattaa kestävyyttä ja overhealia, Ice Cube tukee jäädytysbuildia, Anvil lisää aseiden päivityksiin ylimääräisen statin ja Microwave voi monistaa esineitä. Lisäksi pelissä on crit-, myrkky-, lifesteal- ja thorns-synergioita.

Shrinet ovat riski–palkinto-valintoja: ne voivat antaa tehokkaan palkinnon, mutta samalla viholliset, bossit tai runin vaikeus voivat vahvistua. Powerupit ovat väliaikaisia kentältä poimittavia bonuksia, eivät pysyviä esineitä.

### Esimerkkibuildeja

- Turvallinen aloittelijabuild: Flamewalker + Bow + Lightning Staff.
- Lähitaistelubuild: Katana + Aura + Axe tai Scythe.
- Crit-build: Dice + Revolver + Bow tai Sniper Rifle.
- Tank-build: Aegis + Aura + Armor, Thorns ja HP.
- Myrkkybuild: Poison Flask + Revolver + Duration ja Projectile Speed.
- Crowd-control-build: Black Hole + Frostwalker + Lightning Staff.

Tarkat perusvahingot, cooldownit ja upgrade-arvot riippuvat peliversiosta ja muuttuvat päivityksissä. Tämä koonti kuvaa aseiden toimintaa ja statseja yleisellä tasolla; numeeriset arvot kannattaa tarkistaa aina kyseisen version pelistä tai ajantasaisesta tietokannasta.

## Muutosloki

Lyhyt loki tehdyistä muutoksista. Uusin ylimpänä.

- 2026-09-08: Aura-efektin näkyvä koko korjattu vastaamaan oikeaa vahinkosädettä (tekstuurin läpinäkyvä reuna teki sen aiemmin liian pieneksi; kerroin 3.386→4.5). Vahvistettu skaalautuvan Size-statin mukana. Poistettu Lightning-aseen vanha keltainen "Chain lightning" -kuutiopalkki, jäljellä vain uusi Zap VFX -osumaefekti (`SurvivorArsenal.cs`, `SurvivorProgression.cs: AttackWeapons`).
- 2026-09-08: Aura-aseelle lisätty FX_magic_plane_II-paketin taikaympyrä violettina versiona (uudet materiaalit `Flipbook_Effects_Purple01-03.mat` + prefab-variantti `FX_Purple.prefab`, tintattu `_TintColor`-ominaisuudella olemassa olevista Blue-materiaaleista). Seuraa pelaajaa ja skaalautuu Auran säteen (Size-statin) mukana vanhan LineRenderer-renkaan rinnalla (`SurvivorGame.cs: auraEffectPrefab`, `SurvivorArsenal.cs: CastArsenal, TickOrbits, ClearArsenal`).
- 2026-09-08: Lightning-aseen (Salamasauva) ketjuosumiin lisätty Zap VFX -paketin sininen räjähdysefekti (`Assets/Vefects/Zap VFX URP/VFX/Zap/Particles/VFX_Zap_02_Blue.prefab`) jokaisen osuman kohdalle vanhan palkki-visualisoinnin lisäksi (`SurvivorGame.cs: lightningZapEffect`, `SurvivorProgression.cs: AttackWeapons`).
- 2026-09-08: Tavalliset viholliset ("Chaser"/"Brute") käyttävät nyt Toon_RTS-luurankomallia (`Assets/Toon_RTS/unded_Demo/prefab/UD_demo_character.prefab`) primitiivikapselin sijaan, animoituna (Idle/Walk/Attack, uusi `Assets/Survivor/Animations/UndeadEnemy.controller`). Elite-viholliset skaalattu isommaksi ja väritetty oranssiksi property-blockilla. Koko 2x/2.8x (elite), kääntyvät liikesuuntaan. Jos prefabia ei ole asetettu, putoaa takaisin vanhaan kapseliin (`SurvivorGame.cs: SpawnEnemy, Update`).
- 2026-09-08: Toteutettu koonnin loput 21 asetta (Revolver, Aegis, Bananarang, Axe, Space Noodle, Sniper Rifle, Slutty Rocket, Mines, Wireless Dagger, Frostwalker, Tornado, Dexecutioner, Blood Magic, Black Hole, Poison Flask, Katana, Dragon's Breath, Dice, Hero Sword, Corrupted Sword, Scythe). Yhteensä 30 pelattavaa prototyyppiasetta; asekaapissa 10 sivua, kaikki myös tasovalinnoissa ja arkuissa. Uudet erikoisammukset, alueet, kilpi, jäädytys, teloitussuojaus, kierroskohtainen Blood Magic -HP ja viikatteen lataus: `SurvivorAdvancedWeapons.cs`. Nykyiset 1 % Tomet, omistetun aseen bonus, päävalikko, 60-säteinen areena ja bossisäännöt säilytetty. Asekohtaiset testit ja kaikkien 30 aseen yhteistesti ajettu Unity CLI:llä; ei uusia konsolivirheitä. Mallit/efektit ovat prototyyppitasoa, pysyviä aseiden avausehtoja ei ole eikä uutta releasea julkaistu. Ohjeet: `Assets/Survivor/README-AllWeapons.md`; testit: `PrototypeTools/VerifyCompleteWeapons.cs` ja `VerifyCurrentRules.cs`.

- 2026-09-08: Geneeristen Tomejen (Damage, Cooldown, Size, Movement, Duration, Projectile Speed) prosenttibonukset pudotettu ~20 %:sta 1 %:iin per valinta, ja Precision Tome 10 prosenttiyksiköstä 1:een — kuvaustekstit päivitetty vastaamaan (`SurvivorProgression.cs: ApplyUpgrade, UpgradeDetails`).
- 2026-09-08: Vihollisen tappaessa 1 % mahdollisuus pudottaa terveyspurkki (punertava pallo), joka parantaa 25 HP kerättäessä (max HP:hen asti) — erillinen XP/kulta-pudotuksista (`SurvivorGame.cs: Hit, Drop, Update`).
- 2026-09-08: Vihollisten elämä/enimmäismäärä skaalautuu enemmän per alue (+45 % elämä, +30 kpl katto per bossin jälkeinen alue), ja XP per tappo pienenee tasojen myötä (-5 %/taso, pohja 35 %) (`SurvivorGame.cs: SpawnEnemy, GrantExperience, Update`).
- 2026-09-08: Havaittiin ettei Discordin GitHub-integraatio näytä release-tekstiä lainkaan (vain otsikkorivi) — poistettiin se repo-webhook ja siirryttiin lähettämään julkaisuilmoitus suoraan Discordin webhookiin käsin muotoillulla viestillä. Ks. "Julkaisuprosessi"-osio. Webhook-URL siirretty gitignoroituun `.secrets/discord-webhook.txt`-tiedostoon (repo on public, URL ei saa päätyä sinne).
- 2026-09-08: Kun 90s bossiportaali-ajastin nollautuu, uusien vihollisten spawni loppuu heti (olemassa olevat jäävät jäljelle tapettavaksi) — pelaaja siivoaa loput ja kävelee sitten portaalille kutsumaan bossin. XP-kerääminen estyy bossin aikana (kulta yhä kertyy). Bossin kuoltua alue vaihtuu automaattisesti ~2s kuluttua (`SurvivorGame.cs: Hit`, `SurvivorProgression.cs: TickProgression, Interact`).
- 2026-09-08: Areena kasvatettu (säde 38 → 60, pilarit/lattiamerkit/arkut/pyhäkkö/portaali skaalautuvat mukana). Kokeiltiin lisäksi kiipeäviä kukkuloita (korkeusvaihtelu), mutta ne poistettiin käyttäjän pyynnöstä — kartta jäi isommaksi mutta tasaiseksi (`SurvivorGame.cs`, `SurvivorPottu.cs`, `SurvivorProgression.cs`).
- 2026-09-08: Kauppa-, asevalinta- ja tasopäivitysvalikot piilottavat nyt taustalla olevat HUD-tekstit (yläpalkki, ohjeet, "ALUE X" / "Arkut:" -rivit) kokonaan sen sijaan että ne vain himmenivät läpinäkyvästi (`SurvivorGame.cs`, `SurvivorProgression.cs`).
- 2026-09-08: "Poistu päävalikkoon" -kesken jäävä kierros tallentaa nyt selviytymisajan (jos ennätys) ja maksaa Silver-palkkion tapoista, kuten normaali kuolema (`SurvivorGame.cs: ExitToMainMenu`).
- 2026-09-08: Jo omistetun aseen valitseminen tasopäivityksessä antaa nyt aina joko +15 % Damage tai +15 % Size (n. 35 % erillinen mahdollisuus kummallekin, taataan vähintään toinen) — ei enää koskaan "tyhjää" päivitystä (`SurvivorProgression.cs: ApplyUpgrade`, `GrantWeaponBonus`).
- 2026-09-08: Lisätty ESC-taukovalikkoon "Poistu päävalikkoon" -nappi ja päävalikkoon "LOPETA"-nappi (`SurvivorGame.cs`).
- 2026-09-08: Pystytetty GitHub-repo ([Marcebooy/SurvivorPrototype](https://github.com/Marcebooy/SurvivorPrototype), public), GitHub Releases -pohjainen build-jakelu, Discord-webhook release-ilmoituksille, sekä itsepäivittyvä launcher (`Launcher/`) kaverille jaettavaksi. Pysyvä latauslinkki: `https://github.com/Marcebooy/SurvivorPrototype/releases/download/launcher/SurvivorPrototypeLauncher.exe`.
