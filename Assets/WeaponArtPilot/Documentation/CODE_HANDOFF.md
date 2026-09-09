# Codelle: myöhempi WeaponAttachmentPoint-tehtävä

ALOITUSEHTO: WeaponStats-pipeline on ensin kokonaan valmis. Tämä dokumentti
ei käynnistä refaktorointia eikä ole viesti toiselle tehtävälle.

Toteuta yksi WeaponAttachmentPoint-järjestelmä käyttäen tämän hakemiston
CONVENTIONS.md:tä ja attachment_profiles.json:ia. Korvaa vasta silloin
SwordVariantVisual.cs:n likimääräinen vartalo-offset oikealla socketilla.

1. Lue profiilit ja ratkaise täsmälliset ajuripolut hahmon mallijuuren alta.
   Puuttuva ajuri on virhe; älä sijoita asetta hiljaisesti kohtaan (0,0,0).
2. Seuraa oikeaa luuta ja normalisoi 100-kertainen FBX-luuskaala. Pottulle
   tarvitaan erillinen, pannun näkyvyydestä ja Size-skaalasta riippumaton seuraaja.
3. Säilytä raakamuodon ImportedModel-tuontirotaatio neutraalin WeaponVisual-juuren alla.
   Kirjaston Unity-prefabit toteuttavat tämän jo assettitasolla.
4. Sovita animaation hyökkäyspuoli anatomiseen käteen: nykyisten `.R`-luun klippien
   puoli ei vastaa Unityn +X:ää. Älä riko nykyisiä Legacy-klippipolkuja luunimien muutoksilla.
5. Piilota/irrota vanhat hahmoon leivotut asemesh-osat näyttämättä kahta asetta.
   Kokonaista Renderer-komponenttia ei saa piilottaa, jos se sisältää myös kättä.
6. Pidä aseiden käytös, statsit ja vahinkoalueet erillään visuaalisesta skaalasta.
   Kaksikätisen aseen SupportGrip ohjaa tukikäden IK:ta, ei kaksoisparentointia.
7. Säilytä Kaksoiskajo sekä Kalman kaari omine mesh-/materiaalikokonaisuuksineen.
   Ne noudattavat samaa kiinnityssopimusta, mutta eivät ole pohjien värivariantteja.
8. Laatutason 1–3 tähteä ohjaavat samoja meshejä ja hillittyjä FX-preset-arvoja;
   mukana oleva catalog.json kuvaa assettipresetit, ei vielä ajettavaa pelilogiikkaa.

Hyväksymistestit: 9 hahmoa × kädet/selkä/vyö, liikkuminen ja roll, perheen
hyökkäys-/tähtäysanimaatio, Pottun Size 1–3, positiivinen yhtenäinen skaala,
ei vanhaa ja uutta asetta päällekkäin, ei tähtien muuttamaa kahvakokoa.
Vertaile gallerian 50 cm testilevyyn ja lähdetiedoston metreihin.

Tähän assettityöhön kuuluvat vain WeaponBases/ ja Assets/WeaponArtPilot/.
WeaponStats-, SurvivorGame- ja SwordVariantVisual-tiedostoihin ei ole koskettu.
