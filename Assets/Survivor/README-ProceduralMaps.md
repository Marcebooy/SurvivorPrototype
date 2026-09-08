# Proseduraaliset kartat (generaattori v1)

Paina Play Unityssä ja aloita peli. Päävalikon **KARTAN SEED** voi jättää tyhjäksi (satunnainen uusi kierros) tai siihen voi syöttää 32-bittisen kokonaisluvun, myös 0:n tai negatiivisen luvun. **Käytä viimeisintä seediä** täyttää viimeksi pelatun kartan seedin. Pelin aikana seedin nappi kopioi sen leikepöydälle.

## Pelisilmukka

- Kierros alkaa kartalta 1, hahmon ja aloitusaseen valinnasta.
- Jokaisella kartalla on 8–12 huonetta, leveät yhdyskäytävät, kolme arkkua, pyhäkkö ja bossihuone. Minimapissa valkoinen on pelaaja, keltainen arkku, punainen pyhäkkö ja sininen bossiportaali.
- Aiempi 90 sekunnin sääntö säilyy: sen jälkeen uusien tavallisten vihollisten tulo loppuu ja bossin voi kutsua bossihuoneen portaalilta E:llä.
- Bossin kuolema rauhoittaa taistelun ja vaihtaa noin kahden sekunnin kuluttua automaattisesti seuraavalle kartalle. Myös odottava tasopäivitys säilyy eikä estä siirtymää.
- Seuraava kartta saa uuden seedin. Pelaaja siirtyy uuden kartan alkuhuoneeseen, terveys palautuu ja vanhat viholliset, pudotukset, ammukset sekä efektit siivotaan.
- Aseet, Tomet, taso, XP, kulta ja kierroksen statbonukset säilyvät. Uusi kierros aloittaa ne alusta kuten aiemmin.
- Bossin HP on edelleen `900 × kartan numero × curse`. Tavallisten vihollisten HP- ja määräskaalaus käyttää myös kartan numeroa.

## Seedit ja tallennus

`ProceduralMapLayout.GeneratorVersion` on 1. Sama seed ja generaattoriversio tuottavat saman huonejaon, käytävät ja maamerkkien paikat riippumatta kartan numerosta tai taistelun satunnaisluvuista. Huonejako käyttää omaa eksplisiittistä xorshift-generaattoria. Seuraavan kartan seed johdetaan nykyisestä seedistä, joten myös karttojen sarja on toistettavissa.

Unity PlayerPrefs tallentaa jokaisen kartan luonnissa:

- `BonkSurvivor.Prototype.v1.LastMapSeed`
- `BonkSurvivor.Prototype.v1.LastMapNumber`
- `BonkSurvivor.Prototype.v1.MapSeedHistory`: JSON, jossa kierroksen alkuseed ja jokaisen käydyn kartan numero, seed ja generaattoriversio.

Historia koskee viimeisintä aloitettua kierrosta. Seedit tallentavat kartat, eivät koko hahmon tai kesken olevan taistelun tallennuspeliä. Myöhemmän kartan seedin syöttäminen päävalikkoon toistaa sen geometrian uutena kierroksena kartan 1 vaikeudella.

Koodista: `game.StartSeededRun(12345)`, `game.CurrentMapSeed`, `game.RunSeed`, `game.MapSeeds`, `game.MapLayout`. Seedit pysyvät toistettavina, kun v1-algoritmia ei muuteta. Generaattorin muutoksissa nosta versiota ja säilytä vanha algoritmi, jos vanhojen seedien tuki halutaan pitää.

## Rakenne

- `ProceduralMapLayout.cs`: ruudukon huoneiden yhdistävä puu, lisäsilmukat, kaukaisimman bossihuoneen valinta, yhteyksien tarkistus, liikkumisrajat ja BFS-reitinhaku. 4 yksikön ruudut; huoneet 28–36 yksikköä leveitä ja käytävät 12 yksikköä.
- `SurvivorProceduralMaps.cs`: geometria, erillinen kartan juurihierarkia ja resurssien siivous, seed-historia, siirtymäajastin ja minimappi.
- `SurvivorProgression.cs`: bossi ja seuraavan kartan käynnistys; `SurvivorGame.cs`: pelisilmukan kytkennät.
- `SurvivorNetwork.cs` / `SurvivorGameNetwork.cs`: isännän seedin, karttanumeron ja generaattoriversion lähetys liittyvälle kaverille sekä kartanvaihdossa. Omistaja siirtää oman verkkohahmonsa alkuhuoneeseen. Kahden koneen Relay-peliä ei ole tämän muutoksen yhteydessä testattu; aiemman moninpelin vaihe 1:n rajoitukset, kuten bossin täydellisen replikoinnin puuttuminen, säilyvät.

Ulkoasu on perusversion low-poly-huoneisto, jossa on matalat kivireunat ja koristepuita. Maasto on tasainen. Matalat reunat rajoittavat liikkumista; ne eivät estä aseiden ammuksia/aluevaikutuksia. Reitinhaku pitää viholliset käytävillä, eikä liike tai väistö voi oikaista seinän läpi. Vanha metsäkarttakoodi on säilytetty lähdekoodissa, mutta oletuspeli käyttää uutta generaattoria; vanha `VerifyForestMap.cs` koskee aiempaa metsäkarttaa.

## Varmennus

Unity Editorin Play Modessa:

```powershell
unity command eval_file --file PrototypeTools/VerifyProceduralLayout.cs --timeout 60000
unity command eval_file --file PrototypeTools/VerifyProceduralProgression.cs --timeout 60000
```

Layout-testi kattaa 207 seediä (myös 0 ja int-raja-arvot), toistettavuuden, yhteydet, seinät ja 222 reitinhakua. Integraatiotesti tappaa kolme peräkkäistä bossia, tarkistaa automaattisen siirtymän myös valikon/tasovalinnan aikana, buildin säilymisen, resurssien siivouksen sekä seed-historian tallennuksen ja uudelleenluonnin. Se palauttaa seed-asetukset testin lopuksi. Testejä tulee ajaa vain testikierroksessa: ne korvaavat nykyisen kierroksen tilan.
