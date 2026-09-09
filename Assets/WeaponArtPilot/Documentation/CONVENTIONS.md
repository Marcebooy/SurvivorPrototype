# Weapon attachment conventions v1 – pilotin sopimus

Tämä on assettipuolen kiinnityspilotti. Coden WeaponStats-pipelinea tai nykyistä
SwordVariantVisual.cs-tiedostoa ei muuteta. Yleistetty WeaponAttachmentPoint
otetaan käyttöön erillisessä tehtävässä vasta WeaponStats-työn valmistuttua.

## Yksiköt ja akselit

Unityn hahmon paikallinen koordinaatisto: +X hahmon anatominen oikea, +Y ylös,
+Z eteen. Koordinaatit määritellään mallin juureen nähden ilman pelaajan maailman
sijaintia, kääntymistä tai roll-pivotin rotaatiota. 1 Unity-yksikkö = 1 metri.
Blender-lähteet: +Z ylös, -Y eteen; nykyinen FBX-tuonti muuntaa pisteen
`(x,y,z) -> (-x,z,-y)` Unityyn. Tämä mitattiin tuoduista luista.

Raaka FBX voi sisältää akselikorjauksen juuren rotaatiossa. Säilytä ImportedModel-
lapsen tuontirotaatio. Neutraalin WeaponVisual-juuren sijainti on (0,0,0),
rotaatio (0,0,0,1), skaala (1,1,1). Älä nollaa FBX:n sisäistä rotaatiota.

Aseen paikallinen +Y: terästä/varresta kohti kärkeä; +Z: aseen etupuoli.
Ampuma-aseella +Y on kahvan suunta ylöspäin ja piippu osoittaa +Z:aan.
Jousella +Y lapojen pituusakseli, +Z nuolen lentosuunta.
Kelluvilla esineillä origo on keskipiste, +Y ylös, +Z tunnistettava etupuoli.
Vasenkätinen versio ei käytä negatiivista skaalaa: sama geometria ja oikea rotaatio.

## Neljä pistettä

| Piste | Origo | Leposuunta mallissa | Kiinnityksen periaate |
|---|---|---|---|
| RightHand | Oikean kämmenen sulkeutuva ote | Tavallisesti 25° ulospäin ja hieman eteen; +Y akseli taulukossa | Oikeaa kättä liikuttava lähin oikea luu |
| LeftHand | Vasemman kämmenen ote | Peilattu ULKOSUUNTA mutta positiivinen skaala | Vasenta kättä liikuttava luu |
| Back | Kahvan säilytyspiste yläselässä | Kärki alas ja oikealle noin 20°; aseen +Z selästä ulospäin | Body; ei liikkuva viitan luu |
| Belt | Kahva oikealla lantiolla | Kärki alas; +Z ulospäin hahmon oikealle | Body; ei reisi tai vaatesuikale |

Ninja ja Berserker käyttävät alaviistoon suunnattua lepo-otetta, joka vastaa
nykyisten lähiasemallien kantotapaa. Pottun oikea käsi on poikkeus: akseli +Y
osoittaa mallin +Z:aan eli eteenpäin ja aseen etupuoli ylöspäin. Tämä vastaa
nykyistä pannun otetta. Täsmälliset akselivektorit KAIKILLE 36 pisteelle löytyvät
`CHARACTER_SOCKET_TABLE.md`:stä; paikalliset kvaterniot ja sijainnit JSONista.
Tämä on neutraali kantoprofiili, ei pistoolin tähtäysasento tai jousen vetopose.
Käyttötyypin animaatio/IK ratkaisee hyökkäysasennon myöhemmässä integraatiossa.

## Tuonnissa todetut kaksi poikkeamaa

1. Kahdeksassa FBX-hahmossa nykyinen `.L` on tuonnin jälkeen +X-puolella ja `.R`
   -X-puolella. Anatominen RightHand käyttää siksi näissä `.L`-nimistä ajuria.
   Luun nimi ei ole sopimus kädestä. Nykyiset hyökkäysklipit käyttävät usein
   `.R`-nimistä puolta: Code tarvitsee erillisen hyökkäyspuolen kartoituksen.
   Älä nimeä tai peilaa kaikkia luita automaattisesti, sillä klippipolut rikkoutuvat.
2. Näiden hahmojen luissa on 100-kertainen periytyvä skaala. Bind-matriiseista
   laskettu localPosition on luun paikallisissa Unity-koordinaateissa (käytännössä
   1/100 mallin metreistä). `bindModelPosition` on aina METREJÄ. JSONin localScale
   0,01 kompensoi skaalaa nykyisessä bind-asennossa. Mallin tuontiskaalaa ei muuteta.

Esimerkki: Ritari RightHand käyttää `Ritari_Rig/Root/Body/Arm.L`-luuta.
Kämmenen piste mallissa on (0,83; 1,14; 0,10) m. JSON sisältää kyseisen luun
koordinaatistoon muunnetun offsetin sekä rotaation ja 0,01-skaalakompensoinnin.

## Ajurien seuraaminen ja skaala

Tulevan järjestelmän luotettava laskenta:

```
worldPosition = driver.TransformPoint(profile.localPosition)
worldRotation = driver.rotation * profile.localRotation
weaponWorldScale = characterUniformScale * weaponVisualScale
```

Aseen mittakaava ei saa periä luun 100-kerrointa, Pottun vartalon squash/stretch-
animaatiota eikä hyökkäyksen vahinkoalueen Size-statistiikkaa. Käytä tarvittaessa
erillistä jäykkää seuraajaa normalisoidun hahmojuuren alla. Pelkkä alkuhetken
localScale-kompensointi ei riitä Pottun animoidun ei-uniformin vartaloskaalan aikana.
Hahmokohtainen `defaultVisualScale` on SUUNNITTELUN lähtöarvo (0,85–1,45), ei
mitattu luuskaala eikä pelin nykyinen asetus. Testikappale kuvataan aina 1:1-koossa.

Kaksikätinen ase kiinnittyy yhteen pääkäteen. `SupportGrip` on aseessa oleva
tukikäden tavoite; asetta ei parentoida kahteen käteen. Tuki-IK ei vielä kuulu pilottiin.
Säilytyspisteet kuvaavat aserungon origon sijainnin. Pitkä sauva/jousi selässä
tarvitsee perhekohtaisen Stow-offsetin ja törmäystarkistuksen; kaikkia aseita ei
voi siirtää yhdellä kantosäännöllä vyölle. Tähdet eivät muuta kahvan kokoa.

## Referenssikappale ja tarkistuskohtaus

`SocketProbe.blend` ja `SocketProbe.fbx` sisältävät 50 mm halkaisijaisen,
200 mm pitkän kahvan, 500 mm pitkän vaalean testilevyn, 100 mm mittamerkit
sekä RGB-akselit: punainen +X, vihreä +Y, sininen +Z. Sininen uloke paljastaa
väärän kierron. Kärkipiste on 600 mm origosta. Testilevy alkaa 100 mm origon yläpuolelta.

Unity: `Assets/WeaponArtPilot/Attachments/AttachmentGallery.unity`.
9 hahmoa ja jokaisessa 4 WAP_-nimistä pilotin tyhjää transformia ja testikappale.
FBX-hahmot ovat oikeita projektin malleja bind-asennossa. Pottu on lähdekoodin
mitoilla tehty geometrinen referenssi, koska sillä ei ole omaa FBX-riggiä.
Mallien harmaa materiaali ja vanhojen aseosien osittainen piilotus koskevat vain
tarkistuskohtausta. Alkuperäisiin prefab-/FBX-tiedostoihin ei tehdä muutoksia.
Testimallit ovat layerilla 31; se on vain gallerian kuvausrajaus.
Previews-kansiossa on etu- ja takakuvat kaikista hahmoista.

## Mitä on varmennettu ja mitä ei

Unity CLI: ajuripolut löytyvät, kaikki 36 sijaintia vastaavat bind-taulukkoa
0,5 mm toleranssilla, kompensoidut skaalat ovat 1, ja FBX-testikappaleen Tip
on pisteen +Y-akselilla 0,600 m päässä. Alkuperäinen aktiivinen scene palautetaan
gallerian luonnin jälkeen. Tulos `Previews/unity_attachment_verification.json`.

Vanhojen aseiden täydellinen irrottaminen, käsien uudet tartunta-asennot,
kaikkien klippien läpileikkaus-/osumatarkistus, selkävarusteiden keskinäinen
törmäys ja Pottun liikkuvan seuraajan toteutus jäävät integraatiotehtävään.
Pisteiden identiteetti, yksiköt, akselit ja dataformaatti ovat tämän pilotin
konventio; lopullisen mallin suurempi suojus voi edellyttää pieniä sovituskorjauksia.
