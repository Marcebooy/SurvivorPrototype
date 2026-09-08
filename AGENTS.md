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

- 2026-09-08: Korjattu käyttäjän löytämä puute: kaveri näki isännän mutta ei vihollisten animaatioita, ei XP/kulta/terveyspudokkeita maassa, eikä iskujen animaatioita. Kolme erillistä syytä, kolme korjausta: (1) **Viholliset**: `Animator.SetFloat/SetTrigger`-kutsut eivät koskaan olleet verkotettuja - `NetworkAnimator` lisätty `UD_demo_character.prefab`iin, synkkaa Animatorin tilan automaattisesti. (2) **Pudokkeet**: XP/kulta/terveyspallot olivat pelkkiä paikallisia primitiivejä, eivät koskaan `NetworkObject`eja - uusi `DropNet.cs`+prefab (väri `IsHealth`-NetworkVariablesta), käytössä vain kun `IsNetworkHost` (offline yksinpeli käyttää yhä alkuperäistä `Shape()`-primitiiviä muuttumattomana - varmistettu erikseen ettei `dropPrefab`in pelkkä olemassaolo muuta yksinpelin ulkoasua). (3) **Iskujen/liikkeen animaatiot**: `characterVisual.Swing/Bonk/Block/Shockwave` ja `Tick()` ajetaan vain isännän omalla koneella riippumatta siitä kenen puolesta (isäntä tai kaveri) - kumpikaan REMOTE-osapuoli ei koskaan nähnyt niitä. Uusi `TriggerRelayVisual`-kääre (`ICharacterVisual`) toistaa diskreetit laukaisimet RPC:llä (`RemotePlayerNet`/`HostAvatarNet`, molemmat `[Rpc(SendTo.NotServer)]`) jokaisen osapuolen omaan paikalliseen malliin; jatkuva kävelyanimaatio (`Tick`) ei tarvitse verkotusta - jokainen osapuoli päättelee sen paikallisesti jo replikoidusta sijaintimuutoksesta. Sivuhuomio korjattu samalla: `HostAvatarNet`in oma peili näkyi myös isännälle itselleen (kaksi päällekkäistä hahmoa) - isännän oma kopio piilottaa nyt renderoijansa (`IsServer`-tarkistus), vain muut osapuolet näkevät peilin. Testattu Unity CLI:llä Play Modessa: isännän 29 renderoijaa pois päältä omalla koneella, kaveri tappoi vihollisen virheittä (Sword/Lightning), pudokas ilmestyi oikeana `NetworkObject`ina (`IsSpawned=True`), yksinpelin regressio (5 tappoa, 0 `DropNet`-instanssia offline) puhtaana. Julkaistu v0.3.3, ei Discord-ilmoitusta.

- 2026-09-08: Korjattu käyttäjän v0.3.1:ssä löytämä bugi: kaveri ei nähnyt omaa hahmoaan eikä isäntää pelissä. Syy oli kaksiosainen: (1) Vaihe 2:n `Combatant`-refaktoroinnissa kaverin hahmomalli rakentui kahdesta eri, keskenään kilpailevasta järjestelmästä (host-simuloitu `current`-osoittimen kautta ajettu `BuildCharacterVisual` JA uusi `RemotePlayerNet`in NetworkVariable-pohjainen rakennus) samaan Transformiin - isäntä näki kaksi päällekkäistä mallia, kaveri ei nähnyt mitään koska hänen oma koneensa ei koskaan ajanut kumpaakaan oikeaan aikaan. Korjattu niin että vain `RemotePlayerNet.RebuildVisual()` rakentaa mallin (ajetaan jokaisella osapuolella itsenäisesti samasta replikoidusta hahmovalinnasta, ei itse verkota meshiä), ja se kytkeytyy suoraan isännän `Combatant.CharacterVisual`iin taisteluanimaatioita varten (`SurvivorPottu.cs: SelectCharacter` ohittaa oman mallinsa rakennuksen kun `current != hostState`). (2) Isännän oma pelaajahahmo ei ollut koskaan verkotettu mitenkään - kaveri ei voinut nähdä isäntää ollenkaan. Uusi `HostAvatarNet.cs`+prefab: kevyt `NetworkObject` jonka isäntä luo isännöinnin alkaessa ja jonka sijainnin/hahmovalinnan se peilaa joka ruudulla omasta `player`-Transformistaan (`TickNetworkCoop`); muut osapuolet rakentavat siitä saman deterministisen mallin paikallisesti täsmälleen samalla `BuildCharacterVisualOn`-mekanismilla kuin kaverin oma hahmo. Testattu Unity CLI:llä Play Modessa palvelin-omisteisella synteettisellä kaverilla: täsmälleen yksi lapsiobjekti kaverin transformissa hahmonvalinnan jälkeen (ei kaksoiskappaletta), `FriendState.CharacterVisual` oikein kytketty, isäntähahmon peili näytti oikean hahmon ja seurasi sijaintia, ase ampui edelleen virheittä. Julkaistu v0.3.2 (`gh release create`), ei Discord-ilmoitusta.

- 2026-09-08: Käyttäjän pyynnöstä proseduraaliset huoneet ja käytävät korvattu yhdellä laajalla avoimella ympäristöllä (generaattori v2). Seed muuttaa alueen orgaanista ulkoreunaa ja maamerkkien paikkoja. Sisäseiniä tai kapeita käytäviä ei ole; kivireuna on vain kartan ulkorajalla. Seedit, minimappi, vaikeusskaalaus ja bossin jälkeinen automaattinen kartanvaihto säilyvät. Vanhan v1-generaattorin seedit tuottavat nyt v2-geometrian. Rooms-kokoelma on yhteensopivuuden vuoksi jäljellä vain näkymättöminä maamerkkiankkureina.

- 2026-09-08: Julkaistu v0.3.1 (edellinen: v0.3.0). Buildattu Unity CLI:llä (unity-editor-mcp:n `build`-työkalulla, koska Editor oli jo auki eikä erillistä batch-prosessia voinut käynnistää samaan projektiin - `unity build`-CLI epäonnistui lisenssilukituksesta), StandaloneWindows64, 0 virhettä. Kaikki committamattomat muutokset (proseduraaliset huone-/metsäkartat, moninpelin vaihe 2) commitoitu ja pushattu ensin (commit `55b2c60`). **Discord-ilmoitusta ei lähetetty käyttäjän pyynnöstä.**

- 2026-09-08: Moninpelin vaihe 2: kaverille oma hahmovalinta, aseet ja levelijärjestelmä (aiemmin kiinteä vihreä kapseli/lähitaistelu). Koko ase-/progressiojärjestelmä (`SurvivorProgression.cs`, `SurvivorArsenal.cs`, `SurvivorAdvancedWeapons.cs`, ~800 riviä, 30 asetta) oli kovakoodattu yhden `SurvivorGame`-instanssin kenttiin, joten sitä ei parametrisoitu käsin ase kerrallaan - sen sijaan uusi `Combatant.cs` kokoaa kaiken pelaajakohtaisen tilan (statit, aseomistus, asepoolit, XP/level) yhdeksi olioksi, ja `SurvivorGame` pitää `current`-osoitinta joka näyttää joko isännän omaan (`hostState`, aina isännän omassa pelisilmukassa) tai hetkellisesti kaverin `Combatant`iin (vain kun isäntä simuloi kaverin vuoroa `TickNetworkCoop`issa). Kaikki ~30 aseen `this.damage`/`this.player`/`this.weaponLevels`-lukevat metodit toimivat nyt muuttumattomina kumman tahansa pelaajan tilalle - ei tarvinnut kirjoittaa yhtäkään ase-/lasku-riviä uudelleen. Isännän oma pelattavuus säilyi täysin ennallaan (varmistettu Play Modessa: hahmovalinta → asevalinta → viholliset kuolevat → XP/kolikot kertyvät, identtinen ennen/jälkeen). Uudet julkiset apurit (`SurvivorGameNetwork.cs`): `CreateFriendCombatant`, `FriendSelectCharacter`, `FriendSelectStarter`, `FriendChooseUpgrade` (kaikki vaihtavat `current`in väliaikaisesti try/finally-lohkossa). Kaveri lähettää valinnat NGO 2.x:n uusilla `[Rpc(SendTo.Server)]`-metodeilla (`RemotePlayerNet.cs`: `RequestCharacterRpc`/`RequestStarterRpc`/`RequestUpgradeRpc` - projektin ensimmäinen RPC-käyttö, aiempi karttasiemenen synkka käytti `CustomMessagingManager`-viestejä) koska asiakas voi rakentaa koko valintaruudun (hahmokortit, asekaappi, level-up-vaihtoehdot) suoraan jaetusta staattisesta datasta (`CharacterRoster`, `UpgradeNames`/`UpgradeDetails`) ilman verkkoliikennettä - vain itse valinta ylittää verkon. Kaverin HP/Level/Coins/PendingChoices/Choices/Selecting-tila peilataan takaisin asiakkaalle uusilla `RemotePlayerNet`in `NetworkVariable`eilla joita isäntä päivittää joka `TickNetworkCoop`-tikillä. Kaveri kerää nyt myös XP/kulta/terveyspurkkeja pudokkeista lähellään (sama sääntö kuin isännällä) ja Aegis-kilpi toimii kaverillakin (`BlockWithAegis()` uudelleenkäytetty `current`in kautta). Testattu Unity CLI:llä Play Modessa `eval`-kutsuilla palvelin-omisteisella synteettisellä `RemotePlayerNet`illä (toista konetta ei käytettävissä): hahmovalinta-RPC, asevalinta-RPC, Sword tappoi oikean vihollisen ja kaveri sai XP:n/kolikon - konsoli puhdas (vain jo dokumentoidut Netcode-tuhoamis-NRE:t). **Tunnettu rajoitus löytyi testissä**: koska `TickNetworkCoop` kutsutaan isännän oman `Update()`in sisältä pelin päähaarasta, isännän kuolema (`Finished=true`) pysäyttää myös kaverin simulaation kokonaan, vaikka kaveri olisi yhä elossa - ei korjattu tässä, vaatii jatkokäsittelyä. Ei mukana (kuten aiemmin dokumentoitu): kaverin Silver-metaprogressio/kulta-Kauppa, uuden kierroksen/aluevaihdon täysi kaverin tilan resetointi, kahden oikean koneen testi. Suunnitelma: `C:\Users\marku\.claude\plans\squishy-coalescing-flame.md`.

- 2026-09-08: Korjattu käyttäjän testissä löytämä bugi: viholliset eivät koskaan kohdistaneet liikettään kaveripelaajaan (`RemotePlayerNet`), vain isäntään - kaveri saattoi seistä keskellä vihollislaumaa ilman että yksikään tuli häntä kohti, ellei sattumalta ollut jo isännän vieressä. Syy: `Update()`:n vihollissilmukka käytti kovakoodattua `player.position`-kohdetta sekä liikkeen suuntaan (`MapLayout.PursuitDirection`/`MoveOnMap`) että kääntymiseen. Uusi `NearestCombatantPosition(from)`-apumetodi (`SurvivorGameNetwork.cs`) valitsee liikkeen kohteeksi lähimmän elossa olevan pelaajan (isäntä tai mikä tahansa `RemotePlayerNet.Active`-listan kaveri); kontaktivahinko isäntään lasketaan yhä erikseen tuoreesta `player.position`-etäisyydestä ettei kaveri-kohdistus vahingossa muuta isännän omaa osumalogiikkaa. Kaverin oma kontaktivahinko/lähitaistelu hoidettiin jo ennestään `TickNetworkCoop`:ssa etäisyyspohjaisesti riippumatta siitä mitä vihollinen jahtaa. Varmennettu Unity CLI:n Play Modessa: isännöitiin oikeasti Relayn kautta, luotiin palvelin-omisteinen `RemotePlayerNet`-instanssi `eval`-kutsulla (koska toista konetta ei ole käytettävissä), ja kutsuttiin `NearestCombatantPosition`-metodia suoraan reflektiolla kahdesta eri pisteestä - piste lähempänä kaveria palautti kaverin sijainnin, piste lähempänä isäntää isännän sijainnin. Sivuhavaintoja testistä (ei koodimuutoksia): kaveri-instanssi kuoli oikeasti lähellä oleviin vihollisiin kontaktivahingosta testin aikana, mikä epäsuorasti vahvisti että `TickNetworkCoop`in kaverille kohdistuva vahinko toimii. Hahmonvalinta ei ole eikä ole ollut käytettävissä kaverille (`RemotePlayerNet` on aina kiinteä vihreä kapseli, ei asekaappia/leveleitä) - tämä on tarkoituksellinen vaiheen 1 rajaus, ei bugi, dokumentoitu jo aiemmassa moninpeli-merkinnässä. Ei buildia/julkaisua. Kahden oikean koneen testi on yhä tekemättä.

- 2026-09-08: Proseduraalinen huone-/käytävägeneraattori v1 oletuskartaksi (`ProceduralMapLayout.cs`, `SurvivorProceduralMaps.cs`). Jokaisessa kartassa 8–12 yhdistettyä huonetta, lisäsilmukoita, kaukainen bossihuone, arkut ja pyhäkkö; minimappi ja seedin syöttö/kopiointi. Bossin kuolema vaihtaa automaattisesti noin 2 s kuluttua uuteen seediin, siivoaa vanhan kartan resurssit/taisteluoliot ja siirtää pelaajan alkuhuoneeseen; build ja odottavat tasopäivitykset säilyvät. Aiempi 90 s bossiportaalisääntö ja karttanumeron vaikeusskaalaus säilyvät. Seed-historia, karttanumero ja generaattoriversio tallentuvat PlayerPrefsiin; samalla seedillä voi toistaa myös myöhemmän kartan geometrian. BFS-reitinhaku ja ruudukkopohjainen liike estävät vihollisia ja pelaajaa oikaisemasta seinien läpi. Moninpelille seed-/karttanumeroviesti liittymisessä ja siirtymässä; ei kahden koneen testiä. Unity CLI: 207 erilaista/toistettavaa/yhtenäistä karttaa, 222 onnistunutta reittiä, seinätestit ja kolme peräkkäistä bossisiirtymää läpäisty. Myös normaali Update-ajastin vaihtoi kartan 1 → 2 kesken odottavan tasovalinnan ilman uusia konsolivirheitä. Yhdessä käsin tehdyssä tarkistuksessa asetettiin vahingossa PendingChoices ilman vaihtoehtolistaa; tästä tulleet GUI-virheet olivat virheellisen testitilan seuraus, normaali GrantExperience-polku testattiin uudelleen virheittä. Aiemmat Netcode-tuhoamisen NRE:t säilyvät. Ohje: `Assets/Survivor/README-ProceduralMaps.md`; testit: `PrototypeTools/VerifyProceduralLayout.cs` ja `VerifyProceduralProgression.cs`. Ei julkaisua.

- 2026-09-08: Uusi Mosswood-metsäkartta nykyisen pyöreän areenan tilalle (`SurvivorForestMap.cs`). Tasainen, epäsymmetrinen 10-sivuinen alue, noin 195 × 159 yksikköä nykyisellä arenaRadius=60-asetuksella ja yli 1,8-kertainen pinta-ala. 88 puuta/lohkaretta, sammalläikkiä, kivinen reunus ja reunapuita; polut nykyisille arkuille, pyhäkölle ja portaalille. Kiinteä oma satunnaissiemen säilyttää saman geometrian hostilla ja asiakkaalla muuttamatta pelin Random-tilaa. Pelaaja, väistö, viholliset ja RemotePlayerNet käyttävät yhteisiä karttarajoja ja maastoesteiden ohitusta; vihollis- ja bossispawnit sijoitetaan vapaalle maalle. Unity CLI Play Mode -tarkistus (`PrototypeTools/VerifyForestMap.cs`): 2000 rajaprojektiota, kaikkien 88 esteen ohitus, 120 kulmaspawnia, kaikkien maamerkkien saavutettavuus ja bossin kutsuminen läpäisty. Pelinäkymä ja yleiskuva tarkistettu (`PrototypeTools/forest-game.png`, `forest-overview.png`). Konsolissa vain aiemmin dokumentoidut Netcode-tuhoamisen NullReferenceExceptionit, ei kartan käännös-/ajovirheitä. Kahden koneen moninpeliä ei testattu. Ei buildia tai julkaisua.

Lyhyt loki tehdyistä muutoksista. Uusin ylimpänä.

- 2026-09-08: Julkaistu v0.3.0 (edellinen: v0.2.2). Buildattu Unity CLI:llä (StandaloneWindows64, 0 virhettä, 6 varoitusta), zipattu ja liitetty GitHub-releaseen [v0.3.0](https://github.com/Marcebooy/SurvivorPrototype/releases/tag/v0.3.0). Kaikki edellisen releasen jälkeiset committamattomat muutokset (4 uutta hahmoa, moninpelin vaihe 1, kaikkien 30 aseen osumaefektit, ase-/Tome-slottikatot, Netcode-siivous) commitoitu ja pushattu ensin (commit `b935109`). Discord-ilmoitus lähetetty webhookilla käyttäjän vahvistuksen jälkeen.
- 2026-09-08: Iskuosumille lisätty asset-pakettien VFX-efektejä, joita projektissa ei vielä käytetty (aiemmin vain Lightning ja Aura käyttivät oikeita partikkeliefektejä, loput 28 asetta pelkkiä primitiivi-"Shape"-muotoja). Kuusi uutta `SurvivorGame`-kenttää (`slashEffect`, `fireImpactEffect`, `poisonImpactEffect`, `frostImpactEffect`, `voidImpactEffect`, `boltImpactEffect`) kytketty Unity CLI:llä Survivor Game -olioon: `FX_Orange_Slash_1` (Eric VFX Studio) terä-/lähitaisteluaseille (Sword, HeroSword, CorruptedSword, Scythe, Katana, Dexecutioner, Axe), `FX_Fireball` (Eric VFX Studio) räjähtäville/tuliaseille (Rocket, DragonBreath, Mines), `FX_Green_Hit` (Eric VFX Studio) Poison Flaskille, ja kolme uutta väriä Zap VFX URP -paketista jo käytössä olleen sinisen (Lightning) rinnalle: valkoinen Frostwalkerille, musta Black Holelle ja Space Noodlelle, violetti lopuille etä-/taika-aseille (Bow, Sniper, Revolver, Bananarang, Wireless Dagger, Aegis, Dice, Blood Magic, Tornado). Uusi `ImpactEffectFor`/`SpawnImpact`-apupari (`SurvivorAdvancedWeapons.cs`) kytketty jaettuihin `AdvancedPulse`/`AdvancedBeam`-funktioihin (kattaa suoraan 12 asetta kerralla), projektiiliosumiin (`TickAdvancedShots`), vyöhykkeiden syntyhetkeen (`AddAdvancedZone` - kertaluontoinen "cast"-purske, ei toistuvaa spämmiä tick-silmukassa) sekä erikseen Swordiin (`AttackWeapons`) ja Bow'n nuoliosumiin (`TickBolts` - Bow puuttui aiemmin osumaefektistä kokonaan). Koko skaalattu `Size`-statin mukana kuten Aura jo aiemmin. Play Modessa visuaalisesti todennettu (kaikki 30 asetta tasolle 2, viholliset ympärillä): `FX_Fireball` osoittautui autorisoitu ~9x liian suureksi tämän pelin mittakaavaan (natiivilla skaala 1:llä yksittäinen räjähdys peitti koko areenan keskustan, moninkertaisesti hahmoa suurempi) - kaikki muut paketit (viilto, myrkky, jää, tyhjiö, taikaosuma) näyttivät oikean kokoisilta jo skaalalla 1. Lisätty `ImpactBaseScale`-kerroin (0.15) vain tulipaketille korjaamaan tämä; muut käyttävät suoraan `Size`-statin mukaista skaalaa ilman erillistä korjausta, samaan tapaan kuin Lightningin Zap-efekti jo aiemmin. Ei uusia konsolivirheitä (ainoat esiintyneet olivat jo dokumentoidut Netcode `NetworkObject.OnDestroy`-NRE:t, toistuivat koska Play Mode käynnistettiin/pysäytettiin testissä useaan kertaan).
- 2026-09-08: Korjattu suurin osa Play Modea pysäytettäessä syntyneistä Netcode-konsolivirheistä (41 → 2). Syy: Unity tuhoaa scenen oliot Stopissa ilman taattua järjestystä, ja jos `NetworkManager` tuhoutuu ennen verkotettuja `NetworkObject`-olioita (viholliset, RemotePlayerNet), nämä yrittävät sen jälkeen viitata jo nollattuun `NetworkManager`-tilaan → `RemoveNetworkObjectFromSceneChangedUpdates`-NRE-vyöry. Lisätty `SurvivorNetwork.cs`iin `OnApplicationQuit`- ja Editor-kohtainen `EditorApplication.playModeStateChanged`-kytkentä (`ExitingPlayMode`), jotka kutsuvat `Disconnect()`/`NetworkManager.Shutdown()`ia siististi *ennen* kuin Unity ehtii aloittaa olioiden tuhoamisen. Testattu Unity CLI:llä Play Mode -käynnistyksellä/pysäytyksellä: 41 virhettä → 2. Jäljellä olevat kaksi `NullReferenceException`-riviä (`NetworkSceneManager.Dispose()`, `SceneEventDataStore` on `null` jos verkkopeliä ei koskaan käynnistetty) ovat Netcode for GameObjects -paketin oma bugi (puuttuva null-tarkistus `Library/PackageCache`-koodissa, ei korjattavissa projektista) - nämä olivat jo aiemminkin tiedossa (ks. alla oleva 2026-09-08-merkintä "Neljä uutta pelattavaa hahmoa"), käyttäjä päätti jättää ne ennalleen koska ne ovat harmittomia eivätkä riko peliä.
- 2026-09-08: Neljä uutta pelattavaa hahmoa lisätty: Golem, Metsästäjä, Ninja, Paladiini (yhteensä nyt 9 hahmoa). Jokainen samalla FBX-tuontikaavalla kuin Ritari/Berserkeri (`Assets/Survivor/Editor/<Nimi>ImportPostprocessor.cs`, Legacy-rig, URP-materiaalit Blenderin väriarvoista, malli+animaatiot kopioitu `Unity/<Nimi>/` → `Assets/Survivor/Characters/<Nimi>/Resources/`). Aloitusase-valinnat ja perustelut: **Golem** → Chunkers (malliin ei kuulu lähitaisteluasetta erikseen, ja mallin "kiertävät kivet" -shokkiaalto-VFX muistuttaa suoraan Chunkersia); uusi `ICharacterVisual.Shockwave()`-koukku laukeaa jokaisesta osumasta (`ReceiveDamage`), toistaen Stoneform_Shockwave-animaation - readmen oma ehdotus ja täsmää AGENTS.md:n passiiviin. **Metsästäjä** → Bow (readmessä vain yksi vaihtoehto); Bow-aseelle ei ollut aiemmin hahmokoukkua, lisätty `characterVisual.Swing(aim)` `AttackWeapons()`:n Bow-haaraan. **Ninja** → Katana eikä Wireless Dagger (Katanalle lisätty vastaava `Swing()`-kutsu `CastAdvanced`:n Katana/Dexecutioner/BloodMagic/Dice-ryhmään); Shadowstep-animaatio korvaa muiden hahmojen geneerisen "rullaa"-väistöasennon lukemalla jaettua `roll`-parametria `Tick()`:ssä, ei vaatinut uutta rajapintakoukkua. **Paladiini** → Aura eikä Aegis (sama päättely kuin Ritarin Sword > Aegis -valinnassa: Aegis yksinään ei tuota jatkuvaa vahinkoa), mutta Attack_CleanSlash/Block_Holy kytketty siltä varalta että Sword-perhe tai Aegis poimitaan myöhemmin; Divine_Beam jää käyttämättä (ei vastaavaa asetta). Hahmovalintaruutu muutettu yksiriviseltä ruudukoksi (5 saraketta, 2 riviä 9 hahmolle, `DrawCharacterSelect`), ja korttien kuvausteksti sai oman pienemmän fonttityylin (`cardTextStyle`) ja "Valitse"-napin korkeutta kasvatettiin ettei teksti leikkaudu. Testattu Unity CLI:llä Play Modessa: kaikki neljä valittavissa, aseet tekevät vahinkoa (Aura-kehä näkyvissä heti, Chunkers-kivet kiertävät), Ninjan Shadowstep ja Golemin Shockwave laukesivat virheittä suoralla reflektiokutsulla. Ainoat uudet konsolivirheet ovat Unity Netcode -paketin omia (NetworkManager/NetworkObject-tuhoamisen null-viittaukset, dokumentoitu jo yllä olevassa merkinnässä) - ei liity hahmoihin.
- 2026-09-08: Asepäivitysten saanti rajattu: pelaaja voi run-kohtaisesti omistaa enintään 5 eri asetta ja 5 eri Tomea (`MaxWeaponKinds`/`MaxTomeKinds`, `SurvivorProgression.cs`). Sekä tasopäivitysvalinnat (`RollChoices`) että arkkujen palkinnot (`Interact`, kind 0 - käytti aiemmin täysin suodattamatonta `Random.Range(0, UpgradeNames.Length)`ia, joten arkku saattoi jo aiemmin rikkoa Quantity/Crit-katot; nyt molemmat käyttävät samaa `BuildUpgradePool()`ia) noudattavat samaa rajaa: jo omistettuja aseita/Tomeja voi yhä tasottaa loputtomiin, mutta 6. eri lajia ei enää tarjota kun kumpikin katto on täynnä. Tomeille lisätty oma seuranta (`tomeLevels`, uusi `TomeCount`-ominaisuus `WeaponCount`-parin rinnalle) - aiemmin Tomet olivat pelkkiä suoraan sovellettavia statbonuksia ilman omistustietoa. HUD näyttää nyt "Aseet X/5 • Tomet X/5" (`DrawProgression`). Motivaatio: usealla hahmolla on nyt oma aloitusase (Ritari/Velho/Necromancer/Berserkeri), ja rajattu slottimäärä pakottaa build-valintoja sen sijaan että 90 sekunnin sisään saisi teoriassa kaikki 30 asetta. Testattu Unity CLI:n `eval`-kutsulla Play Modessa: reflektiolla ajettu 300 satunnaista `ApplyUpgrade`-kutsua `BuildUpgradePool()`in kautta Ritarilla - aseita ja Tomeja jäi tasan 5/5, kumpikaan ei ylittänyt kattoa. Ei uusia konsolivirheitä (kaksi olemassa olevaa Netcode `NetworkManager.OnDestroy`-poikkeusta play modesta poistuttaessa, ei liity tähän muutokseen - esiintyi jo ennen tätä muutosta moninpelipaketin lisäyksen myötä).

- 2026-09-08: Moninpelin vaihe 1 (kaverin kanssa pelaaminen Relayn kautta) toteutettu. Lisätty paketit `com.unity.netcode.gameobjects` (2.13.2), `com.unity.services.relay` (1.2.0), `com.unity.services.authentication` (3.7.4), `com.unity.services.core` (1.18.0) - projekti oli jo linkitetty Unity Cloud -projektiin (`cloudProjectId`), joten Relay toimi suoraan ilman dashboard-asetuksia. Uudet skriptit: `SurvivorNetwork.cs` (UGS-alustus, anonyymi kirjautuminen, Relay-allokaatio/liittymiskoodi, host/join), `RemotePlayerNet.cs` (kaverin hahmo - yksinkertainen vihreä kapseli, kiinteä lähitaistelu, ei omaa asekaappia/leveleitä vielä), `ClientAuthoritativeTransform.cs` (kaverin oma kone ohjaa liikettä, ei isäntäviivettä), `SurvivorGameNetwork.cs` (host-only kontaktivahinko/lähitaistelutikki kaverille, asiakaskoneen kamera/HUD). Isäntä pelaa täysin ennallaan (`SurvivorGame.Update`/`SpawnEnemy` gatetaan `NetworkManager.Singleton`-tilalla); liittyvä kaveri ei aja omaa simulaatiota, vaan renderöi isännän deterministisen areenan (`BuildWorld` ei käytä satunnaisuutta) ja isännän replikoimat viholliset (`NetworkObject`/`NetworkTransform` lisätty `UD_demo_character.prefab`iin). Päävalikkoon uusi "MONINPELI"-paneeli (`SurvivorGame.cs: DrawMultiplayerPanel`): "ISÄNNÖI KAVERILLE" / liittymiskoodikenttä / "LIITY KOODILLA". Testattu Unity CLI:llä Play Modessa `eval`-kutsuilla: `HostGame()` tuottaa oikean Relay-liittymiskoodin ja `NetworkManager.StartHost()` onnistuu virheittä (huomio: `AddNetworkPrefab`-kutsut piti poistaa, koska NGO rekisteröi molemmat verkko-objektit jo automaattisesti `Assets/DefaultNetworkPrefabs.asset`iin - kaksinkertainen rekisteröinti aiheutti "duplicate GlobalObjectIdHash"-virheitä). Toista pelaajaa (LIITY KOODILLA -polkua, kaverin näkökulmaa) ei ole vielä testattu oikeasti kahdella koneella - vaatii käyttäjän oman testin kaverin kanssa internetin yli. Ei mukana: kaverin oma ase-/level-/kauppajärjestelmä, hahmovalinta kaverille, täysi ammusten replikointi, yli 2 pelaajaa, launcher/release-integraatio. Suunnitelma: `C:\Users\marku\.claude\plans\squishy-coalescing-flame.md`.

- 2026-09-08: Berserkeri lisätty viidenneksi pelattavaksi hahmoksi (`BerserkerVisual.cs`, `Assets/Survivor/Editor/BerserkerImportPostprocessor.cs`, kolme FBX:ää `Unity/Berserker/` → `Assets/Survivor/Characters/Berserker/Resources/`, sama Ritari-tyylinen kolmen-FBX-yhden-rigin yhdistely). Aloitusaseeksi valittu Corrupted Sword eikä Axe, vaikka malli kantaa kahta kirvestä: Axe on erillinen heittoprojektiili joka ei laukaisisi hahmon omaa `Attack_Double`-animaatiota, kun taas Corrupted Sword käyttää samaa lähitaistelu-Swing()-koukkua kuin Ritarilla ja sen vahinkokaava (`1+2*(1-HP/maxHP)`) vastaa suoraan mallin sisäänrakennettua Rage-järjestelmää. Lisätty uusi HP-pohjainen animaatiotilan vaihto: kun terveys alle 30 %, kävely vaihtuu paikallaan-pysyvään Rage_LowHP-hengitys-/kipinäsilmukkaan (README:n ehdottama "rage = 1 - HP/maxHP" -kytkentä, toteutettu tilanvaihtona koska Legacy-animaatio ei tue jatkuvaa blendausta). Tätä varten `ICharacterVisual.Tick` sai uuden `healthFraction`-parametrin - päivitetty kaikkiin viiteen hahmovisuaaliin, vaikka vain Berserkeri käyttää sitä toistaiseksi. Testattu Unity CLI:llä Play Modessa: viiden hahmon valintaruutu, Corrupted Sword tekee jatkuvaa vahinkoa, Rage-asento laukeaa virheittä kun HP pakotettiin alle kynnyksen. Ei uusia konsolivirheitä.
- 2026-09-08: Necromancer lisätty neljänneksi pelattavaksi hahmoksi (`NecromancerVisual.cs`, `Assets/Survivor/Editor/NecromancerImportPostprocessor.cs`, malli kopioitu `Unity/Necromancer/Necromancer_Glide.fbx` → `Assets/Survivor/Characters/Necromancer/Resources/`). Sama rakenne kuin Velholla: yksi FBX, yksi looppaava Glide-animaatio, Legacy-rig, ei lähitaisteluanimaatiota. Aloitusaseeksi valittu Bone (Pomppuluu) eikä Scythe - malli kantaa pelkkää luusauvaa/kelluvaa kalloa ilman minkäänlaista lähitaisteluasetta, ja Bone (etähyökkäys, kimpoaa vihollisten välillä) sopii sekä visuaaliseen että AGENTS.md:n "kutsuihin ja tappojen ketjuttamiseen" -kuvaukseen paremmin. Hahmovalintaruutu muutettu 2 kortin kiinteästä layoutista dataan perustuvaksi `CharacterRoster`-taulukoksi (`SurvivorPottu.cs: DrawCharacterSelect, DrawCharacterCard`), joten uusien hahmojen lisääminen jatkossa on yksi taulukkorivi. HUD:n hahmonimi-otsikko sai oman pienemmän fonttityylin (`hudNameStyle`), koska "NECROMANCER / LUU" ei mahtunut aiempaan otsikkokokoon. Testattu Unity CLI:llä Play Modessa neljän hahmon valintaruutu ja Necromancerin pelillinen toiminta; ei uusia konsolivirheitä.
- 2026-09-08: Ritari lisätty kolmanneksi pelattavaksi hahmoksi Potun ja Velhon rinnalle (`SurvivorPottu.cs: DrawCharacterSelect, SelectCharacter, BuildCharacterVisual` - hahmovalintaruutu laajennettu kolmeen korttiin). Aloitusaseeksi valittu Sword eikä Aegis, vaikka hahmon malliin kuuluu kiinteästi sekä miekka että kilpi: Aegis yksinään laukeaa vain pelaajan ottaessa osuman eikä tuota jatkuvaa vahinkoa, joten se sopii huonosti aloitusaseeksi tässä pelissä - Aegis jää luontevaksi myöhemmäksi poiminnaksi. Ritarin visuaali (`RitariVisual.cs`) on ainoa toistaiseksi jolla on oikeat hyökkäys- ja torjunta-animaatiot (Attack_Slash Swordin iskuille, Block Aegiksen torjunnalle - uusi `ICharacterVisual.Block()`-koukku kutsutaan `BlockWithAegis()`:sta): hahmon kolme FBX:ää (`Unity/Knight/Ritari_Walk_Heavy.fbx`, `_Attack_Slash.fbx`, `_Block.fbx`, kopioitu `Assets/Survivor/Characters/Ritari/Resources/`) jakavat saman 12-luun rigin, ja vain kävelymallin mesh näytetään - muiden kahden animaatioklipit poimitaan ajonaikaisesti samalle Animation-komponentille. Uusi editor-tuontiskripti `Assets/Survivor/Editor/RitariImportPostprocessor.cs` asettaa Legacy-riggauksen, kävelyn loopin (attack/block eivät looppaa) ja URP-materiaalit Blenderin väriarvoista. Testattu Unity CLI:llä Play Modessa: hahmovalinta, jatkuva miekkavahinko heti alusta (ehti tasolle 2 sekunneissa), Block-koukku laukaistu virheittä. Ei uusia konsolivirheitä.
- 2026-09-08: Velho lisätty toiseksi pelattavaksi hahmoksi Potun rinnalle. Uusi hahmonvalintaruutu ennen aloitusaseen/asekaapin valintaa (`SurvivorPottu.cs: DrawCharacterSelect, SelectCharacter, BuildCharacterVisual`); Pottu toimii kuten ennen (asekaappi, 30 asetta), Velho saa aina suoraan Salamasauvan (Lightning - ketjuttuva sähköisku) eikä näe asekaappia. Velhon visuaali on oikea Blenderissä tehty malli (`Unity/Wizard/Velho_Walk.fbx`, kopioitu `Assets/Survivor/Characters/Velho/Resources/`), ei proseduraalinen kuten Pottu. Uusi `ICharacterVisual`-rajapinta yhdistää Potun ja Velhon liikkumis-/hyökkäyskoodin (`MovePottu`, `AttackWeapons`, `CastAdvanced`) hahmoriippumattomaksi. Uusi editor-tuontiskripti (`Assets/Survivor/Editor/VelhoImportPostprocessor.cs`) asettaa FBX:n Legacy-riggauksen, animaatiolooppauksen ja URP-materiaalit automaattisesti Blenderin alkuperäisistä väriarvoista - ei vaadi käsisäätöä Unity Editorissa. Testattu Unity CLI:llä Play Modessa (molemmat hahmot, konsoli puhdas).
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
