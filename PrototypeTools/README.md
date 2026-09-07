# Bonk Survivor – ensimmäinen pelattava prototyyppi

Avaa `Assets/Scenes/SurvivorPrototype.unity` ja paina Play. Arena ja hahmot luodaan Play-tilassa, joten editointinäkymässä näkyy vain Survivor Game -objekti.

- WASD / nuolet: liiku. Myös peliohjaimen vasen tatti toimii.
- Automaattinen lähialuehyökkäys ja lähimpään viholliseen suunnattu ammus.
- Siniset kiteet antavat XP:tä ja kultaa. Liiku niiden lähelle kerätäksesi ne.
- Jokainen taso: +3 vahinkoa, +5 maksimi-HP:tä ja 15 HP:n parannus. Joka kolmas taso: +0.15 hyökkäystä sekunnissa.
- TAB / Escape tai Kauppa-painike: tauko ja kauppa. Ostettavana hyökkäysnopeus, vahinko, liikenopeus, keräyssäde ja parannus. Ostohinta nousee parannuskohtaisesti.
- Selviä 5 minuuttia. R aloittaa uuden kierroksen kuoleman tai voiton jälkeen.
- Parannukset ja kulta ovat kierroskohtaisia, eikä tallennusta vielä ole.

Pääasiallinen lähdekoodi: `Assets/Survivor/Scripts/SurvivorGame.cs`. `PrototypeTools/SurvivorGame.cs` on alkuperäinen CLI-tuonnin lähde, ei Unityn kääntämä tiedosto. Jatkokehitys tehdään Assets-kansion tiedostoon.

## Varmennus

Unity 6000.6.0f1, avoin Editor, Unity CLI. Skripti kääntyi ilman konsolivirheitä. Koeajossa automaattinen hyökkäys kaatoi vihollisia. `unity command eval_file --file PrototypeTools/Verify.cs` suorittaa Play-tilassa 14 pelilogiikan tarkistusta; testi nollaa nykyisen prototyyppikierroksen. HUD ja kauppa tarkastetaan kuvakaappauksista. Ei vielä standalone-buildia tai käsin tehtyä pelituntuman arviointia.

## Seuraavat kehitysaskeleet

1. Pelituntuma: hahmomalli, juoksu- ja lyöntianimaatio, osumapalaute ja äänet.
2. Erota prototyypin vastuista pelaaja, aseet, viholliset, eteneminen ja UI omiksi komponenteikseen; siirrä tasapaino ScriptableObject-tietoihin.
3. Lisää asevalinnat, tasokohtaiset satunnaiset parannusvaihtoehdot ja pomovihollinen.
4. Rakenna pysyvä eteneminen ja tallennus, kun kierroksen tasapaino toimii.
