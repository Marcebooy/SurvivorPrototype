# Tuhkaerämaa — kiinteä ympäristö / taidepaketti

Muokattava low-poly-karttapohja samalla 195 × 159 yksikön tasaisella ulkorajalla kuin Mosswood ja Luuluola (arenaRadius 60). Avoin keskusta, hiiltyneet puut, tumma basalttireuna, ruosteinen tuhka ja oranssinkeltaiset hehkuhalkeamat. Ei sisähuoneita tai käytäväseiniä.

## Tiedostot ja muokkaaminen

- `Resources/AshWastes/Tuhkaeramaa_Environment.prefab`: valmis **muokattava prefab**, ei scene. Avaa Prefab Modessa. Maasto, maamerkit, koristeet ja sijoituspisteet ovat erillisiä lapsiolioita.
- `Resources/AshWastes/Materials/`: seitsemän muokattavaa URP/Lit-materiaalia.
- `Resources/AshWastes/Meshes/`: tallennetut, jaetut low-poly-meshit.
- `Scripts/AshWastesEnvironment.cs`: maaston ja kaikkien maamerkkien muokattava C#-mallinnuslähde.
- `Editor/AshWastesBuilder.cs`: materiaalipaletti, assettien tallennus ja renderöidyt esikatselut. **Tools → Survivor → Rebuild Tuhkaeramaa art package** luo paketin uudelleen lähdekoodista. Uudelleenrakennus palauttaa prefab-geometrian ja materiaalivärit koodin arvoihin; tallenna omat käsimuokkaukset prefab-varianttiin tai muuta lähdekoodia.

Materiaaleilla ei ole tekstuuririippuvuuksia: tyyli syntyy tasaisista väreistä, tasaisista pintanormaaleista ja emissiivisistä halkeamista. PNG-tekstuuria ei tarvita renderöintiin. Hehku näkyy emissiivisenä värinä ilman Bloomia; ympärille leviävä loiste vaatii projektin Bloom-asetuksen.

| Materiaali | sRGB | Käyttö |
|---|---|---|
| AshEarth | #66372C | lämmin tuhkamaa |
| OchreDust | #804D32 | tuulen kasaama hiekka |
| Basalt | #34221C | tumma kivi, reunus ja monoliitit |
| IronOxide | #703023 | tummanpunainen rautaoksidi |
| CharredWood | #241A18 | hiiltyneet puut |
| Ember | #F45B16 | halkeamien reuna, emission ×2 |
| HotCore | #FFBE55 | halkeamien kapea ydin, emission ×2,5 |

## Maamerkit ja ankkurit

`LANDMARKS - editable` sisältää viisi ryhmää. Ankkurit ovat tyhjiä Transformeja, joihin Unity-puoli voi asettaa pelilliset arkut/pyhäkön/portaalin:

| Ryhmä | Konsepti | Ankkuri | X, Z (arenaRadius 60) |
|---|---|---|---|
| 01 Split monolith | kaksi haljennutta korkeaa basalttikiveä ja hehkuva siirros | Chest_01 | 15,79; 14,21 |
| 02 Cinder grove | kuolleiden puiden pieni aukio | Chest_02 | −28,42; 18,95 |
| 03 Ember basin | matala kivikehä ja hehkuhalkeama | Chest_03 | 31,58; −23,68 |
| 04 Burnt sentinel | suuri hiiltynyt puu pyhäkköaukion takana | Shrine | −18,95; −18,95 |
| 05 Ember crown | viisi basalttipilaria ja hehkuva maanhalkeama | Portal | 0; 39,47 |

Lisäksi `PlayerSpawn` on kartan keskellä. Sijoituspisteet vastaavat nykyisen kiinteän kartan CreateLandmarks-koordinaatteja. Halkeamat ovat visuaalisia, eivät vahinkoa tekeviä laava-alueita.

## Pelikoodin kytkentä

Projektissa on uusi `SurvivorAshWastesMap.cs` / `BuildAshWastesMap()`. Se lataa prefab-paketin Resourcesista, asettaa `fixedMapRoot`-viitteen, skaalaa `arenaRadius / 60` ja rekisteröi 16 maastoesteen ympyrät olemassa olevaan `forestObstacles`-listaan. Assettien yhteiset materiaalit ja meshit kuuluvat pakettiin, joten niitä ei lisätä tuhottavien ajonaikaisten resurssien listoihin.

Tämä toimitus on ympäristö- ja materiaalipaketti. Sitä ei ole vielä lisätty MapType-valikkoon, karttaesineiden pudotuksiin tai inventoryyn. Pelikoodin puolella lisää tarvittaessa uusi enum-arvo ja `BuildFixedMap`-haara, joka kutsuu `BuildAshWastesMap()` nykyisen resurssien siivouksen jälkeen. Säilytä `MapLayout = null` kuten muissa kiinteissä kartoissa. Muokattaessa kivien/puiden sijainteja käsin päivitä myös prefab-juuren `obstacles`-lista; uudelleenrakennus lähdekoodista laskee sen automaattisesti.

## Tarkistettu

Paketti rakennettu ja renderöity Unity 6000.6.0f1:ssä. Seitsemän materiaalia, kaikki mesh-viittaukset tallennettuina assetteina, kuusi sijoitusankkuria ja 16 maastoestettä. Jokaisella ankkurilla vähintään 1,5 yksikön vapaa säde suhteessa rekisteröityihin esteisiin. Paketti ei sisällä vihollisia, kameran tai valojen ajonaikaisia muutoksia, vahinkologiikkaa tai uutta sceneä.
