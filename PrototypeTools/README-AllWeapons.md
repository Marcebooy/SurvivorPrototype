# Potun 30 aseen prototyyppivalikoima

Kaikki AGENTS-koonnin 30 asetta on toteutettu pelattavina prototyyppeinä. Sword on Potun Pannu. Päävalikon PELAA avaa 10-sivuisen asekaapin, kolme asetta per sivu. Kaikki aseet voi valita aloitusaseeksi ja saada satunnaisista tasopäivityksistä tai arkuista. Avausehtoja ei vielä ole.

## Lisätyt 21 asetta

| Ase | Toteutettu toiminta |
| --- | --- |
| Revolver | Kolmen luodin sarja + Quantity-lisät. Luodit kimpoavat aiemmin osumattomiin kohteisiin. |
| Aegis | Automaattisesti latautuva yhden osuman kilpi. Torjunta aiheuttaa aluevastaiskun. Cooldown nopeuttaa latausta. |
| Bananarang | Ulos lentävä ja pelaajaan palaava läpäisevä ammus; yksi osuma per kohde kumpaankin suuntaan. |
| Axe | Hidas pyörivä kirves läpäisee vihollisia. Quantity lisää kirveitä. |
| Space Noodle | Määräaikainen linkki lähimpään kohteeseen, toistuva vahinko linkin varrella. Katkeaa kohteen kuollessa tai karatessa. |
| Sniper Rifle | Hidas, voimakas 45 metrin läpäisevä osumalinja. |
| Slutty Rocket | Hakeutuva raketti ja aluevahinko. |
| Mines | Paikalle jätettävä miina virittyy 0,4 sekunnissa, räjähtää vihollisen lähestyessä ja vanhenee. |
| Wireless Dagger | Hakeutuva tikari kimpoaa kohteiden välillä. |
| Frostwalker | Jääalue tekee vahinkoa, hidastaa ja jäädyttää tavalliset viholliset lyhyesti. Bossi vain hidastuu. |
| Tornado | Eteenpäin liikkuva pyörre aiheuttaa toistuvaa vahinkoa ja työntää vihollisia. |
| Dexecutioner | Lähiterä ja tasosta riippuva teloitusmahdollisuus. Elitet ja bossit eivät ole teloitettavissa. |
| Blood Magic | Lyhyen kantaman verenimuisku parantaa 1 HP. Joka kymmenes tappo asetta omistaessa kasvattaa kierroksen Max HP:tä yhdellä (katto +100). |
| Black Hole | Määräaikainen vetävä vahinkoalue. Bossiin 20 % normaalista vetovoimasta. |
| Poison Flask | Myrkkylammikko lähimmän vihollisen kohdalle; tekee toistuvaa vahinkoa. Heiton lentorataa ei vielä animoida. |
| Katana | Nopeat automaattiset lähiosumat. Quantity lisää erillisiä kohteita. |
| Dragon's Breath | Suunnattu tulikartio; selän takana olevat viholliset jäävät ulkopuolelle. |
| Dice | Vahinkokerroin nopan 1–6 tuloksen mukaan. Kuutonen lisää crit-mahdollisuutta 1 prosenttiyksikön, enintään 70 %. |
| Hero Sword | Lähisivallus ja läpäisevät etäviillot. |
| Corrupted Sword | Lähisivallus voimistuu puuttuvan HP:n mukaan, enintään 3-kertaiseksi. |
| Scythe | 360 asteen isku. Kolme elite-tappoa lataa 3-kertaisen iskun; lataus kulutetaan. |

## Eteneminen ja rajaukset

- Aseen taso kasvattaa sen vahinkoa. Omistetun aseen uudelleenvalinta noudattaa nykyistä taattua +15 % Damage/Size-bonussääntöä.
- Geneeristen Tomejen aiemmat 1 % bonukset säilyvät. Duration vaikuttaa myös uusiin määräaikaisiin alueisiin, linkkeihin, miinoihin ja erikoisammuksiin; Projectile Speed liikkuviin erikoisammuksiin ja tornadoon.
- Blood Magicin HP-kasvu kestää tämän kierroksen ja alueenvaihdot, mutta ei tallennu seuraaville kierroksille. Silverillä ostettava pysyvä HP on erillinen järjestelmä.
- HUD näyttää Aegisin valmiuden, viikatteen latauksen, Blood Magicin lisä-HP:n ja viimeisen nopan tuloksen. Suuresta buildista näytetään viisi asetta ja loppujen määrä.
- Uusilla aseilla on yhteinen 200 aktiivisen erikoisammuksen, 48 alueen ja 160 uusien lyhyiden osumavisuaalien raja. Vanhat erikoisalueet poistuvat aluekaton ylittyessä. Rajat voivat vähentää ääritilanteen visuaaleja tai ammusmäärää.
- Mallit ja efektit ovat proseduraalisia prototyyppimuotoja. Kullakin aseella ei vielä ole viimeisteltyä kädessä pidettävää mallia, omaa animaatiota tai ääntä. Arvot ovat tämän pelin omia, eivät MegaBonkin tarkkoja arvoja.

## Koodi ja varmennus

Uusi logiikka ja asekuvaukset: `Assets/Survivor/Scripts/SurvivorAdvancedWeapons.cs`. Integraatiot: SurvivorArsenal, SurvivorProgression, SurvivorGame ja SurvivorPottu. Aiemmat enum- ja upgrade-ID:t säilyvät.

`PrototypeTools/VerifyCompleteWeapons.cs`: aloitusvalinnat, uusiin aseisiin johtavat päivitykset, kaikki 21 erikoistoimintoa, torjunta, teloituksen suojaukset, paluuosuma, viikatteen lataus, Blood Magicin eteneminen ja efektien siivous. `VerifyCurrentRules.cs`: nykyiset Tomet ja bossisäännöt sekä 10 simuloitua sekuntia kaikkien 30 aseen ja Quantity 6:n kanssa. Testit ajetaan Unity CLI eval_file -komennolla Play-tilassa ja nollaavat testikierroksen. Ne eivät julkaise peliä tai osta pysyviä parannuksia.

Pitkien kierrosten tasapaino ja suorituskyky tavallisessa peliajossa vaativat vielä pelitestausta. Tässä muutoksessa ei tehty uutta build-julkaisua.
