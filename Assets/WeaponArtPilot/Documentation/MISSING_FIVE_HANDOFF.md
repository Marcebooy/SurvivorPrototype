# Viiden puuttuneen aseen täydennys – 10.9.2026

Kaikki viisi ovat nyt lähteen ja Unity-projektin catalog.json:ssa. Uusilla riveillä
on myös weaponEnum, resource ja prefabPath, jotta kytkentä on yksiselitteinen.
Prefabit ovat `Assets/WeaponArtPilot/Library/Prefabs/Resources/`-kansiossa.

| Weapon enum | Resources.Load-avain | Perhe | Vaihdettu moduuli |
|---|---|---|---|
| Dexecutioner | Blade_Dexecutioner | OneHandBlade | Leveä viistetty teloitusterä, suora väistin, raskas pommeli |
| HeroSword | Blade_HeroSword | OneHandBlade | Symmetrinen suippoterä, kultainen siipiväistin, kristallipommeli |
| WirelessDagger | Blade_WirelessDagger | OneHandBlade | Lyhyt tikarinterä, pieni kaartuva väistin, kevyt pommeli |
| DragonBreath | Staff_DragonsBreath | Staff | Lohikäärmeen leukaa muistuttava haarukka, sarvet ja hiilloskristalli |
| Flamewalker | Relic_Flamewalker | FloatingRelic | Hiillosmalja ja liekkisiluetti yhteisessä reliikkikehyksessä |

Coden WeaponVisualDefs-taulukkoon lisättävät rivit:

```csharp
new WeaponVisualDef { weapon = Weapon.Dexecutioner, resource = "Blade_Dexecutioner" },
new WeaponVisualDef { weapon = Weapon.HeroSword, resource = "Blade_HeroSword" },
new WeaponVisualDef { weapon = Weapon.WirelessDagger, resource = "Blade_WirelessDagger" },
new WeaponVisualDef { weapon = Weapon.DragonBreath, resource = "Staff_DragonsBreath" },
new WeaponVisualDef { weapon = Weapon.Flamewalker, resource = "Relic_Flamewalker" },
```

Tässä toimituksessa taulukkoa ei muutettu. Prefabit toimivat nykyisen RightHand-
kiinnityksen kautta: neutraali juuri, Mount_Grip origossa, positiivinen skaala,
ImportedModel-lapsen FBX-korjaus säilytetty. Flamewalker on käden kohdalla kelluva
fokus; se ei vaadi uutta socketia tai orbitointilogiikkaa. Pelin maahan jättämä
tulijälki pysyy nykyisen hyökkäysjärjestelmän vastuulla. EffectOrigin sopii fokuksen
pieneen hiillos-VFX:ään, ei maahan jätettävien vahinkoalueiden spawn-pisteeksi.

DragonBreath käyttää samaa vartta ja kahvaa kuin Lightning/Firestaff. Muzzle on
fokuksen kärjessä; perheen +Y-pituusakseli säilyy. Nykyinen eteen suuntautuva
tulikartio seuraa pelin aim-suuntaa, eikä sen suuntaa pidä korvata sauvan lepoakselilla.

Uudet terät käyttävät täsmälleen yhteistä 22 cm nahkakahvaa ja tangenttirunkoa.
WirelessDaggerin pienempi terä ei skaalaa kahvaa. Materiaalit ja 1–3 tähden presetit
ovat alkuperäisestä paletista; uusia tekstuurisettejä ei tarvita.

Muokattavat kokoelmat: `Library/WeaponBases.blend`. Uusintarakennus:
aja alkuperäisen build_families.py:n jälkeen build_missing.py Blenderin Pythonilla.
build_missing.py päivittää vain nämä viisi kokoelmaa ja niiden FBX:t sekä katalogin.
Unity-tuonti: import_missing.cs (CLI eval_file); tämä säilyttää aiemmat prefabit
ja Coden tekemän Resources-siirron. Älä aja vanhaa import_library.cs:ää nykyisen
integraation päälle, sillä se käyttää ensimmäisen toimituksen prefab-polkuja.

Varmennettu: 26/26 FBX-uudelleentuontia (UV:t, geometria, mitat ja socketit),
5/5 uutta Unity-prefabia (materiaalit, metriset rajat, otteen origo ja Resources.Load).
Raportit Library/verification.json ja Library/missing_unity_verification.json.
Esikatselu: Library/MissingFive_preview.png, samassa järjestyksessä kuin taulukko.
Uusien aseiden peliaikainen näkyvyys testataan Coden taulukkokytkennän jälkeen.
