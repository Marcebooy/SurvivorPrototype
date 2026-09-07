# Potun asekaappi – 9 asetta

SurvivorPrototype → Play. Selaa aloitusaseita Edelliset / Seuraavat -painikkeilla. Kaikki aseet ovat myös satunnaisissa tasovalinnoissa ja arkuissa. Saman aseen valitseminen uudelleen nostaa sen tasoa.

| Ase | Toiminta ja päivitykset |
| --- | --- |
| Pannu | Leveä sivallus. Damage, Size, Quantity, Cooldown ja asetaso. |
| Jousi | Läpäisevät nuolet. Damage, Size, Quantity, Projectile Speed, Cooldown. |
| Salama | Ketjuttuvat osumat. Damage, Quantity ja Cooldown. |
| Kiertokivet | Kaksi kiveä kiertää pelaajaa ja tekee kosketusvahinkoa. Quantity lisää kiviä, Size kasvattaa kokoa ja rataa, Projectile Speed kiertonopeutta. |
| Tulijälki | Jättää paikalleen palavia läiskiä. Damage, Size, Duration ja Cooldown. Ei Quantity-vaikutusta. |
| Pomppuluu | Kimpoaa aiemmin osumattomaan viholliseen 10 metrin sisällä. Asetaso lisää kimpoiluja joka toisella tasolla. Quantity lisää luita. |
| Tulipallo | Räjähtää ensimmäisestä osumasta ja vahingoittaa lähellä olevia. Damage, Quantity, Size, Projectile Speed ja Cooldown. |
| Aura | Pelaajaa seuraava vihreä vahinkokehä. Damage, Size ja Cooldown. Quantity ei vaikuta. |
| Haulikko | Viiden haulin lyhyen kantaman viuhka, noin 9 metrin kantama. Quantity lisää hauleja. Damage, Size, Projectile Speed ja Cooldown. |

Uudet Duration- ja Projectile Speed -Tomet lisäävät kestoa 25 % ja nopeutta 20 %. Uusien aseiden asetaso kasvattaa vahinkoa 25 prosenttiyksikköä per taso. Aseiden perusvahingot ja hyökkäysvälit on tasapainotettu prototyyppiä varten; ne eivät kopioi MegaBonkin tarkkoja numeroita.

Uudet aseet käyttävät omia hyökkäysajastimia. Ammuksia on enintään 180 ja tuliläiskiä 80; rajat hillitsevät erittäin suurten buildien kuormaa. Tauko pysäyttää aseiden ajastimet, ja alueenvaihto tyhjentää vanhat efektit säilyttäen aseet ja Tomet. Uusi kierros nollaa aseet ja niiden parannukset.

Pääasiallinen uusi lähde: Assets/Survivor/Scripts/SurvivorArsenal.cs. PrototypeTools-lähteet ovat CLI-tuonnin välitiedostoja.

Varmennus Unity CLI:n Play-tilassa: VerifyArsenal.cs (29 tarkistusta), VerifyV2-Arsenal.cs (23), VerifyPottu.cs (14). Alkuperäisen VerifyV2.cs:n kolmen aloitusaseen raja on päivitetty yhdeksään VerifyV2-Arsenal.cs:ssä. Testit nollaavat testikierroksen. Pitkän kierroksen tasapaino ja aseiden lopulliset mallit ovat edelleen jatkokehitystä.
