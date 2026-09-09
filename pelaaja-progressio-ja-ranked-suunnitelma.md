# MegaBonk/SurvivorPrototype — Pelaajataso, Ranked, Leaderboard & Mappi-tila -suunnitelma

Pohjautuu `AGENTS.md`-tiedostoon (Unity-projekti `My project`, GitHub: Marcebooy/SurvivorPrototype). Tämä on ideointi-/suunnitteludokumentti. Kohta 0.5 seuraa mitä on jo oikeasti toteutettu ja testattu koodissa (ei vain suunniteltu).

## 0.5. Toteutustilanne (päivitetty 2026-09-09, seuraa AGENTS.md:n Muutoslokia)

Mappi-tilan pelattava versio on nyt kokonaisuudessaan valmis alkuperäisen vision mukaisesti: omistuspohjainen loadout, kauppa, ase-variantit, variantin laatutaso, keskitetty hahmoprofiili ja ase-affiksien craftausjärjestelmä (nyt 13 aseella). Kaikki testattu Unity CLI:llä Play Modessa **ja committoitu gitiin** (viimeisin commit `1669638` + Muutoslokin päivitys `146c923`). Ei vielä julkaistu pelaajille.

**Valmiina:**
- Account Level + per-hahmo Mastery (1–100, ei pelimekaanista bonusta) — data + UI (Mastery-badge hahmokorteissa, Account Lv. päävalikossa).
- Päävalikon välilehtinavigaatio (`MenuScreen`-enum: KOTI, HAHMOT, MONINPELI, MAPIT, TULOSTAULUT*, ASETUKSET — *placeholder).
- Kaksi kiinteää Map Typea: Mosswood (metsä), Luuluola (harmaa/luuvalkoinen, hautakivet).
- Mappi-inventaario: karttaesineet droppaavat bosseilta (18 % mahdollisuus), omistetaan `(MapType,Tier)`-pareina, kulutetaan pelatessa. Proseduraalinen kartta pysyy aina ilmaisena/rajattomana oletuksena.
- Tier 1–3 per Map Type, painotettu droppi (60/30/10 %), kertoimet vihollisten HP:hen/kattoon (1.0×/1.35×/1.7×) olemassa olevan per-alue-skaalauksen päälle. Nopeutta ei skaalata.
- MAPIT-tab näyttää kaikki (Map Type × Tier) -yhdistelmät omistettuine määrineen, ja sisältää oman PELAA-napin.
- **Per-hahmo ase-/taito-omistus + kauppa (`CharacterShop.cs`)**: jokaisella hahmolla oma `ownedUpgrades`-joukko (aseet+Tomet), hahmon kiinteä aloitusase aina omistettuna oletuksena. Hinta kasvaa omistettujen määrän mukaan (25 + omistettuja×20), maksetaan Silverillä.
- **HAHMOT-tab — keskitetty hahmoprofiili**: hahmovalitsin, Mastery/Silver, koko 30 aseen + 9 Tomen ostoruudukko, **karttavarastoyhteenveto** (`MapInventorySummary()`, näyttää vain nollasta poikkeavat parit, esim. "Mosswood T1×3, Luuluola T2×1"), ja **muokattava oletusloadout** (`defaultLoadout`, sama tallennusmalli kuin `ownedUpgrades`illa) mukaan lukien variantti-tähdet, affiksit ja Paranna-nappi samalla UI:lla kuin run-kohtaisessa loadout-ruudussa. Koko tab scrollattava (`characterTabScroll`) koska sisältö ei enää mahtunut yhdelle ruudulle.
- **Mappi-tilan loadout-ruutu esitäyttyy oletusloadoutista**: `ResetProgression()` lukee `defaultLoadout`in kun Mappi-tila valittu, ohittaen ei-enää-omistetut id:t turvaverkolla. Esitäyttö on vain lähtökohta — run-kohtainen muokkaus (`ConfirmLoadout`/`SkipLoadout`) ei koskaan kirjoita takaisin pysyvään oletukseen; tämä on erikseen testattu ja vahvistettu.
- **Ase-variantit — pilotti kolmelle aseelle (`WeaponVariants.cs`)**: Sword→Kaksoisterä, Bow→Kimmokaari, Chunkers→Iskukivet. Sama ase, vaihtoehtoinen käytös, ei vie loadout-paikkaa. Avataan pysyvästi pelaamalla (10 % droppi bossinkaatohaarasta), vain omistetuille aseille, vain Mappi-tilassa.
- **Variantin laatutaso (1–3 tähteä)**: globaali `variantQuality` + `qualityMaterial`-pinovaluutta. Oma bossinkaatorulla `TryDropQualityMaterial()` (15 %), droppaa vain jos jollain avatulla variantilla on vielä tilaa parantua. Maltillinen bonus (+10 %/+20 % per tähti, max +20 %).
- **Ase-affiksien craftausjärjestelmä (`WeaponAffixes.cs`)** — nyt 13 aseella: alkuperäinen 7 aseen pilotti (Sword, Flamewalker, Lightning, Firestaff, Chunkers, Bone, Bow) + laajennettu 6 avattavalle aseelle (Revolver, Axe, Katana, Shotgun, Frostwalker, Black Hole). Max 2 affiksislottia per ase, +15–35 % arvot, affiksipooli suoraan dokumentoiduista statshyötylistoista. Kaksi omaa droppimateriaalia ("Lisää affiksi", "Täysi reroll"), kumpikin ~11–12 % bossinkaatorulla, vain Mappi-tilassa, vain omistetulle aseelle. Taistelukytkentä ketjutettu jokaisen aseen omaan kaavaan (ei yhtä keskitettyä stat-putkea) — pilotin 7 aseesta kaksi arkkitehtuurikompromissia (Sword/Lightning jaettu ajastin; Bow:n Crit paikallinen vahinkokerroin), laajennuksen 6 aseesta viidellä oma `advancedTimers[i]`-ajastin (ei kompromissia) ja Katanalla sama paikallinen-crit-ratkaisu kuin Bow:lla. Testattu kattavasti kummallakin kierroksella (droppijakauma, slottien täyttyminen/hylkäys, taistelukytkentä suoralla laskennalla, 0-vaikutus Selviytymistilassa) — committoitu (`44ee574`+`1fc7f1b` pilotti, `1669638`+`146c923` laajennus).
- Selviytymistila (proseduraalikartta) on kaiken tämän ajan pysynyt täysin muuttumattomana — kaikki yllä oleva vaikuttaa vain Mappi-tilassa (`activeMapType != Procedural`).

**Tunnettu avoin puute:** moninpelin karttasynkronointi (`SurvivorGameNetwork.cs`) ei tunne `MapType`/Tieriä — jos isäntä pelaa kiinteällä kartalla, kaverin kartta ei todennäköisesti synkkaa oikein. Tietoisesti jätetty korjaamatta, priorisoitu myöhemmäksi (päätös 2026-09-09).

**Ei vielä tehty**: yhtenäinen WeaponStats-pipeline (ks. kohta 10.5, aloitettu Codella 2026-09-09, vaiheet 1–3 valmiit), aseiden visuaalinen arkkityyppipohja-työnjako Astralle (ks. kohta 10.6, aloitettu), fragmenttisysteemi (variantit lopuille 27 aseelle), affiksijärjestelmän laajennus lopuille 17 aseelle (13/30 tehty), sustain-resurssi/"Map Shard" pysyvälle Tier-avaukselle ilman RNG-droppia, leaderboardit, ranked, achievements, moninpelin karttasynkronointikorjaus, uudet Map Typet (Tuhkaerämaa/Jäätikkö/Suo), hideout (kohta 10.4), Crit-mekaniikan yleistäminen globaaliksi (ks. kohta 10.5, tietoisesti erotettu WeaponStats-refaktoroinnista).

## 0. Pelirakenne: kaksi erillistä tilaa (PÄÄTÖS)

Peliin tulee kaksi selkeästi erillistä pelimuotoa, ei yhtä sekoitusta:

1. **Selviytymistila (nykyinen peli, säilyy sellaisenaan)** — nykyinen MegaBonk-tyylinen looppi: valitse hahmo, aloita yhdellä aseella, satunnaiset tasopäivitykset kesken runin, proseduraalinen/kiinteä kartta, selviydy mahdollisimman pitkään. Tämä on ja pysyy **build-hiekkalaatikkona**. Ei loadout-rajoituksia, ei kiinteitä karttoja — pysyy sellaisena kuin nyt. Iskut toimivat automaattisesti (MegaBonk-tyylinen auto-taistelu) — tämä pysyy koko pelin ytimenä myös Mappi-tilassa, statscraftaus ei muuta sitä perusperiaatetta.
2. **Mappi-tila (endgame, Path of Exile -henkinen)** — pelaaja tuo mukanaan **kiinteän loadoutin** (aseita/Tomeja joita on ostanut Silverillä ja asettanut oletukseksi HAHMOT-tabissa, avattuja variantteja, niiden laatutasoja ja craftattuja affikseja), valitsee Map Typen ja Tierin, ja pelaa sillä. Tästä tulee ranked-/leaderboard-/kokoelmavetoinen kokonaisuus.

**Miksi tämä jako on hyvä ratkaisu:**
- Selviytymistilan satunnaisuuskoukku on jo koko genren perusasia, mutta se on myös kulunut. Mappi-tilan kokoelma-/theorycrafting-looppi (osta ase → avaa variantti → paranna laatua → craftaa affikseja → aseta oletusloadout HAHMOT-tabissa → testaa kovemmalla Tierillä) koukuttaa toisella tavalla: se motivoi pelaamaan seuraavan session.
- Uudet/rennot pelaajat voivat pysyä Selviytymistilassa eikä heidän tarvitse ymmärtää Tier-/loadout-/kauppasysteemiä ollenkaan.
- Mappi-tilan tulokset ovat reilusti vertailukelpoisia leaderboardilla, koska loadout on kiinteä ennen runia.

**Markkinatilanne (huomioitu 2026-09-09):** genre on erittäin täynnä — suoraan tähän yhdistelmään (bullet heaven -auto-taistelu + PoE-henkinen craftaus/endgame) on jo olemassa suora kilpailija, **Astroloot** (Steam Early Access, hyvät arvostelut, "PoE ilman turhaa puuduttavuutta"). Pelkkä yhdistelmä ei siis riitä erottautumiseksi — erottautumisen on tultava konkreettisesta laajuudesta/syvyydestä (30+ asetta, hahmokohtainen kauppa, useita Map Typeja, moninpeli) tai muusta ominaisuudesta jota kilpailijat eivät tee. Tämä kannattaa pitää mielessä jokaisessa isossa suunnittelupäätöksessä eteenpäin: kysy "onko tämä jotain mitä Astroloot/kilpailijat eivät jo tee hyvin?"

## 1. Nykytila (mitä jo on pohjapelissä, ennen Mappi-tilaa)

- Roguelike bullet-heaven, 20 hahmoa (koonnissa kuvattu; pelissä toistaiseksi 9 toteutettu), 30 asetta, esineitä, Tomeja.
- **Silver-valuutta**: kerätään pelikerroittain, käytetään kuoleman jälkeen pysyviin avauksiin/parannuksiin sekä HAHMOT-tabin ase-/Tome-ostoihin.
- Moninpeli (vaihe 2, Relay/NGO): isäntä + yksi kaveri, kaverilla oma hahmo/asevalinta/leveli.
- Julkaisu GitHub Releases + itsepäivittyvä launcher + Discord-webhook-ilmoitukset.

## 2. Pelaajataso / Account-level progressio — TOTEUTETTU

Ks. kohta 0.5. Tallennus `PlayerPrefs` samalla mallilla kuin Silver. Palkinnot (skin-tint, badge, taunt-ele, Discord-maininta Mastery 100:sta) eivät vielä ole toteutettuja — vain data+perus-UI on valmis.

## 3. Mappi-tila: Map Type + Tier — TOTEUTETTU

Ks. kohta 0.5. Alkuperäinen suunnitelma sisälsi 4–6 Map Typeä — toteutettu toistaiseksi kaksi, loput voi lisätä samalla mallilla myöhemmin. Sustain-resurssi ("Map Shard") EI ole toteutettu.

## 4. Loadout-systeemi Mappi-tilaan — TOTEUTETTU KOKONAAN (omistuspohjainen + variantit + laatutaso + affiksit + hahmoprofiili)

Kaikki osat toteutettu ja committoitu: `CharacterShop.cs`, HAHMOT-tab keskitettynä hahmoprofiilina, oletusloadout, ase-variantit, variantin laatutaso, ase-affiksit (13/30 aseella). Ks. kohta 0.5 tekniset yksityiskohdat.

## 5. Drop-/kokoelmasysteemi — OSITTAIN TEHTY

**A) Etenemispolttoaine** — TOTEUTETTU kartoille (droppaus+inventaario+kulutus, Tier-painotettu). Ei Map Shard -sustain-resurssia.

**A2) Hahmokohtainen ase-/Tome-kauppa** — TOTEUTETTU.

**B) Build-variaatio ja identiteetti** — TOTEUTETTU pilottina (Sword/Bow/Chunkers). **Ei vielä tehty**: variantit lopuille 27 aseelle, fragmenttisysteemi, vaihtoehtoiset aloitusaseet, Map Fragmentit uusille Map Typeille, Tier-sidonnainen kosmetiikka.

**C) Kertakäyttöiset boostit ("Vahvistussinetti")** — EI VIELÄ TEHTY.

**D) Kartoilta droppaava upgrade-materiaali variantin laatutasolle** — TOTEUTETTU.

**E) Ase-affiksien craftausjärjestelmä** — TOTEUTETTU 13 aseella (7 pilotti + 6 laajennus), committoitu (`44ee574`/`1fc7f1b`, `1669638`/`146c923`). Ks. kohta 10.3. **Ei vielä tehty**: laajennus lopuille 17 aseelle — suositellaan tehtäväksi vasta kohdan 10.5 WeaponStats-pipelinen jälkeen.

**Miksi tämä koukuttaa paremmin kuin pelkkä nykyinen tyyli**: Kokoelma-/theorycrafting-looppi koukuttaa pelikertojen *välissä* — tämä on nyt kokonaisuudessaan todistettu toimivaksi kartoilla, hahmokaupalla, varianteilla, laatutasolla, affiksicraftauksella ja hahmoprofiililla.

## 6. Ranked-systeemi — EI VIELÄ TEHTY

- Mappi-tila on ensisijainen ranked-alusta — perusta on nyt olemassa koodissa, mutta itse leaderboard/pisteytys puuttuu.
- **Suositus**: hoida ensin moninpelin karttasynkronointibugi (kohta 0.5) ennen kuin ranked/leaderboardit rakennetaan päälle.

## 7. Leaderboards — tekninen toteutus — EI VIELÄ TEHTY

- Unity Gaming Services on jo käytössä (Relay, Authentication, Core) → Unity Cloud Leaderboards luontevin jatko.
- TULOSTAULUT-tab on jo olemassa päävalikossa placeholderina.

## 8. Muita ideoita jatkokehitykseen — EI VIELÄ TEHTY

- Achievements/Saavutukset, run-historia/tilastonäkymä, ghost/replay, kausittainen kosmetiikkarata, co-op-oma leaderboard.
- Armor/varusteslotti kokonaan uutena varustekategoriana — **kohdistuu suoraan kohdan 10.5 WeaponStats-pipeline-ehdotukseen**: kun aseiden statit kulkevat yhden yhteisen rakenteen kautta, armor on luontevasti sama rakenne toisella `applicableStats`-suodattimella (esim. Armor/Resistance/HP/MoveSpeed). Oma isompi suunnittelukeskustelu myöhemmin, jos affiksicraftaus toimii hyvin aseilla.
- "Hideout" — pelaajan oma kävelytettävä tukikohta. Ks. kohta 10.4 täydelliselle kuvaukselle. Markus pitää tätä itse erittäin hyvänä lisänä joka toisi peliin lisää syvyyttä; suositus on silti rakentaa se vasta nykyisen ydinloopin (loput variantit/fragmentit, affiksien laajennus, kartat) valmistuttua.

## 9. Toteutusjärjestys — päivitetty tilanne

1. ~~Account Level + per-hahmo Mastery~~ — TEHTY, committoitu (2026-09-09)
2. ~~Map Type -pohja~~ — TEHTY, 2/4-6 karttaa (2026-09-09)
3. ~~Tier-skaalaus~~ — TEHTY, inventaariopohjainen drop-systeemi (2026-09-09)
4. ~~Per-hahmo ase-/taito-omistus + kauppa + HAHMOT-tab + omistuspohjainen loadout~~ — TEHTY, committoitu (2026-09-09)
5. ~~Ase-/Tome-variantit (Sword/Bow/Chunkers-pilotti)~~ — TEHTY, committoitu (2026-09-09)
6. ~~Variantin laatutaso (upgrade-materiaali)~~ — TEHTY, committoitu, commit `b3e23ff` (2026-09-09)
7. ~~HAHMOT-tabin laajennus keskitetyksi hahmoprofiiliksi + oletusloadout~~ — TEHTY, committoitu, commit `182b547` (2026-09-09)
8. ~~Ase-affiksien craftausjärjestelmä (7 aseen pilotti)~~ — TEHTY, committoitu, commit `44ee574` + Muutosloki `1fc7f1b` (2026-09-09)
9. ~~Ase-affiksien laajennus 6 lisäaseelle (Revolver/Axe/Katana/Shotgun/Frostwalker/Black Hole, yht. 13/30)~~ — TEHTY, committoitu, commit `1669638` + Muutosloki `146c923` (2026-09-09)
10. **KÄYNNISSÄ: Yhtenäinen WeaponStats-pipeline (ks. kohta 10.5)** — ehdotettu ja hyväksytty 2026-09-09. Vaihe 1 (kartoitus, 8 statin lista) ja vaihe 2 (`WeaponStats`-struct + arkkityyppi/`applicableStats`-taulukko kaikille 30 aseelle) valmiit. Vaihe 3 (`GetWeaponStats()`-laskentafunktio) valmis 2026-09-09 — testattu Play Modessa, käännös 0 virhettä. Tässä vaiheessa vahvistettiin tietoisesti: Crit pysyy paikallisena vahinkokertoimena (ei muuteta globaaliksi crit-mahdollisuudeksi tämän refaktoroinnin osana, ks. alempi "Crit yleiseksi" -kohta). Seuraavaksi vaihe 4: yhtenäinen ajastintaulukko.
11. Ase-variantit laajemmin (loput 27 asetta) tai fragmenttisysteemi niiden avaamiseen; affiksijärjestelmän laajennus lopuille 17 aseelle — nyt WeaponStats-pipelinen päälle, ei enää bespoke-koodina per ase.
12. Moninpelin karttasynkronointikorjaus (Map Type/Tier) — ennen ranked/leaderboardeja
13. Unity Cloud Leaderboards
14. Achievements, kausisysteemi, ghost-replay, daily-haasteet, lisää Map Typeja (Tuhkaerämaa/Jäätikkö/Suo), Map Shard -sustain-resurssi
15. **"Hideout" — kävelytettävä 3D-tukikohta (ks. kohta 10.4)** — ehdotettu 2026-09-09, tarkoituksella myöhäisessä vaiheessa: rakennetaan vasta kun kohta 11 (loput variantit/fragmentit/affiksit) on täysin valmis, koska hideout on iso 3D/taidetyö esityskerroksena olemassa olevien järjestelmien päällä, ei uusi mekaniikka

Rinnakkainen visuaalinen työ (ei kytköksissä yllä olevaan järjestykseen, ks. myös kohta 10.6): Astra aloitti kiinnityspisteiden pilotin (kattaa kaikki 9 pelattavaa hahmoa, 36 kiinnityspistettä) ja ensimmäiset pohjaperheet (Katana/Corrupted Sword-teräpohja, sauvapohja, neljän ampuma-aseen pohja, kelluvien esineiden kitti, jousi, raskas varsiase) 2026-09-09 — ks. kohta 10.6. Astra tekee myös 3D-hahmomalleja (9/20 valmis, seuraavana Pyromancer) ja uusia karttoja (Tuhkaerämaa ehdotettu) puuttuvien Map Typejen pohjaksi. Ensimmäinen "flagship"-ase (Kaksoiskajo, Sword/Kaksoisterä-teemainen) on tuotu peliin ja toimii visuaalisesti (`SwordVariantVisual.cs`), ei vielä committoitu — pientä hienosäätöä kesken. Toinen näyttävä ase (pääkallo/viikate-teemainen velhon sauva) on konseptivaiheessa Astralla.

## 10. Ehdotuksia harkinnassa

**10.3. Ase-affiksien craftausjärjestelmä — TOTEUTETTU, 13/30 aseella (ehdotettu ja hyväksytty 2026-09-09, committoitu 2026-09-09 kahdessa erässä).**

Idea: PoE-henkinen mutta yksinkertaistettu craftausvaluuttajärjestelmä aseiden statseille (esim. "+30% damage"). Rajattu tietoisesti pelkkiin aseisiin — armor/varusteslotteja ei ole olemassa pelissä, joten niiden lisääminen on oma erillinen isompi päätös myöhemmäksi (ks. kohta 8).

Toteutettu rakenne:
- Uusi `WeaponAffixes.cs`, sama malli kuin `WeaponVariants.cs`/`CharacterShop.cs`.
- Affiksit per (hahmo, ase), max 2 slottia per ase.
- **Erä 1 — pilotti (7 asetta)**: Sword, Flamewalker, Lightning, Firestaff, Chunkers, Bone, Bow — asetta joilla oli valmiiksi dokumentoitu statshyötylista.
- **Erä 2 — laajennus (6 asetta)**: Revolver (Damage/Bounces/Quantity/Cooldown), Axe (Damage/Size/Quantity/Cooldown), Katana (Damage/Cooldown/Crit/Quantity), Shotgun (Damage/Quantity/Size/Cooldown), Frostwalker (Damage/Duration/Size/Cooldown), Black Hole (Damage/Size/Duration/Cooldown) — statshyötylistat kirjattu ensin AGENTS.md:ään samaan tyyliin kuin aloitusaseilla, sitten `AffixPool`-merkinnät koodiin.
- Affiksipooli per ase suoraan AGENTS.md:n statslistoista.
- Arvot +15–35 %.
- Kaksi craftausvaluuttaa: "Lisää affiksi" (jos slotteja vapaana) ja "Täysi reroll" (poistaa kaikki, rullaa uudet). Yksittäisen arvon rerollaus (kuten PoE:n Divine Orb) jätetty tietoisesti myöhemmäksi.
- Oma droppirulla kartoilta, ~11–12 % kummallekin materiaalille, riippumaton nykyisistä (18 %/15 %/10 %).
- Rajaukset: vain omistetuille aseille, vain Mappi-tilassa, Selviytymistila koskematon (varmennettu 0 dropeista, ja `AffixMultiplier`=1 aina Selviytymistilassa taistelussa).
- Arkkitehtuurikompromissit: pilotin 7 aseesta kaksi — Sword/Lightning jakavat `SurvivorGame`:n yhteisen `attackTimer`in (Cooldown-affiksi nopeuttaa jaettua sykettä, sama periaate kuin Cooldown Tome), Bow:n Crit-affiksi toimii paikallisena vahinkokertoimena `Bolt.power`:iin. Laajennuksen 6 aseesta viidellä oma `advancedTimers[i]`-ajastin (`SurvivorAdvancedWeapons.cs`, ei kompromissia), Shotgun kulki jo yleistetyn `CastArsenal()`/`TickArsenal()`-koodin läpi (ei vaatinut uutta taistelukoodia ollenkaan), Katanan Crit-affiksi sama paikallinen-kerroin-ratkaisu kuin Bow:lla.

**Testaus (Unity CLI, Play Mode), molemmat erät**: droppijakauma 4000 kutsulla molemmille materiaaleille kummassakin erässä (~11–12 %, riippumattomia), 0 dropeista Selviytymistilassa, slottien täyttyminen/kolmannen affiksin hylkäys ilman materiaalihukkaa, "Täysi reroll" tyhjentää+täyttää tasan 2 uutta arvoa, ja taistelukytkentä varmennettu suoralla laskennalla kummassakin erässä (Sword: pakotettu +20 % Damage, mitattu vahinko täsmäsi 21,6×0,65=14,04; Axe: pakotettu +25 % Damage, mitattu `AdvancedShot.power`=33,75 täsmäsi 18×1,25×1,5) — molemmissa Selviytymistila antoi tasan saman tuloksen kuin ilman affiksia. Laajennuserässä ei yhtään konsolivirhettä (parempi tulos kuin pilotissa).

**Tila**: **Committoitu kahdessa erässä** — pilotti `44ee574`+`1fc7f1b`, laajennus `1669638`+`146c923`, molemmat 2026-09-09. Työhakemistossa oli molemmilla kerroilla samaan aikaan toisen, tähän liittymättömän session WIP (VFX-poolisysteemi, Necromancer-bossi, Tuhkaerämaa-taidepaketti) samoissa jaetuissa tiedostoissa — Code erotti muutokset täsmällisesti diffillä varmentaen kummallakin kerralla, committasi vain affiksit, ja palautti toisen session WIP:n koskemattomana takaisin.

**10.4. "Hideout" — pelaajan oma tukikohta (ehdotettu 2026-09-09, Markuksen mielestä erittäin hyvä lisä).**

Idea: PoE-henkinen kävelytettävä 3D-tukikohta, josta pääsisi kaikkeen Mappi-tilan sisältöön yhdestä paikasta kartta-/valikkonavigoinnin sijaan. **Markus piti ideaa itse erittäin hyvänä lisänä, joka toisi peliin lisää syvyyttä** — tämä ei ole vain "ehkä joskus"-muistiinpano, vaan hänen oma näkemyksensä siitä että tämä kannattaa lopulta tehdä.

Sisältäisi ainakin:
- **Karttalaite** — vastine MAPIT-tabille, mistä valitaan Map Type + Tier ja aloitetaan run.
- **Stash** — vastine HAHMOT-tabin ostoruudukolle/omistukselle, visuaalisena varastona.
- **Craftausasema** — affiksi-UI (kohta 10.3) fyysisenä pisteenä tukikohdassa.
- **Trofeehuone** — Account Level/Mastery-palkinnot ja saavutukset (kohta 8) näkyvästi esillä.
- **Tulostaulu-seinä** — kun leaderboardit (kohta 7) toteutuvat.
- Mahdollisesti co-op-vierailu toisen pelaajan tukikohtaan myöhemmin.

Tekninen huomio: suurin osa toiminnallisuudesta on jo olemassa OnGUI-välilehtinä (HAHMOT/MAPIT/affiksi-UI) — hideout olisi siis pääosin **esityskerros** (kävelytettävä 3D-tila samojen järjestelmien päällä), ei uusi pelimekaniikka. Tämä tekee siitä turvallisen lisän: ei riskiä rikkoa olemassa olevaa dataa/logiikkaa.

Keskusteltiin myös vaihtoehdosta rakentaa hideout heti nyt niin, että dropit kerätään fyysisesti inventaarioon, viedään stashiin ja mappifragmentit vietäisiin karttalaitteelle ennen runin aloitusta (diegeettinen versio nykyisistä tabeista). Suositus oli silti odottaa: se tarkoittaisi koko drop-/inventaariologiikan naamioimista fyysiseksi kertaalleen nyt, mikä jouduttaisiin tekemään osittain uudelleen kun uudet järjestelmät (kuten affiksicraftaus, joka juuri laajeni) pitäisi liittää samaan flow'hun, ja se veisi Astran/Coden kapasiteettia juuri kesken olevalta ydintyöltä.

**Suositeltu ajoitus**: ei rakenneta vielä. Rakennetaan vasta kun nykyinen ydinlooppi (loput variantit/fragmenttisysteemi, affiksien laajennus, mahdolliset uudet Map Typet) on kokonaan valmis ja pelattu kuntoon — hideout on iso 3D/taidetyö (ympäristö, mallit, kamera/liikkuminen) joka kannattaa tehdä kerralla kunnolla sen sijaan että se veisi Astran/Coden kapasiteettia ydinsysteemeiltä kesken. Kun ydinlooppi on valmis, hideout on luonteva ja matalariskinen "viimeistelyprojekti" joka sitoo koko Mappi-tilan yhteen paikkaan ja voi silloin sisältää myös fyysisen drop→stash→karttalaite-loopin.

**10.5. Yhtenäinen WeaponStats-pipeline — EHDOTETTU 2026-09-09, KÄYNNISSÄ (Markuksen hyväksymä suunta).**

**Ongelma**: Markuksen tavoite on "monta monta monta" equipattavaa asetta hahmoille, MegaBonk-tyylisellä auto-taistelulla mutta PoE-tyylisellä stats-craftauksella (myöhemmin myös armoreihin). Nykyinen tapa (kohta 0.5/10.3) integroi jokaisen aseen bespoke-koodina: osa jakaa `attackTimer`in, osa käyttää `weaponTimers[i]`, osa `advancedTimers[i]`; Bow/Katana tarvitsivat paikallisen Crit-hackin koska globaalia crit-koukkua ei ole; affiksit on ketjutettu jokaisen aseen omaan vahinkokaavaan erikseen. Tämä on jo tuottanut viisi dokumentoitua arkkitehtuurikompromissia 13 aseella — ei skaalaudu kymmeniin aseisiin.

**Ratkaisu — kaksi kerrosta:**

1. **Yksi yleinen `WeaponStats`-rakenne + yksi keskitetty laskentafunktio.** Universaali stat-kirjasto — **vahvistettu 2026-09-09 (vaihe 1, Coden kartoitus)**: Damage, Cooldown, Size, Quantity, Duration, ProjectileSpeed, Bounces, Crit (8 statia, sama nimistö kuin nykyisessä `AffixStat`-enumissa, ei muutosta). Pierce ja Bounces pidetään toistaiseksi yhtenä samana mekanismina, ei eriytetä omiksi stateiksi — kumpikaan ei ole vielä oikeasti tarvinnut olla erillinen, eriyttäminen olisi spekulatiivista; jos joskus tulee ase joka tarvitsee molemmat samaan aikaan, eriytetään silloin (pieni lisäys `applicableStats`-malliin, ei uudelleenkirjoitus). `GetWeaponStats(w)` (taistelukoodin lukema, Selviytymistila-portitettu) ja `GetWeaponStatsFor(character, weapon)` (UI/testaus, sama malli kuin `AffixMultiplierFor`) laskevat kertaalleen: peruslukemat × variantin laatutaso × affiksit × (myöhemmin) armor. Kaikki aseet lukevat tästä yhdestä rakenteesta oman ad hoc -kertoimensa sijaan.
2. **Arkkityyppi + `applicableStats`-suodatin per ase (ja myöhemmin per varuste), jotta statit eivät vuoda väärään asetyyppiin.** Jokainen ase saa käytöstyyppitagin — **vahvistettu 2026-09-09 (vaihe 2)**: Melee (määritelty laajasti: välitön isku lähimpään/useampaan kohteeseen kantaman sisällä — kattaa myös Dicen ja Blood Magicin, ei vain kirjaimellista lähitaistelua), Ranged-Projectile, Orbit, AoE, Beam (rajattu tiukemmin: linjamainen/ketjuuntuva efekti joka voi osua useaan viholliseen matkalla — Lightning-ketju, Sniper/Space Noodle, erotettu Meleestä juuri tämän mekaanisen eron takia). Hero Sword luokiteltu hybridiksi (Melee ensisijaisen käytöksen mukaan, mutta Quantity mukana koska "Quantity lisää viiltoja"). 13 jo toteutetun aseen `applicableStats` on tarkalleen identtinen niiden nykyisen `AffixPool`-listan kanssa (ei käytösmuutosta, yksi totuuden lähde). Loppujen 17 aseen listat ovat paras arvio niiden `CastAdvanced`/`CastArsenal`-käytöksestä — vahvistetaan lopullisiksi vasta kun niille tehdään affiksit (kohta 10.3). **Huomio**: tämä pelimekaaninen arkkityyppijako on eri taksonomia kuin Astran visuaalinen pohjaperhejako (kohta 10.6) — esim. Dice on pelimekaanisesti "Melee" mutta visuaalisesti "kelluva reliikki". Näitä ei pidä sekoittaa samaksi listaksi. `AffixPool` per ase generoidaan automaattisesti `applicableStats`-listasta käsin kirjoittamisen sijaan. Sama malli laajenee myöhemmin armoriin omalla `applicableStats`-joukolla (Armor/Resistance/HP/MoveSpeed) samaa taustakoneistoa käyttäen.

**Yhtenäinen ajastinjärjestelmä**: nykyiset kolme rinnakkaista ajastinmallia (jaettu `attackTimer`, `weaponTimers[i]`, `advancedTimers[i]`) yhtenäistetään yhdeksi taulukoksi — jokaisella omistetulla aseella oma ajastinslotti. Tämä poistaa Sword/Lightning-tyyppiset jaetun-ajastimen kompromissit ja tekee Cooldown-statista/-affiksista täsmällisen jokaiselle aseelle riippumatta aseiden määrästä. Ei aloitettu (vaihe 4).

**Crit yleiseksi — TIETOISESTI EROTETTU tästä refaktoroinnista (päätetty 2026-09-09, vaihe 3:n aikana).** Alkuperäinen ehdotus oli "globaali CritChance/CritMultiplier osaksi WeaponStats-rakennetta, ei enää paikallista per-ase-hackia". Vaihetta 3 tehdessä Code tunnisti oikein, että tämä olisi aidosti pelaajalle näkyvä käytösmuutos (crit-flashit useammin, kriittisten osumien tilastollinen jakauma muuttuisi tasaisesta kertoimesta todelliseksi vaihteluksi) — se olisi rikkonut tämän tehtävän kovan rajan "pelaajalle näkyvä käytös ei saa muuttua ollenkaan". Päätös: `WeaponStats.Crit` pysyy identtisenä nykyisen paikallisen vahinkokertoimen kanssa (Bow/Katana-malli) myös vaiheessa 5. "Crit yleiseksi" on nyt oma, erillinen, myöhemmin erikseen hyväksyttävä ja testattava tehtävä — koska se muuttaa käytöstä tarkoituksella, sitä ei voi testata suoralla laskenta-vertailulla vanhaan (vanhaa arvoa ei enää olisi vertailukohtana), vaan se tarvitsee oman hyväksytyn testaussuunnitelman kun sitä aletaan tehdä.

**Miksi tämä ennen muuta**: kun tämä on tehty, uuden aseen lisääminen on peruslukemat + `applicableStats`-lista + käytöstyyppi + visuaali/ammusprefab — ei uutta taistelukoodihaaraa eikä uutta arkkitehtuurikompromissia joka kerta. Tekemällä tämä ennen affiksien laajennusta lopuille 17 aseelle (kohta 10.3) ja variantteja lopuille 27 aseelle (kohta 5B) vältetään saman bespoke-työn toistaminen 17+27 kertaa, ja refaktorointi tehdään 13 aseen läpi (nykyinen laajuus) sen sijaan että se pitäisi tehdä jälkikäteen 30+ aseen läpi.

**Riski/rajaus**: tämä on puhdas sisäinen refaktorointi olemassa olevalle 13 aseen taistelukoodille — pelaajalle näkyvä käytös (vahingot, cooldownit, affiksien vaikutus) ei saa muuttua, vain koodin rakenne. Selviytymistila ei saa muuttua miltään osin (sama periaate kuin kaikissa aiemmissa muutoksissa). Testataan Unity CLI:llä per ase samalla suoran laskennan menetelmällä kuin affiksit on jo testattu (pakota tunnettu stat-arvo, laukaise hyökkäys, vertaa mitattua vahinkoa laskettuun) — jos yksikin nykyisistä 13 aseesta antaa eri tuloksen kuin ennen refaktorointia, se on regressio joka pitää korjata ennen committia.

**Eteneminen**:
- Vaihe 1 (kartoitus: kaikkien 30 aseen statit + ajastinmallit koottu taulukoksi, 8 statin lista vahvistettu) — VALMIS 2026-09-09.
- Vaihe 2 (`WeaponStats`-struct + arkkityyppi/`applicableStats`-taulukko kaikille 30 aseelle) — VALMIS 2026-09-09, hyväksytty.
- Vaihe 3 (keskitetty `GetWeaponStats(w)`/`GetWeaponStatsFor(character, weapon)`-laskentafunktiot) — VALMIS 2026-09-09. Testattu Play Modessa: pakotettu Sword+Pottu Damage +22%/Size +14% → oikeat kertoimet, Quantity/Cooldown pysyivät 1:ssä; Mappi-tilassa ja Selviytymistilassa oikea käytös (Selviytymistilassa tasan Identity); ase ilman `WeaponMetadata`-merkintää (Aegis) palautti Identityn. 0 konsolivirhettä, käännös 0 virhettä. Crit pidetty paikallisena kertoimena (ks. yllä).
- Vaihe 4 (yhtenäinen ajastintaulukko) — SEURAAVANA.
- Vaihe 5 (migraatio: 13 nykyisen aseen siirto uuteen putkeen, per-ase regressiotestaus) — ei aloitettu.

**Tila**: ehdotettu ja hyväksytty 2026-09-09, aloitettu Codella samana päivänä (vaiheet 1–3 valmiit). Ks. kohta 9 (toteutusjärjestys).

**10.6. Aseiden visuaalinen arkkityyppipohja-työnjako Astralle — EHDOTETTU 2026-09-09, KÄYNNISSÄ (rinnakkainen 10.5:n kanssa, ei riippuvuutta Codeen).**

**Ongelma**: sama skaalausongelma kuin 10.5:ssä, mutta 3D-tuotannon puolella — jos jokainen "monta monta monta" -tavoitteen aseista vaatii täysin uniikin mallin ja riggauksen alusta asti, Astran työmäärä ei skaalaudu 30+ aseeseen.

**Ratkaisu — Astran tarkennettu suunnitelma (2026-09-09), hyväksytty:**

Kuusi pohjaperhettä (yksi kutakin otetta ja siluettia kohti) + pieni erillinen esine-/ammuskitti:

1. **Yksikätinen teräase** — Katana, Dexecutioner, Hero Sword, Corrupted Sword; Wireless Dagger lyhyenä versiona. Vaihdettavat osat: terä, väistin, pommeli, sama kahva/ote. Kaksoiskajo säilyy yksilöllisenä.
2. **Sauva** — Lightning Staff, Firestaff, mahdollinen taikafokus Dragon's Breathille. Sama varsi/kädensija, vaihdettava kärki/kristalli/koristekaulus. Kalman kaari säilyy yksilöllisenä.
3. **Modulaarinen ampuma-ase** — Revolver, Shotgun, Sniper Rifle, Slutty Rocket. Yhteinen kahva/runko; lyhyt piippu/haulikkopää/pitkä piippu/laukaisuputki. Kaksi asentoprofiilia: pistooli yhdellä kädellä, pitkä ase kahdella (tukikäden paikka).
4. **Kelluva taikafokus/reliikki** — Chunkers, Dice; ehdotettu kantaja Auralle, Aegisille, Black Holelle, Space Noodlelle, Blood Magicille. Vaikutuksen tunnistettavuus tulee pääosin VFX:stä.
5. **Kaksikätinen varsiase** — Scythe; tulevat raskaat kirveet/vasarat/suuret terät. Nykyinen Axe on lentävä kirves, tarvitsee erillisen ammusversion.
6. **Jousi** — Bow ja sen variantit. Oma erillinen perhe, koska ote/jänne/laukaisuliike poikkeavat ampuma-aseista.

**Pieni yhteinen esine-/ammuskitti**: luu, pullo, miina, banaanikaari, heittokirveen pää — Bone/Poison Flask/Mines/Bananarang tarvitsevat tunnistettavan muodon mutta eivät omaa riggausprojektia.

**Efektipohjaiset (ei kädessä pidettävää mallia)**: Aura, Frostwalker, Black Hole, Tornado — tunnistetaan ensisijaisesti VFX:stä, mallintaminen sauvoiksi kasvattaisi mallimäärää turhaan.

**Poikkeus — flagship-aseet pysyvät täysin uniikkeina**: Kaksoiskajo ja tuleva pääkallo/viikate-sauva.

**Laatutasot (kevyt malli)**: ★ perusmateriaalit + tunnusväri, ★★ koristepaneeli/reunakorostus + maltillinen emissio, ★★★ pieni lisämoduuli + lyhyt hyökkäyksenaikainen tehoste. Identiteetti = siluetti, variantti = yksi tunnusomainen muutos, tähdet = viimeistely.

**Mallinnusjärjestys (Astran suunnitelma, hyväksytty):**
1. **Kiinnitysten pilotti ensin** — oikea/vasen käsi, selkä, vyö + mitta-/akselisäännöt, ennen mitään pohjamallia. VALMIS 2026-09-09: kattaa kaikki 9 pelattavaa hahmoa, 36 pistettä, erillinen Unity-tarkistuskohtaus sijainnin/akselien/skaalauskompensoinnin varmistamiseksi. Tuotti dokumentit `CODE_HANDOFF.md`/`CONVENTIONS.md` Codelle luettavaksi kiinnityspiste-koodityötä varten.
2. Yksikätinen teräase — pilotti Katana + Corrupted Sword (sama pohja, eri ilme).
3. Sauva — Lightning + Firestaff.
4. Ampuma-ase — Revolver + Shotgun ensin, sitten Sniper + raketinheitin (tukikäden testaus).
5. Kelluva reliikki + pieni esinekitti.
6. Jousi, sitten kaksikätinen varsiase.

**Koodiriippuvuus (koordinoitava Coden kanssa)**: kiinnityspisteiden yleistäminen koodissa (`WeaponAttachmentPoint`-järjestelmä, korvaa `SwordVariantVisual.cs`:n nykyisen hahmon-runkoon-sidotun likimääräisen sijoittelun) annetaan Codelle omana tehtävänä **vasta kun WeaponStats-pipeline (kohta 10.5) on kokonaan valmis** — ei samaan aikaan, jotta kaksi isoa rinnakkaista refaktorointia ei osu samoihin tiedostoihin. Astra voi silti edetä mallinnustyössä (vaiheet 2–6) sitä ennen ilman Codea, koska kiinnityskonventiot on jo dokumentoitu.

**Tila**: ehdotettu 2026-09-09, aloitettu samana päivänä Astralla (vaihe 1, kiinnityspilotti + ensimmäiset pohjaperheet, valmis). Astra osui 2026-09-09 käyttörajaan kesken työn, jatkaa itsestään rajan vapautuessa.

---
*Tämä dokumentti on ideointipohja jatkokeskustelulle. Katso AGENTS.md:n Muutosloki (Unity-projektin juuressa) tarkimmat tekniset yksityiskohdat ja testaustulokset kustakin toteutetusta osasta. Kopio elää myös Unity-projektin juuressa (`pelaaja-progressio-ja-ranked-suunnitelma.md`) jotta paikallinen koodausagentti (Code) pääsee siihen käsiksi suoraan — muista päivittää molemmat kun jompaakumpaa muokataan.*
