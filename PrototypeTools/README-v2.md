# Survivor v2 – AGENTS-koonnin mukainen pelisilmukka

Avaa `Assets/Scenes/SurvivorPrototype.unity` ja paina Play. Valitse Sword, Bow tai Lightning. Aloitat yhdellä aseella, ja tasovalinnat voivat lisätä muut aseet buildiin.

- WASD / nuolet: liiku. Aseet hyökkäävät automaattisesti.
- XP-tasolla peli pysähtyy: valitse yksi kolmesta satunnaisesta ase- tai Tome-päivityksestä. Useampi kerralla saavutettu taso jonoutuu. Aiemmat automaattiset tasobonukset on korvattu valinnoilla.
- Tomes: Damage, Cooldown, Quantity, Size, Precision, Armor ja Movement. Quantity vaikuttaa miekan vahinkoon lisäiskukertoimena, nuolten määrään ja salaman kohteisiin. Size vaikuttaa miekan kantamaan ja nuolten osumakokoon. Tarkat arvot ovat tämän prototyypin omaa tasapainotusta.
- Kultaiset arkut: lähesty ja paina E. 12 kultaa antaa satunnaisen päivityksen, kerran per arkku.
- Punainen pyhäkkö: E antaa +30 % vahinkoa mutta kasvattaa nykyisten ja tulevien vihollisten kestävyyttä ja nopeutta 20 %. Kerran per alue; kirous kertyy alueiden välillä.
- Sininen portaali pohjoisessa: 90 sekunnin jälkeen E kutsuu bossin. Voita se ja palaa portaaliin. E vie seuraavaan vaikeampaan alueeseen ja säilyttää buildin.
- Bossi aiheuttaa lähietäisyydellä aluevahinkoa kolmen sekunnin välein. Pysy liikkeessä.
- TAB / Escape: tauotettu kultakauppa. Käytettävissä kierroksen aikana kaikkialla.
- Kuollessa saat Silveriä: yksi jokaista viittä kaatoa kohti sekä 20 jokaista bossia kohti. Silver ja ostettu pysyvä HP tallentuvat paikallisesti PlayerPrefsiin.
- Aloitus- ja kuolemaruudusta voit ostaa +10 pysyvää HP:tä, enintään 10 kertaa. R aloittaa kuoleman jälkeen uuden kierroksen.

## Toteutuksen rajaus

Kolme asetta, seitsemän Tomea, arkut, pyhäkkö, bossi ja toistuva aluekierto. Alueet käyttävät samaa prototyyppiareenaa kasvavalla vaikeudella. Hahmovalinnat, muut aseet, varsinaiset erikoisesineet, tehtävät, biomeja vaihtavat kentät ja sisältöavaukset ovat vielä jatkokehitystä. Viiden minuutin automaattivoitto on poistettu.

Unity kääntää `Assets/Survivor/Scripts/SurvivorGame.cs`- ja `SurvivorProgression.cs`-tiedostot. PrototypeTools-kansion lähdekopiot ovat vain CLI-tuonnin välitiedostoja, eivät ylläpidettävä pelilogiikka. Vanha README ja Verify.cs koskevat v1:tä; käytä tämän version ohjeita ja VerifyV2.cs:ää.

## Tarkistukset

`unity command eval_file --file PrototypeTools/VerifyV2.cs` Play-tilassa tarkistaa valinnan, XP-jonon, tauot, arkut, pyhäkön, bossin, aluevaihdon, Silver-palkkion, pysyvän HP:n sekä kaikkien aseiden osumat ja nuolten läpäisyn. Se nollaa testikierroksen mutta palauttaa aiemmat tallennusarvot finally-lohkossa. Testi on kehityksen savutesti, ei täydellinen pelituntuman tai pitkän kierroksen suorituskyvyn arviointi.
