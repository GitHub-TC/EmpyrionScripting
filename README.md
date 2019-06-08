# Empyrion Scripting
## Installation
1. Downloade die ZIP Datei von https://github.com/GitHub-TC/EmpyrionScripting/releases
1. UnZip die Datei in dem Verzeichnis Content\\Mods directory

NOTE: Dies ist eine SEHR frühe Version der LCD A10 mod mit der neuen ModAPI !!! beide sind noch vielen Änderungen unterworfen!!!

#### Installation für SinglePlayer
1. Downloade die ZIP Datei von https://github.com/GitHub-TC/EmpyrionScripting/releases
1. UnZip die Datei in dem Verzeichnis Content\\Mods directory
1. Das Spiel MUSS dann ohne EAC gestartet werden damit die Mods geladen werden

### Wofür dient diese MOD?
![](Screenshots/DemoShipScreen.png)

Echte Spielinhalte direkt auf einem LCD ausgeben

Eine dem Struktur 'LCDInfo-Demo' findest du im workshop
https://steamcommunity.com/workshop/filedetails/?id=1751409371

#### Hilfe
![](Screenshots/RedAlert.png)
![](Screenshots/LCD1.png)

YouTube video;
* https://www.youtube.com/watch?v=8nEpEygHBu8
* https://youtu.be/XzYKNevK0bs
* https://youtu.be/SOnZ_mzytA4
* https://youtu.be/oDOSbllwqSw

## Beispiele
Allgemein: 
Benötigt werden mindestens 2 LCD und mindestens 1 Container
1. LCD 1 (Eingabe) wird mit der Abfrage programmiert siehe Beispiele unten
1. LCD 2 (Ausgabe) Muss eindeutigen Namen haben z.B. "LCD Alle Erze"
1. Jeder Kontainer der eine Information ausgeben soll, muss einen eindeutigen Namen haben

Wenn ihr die Produktename auf deutsch haben wollt, dann anstatt {{Name}} {{i18 Key 'Deutsch'}}

Unten stehen die ID Nummer für Erze und Barren.<br/>
Einige Funktionen benötigen ein Komma"," andere benötigen Simikolon ";".<br/>
Alles in "" sind Texte und nicht mit anzugeben.<br/>
Einzelne ' sind mit anzugeben.<br/>
Man kann eine Information auch auf 2 LCD's anzeigen lassen dann bei LCDInfo:"Name LCD";"Name CD2"<br/>
Man kann auf einem LCD auch den Inhalt verschiedner Kisten anzeigen lassen!<br/>
 
 ---
## Was ist in der Kiste/Container/ContainerController/MunitionsKiste/Kühlschrank

Eingabe im LCD 1 (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT Optional"
{{items E '"Name der Kiste"'}}
{{Count}}{{Name}}
{{/items}}
```
Bsp:
```
LCDInfo:LCD Alle Erze
Meine Erze
{{#items E 'Alle Erze'}}
{{Count}}{{i18 Key 'Deutsch'}}
{{/items}}
```
---
## Ausgabe aller Erze in der Basis/Schiff/HV/CV

Eingabe im LCD (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#test ID in '2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
LCDInfo:LCD Alle Erze
Meine Erze
{{#each S.Items}}
{{#test Id in '2248,2249,2250,2251,2252,2253,2269,2270,2284,2297,2280,2254'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
---
## Ausgabe aller Barren in der Basis/Schiff/HV/CV

Eingabe im LCD (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#each S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
LCDInfo:LCD Barren
Alle meine Barren in der Basis:
{{#each S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
-----------------------------------------------------------------------------------------
## Ausgabe dieser per ID festgelegten Produkte (hier sind es alle Barren die es gibt im Spiel)
Eingabe im LCD (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD Alle Barren im Spiel
Alle Barren im Spiel:
{{#itemlist S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
-----------------------------------------------------
## Anzeige eines bestimmten Produktes in der Basis/Schiff/HV/CV
```
Eingabe im LCD (alles ohne "")
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist S.Items '2249'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD EISEN ERZ
Meine EisenErz und Barren
{{#itemlist S.Items '2249;2272'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
------------------------------------------------------------------
## Welche Erze sind alle, bzw. nur noch X Anzahl über

Hier werden alle Erze angezeigt wo nur 1-1000 auf der Basis vorhanden ist.
```
{{#itemlist S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
{{#test Count geq 1}}
{{#test Count leq 1000}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/test}}
{{/itemlist}}
```
---
## Hier werden alle Erze angezeigt die nicht mehr auf der Basis sind
```
{#itemlist S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
{{#test Count leq 0}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/itemlist}}
```
-----------------------------------------------------
## Welcher Spieler ist auf der Basis/Schiff gerade aktiv

Eingabe im LCD (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"Eingabe im LCD (alles ohne "")
"TEXT optional"
{{#each P.Player}}
 "-" {{Name}}
{{/each}}
```
Bsp.
```
LCDInfo:LCD Info W1
Player:
{{#each P.Player}}
 - {{Name}}
{{/each}}
```
------------------------------------------------------
## Datum und Uhrzeit anzeigen lassen

Eingabe im LCD (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"Eingabe im LCD (alles ohne "")
"TEXT optional"
{{datetime}}

{{datetime 'HH:mm'}}

{{datetime 'dd MMM HH:mm:ss' '+7'}}
```
Bsp.
```
LCDInfo:LCD UHRZEIT
Wie spät ist es?
{{datetime}}

{{datetime 'HH:mm'}}

{{datetime 'dd MMM HH:mm:ss' '+7'}}
```
----------------------------------------------------
## SCROLLEN:
Wenn zu viele Produkte nicht angzeigt werden können, dann kann man auch Scrollen
Hier werden 5 Produkte angezeigt mit 2 Sekunden Scrollgeschwindigkeit, wenn mehr als 5 Items zur Verfügung stehen. 
```
{{#scroll 5 2}}
{{#items E '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
```
Bsp.
```
{{#scroll 5 2}}
{{#items E 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}

{{#scroll 10 1}}
{{#each S.Items}}
 - [{{Id}}]:{{Name}}
{{/each}}
{{/scroll}}
```
----------------------------------------------------
## Intervalle:
Es kann alles in Intervallen angezeigt werden. Hier im Beispiel wäre es ein Pfeil
Man kann auch den Inhalt von 2 Kisten anzeigen lassen
```
{{#intervall 1}}
= = = = = = = = = = = = = = = = >
{{else}}
 = = = = = = = = = = = = = = = =>
{{/intervall}}
```
oder hier sind sind 2 Kisten die abwechselnd angezeigt werden.
```
{{#intervall 2}}
"Text optional"
{{#items E '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}
"Text optional"
{{#items E '"Name der Kiste2"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{/intervall}}
```
Bsp.
```
{{#intervall 2}}

Kühlschrank 1:

{{#items E 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}

Kühlschrank 2:

{{#items E 'Kühlschrank 2'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{/intervall}}
```
----------------------------------------------------
## Farbe Schrift und Hintergrund, Schriftgrösse und Intervall
Im folgendem Beispiel wechselt alle 5 Sekunden das Wort "Hallo" und "Welt"
dann wechselt auch alle 5 Sekunden die Schriftgrösse
Es wechselt jede Sekunde die Schriftfarbe und jede Sekunde der Hintergrund
```
{{#intervall 5}}
Hallo
{{else}}
Welt
{{/intervall}}

{{#intervall 5}}
{{fontsize 8}}
{{else}}
{{fontsize 15}}
{{/intervall}}

{{#intervall 1}}
{{color 'ff0000'}}
{{else}}
{{color '00ff00'}}
{{/intervall}}

{{#intervall 1}}
{{bgcolor 'ffff00'}}
{{else}}
{{bgcolor '000000'}}
{{/intervall}}
```
----------------------------------------------------
## ERZE und BARREN IDENTIFIKATIONS NUMMER:

+ Item Id: 2248, Name: Magnesium Erz
+ Item Id: 2249, Name: Eisen Erz
+ Item Id: 2250, Name: Kobalt Erz
+ Item Id: 2251, Name: Silizium Erz
+ Item Id: 2252, Name: Neodymium Erz
+ Item Id: 2253, Name: Kupfer Erz
+ Item Id: 2254, Name: Promethium
+ Item Id: 2269, Name: Erestrum Erz
+ Item Id: 2270, Name: Zascosium Erz
+ Item Id: 2284, Name: Sathium Erz
+ Item Id: 2293, Name: Pentaxid Erz
+ Item Id: 2297, Name: Gold Erz

---

+ Item Id: 2271, Name: Magnesiumpulver
+ Item Id: 2272, Name: Eisen Barren
+ Item Id: 2273, Name: Kobalt Barren
+ Item Id: 2274, Name: Silizium Barren
+ Item Id: 2275, Name: Neodymium Barren
+ Item Id: 2276, Name: Kupfer Barren
+ Item Id: 2277, Name: Promethium Pallets
+ Item Id: 2278, Name: Erestrum Barren
+ Item Id: 2279, Name: Zascosium Barren
+ Item Id: 2280, Name: Stein
+ Item Id: 2281, Name: Steinstaub
+ Item Id: 2285, Name: Sathium Barren
+ Item Id: 2294, Name: Pentaxid Kristalle
+ Item Id: 2298, Name: Gold Barren

---

+ Item Id: 2248, Name: MagnesiumOre
+ Item Id: 2249, Name: IronOre
+ Item Id: 2250, Name: CobaltOre
+ Item Id: 2251, Name: SiliconOre
+ Item Id: 2252, Name: NeodymiumOre
+ Item Id: 2253, Name: CopperOre
+ Item Id: 2254, Name: PromethiumOre
+ Item Id: 2269, Name: ErestrumOre
+ Item Id: 2270, Name: ZascosiumOre
+ Item Id: 2284, Name: SathiumOre
+ Item Id: 2293, Name: PentaxidOre
+ Item Id: 2297, Name: GoldOre

---

+ Item Id: 2271, Name: MagnesiumPowder
+ Item Id: 2272, Name: IronIngot
+ Item Id: 2273, Name: CobaltIngot
+ Item Id: 2274, Name: SiliconIngot
+ Item Id: 2275, Name: NeodymiumIngot
+ Item Id: 2276, Name: CopperIngot
+ Item Id: 2277, Name: PromethiumPellets
+ Item Id: 2278, Name: ErestrumIngot
+ Item Id: 2279, Name: ZascosiumIngot
+ Item Id: 2280, Name: CrushedStone
+ Item Id: 2281, Name: RockDust
+ Item Id: 2285, Name: SathiumIngot
+ Item Id: 2294, Name: PentaxidCrystal
+ Item Id: 2298, Name: GoldIngot

---
# Technical
Syntaxdocu:
+ http://handlebarsjs.com/
+ https://github.com/rexm/Handlebars.Net

### CustomHelpers (test)
* {{#test Select Op Value}}
  * Op: eq is =
  * Op: leq is <=
  * Op: le is <>
  * Op: geq is >=
  * Op: ge is >
  * Op: in 
    * Value: '1,2,3,42'
    * Value: '1-3,42'
    * Value: 'A,xyz,mag'

### CustomHelpers (intervall)
* {{#intervall sec}}
  * intervall in seconds

### CustomHelpers (scroll)
* {{#scroll lines delay}}
  * Text scroll block with (lines) od text, scrolls with (delay) seconds

### CustomHelpers (itemlist)
* {{#itemlist list 'id1;id2;id3'}}
  * Itemlist the the selected items (ids) even if they don't in the list (list)

### CustomHelpers (i18n)
* {{#i18n Select 'Language'}}
  * Language: English,Deutsch,Français,Italiano,Spanish,...
    look at \[ESG\]\\Content\\Extras\\Localization.csv at the first line

### CustomHelpers (datetime)
+ {{datetime}} = Display the Datetime
+ {{datetime 'format'}} = uses the formatstring
+ {{datetime 'format' '+5'}} = adds N hours

DateTime format:
+ https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netframework-4.8#System_DateTime_ToString_System_String_

### Whats next?


ASTIC/TC

***

English-Version:

---

# Empyrion LCD Info
## Installation
1. Download the ZIP file from https://github.com/GitHub-TC/EmpyrionScripting/releases
1. unzip the file in the Content\\Mods directory

NOTE: This is an EARLY A10 mod with the new ModAPI !!! both are heavily WorkInProgress !!!

### Whats for?
![](Screenshots/DemoShipScreen.png)

Displays various informations directly and live on LCD screens.

Get the demo structure 'LCDInfo-Demo' from the workshop
https://steamcommunity.com/workshop/filedetails/?id=1751409371

#### Help
![](Screenshots/RedAlert.png)
![](Screenshots/LCD1.png)

YouTube video;
* https://www.youtube.com/watch?v=8nEpEygHBu8
* https://youtu.be/XzYKNevK0bs
* https://youtu.be/SOnZ_mzytA4
* https://youtu.be/oDOSbllwqSw

## Examples
General:
At least 2 LCDs and at least 1 container are required
1. LCD 1 (input) is programmed with the query see examples below
1. LCD 2 (output) Must have unique name, e.g. "LCD All ores"
1. Each container that is to output information must have a unique name

If you want the product name in German, then instead of {{Name}} {{i18 Key 'Deutsch'}}

Below is the ID number for ores and ingots.<br/>
Some functions require a comma "," others require a simcard ";".<br/>
Everything in "" are texts and not to be specified.<br/>
Individuals are to be indicated.<br/>
One can also display an information on 2 LCD's then at LCDInfo: "Name LCD"; "Name LCD2"<br/>
You can also display the content of various boxes on an LCD!<br/>

---
## What's in the box / container / container controller / ammo box / refrigerator

Eingabe im LCD 1 (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT Optional"
{{items E '"Name der Kiste"'}}
{{Count}}{{Name}}
{{/items}}
```
Bsp:
```
LCDInfo:LCD Alle Erze
Meine Erze
{{#items E 'Alle Erze'}}
{{Count}}{{i18 Key 'Deutsch'}}
{{/items}}
```
---
## Output of all ores in the base / ship / HV / CV

Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#test ID in '2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
LCDInfo:LCD Alle Erze
Meine Erze
{{#each S.Items}}
{{#test Id in '2248,2249,2250,2251,2252,2253,2269,2270,2284,2297,2280,2254'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
---
## Output of all bars in the base / ship / HV / CV

Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#each S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
LCDInfo:LCD Barren
Alle meine Barren in der Basis:
{{#each S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
-----------------------------------------------------------------------------------------
## Output of these ID-defined products (here are all ingots in the game)
Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD Alle Barren im Spiel
Alle Barren im Spiel:
{{#itemlist S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
-----------------------------------------------------
## Display of a specific product in the base / ship / HV / CV

Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist S.Items '2249'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD EISEN ERZ
Meine EisenErz und Barren
{{#itemlist S.Items '2249;2272'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
------------------------------------------------------------------
## Which ores are all, or only X number over

Here all ores are displayed where only 1-1000 exists on the basis.
```
{{#itemlist S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
{{#test Count geq 1}}
{{#test Count leq 1000}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/test}}
{{/itemlist}}
```
---
## Here all ores are displayed that are no longer based
```
{#itemlist S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
{{#test Count leq 0}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/itemlist}}
```
-----------------------------------------------------
## Which player is currently active on the base / ship

Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"Input on the LCD (everything without "")
"TEXT optional"
{{#each P.Player}}
 "-" {{Name}}
{{/each}}
```
Bsp.
```
LCDInfo:LCD Info W1
Player:
{{#each P.Player}}
 - {{Name}}
{{/each}}
```
------------------------------------------------------
## Display date and time

Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"Input on the LCD (everything without "")
"TEXT optional"
{{datetime}}

{{datetime 'HH:mm'}}

{{datetime 'dd MMM HH:mm:ss' '+7'}}
```
Bsp.
```
LCDInfo:LCD UHRZEIT
Wie spät ist es?
{{datetime}}

{{datetime 'HH:mm'}}

{{datetime 'dd MMM HH:mm:ss' '+7'}}
```
----------------------------------------------------
## SCROLLEN:
If too many products can not be displayed, then you can also scroll
5 products are shown here with 2 seconds scrolling speed if more than 5 items are available.
```
{{#scroll 5 2}}
{{#items E '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
```
Bsp.
```
{{#scroll 5 2}}
{{#items E 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}

{{#scroll 10 1}}
{{#each S.Items}}
 - [{{Id}}]:{{Name}}
{{/each}}
{{/scroll}}
```
----------------------------------------------------
## Intervalle:
Everything can be displayed at intervals. Here in the example it would be an arrow
You can also display the contents of 2 boxes
```
{{#intervall 1}}
= = = = = = = = = = = = = = = = >
{{else}}
 = = = = = = = = = = = = = = = =>
{{/intervall}}
```
or here are 2 boxes that are displayed alternately.
```
{{#intervall 2}}
"Text optional"
{{#items E '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}
"Text optional"
{{#items E '"Name der Kiste2"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{/intervall}}
```
Bsp.
```
{{#intervall 2}}

Kühlschrank 1:

{{#items E 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}

Kühlschrank 2:

{{#items E 'Kühlschrank 2'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{/intervall}}
```
----------------------------------------------------
## Color font and background, font size and interval
In the following example, the word "Hello" and "World" changes every 5 seconds.
then the font size also changes every 5 seconds
The font color changes every second and the background every second
```
{{#intervall 5}}
Hallo
{{else}}
Welt
{{/intervall}}

{{#intervall 5}}
{{fontsize 8}}
{{else}}
{{fontsize 15}}
{{/intervall}}

{{#intervall 1}}
{{color 'ff0000'}}
{{else}}
{{color '00ff00'}}
{{/intervall}}

{{#intervall 1}}
{{bgcolor 'ffff00'}}
{{else}}
{{bgcolor '000000'}}
{{/intervall}}
```
----------------------------------------------------
## ERZE und BARREN IDENTIFIKATIONS NUMMER:

+ Item Id: 2248, Name: Magnesium Erz
+ Item Id: 2249, Name: Eisen Erz
+ Item Id: 2250, Name: Kobalt Erz
+ Item Id: 2251, Name: Silizium Erz
+ Item Id: 2252, Name: Neodymium Erz
+ Item Id: 2253, Name: Kupfer Erz
+ Item Id: 2254, Name: Promethium
+ Item Id: 2269, Name: Erestrum Erz
+ Item Id: 2270, Name: Zascosium Erz
+ Item Id: 2284, Name: Sathium Erz
+ Item Id: 2293, Name: Pentaxid Erz
+ Item Id: 2297, Name: Gold Erz

---

+ Item Id: 2271, Name: Magnesiumpulver
+ Item Id: 2272, Name: Eisen Barren
+ Item Id: 2273, Name: Kobalt Barren
+ Item Id: 2274, Name: Silizium Barren
+ Item Id: 2275, Name: Neodymium Barren
+ Item Id: 2276, Name: Kupfer Barren
+ Item Id: 2277, Name: Promethium Pallets
+ Item Id: 2278, Name: Erestrum Barren
+ Item Id: 2279, Name: Zascosium Barren
+ Item Id: 2280, Name: Stein
+ Item Id: 2281, Name: Steinstaub
+ Item Id: 2285, Name: Sathium Barren
+ Item Id: 2294, Name: Pentaxid Kristalle
+ Item Id: 2298, Name: Gold Barren

---

+ Item Id: 2248, Name: MagnesiumOre
+ Item Id: 2249, Name: IronOre
+ Item Id: 2250, Name: CobaltOre
+ Item Id: 2251, Name: SiliconOre
+ Item Id: 2252, Name: NeodymiumOre
+ Item Id: 2253, Name: CopperOre
+ Item Id: 2254, Name: PromethiumOre
+ Item Id: 2269, Name: ErestrumOre
+ Item Id: 2270, Name: ZascosiumOre
+ Item Id: 2284, Name: SathiumOre
+ Item Id: 2293, Name: PentaxidOre
+ Item Id: 2297, Name: GoldOre

---

+ Item Id: 2271, Name: MagnesiumPowder
+ Item Id: 2272, Name: IronIngot
+ Item Id: 2273, Name: CobaltIngot
+ Item Id: 2274, Name: SiliconIngot
+ Item Id: 2275, Name: NeodymiumIngot
+ Item Id: 2276, Name: CopperIngot
+ Item Id: 2277, Name: PromethiumPellets
+ Item Id: 2278, Name: ErestrumIngot
+ Item Id: 2279, Name: ZascosiumIngot
+ Item Id: 2280, Name: CrushedStone
+ Item Id: 2281, Name: RockDust
+ Item Id: 2285, Name: SathiumIngot
+ Item Id: 2294, Name: PentaxidCrystal
+ Item Id: 2298, Name: GoldIngot

---
# Technical
Syntaxdocu:
+ http://handlebarsjs.com/
+ https://github.com/rexm/Handlebars.Net

### CustomHelpers (test)
* {{#test Select Op Value}}
  * Op: eq is =
  * Op: leq is <=
  * Op: le is <>
  * Op: geq is >=
  * Op: ge is >
  * Op: in 
    * Value: '1,2,3,42'
    * Value: '1-3,42'
    * Value: 'A,xyz,mag'

### CustomHelpers (intervall)
* {{#intervall sec}}
  * intervall in seconds

### CustomHelpers (scroll)
* {{#scroll lines delay}}
  * Text scroll block with (lines) od text, scrolls with (delay) seconds

### CustomHelpers (itemlist)
* {{#itemlist list 'id1;id2;id3'}}
  * Itemlist the the selected items (ids) even if they don't in the list (list)

### CustomHelpers (i18n)
* {{#i18n Select 'Language'}}
  * Language: English,Deutsch,Français,Italiano,Spanish,...
    look at \[ESG\]\\Content\\Extras\\Localization.csv at the first line

### CustomHelpers (datetime)
+ {{datetime}} = Display the Datetime
+ {{datetime 'format'}} = uses the formatstring
+ {{datetime 'format' '+5'}} = adds N hours

DateTime format:
+ https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netframework-4.8#System_DateTime_ToString_System_String_

### Whats next?


ASTIC/TC
