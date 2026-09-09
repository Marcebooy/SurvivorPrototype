# Asepohjat ja kiinnityspistepilotti

Uusin viimeistely: `STYLIZED_UPDATE.md`. Viidelle lisäaseelle on nyt pehmeämmät,
yksityiskohtaisemmat versiot samoilla prefab-avaimilla. Ajantasainen esikatselu on
`Library/StylizedFive_preview.png`; aiempi MissingFive_preview näyttää vanhan tyylin.

Päivitys 10.9.2026: kirjasto sisältää nyt **26 mallia**. Dexecutioner, Hero Sword,
Wireless Dagger, Dragon's Breath ja Flamewalker on lisätty. Katso
`MISSING_FIVE_HANDOFF.md`: täsmälliset enum-/Resources-avaimet, rakenne ja testit.
Muokattava WeaponBases.blend ja catalog.json sisältävät kaikki 26; uudet viisi
prefabia ovat Coden käyttämässä Resources-kansiossa. Alla oleva 9.9. toimituskuvaus
ja 21 mallin galleria kuvaavat ensimmäistä vaihetta. Uusilla viidellä on oma
MissingFiveGallery.unity ja esikatselukuva. Nykyinen FBX-tarkistus kattaa 26 mallia
ja 25 168 kolmiota. Peliaikainen kiinnitysjärjestelmä on sittemmin Coden toteuttama.

Toimitus 9.9.2026. Kuusi pohjaperhettä ja pieni osakitti: yhteensä 21 muokattavaa
assettiversiota. Kiinnityspistepilotti kattaa 9 hahmoa × 4 pistettä = 36 pistettä.
Tämä on assettitoimitus; pelin WeaponAttachmentPoint-integraatio tehdään erikseen
WeaponStats-pipelinen valmistuttua. Kaksoiskajo ja Kalman kaari säilyvät erillisinä.

## Aloita näistä

- `Attachments/CONVENTIONS.md`: yksiköt, akselit, lepoasennot ja skaala.
- `Attachments/CHARACTER_SOCKET_TABLE.md`: hahmokohtaiset mitat ja suunnat.
- `Attachments/attachment_profiles.json`: ajuripolut, paikalliset kvaterniot (xyzw), offsetit ja skaalat.
- `Attachments/CODE_HANDOFF.md`: Codelle annettava myöhempi integraatiotehtävä.
- `Attachments/SocketProbe.blend` ja `.fbx`: muokattava mittakappale.
- `Library/WeaponBases.blend`: kaikkien pohjien muokattavat osat omissa kokoelmissaan.
- `Library/*.fbx`, `Library/Textures/`: 21 mallia ja 30 yhteistä 512 × 512 PNG-karttaa.
- `Library/catalog.json`: mallikohtaiset kiinnityspisteet, materiaalit ja laatutason lähtöarvot.

## Mallinnusjärjestys ja sisältö

| Vaihe | Yhteinen pohja | Mukana olevat versiot |
|---|---|---|
| 1 | Yksikätinen teräase | Katana, CorruptedSword |
| 2 | Sauva | Lightning, Firestaff |
| 3 | Ampuma-ase | Revolver, Shotgun, Sniper, Rocket |
| 4 | Kelluva reliikki | Chunkers, ArcaneFocus, Dice |
| 4b | Pieni osakitti | Bone, PoisonFlask, Mine, Bananarang, ThrowingAxe, QualityRune |
| 5 | Jousi | Recurve |
| 6 | Kaksikätinen varsiase | Scythe, WarAxe, Hammer |

Versiot jakavat perheensä rungon/kahvan ja materiaalipaletin. Uusi väri tehdään
materiaalivaihdolla; pieni siluettimuutos vaihtamalla terää, päätä, piippua tai
koristeosaa. Näin seuraava variantti ei tarvitse omaa alusta rakennettua projektia.
Luettelo ei väitä kaikkien 30 aseen VFX:n tai pelikäytöksen olevan toteutettu.

## Käyttö Unityssä

Assetit ovat jo nykyisessä projektissa hakemistossa `Assets/WeaponArtPilot/`.
Avaa `Attachments/AttachmentGallery.unity` kiinnityspisteiden tarkasteluun tai
`Library/WeaponBaseGallery.unity` aseiden vertailuun. Pelikohtauksiin ei lisätty aseita.

Käytä `Library/Prefabs/`-prefabia: sen juuri on neutraali ja `ImportedModel`-lapsi
säilyttää FBX:n akselikorjauksen. `Mount_Grip`, `Mount_Stow` ja perheen muut Mount-
pisteet ovat Unity-metreissä. Älä nollaa ImportedModel-lapsen tuontirotaatiota.
Teräaseen kärki on +Y, etupuoli +Z; ampuma-aseen piippu ja nuolen suunta ovat +Z.
Ote on origossa. Jäykät aseet seuraavat socketia; ne eivät tarvitse skinnausta.

`WeaponArtPilot.unitypackage` sisältää tämän eristetyn assettihakemiston ja metat.
Se ei sisällä pelin alkuperäisiä hahmoja: AttachmentGallery vaatii tämän projektin
hahmoassetit samoilla GUID-tunnisteilla. Asekirjasto ja sen galleria sisältävät
omat mallit, tekstuurit ja URP/Lit-materiaalit. Kohdeprojekti tarvitsee URP:n.

BaseColor ja Emission ovat sRGB-karttoja. MetallicSmoothness on lineaarinen:
R = metallic, A = smoothness. Yhteiset palettitekstuurit ovat tarkoituksella
yksinkertaiset; tämä toimitus ei sisällä uniikkeja maalattuja normal-karttoja.
Materiaalien hillitty hehku näkyy bloomina vain, jos kohtauksen jälkikäsittely sallii sen.

Laatutason 1–3 tähden emission-kertoimet 0,65 / 1,0 / 1,4 ovat catalogin presettejä.
Lisäriimu ja pulssi ovat integraation ohjeita; automaattista tähtien vaihtoa tai
partikkelijärjestelmää ei ole kytketty. `Kit_QualityRune` tarjoaa lisäkoristeen meshin.

## Muokkaaminen ja vienti

Blender-lähteessä Katana näkyy oletuksena. Avaa haluttu kokoelma Outlinerista ja
vaihda sen viewport-/render-näkyvyys. Osat ovat erillisiä muokkaamista varten;
staattisissa FBX-vienneissä osat on yhdistetty materiaaleittain. Kokoelmien origo
on sama tarkoituksella, joten älä näytä kaikkia päällekkäin.

Jousessa on neljän luun rigi ja `Bow_DrawRelease`-klippi (1–31 ruutua, 30 fps),
jonka vetomatka on 0,24 m. Hahmon käsien IK ja laukaisun ajoitus ovat myöhempää
integraatiota. Muiden aseiden liikkuvia lukkoja, rekyyliä tai latausta ei ole animoitu.

Rakennusskriptit: `assetlib.py`, `build_families.py`, `Attachments/build_probe.py`.
Ne ajetaan Blenderin Pythonilla; tarkista skriptien työtilapolut ennen käyttöä
toisella koneella. `verify_library.py` tarkistaa viennit uudelleentuonnilla.
`import_library.cs` ja `Attachments/build_unity_gallery.cs` ovat Unity CLI:n
eval_file-runkokoodeja, eivät Assets-kansioon lisättäviä MonoBehaviour-skriptejä.

## Varmennus ja jäljellä oleva sovitus

FBX-uudelleentuonti läpäisi 21 mallin UV-, geometria-, mitta- ja socket-tarkistukset:
yhteensä 17 988 kolmiota. Jousen vetoliike varmennettiin uudelleentuodusta rigistä.
Unity-tuonnissa tarkistettiin 21 neutraalia prefab-juurta, materiaalit ja metriset
rajat. Raportit: `Library/verification.json`, `Library/unity_import_verification.json`.

Kaikkien 36 pisteen bind-sijainnit täsmäsivät 0,5 mm toleranssilla; skaalakompensointi
ja 0,6 m kärkipiste tarkistettiin. Raportti ja yhdeksän hahmon etu-/takakuvat ovat
`Attachments/Previews/`-kansiossa. Kahdeksan on oikeita rigattuja hahmoja;
Pottu on nykyisten mittojen mukainen geometrinen referenssi. Tarkistushetkellä
Unity-konsolin virhekysely palautti 0 virhettä.

Tärkeät jatkot: nykyisten FBX-luiden nimet ovat anatomiseen käteen nähden vaihtuneet,
luuskaala on 100 ja Pottun squash/stretch vaatii jäykän seuraajan. Ne on eritelty
konventioissa. Kaikkien hyökkäysanimaatioiden läpileikkaukset, vanhojen asemeshien
poisto, tukikäden IK ja pitkien aseiden säilytysoffsetit testataan integraatiossa.
Pilotti todentaa neutraalin kiinnityssopimuksen, ei valmista peliaikaista järjestelmää.
