# Pottu / BONK

Paina Play SurvivorPrototype-scenessä ja valitse Pannu / BONK. Pottu on proseduraalinen low-poly-peruna: ylisuuri metallikypärä, vihainen ilme, pienet saappaat ja paistinpannu. Juostessa jalat kipittävät, vartalo hyllyy ja kypärä heiluu. Jousi- ja salama-aloitukset ovat yhä käytettävissä; pannu näkyy kun se kuuluu buildiin.

- WASD / nuolet: liiku.
- SPACE / peliohjaimen eteläpainike: kierähdys liikesuuntaan, paikallaan katsesuuntaan.
- Kierähdys kestää 0,38 s, nopeus 18 m/s ja palautumisaika 1,6 s. Suojaa kosketukselta ja bossin alueiskulta. Areenan raja pysäyttää liikkeen.
- E: karttakohteet. TAB / Escape: kauppa ja tauko. Taso- ja kauppavalinnoissa myös animaatiot ja väistön latautuminen pysähtyvät.
- Pannu heiluu automaattisesti lähivihollista kohti; ohut kaari näyttää sivalluksen. Size ja pannun asetaso kasvattavat mallia (visuaalinen maksimikoko 3,5x).
- Osumat väläyttävät vihollisen, nostavat vahinkonumeron ja soittavat syntetisoidun metallisen BONK-äänen. Kriittinen numero on kultainen. Vihollisen aiempi takaisku säilyy.

Lähteet: Assets/Survivor/Scripts/PottuVisual.cs (malli, animaatio, ääni), SurvivorPottu.cs (liike, väistö, osumapalaute), SurvivorGame.cs ja SurvivorProgression.cs (pelisilmukan liitokset). Malli ja ääni luodaan ajossa ilman ulkoisia assetteja. PrototypeTools-kansion lähteet ovat tuonnin välitiedostoja.

Tarkistukset: VerifyV2.cs:n 23 aiempaa tarkistusta ja VerifyPottu.cs:n 14 hahmo- ja väistötarkistusta Play-tilassa Unity CLI:llä. Ääninäytteiden amplitudi tarkistettiin, mutta äänen subjektiivinen kuulohavainto ja käsin tehtävä pelituntuman arviointi jäävät käyttäjän kokeiltavaksi. Kuvakaappaus pottu-portrait.png näyttää hahmon erillisellä tarkastelukameralla; varsinainen pelinäkymä on kauempaa.
