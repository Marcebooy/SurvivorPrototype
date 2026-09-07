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

## Viestintä

- Vastaa käyttäjälle suomeksi, ellei hän pyydä muuta.
- Kerro lyhyesti, mitä muutettiin ja miten muutos varmennettiin.
- Älä väitä tehtävää valmiiksi ilman tarkistusta.

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
