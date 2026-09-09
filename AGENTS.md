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

- Revolver: useita kimpoilevia luoteja. Hyötyy Damage-, Bounces-, Quantity- ja Cooldown-statseista.
- Aegis: torjuu vahinkoa ja vapauttaa vastaiskun.
- Bananarang: ulos lentävä ja takaisin palaava banaani.
- Aura: jatkuva vahinkokehä pelaajan ympärillä.
- Axe: pyörivät, laajasti osuvat kirveet. Hyötyy Damage-, Size-, Quantity- ja Cooldown-statseista.
- Space Noodle: linkittyy viholliseen ja vahingoittaa välissä olevia kohteita. Hyötyy Damage-, Duration-, Size- ja Cooldown-statseista.
- Sniper Rifle: hidas mutta erittäin voimakas läpäisevä laukaus.
- Slutty Rocket: hakeutuvat räjähtävät raketit. Hyötyy Damage-, Quantity-, Size- ja Cooldown-statseista.
- Shotgun: lyhyen kantaman haulikkoparvi. Hyötyy Damage-, Quantity-, Size- ja Cooldown-statseista.
- Mines: vihollisen kosketuksesta räjähtävät miinat. Hyötyy Damage-, Duration-, Size- ja Cooldown-statseista.
- Wireless Dagger: hakeutuvat tikarit, jotka voivat kimmota. Hyötyy Damage-, Bounces-, Quantity- ja Cooldown-statseista.
- Frostwalker: hidastaa ja jäädyttää vihollisia. Hyötyy Damage-, Duration-, Size- ja Cooldown-statseista.
- Tornado: työntää ja vahingoittaa vihollisia. Hyötyy Damage-, Duration-, Size- ja Cooldown-statseista.
- Dexecutioner: teräase, jolla on execute-mahdollisuus tavallisiin vihollisiin.
- Blood Magic: tapot voivat kasvattaa pysyvää Max HP:tä.
- Black Hole: vetää vihollisia kasaan ja vahingoittaa niitä. Hyötyy Damage-, Size-, Duration- ja Cooldown-statseista.
- Poison Flask: luo myrkyllisiä alueita. Hyötyy Damage-, Size-, Duration- ja Cooldown-statseista.
- Katana: nopeat automaattiset iskut lähimpään kohteeseen. Hyötyy Damage-, Cooldown-, Crit- ja Quantity-statseista.
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

## Hahmot

Hahmot ovat selkeitä fantasy-/roguelite-arkkityyppejä. Jokaisella hahmolla on oma aloitusase, yksi vahva passiivi ja selkeä pelityyli. Visuaalinen tyyli on pelkistetty low-poly: suuret aseet, vahvat siluetit ja liioitellut animaatiot.

### Pottu

- Rooli: tasapainoinen lähitaistelija ja perushahmo.
- Aloitusase: Pannu.
- Pelityyli: luotettava lähietäisyyden vahinko, kohtuullinen HP, armor ja liike.
- Visuaalinen idea: ylisuuri metallikypärä, pienet saappaat ja raskas paistinpannu.

### Ritari

- Rooli: tankki ja lähitaistelija.
- Aloitusase: Sword tai Aegis.
- Pelityyli: paljon HP:tä ja armoria, pienempi liikkumisnopeus, vahva knockback ja turvallinen lähitaistelu.
- Passiivi-idea: osa osumista torjutaan kilvellä; torjunta voi ladata vastaiskun.

### Metsästäjä

- Rooli: etäisyysvahinko ja kriittiset osumat.
- Aloitusase: Bow.
- Pelityyli: suuri Projectile Speed, Crit Chance ja kantama, mutta heikompi HP ja Armor.
- Passiivi-idea: kaukana oleviin vihollisiin tehdään lisävahinkoa ja ensimmäinen osuma voi olla kriittinen.

### Velho

- Rooli: area damage ja elementtiefektit.
- Aloitusase: Lightning Staff tai Firestaff.
- Pelityyli: suuri Damage, Size, Quantity ja Duration; heikompi kestävyys ja liike.
- Passiivi-idea: aseiden elementtiefektit voivat levitä lähellä oleviin vihollisiin tai tehostua peräkkäisistä osumista.

### Ninja

- Rooli: nopea glass cannon ja väistöihin perustuva hahmo.
- Aloitusase: Wireless Dagger tai Katana.
- Pelityyli: erittäin suuri Movement Speed ja Evasion, nopea Attack Speed ja matala HP.
- Passiivi-idea: onnistunut väistö antaa hetkellisen hyökkäysnopeus- tai kriittisyysbonuksen.

### Berserkeri

- Rooli: aggressiivinen lähitaistelija, joka hyötyy matalasta HP:stä.
- Aloitusase: Axe tai Corrupted Sword.
- Pelityyli: erittäin suuri Damage ja Knockback, mutta puolustus heikkenee taistelun aikana.
- Passiivi-idea: vahinko kasvaa, kun HP laskee; elite-tapot voivat palauttaa osan HP:stä.

### Necromancer

- Rooli: kutsuihin ja tappojen ketjuttamiseen perustuva hahmo.
- Aloitusase: Bone tai Scythe.
- Pelityyli: heikompi suora vahinko, mutta tappojen jälkeen kentälle jääviä tai vihollisia jahtaavia luurankoja.
- Passiivi-idea: tietyllä tappomäärällä syntyy väliaikainen luurankosoturi tai luupallo.

### Paladiini

- Rooli: puolustava hybridihahmo.
- Aloitusase: Aura tai Aegis.
- Pelityyli: Armor, HP, Healing ja Knockback; hyökkäysnopeus on muita hahmoja pienempi.
- Passiivi-idea: pelaajan ympärillä oleva aura vahvistuu hetkeksi, kun pelaaja torjuu vahinkoa tai kerää healing-esineen.

### Alkemisti

- Rooli: myrkky-, tuli- ja räjähdysalueiden hallitsija.
- Aloitusase: Poison Flask tai Mines.
- Pelityyli: vahva Damage over Time, Duration ja Area; suora osumavahinko on heikompi.
- Passiivi-idea: samaan viholliseen kertyvät elementtiefektit voivat yhdistyä voimakkaaksi reaktioksi.

### Golem

- Rooli: hidas tankki ja alueen hallitsija.
- Aloitusase: Chunkers tai Aura.
- Pelityyli: erittäin suuri HP, Armor, Size ja Knockback, mutta pieni Movement Speed ja Attack Speed.
- Passiivi-idea: pelaaja ei voi joutua yhtä helposti työnnetyksi, ja vahingon saaminen lataa maahan iskeytyvän shokkiaallon.

### Pyromancer

- Rooli: tulivahinkoon ja suuriin alue-efekteihin erikoistunut taikuri.
- Aloitusase: Firestaff tai Flamewalker.
- Pelityyli: suuri Size, Duration ja Damage; tuliefektit sytyttävät vihollisia ketjureaktiona.
- Passiivi-idea: palavat viholliset jättävät kuollessaan lyhyen tulialueen.

### Onnenkorttihahmo

- Rooli: riskialtis, satunnaisuuteen ja harvinaisiin build-yhdistelmiin perustuva hahmo.
- Aloitusase: Dice.
- Pelityyli: korkea Luck ja vaihteleva vahinko; tasovalinnoissa ja arkuissa on parempi mahdollisuus harvinaisiin vaihtoehtoihin.
- Passiivi-idea: osa hyökkäyksistä voi olla poikkeuksellisen heikkoja tai erittäin voimakkaita; hyvät heitot voivat kasvattaa Crit Chancea hetkeksi.

Hahmojen lopulliset arvot kannattaa pitää maltillisina, jotta aseet, Tomes ja esineet ratkaisevat edelleen suurimman osan buildista. Hahmojen pitäisi tuntua erilaisilta jo aloitusaseen, yhden passiivin ja muutaman näkyvän perusstatin perusteella.

## Muutosloki

Vanhemmat merkinnät: ks. CHANGELOG_ARCHIVE.md

- 2026-09-10: Viiden WeaponArtPilot-lisäaseen pehmeämpi tyylitelty viimeistely (Dexecutioner, HeroSword, WirelessDagger, DragonsBreath, Flamewalker): pyöristetyt koristeet, kaarevat profiilit, lohikäärmeen pää ja volumetriset liekit; omat Crafted-palettimateriaalit. Samat prefab/Resources-avaimet ja socket-data, ei runtime-koodimuutoksia. Muokattava lähde WeaponBases/Library/WeaponBases.blend, aiempi versio PreviousBlockout-kansiossa ja työohje STYLIZED_UPDATE.md. FBX- ja Unity-tuonnin tarkistukset toimituksen raporteissa.

- 2026-09-10: WeaponArtPilot-asekirjastoon lisätty puuttuneet Blade_Dexecutioner, Blade_HeroSword, Blade_WirelessDagger, Staff_DragonsBreath ja Relic_Flamewalker. Sama kahva-/sauva-/reliikkipohja ja CONVENTIONS-sopimus; prefabit suoraan Library/Prefabs/Resources-kansioon Coden nykyisen latauksen mukaisesti. catalog.json sisältää uusille malleille weaponEnum/resource/prefabPath-kentät (DragonBreath → Staff_DragonsBreath). Muokattava lähde ja kytkentäohje työtilan WeaponBases/MISSING_FIVE_HANDOFF.md. 26/26 FBX-uudelleentuontia ja 5/5 uutta Resources.Load-prefabia varmennettu. WeaponVisualCatalog-koodia ei muutettu; taulukkorivit annettu ohjeessa Codelle.

- 2026-09-10: **Loput 29 aseen kytkentä WeaponAttachmentPointiin** (pelaaja-progressio-ja-ranked-suunnitelma.md, kohta 10.6) - Sword/Kaksoiskajo oli jo kytketty edellisessä tehtävässä; tässä käytiin läpi Astran 21 pohjamallia (`Assets/WeaponArtPilot/Documentation/catalog.json`) ja kytkettiin kaikki yksiselitteisesti tunnistettavat.
  - **Kytketty onnistuneesti (17 asetta)**, catalog.json:in `"role"`-kentän mukaan: Blade_Katana→Katana, Blade_CorruptedSword→CorruptedSword, Staff_Lightning→Lightning, Staff_Firestaff→Firestaff, Gun_Revolver→Revolver, Gun_Shotgun→Shotgun, Gun_Sniper→Sniper, Gun_Rocket→Rocket, Relic_Chunkers→Chunkers, Relic_Dice→Dice, Kit_Bone→Bone, Kit_PoisonFlask→PoisonFlask, Kit_Mine→Mines, Kit_Bananarang→Bananarang, Kit_ThrowingAxe→Axe (rooli "ThrowingAxe" vs enum "Axe" - hyväksytty koska Axe on peliteksin mukaan nimenomaan lentävä/pyörivä kirves eikä muuta "Axe"-nimistä mallia kilpaile paikasta), Bow_Recurve→Bow, Heavy_Scythe→Scythe.
  - **Uusi `WeaponVisualCatalog.cs`**: yksi taulukkopohjainen komponentti (ei 17 erillistä SwordVariantVisual-kopiota) - sama `WeaponAttachmentPoint.Attach(...,AttachmentSocket.RightHand)`-malli, yksi instanssi per ase (ei per hahmo), uudelleenkiinnitetään hahmon vaihtuessa (`weaponVisualsAttached`-lippu, resetoidaan `BuildCharacterVisual`:ssa samoin kuin `swordVariantAttached`). `TickWeaponVisuals()` kutsutaan `Update()`:sta `TickSwordVariantVisual()`:in vierestä. Kaksikätinen Heavy_Scythe kiinnittyy samaan RightHand-socketiin yhden vanhemman kautta - sen `SupportGrip`-lapsitransform tulee mukana käyttämättömänä, ei kaksoisparentointia (IK rajattu pois tästä tehtävästä, kuten pyydettiin).
  - **Resources-siirto vaadittu ennen kytkentää**: 17 tarvittua prefabia elivät `Assets/WeaponArtPilot/Library/Prefabs/`:ssä ilman `Resources`-kansiota - `Resources.Load` ei olisi löytänyt niitä ajonaikaisesti. Siirretty Unity CLI:n `move_asset`-työkalulla (AssetDatabase.MoveAsset, säilyttää GUID:in) uuteen `Assets/WeaponArtPilot/Library/Prefabs/Resources/`-alikansioon - loput 4 (ks. alla) jätettiin paikoilleen koskemattomina.
  - **(b) Jäävät kokonaan ilman mallia (8 asetta, ei catalog.json-vastinetta ollenkaan)**: Aegis, SpaceNoodle, WirelessDagger, Dexecutioner, BloodMagic, DragonBreath, HeroSword, **ja Flamewalker** (tehtävänannon esimerkkilistassa ei mainittu, mutta catalog.json:issa ei ole sille vastinetta - tarkistettu, ei arvattu). Näille Astra ei ole vielä toimittanut geometriaa.
  - **(c) Epäselvät/ei-kytketyt tapaukset catalog.json:ista**: `Relic_ArcaneFocus` (rooli "ArcaneFocus") - **ei yksiselitteinen**, kohdan 10.6 suunnitelma ehdotti sitä yhteiseksi kantajaksi VIIDELLE eri aseelle (Aura, Aegis, Black Hole, Space Noodle, Blood Magic) eikä catalog.json kerro kumpaa - jätetty kytkemättä, älä arvaa. `Kit_QualityRune` (rooli "QualityRune") - ei ase lainkaan, pelkkä laatutason koristemesh (integraatio-ohje, ei ajettavaa logiikkaa). `Heavy_WarAxe`/`Heavy_Hammer` (roolit "WarAxe"/"Hammer") - toimitettu, mutta **ei vastaavaa Weapon-enum-arvoa olemassa** (kohdan 10.6 roadmapin "tulevat raskaat kirveet/vasarat", ei vielä lisätty peliin) - odottavat tulevaa asetta, ei kytketty.
  - **Efektipohjaiset, ei mallia tarvitse (4 asetta, task-instruktio)**: Aura, Frostwalker, Black Hole, Tornado - tunnistetaan VFX:stä, ei kosketettu.
  - **Testattu Unity CLI:llä Play Modessa**: kaikki 9 hahmoa × 17 asetta (153/153 yhdistelmää) - jokainen malli latautui, kiinnittyi (`parent != null`), ja aktivoitui oikein `weaponLevels`-tilan mukaan; 0 konsolivirhettä koko testin ajan. **Tunnetut rajoitukset löydetty hierarkiatarkistuksella** (sama malli kuin Ritari/Sword-rajoitus, joka oli jo dokumentoitu ennen tätä tehtävää): Necromancerilla baked "Ancient bone staff" (Bone), Hunterilla baked jousi+viini (Bow), Berserkerilla baked kirveet käsissä (Axe/CorruptedSword), Ninjalla baked katana-terät + varjokopioiden omat terät (Katana) - näillä neljällä hahmolla vanha baked-mesh ja uusi WeaponArtPilot-malli näkyvät päällekkäin samaan tapaan kuin Ritarin miekka. Golem/Chunkers ei näyttänyt vastaavaa konfliktia (orbit-ase, ei käsivarren mesh-osaa). Ei korjattu (out of scope, sama rajoitus kuin Ritarilla - vaatisi Astralta erilliset irrotettavat aseosat). Käännös 0 virhettä. Ei koskettu WeaponStats.cs:ään tai muuhun taistelukoodiin.

- 2026-09-09: **Tuhkaerämaa kytketty MapType-enumiin ja MAPIT-tabiin** (pelaaja-progressio-ja-ranked-suunnitelma.md, kohta 0.5/9) - Astran toimittamat `AshWastesEnvironment.cs`/`SurvivorAshWastesMap.cs` (Assets/Survivor/Scripts/) ja `AshWastesBuilder.cs` (Assets/Survivor/Editor/) kytketty pelikoodiin README-Tuhkaeramaa.md:n "Pelikoodin kytkentä" -ohjeen mukaisesti, samalla tavalla kuin Mosswood/Luuluola aikanaan.
  - `SurvivorProceduralMaps.cs`: uusi `MapType.AshWastes`-arvo enumiin. `BuildFixedMap(MapType)` saa kolmannen haaran, joka kutsuu `BuildAshWastesMap()`:ia (README:n ohjeistama funktio, jo olemassa `SurvivorAshWastesMap.cs`:ssä) saman vanhan-kartan-siivouksen jälkeen kuin Mosswood/Luuluola - `MapLayout` pysyy `null`:ina kuten muillakin kiinteillä kartoilla. `DrawMapTypeTab()`:in MAPIT-napit lisätty samaan `foreach`-taulukkoon Mosswood/BoneCaven rinnalle (3×Tier-nappia, sama `DrawMapTypeOption`-esitys ja -nimeäminen kuin muillakin, ei tarvinnut uutta UI-koodia).
  - `MapInventory.cs`: `MapTypeName` sai "Tuhkaerämaa"-nimen, `LoadMapInventory` lataa sen T1-omistuksen samalla mallilla. Kolmen kopion sijaan (`new[] { Mosswood, BoneCave }` esiintyi kolmessa paikassa) refaktoroitu yhdeksi `static readonly DroppableMapTypes`-taulukoksi, jota `TryDropMapItem` (nyt tasan 1/3 mahdollisuus per tyyppi `Random.Range`:llä 50/50:n sijaan), `MapInventorySummary` ja MAPIT-tabin nappilistaus kaikki lukevat - uuden kiinteän kartan lisäys vaatii jatkossa yhden rivin tähän taulukkoon, ei kolmea erillistä muokkausta.
  - **Tietoisesti ei koskettu**: `SurvivorGameNetwork.cs` (moninpelin karttasynkronointi ei tunne MapType/Tieriä millään kartalla - dokumentoitu tunnettu rajoitus, oma myöhempi tehtävänsä kohta 12), `AshWastesEnvironment.cs`/`SurvivorAshWastesMap.cs`/`AshWastesBuilder.cs` itse (Astran toimitus, jo tarkistettu Unityssä).
  - **Testattu Unity CLI:llä Play Modessa, täysi sykli**: Roslyn-evalilla myönnetty AshWastes T1 -karttaesine, valittu MAPIT-tabin tilaa vastaavasti (`selectedMapType`/`selectedMapTier`), `StartGame()` → `SkipLoadout()` (Mappi-tilan loadout-valinta, samalla polulla kuin oikea pelaaja) → tarkistettu `activeMapType=AshWastes`, `fixedMapRoot`=`Tuhkaeramaa_Environment(Clone)` (40 lasta), `forestObstacles.Count=16` (täsmää README:n "16 maastoestettä"), `MapLayout=null` (oikein). `SelectStarter(Sword)` ja 5 sekuntia oikeaa Play Mode -aikaa taistelua - 7 vihollista hengissä, `activeMapType` pysyi AshWastes:ssä koko ajan. `ExitToMainMenu()` palautti `AtMainMenu=true` siististi. **Kontrollitesti**: sama 5 sekunnin taistelusykli Proceduralilla (Selviytymistilan kartta) tuotti täsmälleen saman `NetworkObject.OnDestroy`-`NullReferenceException`-virhesarjan kuin AshWastes-testi (vahvistettu: kaikilla 7 vihollisella on `NetworkObject`-komponentti, virhe tulee Netcode-paketista kun se tuhoutuu ilman aktiivista verkkosessiota Editorissa) - **tämä on siis olemassa oleva, karttatyypistä riippumaton virhe, ei AshWastes-regressio**, ei kirjattu tähän tehtävään korjattavaksi. Karttaesinemäärä ja PlayerPrefs palautettu testiä edeltävään tilaan (0 kpl) puhdistuksessa. Käännös 0 virhettä.

- 2026-09-09: **Ase-affiksijärjestelmän laajennus 6 lisäaseelle** (pelaaja-progressio-ja-ranked-suunnitelma.md, kohta 10.3/11) - Mines, Tornado, Poison Flask, Space Noodle, Slutty Rocket, Wireless Dagger. Affiksit ovat nyt 19/30 aseella (13 aiempaa + tämä 6). Ei koskettu Assets/WeaponArtPilot/-, WeaponBases/- tai WeaponAttachmentPoint.cs/SwordVariantVisual.cs-tiedostoihin (rajaus, Astra saattoi työskennellä samaan aikaan asset-puolella) - `git status` näytti myös kolmannen osapuolen keskeneräistä työtä (SurvivorArsenal.cs/SurvivorGame.cs/SurvivorProgression.cs, uudet Art_Prototype_Stylized/Feyloom/WeaponVfxBuilder.cs/AshWastesBuilder.cs) johon ei koskettu millään tavalla.
  - **AGENTS.md**: kirjattu puuttuneet statshyötylistat kuudelle aseelle ("Hyötyy X-, Y-, Z- ja W-statseista") samalla tyylillä kuin aiemmilla 13:lla - Mines (Damage/Duration/Size/Cooldown), Tornado (Damage/Duration/Size/Cooldown), Poison Flask (Damage/Size/Duration/Cooldown), Space Noodle (Damage/Duration/Size/Cooldown), Slutty Rocket (Damage/Quantity/Size/Cooldown), Wireless Dagger (Damage/Bounces/Quantity/Cooldown).
  - **`WeaponAffixes.cs` refaktoroitu kohdan 10.5 kuvaaman mallin mukaiseksi**: `AffixPool` ei ole enää käsin kirjoitettu taulukko, vaan lazily rakennettu `WeaponStats.cs`:n `WeaponMetadata[w].applicableStats`:sta uudelle `AffixEnabledWeapons`-listalle (19 asetta) - yhden totuuden lähde koodissa ja AGENTS.md:ssä molemmilla puolilla, uuden aseen käyttöönotto vaatii jatkossa enää yhden rivin lisäyksen tähän listaan (kunhan applicableStats+AGENTS.md-lista on jo olemassa). Lazy-rakennus (ei staattinen kenttäalustus) koska `WeaponMetadata` asuu eri partial-class-tiedostossa (WeaponStats.cs) - staattisten kenttien alustusjärjestykseen eri tiedostojen välillä ei voi luottaa.
  - **Valinta perusteltu koodista käsin**: 6 asetta valittiin siitä syystä että niiden `CastAdvanced`/`AddAdvancedShot`/`AddAdvancedZone`-koodi käytti jo `stats.Size`/`stats.Duration`/`stats.Quantity`/`stats.Bounces`-kertoimia identtisellä kaavalla kuin `BlackHole` (jolla affiksit toimivat jo ennestään) - varmistettiin luoduttoman toiminnan sijaan lukemalla koodi ensin. Yksi mahdollinen 7. ehdokas (Sniper) jätettiin tarkoituksella pois, koska sen `LineHit`-kutsu ei lukenut `stats.Size`:a ollenkaan - Size-affiksi ei olisi vaikuttanut mihinkään, ja se korjaus kuuluu vasta Sniperin omaan käyttöönottotehtävään. Rajaukset ennallaan: max 2 affiksislottia, +15-35 %, vain omistetuille aseille, vain Mappi-tilassa (`AffixMultiplier`/`AffixMultiplierFor` samat vartijat kuin ennen), Selviytymistila koskematon (`GetWeaponStats` palauttaa `Identity`:n Proceduralissa, testattu erikseen ettei `activeMapType` jäänyt vahingossa Mosswoodiin - palautettiin Proceduraliksi joka testilohkon jälkeen).
  - **Testattu Unity CLI:llä Play Modessa, samalla suoran laskennan menetelmällä kuin aiemmat 13 asetta**: Roslyn-evalilla pakotettu tunnettu +20 % Damage-affiksi suoraan `weaponAffixes`-dictionaryyn (reflektiolla, ohittaen `RollAffix`:in satunnaisuuden) kullekin 6 aseelle, laukaistu oikea `CastAdvanced(w,level)`-funktio dummy-vihollista vasten (koska Tornado/PoisonFlask/SpaceNoodle/Rocket/WirelessDagger vaativat `Nearest()`:in löytävän kohteen ennen kuin ne tuottavat mitään) ja mitattu syntyneen `AdvancedShot`/`AdvancedZone`-olion `power`-kenttä ennen/jälkeen - **kaikki 6 asetta antoivat tasan 1,2 (base=18→buffed=21,6 tai base=36→43,2 Rocketilla)**, ei yhtään regressiota. Pistokoe muille statseille samalla menetelmällä: Mines Size (2,8→3,36, ratio 1,2), Mines Duration (10→12, ratio 1,2), WirelessDagger Bounces tasolla 3 (baseBounces=2→3, +50 % affiksi), Rocket Quantity (+250 % pakotettu ääriarvo baseQty=1:stä, 1→3 ammusta, `Mathf.RoundToInt`-kaavan mukaisesti) - kaikki täsmäsivät odotettuun kaavaan. **Droppijakauma-otos**: `RollAffix(Mines,...)` 4000 kertaa - vain neljä oikeaa statia (Damage/Duration/Size/Cooldown) esiintyi, ~25 % jokainen (24,7-25,3 %), kaikki prosentit välillä 15-35 %. `TryDropWeaponAffixMaterial()` 3000 kertaa Mines omistettuna - 11,33 % osumaprosentti (odotus 12 %, ero selittyy näyteotoksen vaihtelulla, ~1,1 keskihajonnan sisällä). 0 konsolivirhettä testien aikana (ainoat konsolissa näkyneet virheet olivat aiemman istunnon Netcode-sammutusjäänteitä, eivät tästä testistä). Käännös 0 virhettä.
