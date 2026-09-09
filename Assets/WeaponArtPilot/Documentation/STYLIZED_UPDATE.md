# Pehmeämpi tyylitelty viimeistely – viisi asetta

Päivittää Dexecutionerin, Hero Swordin, Wireless Daggerin, Dragon's Breathin ja
Flamewalkerin saman nimiset prefabit ja FBX:t. Blender-lähde on Library/WeaponBases.blend.
Kahvojen nimellismitat, socket-data, Resources-avaimet ja prefab-polut säilyvät.
Kooditaulukkoa tai muiden 21 aseen geometriaa/materiaaleja ei muutettu.

- Dexecutioner: epäsymmetrinen teloitusterä, metallinen leikkuureuna, hehkuva särö ja kaartuvat väistimen kynnet.
- Hero Sword: kaareva siipiväistin, hopeiset koristeet, kultaiset teräupotukset ja pyöristetty kristallipommeli.
- Wireless Dagger: kaareva teräprofiili, hehkuva kanava, kaartuvat väistimen päät ja leijuvat pienet kristallit.
- Dragon's Breath: pyöristetty lohikäärmeen pää, sarvet, hampaat, silmät, suu ja liekkifokus.
- Flamewalker: kaartuva metallikehys, pyöristetty hiillosmalja ja kolmiulotteiset liekkikielet.

Viisteissä on neljä segmenttiä, putkimuodoissa enemmän poikkileikkauksia ja
koristeissa kaarevaa geometriaa. Pintanormaalit on viimeistelty pehmeiksi
säilyttäen terien särmät. 7 Crafted-materiaalia ja 21 PNG-karttaa ovat vain näille
viidelle: hillittyjä sävyeroja ja pintavaihtelua, ei uniikkeja käsinmaalattuja normal-karttoja.

Kolmiomäärät löytyvät catalog.jsonista: noin 7 700–13 800 per ase ja noin 19 300
lohikäärmesauvassa. Aseet ovat jäykkiä socketiin kiinnitettäviä meshejä. Uusia LOD-
versioita tai mobiilin suorituskykybudjettia ei ole tässä toimituksessa testattu.

Uudelleenrakennus: stylize_missing.py Blenderin Pythonilla, sitten verify_library.py.
Unityssä aja import_crafted_materials.cs ja sen jälkeen import_missing.cs CLI:n
eval_file-komentoina. Pelkkä build_missing.py palauttaa viiden aseen vanhan tyylin.
PreviousBlockout sisältää ennen tätä muutosta talletetun Blender-lähteen ja katalogin.

Tarkistukset: kaikkien 26 FBX:n uudelleentuonti, UV:t, mitat ja socketit;
uusien viiden Resources.Load ja materiaalit. Rakentaja vertaa niiden socket-dataa
edeltävään katalogiin ja keskeyttää, jos se muuttuu. Katso verification.json ja
missing_unity_verification.json. StylizedFive_preview.png on Blender-renderöinti,
StylizedFive_Unity.png Unity-tuonnin tarkistuskuva. Pelin bloom ja valaistus voivat
muuttaa hehkun ulkonäköä. Pelin hyökkäyslogiikkaa ei muutettu.
