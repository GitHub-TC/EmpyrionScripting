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
* https://youtu.be/8nEpEygHBu8 (danke an Olly :-) )
* https://youtu.be/XzYKNevK0bs
* https://youtu.be/SOnZ_mzytA4
* https://youtu.be/oDOSbllwqSw
* https://youtu.be/qhOnj2D3ejo

## Beispiele
Allgemein: 
Benötigt werden mindestens 2 LCD und mindestens 1 Container
1. LCD 1 (Eingabe) wird mit der Abfrage programmiert siehe Beispiele unten. Der Namen des LCDs im ControlPanel MUSS mit "Script:" beginnen.
1. LCD 2 (Ausgabe) Muss eindeutigen Namen haben z.B. "LCD Alle Erze"
1. Jeder Kontainer der eine Information ausgeben soll, muss einen eindeutigen Namen haben

Wenn ihr die Produktename auf deutsch haben wollt, dann anstatt {{Name}} {{i18 Key 'Deutsch'}}

Unten stehen die ID Nummer für Erze und Barren.<br/>
Einige Funktionen benötigen ein Komma"," andere benötigen Simikolon ";".<br/>
Alles in "" sind Texte und nicht mit anzugeben.<br/>
Einzelne ' sind mit anzugeben.<br/>
Man kann eine Information auch auf 2 LCD's anzeigen lassen dann bei Targets:Name LCD;Name LCD2<br/>
Man kann eine Information auch auf n LCD's anzeigen lassen dann bei Targets:LCDAusgabe*<br/>
Man kann eine Information auch auf n LCD's anzeigen lassen welche schon im ScriptLCD Namen angegeben sind Script:LCDAusgabe*<br/>
Man kann auf einem LCD auch den Inhalt verschiedner Kisten anzeigen lassen!<br/>
 
Eine Debugausgabe erhält man mit einem LCD das den Namen "ScriptDebugInfo" hat.

 ---
## Was ist in der Kiste/Container/ContainerController/MunitionsKiste/Kühlschrank

Eingabe im LCD 1 (alles ohne "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT Optional"
{{items E.S '"Name der Kiste"'}}
{{Count}}{{Name}}
{{/items}}
```
Bsp:
```
LCDInfo:LCD Alle Erze
Meine Erze
{{#items E.S 'Alle Erze'}}
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
{{#each E.S.Items}}
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
{{#each E.S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
LCDInfo:LCD Barren
Alle meine Barren in der Basis:
{{#each E.S.Items}}
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
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD Alle Barren im Spiel
Alle Barren im Spiel:
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
-----------------------------------------------------
## Anzeige eines bestimmten Produktes in der Basis/Schiff/HV/CV
```
Eingabe im LCD (alles ohne "")
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist E.S.Items '2249'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD EISEN ERZ
Meine EisenErz und Barren
{{#itemlist E.S.Items '2249;2272'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
------------------------------------------------------------------
## Welche Erze sind alle, bzw. nur noch X Anzahl über

Hier werden alle Erze angezeigt wo nur 1-1000 auf der Basis vorhanden ist.
```
{{#itemlist E.S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
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
{#itemlist E.S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
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
{{#items E.S '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
```
Bsp.
```
{{#scroll 5 2}}
{{#items E.S 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}

{{#scroll 10 1}}
{{#each E.S.Items}}
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
{{#items E.S '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}
"Text optional"
{{#items E.S '"Name der Kiste2"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{/intervall}}
```
Bsp.
```
{{#intervall 2}}

Kühlschrank 1:

{{#items E.S 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}

Kühlschrank 2:

{{#items E.S 'Kühlschrank 2'}}
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
{{fontsize @root 8}}
{{else}}
{{fontsize @root 15}}
{{/intervall}}

{{#intervall 1}}
{{color @root 'ff0000'}}
{{else}}
{{color @root '00ff00'}}
{{/intervall}}

{{#intervall 1}}
{{bgcolor @root 'ffff00'}}
{{else}}
{{bgcolor @root '000000'}}
{{/intervall}}
```
----------------------------------------------------
## ERZE und BARREN IDENTIFIKATIONS NUMMER:
@root/OreIds

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
@root/IngotIds

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
# Technical
Syntaxdocu:
+ http://handlebarsjs.com/
+ http://handlebarsjs.com/reference.html#data
+ https://zordius.github.io/HandlebarsCookbook/index.html
+ https://zordius.github.io/HandlebarsCookbook/0014-path.html
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
  * Intervall in (sec) Sekunden

### CustomHelpers (scroll)
* {{#scroll lines delay}}
  * Text scrollen mit (lines) Zeilen und einer Verzögerung von (delay) Sekunden

### CustomHelpers (itemlist)
* {{#itemlist list 'id1;id2;id3,...'}}
  * Liste der Items (list) auf die Items mit den Ids 'id1;id2;id3,...' filtern. 
    Falls eine Id nicht vorhanden ist wird diese mit einer Anzahl 0 eingefügt.

### CustomHelpers (i18n)
* {{#i18n Select 'Language'}}
  * Language: English,Deutsch,Français,Italiano,Spanish,...
    das Sprachkürzel kann hier, aus der ersten Zeile, entnommen werden \[ESG\]\\Content\\Extras\\Localization.csv

### CustomHelpers (datetime)
+ {{datetime}} = Datum und Uhrzeit anzeigen
+ {{datetime 'format'}} = gemäß dem 'format' ausgeben
+ {{datetime 'format' '+5'}} = N Stunden addieren

DateTime format:
+ https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netframework-4.8#System_DateTime_ToString_System_String_

### CustomHelpers (items)
+ {{#items structure 'box1;box2;fridge*;...'}} = Alle Items aus den Containers (names)='box1;box2;fridge*;...' ermitteln

### CustomHelpers (format)
+ {{format data format}} = Daten (data) gemäß dem Format (format) ausgeben
  + https://docs.microsoft.com/de-de/dotnet/api/system.string.format?view=netframework-4.8#remarks-top

### CustomHelpers (move)
+ {{move item structure names [maxLimit]}}
  + Item (item) in die Struktur (structure) in die Container mit den Namen (names) verschieben
  + [maxLimit] ist ein optionaler Parameter der die Anzahl im Zielcontainer begrenzt

### CustomHelpers (lights)
+ {{lights structure names}}
  + Lichter der Struktur (structure) mit den Namen (names) auswählen

### CustomHelpers (lightcolor)
+ {{lightcolor light color}}
  + Bei Licht (light) die Farbe (color rgb hex) auswählen

### CustomHelpers (lightblink)
+ {{lightblink light interval length offset}}
  + Bei Licht (light) das Intervall (intervall) die Intervalllänge (length) und den Intervalloffset (offset) einstellen

### CustomHelpers (lightintensity)
+ {{lightintensity light intensity}}
  + Bei Licht (light) die Lichtintensität (intensity) einstellen

### CustomHelpers (lightrange)
+ {{lightrange light range}}
  + Bei Licht (light) die Lichtreichweite (range) einstellen

### CustomHelpers (lightspotangle)
+ {{lightspotangle light spotangle}}
  + Bei Licht (light) die Lichtspotwinkel (spotangle) einstellen

### CustomHelpers (lighttype)
+ {{lighttype light type}}
  + Bei Licht (light) die Lichttyp (type) einstellen
	+	Spot
	+	Directional
	+	Point
	+	Area
	+	Rectangle
	+	Disc

### CustomHelpers (devices)
+ {{devices structure customnames}}
  + (structure) (name;name*;*;name)

### CustomHelpers (devicesoftype)
+ {{devicesoftype structure type}}
  + (structure) (type)

### CustomHelpers (setactive)
+ {{setactive block|device active}}

### CustomHelpers (steps)
+ {steps start end \[step\] \[delay\]}}
  + Von (start) nach (end) mit optional einer Schrittweite von (step) und einer (delay)-Sekunden geänderten Zeitbasis

### CustomHelpers (split)
+ {split string separator [removeemptyentries]}}
  + (string) mit dem Trennzeichen (separator) zerteilen.
  + \[removeemptyentries\] falls leere Einträge entfernt werden sollen 'true'

### CustomHelpers (islocked)
+ {{islocked structure x y z}}
  + Prüft ob der Block/Device der (structure) an der Position (x) (y) (z) gesperrt ist.

### CustomHelpers (gettexture)
+ {{gettexture block pos}}
  + Liefert die TexturId des Blocks von der Seite T=Top, B=Bottom,, N=North, S=South, W=West, E=East

### CustomHelpers (settexture)
+ {{settexture block pos textureid}}
  + Setzt die TexturId des Blocks an den Seiten T=Top, B=Bottom,, N=North, S=South, W=West, E=East es können mehrere durch Komma getrennt angegeben werden

### CustomHelpers (random)
+ {random start end}}
  + Zufallswert zwischen (start) und (end) liefern und in den Block als {{this}} hereinreichen

### CustomHelpers (bar)
+ {{bar data min max length \[char\] \[bgchar\]}}
  + Erzeugt eine Balkenanzeige für (data) in dem Bereich von (min) bis (max) mit der Länge (length)
  + Der Balkensymbole für "gefüllt" (char) und den Hintergrund (bgchar) sind optional 

### CustomHelpers (use)
+ {{use data}}
  + Diesen Datensatz im Inhalt zum direkten Zugriff bereitstellen
  + der {{else}} fall wird aufgerufen wenn data == null ist

### CustomHelpers (math)
+ {{math (lvalue) op (rvalue)}}
  + op = +, -, *, /, %

### CustomHelpers (block)
+ {{block structure x y z}}
  + Liefert den Block/Device der (structure) von der Position (x) (y) (z) 

### CustomHelpers (gettext)
+ {{gettext lcddevice}}
  + Liefert den Text des LCD (lcddevice)

### CustomHelpers (settext)
+ {{gettext lcddevice text}}
  + Setzt den Text des LCD (lcddevice) mit dem Text (text)

## SaveGame Scripte
Diese besondere Form von Scripten kann im SaveGame hinterlegt werden. Der BasisPfad dafür ist der
\[SaveGame\]\\Mods\\EmpyrionScripting\\Scripts

in diesem Verzeichnis werden nach folgendem Muster Scriptdateien mit der Endung *.hbs gesucht
* EntityType
* EntityName
* PlayfieldName
* PlayfieldName\\EntityType
* PlayfieldName\\EntityName
* EntityId

Hinweis: EntityType ist BA,CV,SV or HV

### CustomHelpers-SaveGameScripts (readfile)
+ {{readfile @root dir filename}} 
  + (dir)\\(filename) Dateiinhalt wird als ZeilenArray geliefert
  + Falls die Datei nicht existiert wird der {{else}} Teil ausgeführt

### CustomHelpers-SaveGameScripts (writefile)
+ {{writefile @root dir filename}} 
  + (dir)\\(filename) Inhalt des Blockes wird in die Datei geschrieben


### Whats next?


ASTIC/TC

***

English-Version:

---

# Empyrion Scripting
## Installation
1. Download the ZIP file from https://github.com/GitHub-TC/EmpyrionScripting/releases
1. unzip the file in the Content\\Mods directory

NOTE: This is an EARLY A10 mod with the new ModAPI !!! both are heavily WorkInProgress !!!

#### Installation for SinglePlayer
1. Download the ZIP file from https://github.com/GitHub-TC/EmpyrionScripting/releases
1. UnZip the file in the directory Content\\Mods directory
1. The game MUST then be started without EAC so the mods are loaded

### Whats for?
![](Screenshots/DemoShipScreen.png)

Displays various informations directly and live on LCD screens.

Get the demo structure 'LCDInfo-Demo' from the workshop
https://steamcommunity.com/workshop/filedetails/?id=1751409371

#### Help
![](Screenshots/RedAlert.png)
![](Screenshots/LCD1.png)

YouTube video;
* https://youtu.be/Wm_09Q-cvh0 (thanks to Olly :-) )
* https://youtu.be/XzYKNevK0bs
* https://youtu.be/SOnZ_mzytA4
* https://youtu.be/oDOSbllwqSw
* https://youtu.be/qhOnj2D3ejo

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
{{items E.S '"Name der Kiste"'}}
{{Count}}{{Name}}
{{/items}}
```
Bsp:
```
LCDInfo:LCD Alle Erze
Meine Erze
{{#items E.S 'Alle Erze'}}
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
{{#each E.S.Items}}
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
{{#each E.S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
LCDInfo:LCD Barren
Alle meine Barren in der Basis:
{{#each E.S.Items}}
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
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD Alle Barren im Spiel
Alle Barren im Spiel:
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
-----------------------------------------------------
## Display of a specific product in the base / ship / HV / CV

Input on the LCD (everything without "")
```
LCDInfo:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist E.S.Items '2249'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
LCDInfo:LCD EISEN ERZ
Meine EisenErz und Barren
{{#itemlist E.S.Items '2249;2272'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
------------------------------------------------------------------
## Which ores are all, or only X number over

Here all ores are displayed where only 1-1000 exists on the basis.
```
{{#itemlist E.S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
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
{#itemlist E.S.Items '2248;2249;2250;2251;2252;2253;2269;2270;2284;2297;2280;2254'}}
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
{{#items E.S '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
```
Bsp.
```
{{#scroll 5 2}}
{{#items E.S 'Kühlschrank 1'}}
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
{{#items E.S '"Name der Kiste"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}
"Text optional"
{{#items E.S '"Name der Kiste2"'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{/intervall}}
```
Bsp.
```
{{#intervall 2}}

Kühlschrank 1:

{{#items E.S 'Kühlschrank 1'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/items}}
{{else}}

Kühlschrank 2:

{{#items E.S 'Kühlschrank 2'}}
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
{{fontsize @root 8}}
{{else}}
{{fontsize @root 15}}
{{/intervall}}

{{#intervall 1}}
{{color @root 'ff0000'}}
{{else}}
{{color @root '00ff00'}}
{{/intervall}}

{{#intervall 1}}
{{bgcolor @root 'ffff00'}}
{{else}}
{{bgcolor @root '000000'}}
{{/intervall}}
```
----------------------------------------------------
## ERZE und BARREN IDENTIFIKATIONS NUMMER:

@root/OreIds

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
@root/IngotIds

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
+ http://handlebarsjs.com/reference.html#data
+ https://zordius.github.io/HandlebarsCookbook/index.html
+ https://zordius.github.io/HandlebarsCookbook/0014-path.html
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

### CustomHelpers (move)
+ {{move item structure names [maxLimit]}}
  + Item (item) into the structure (structure) in the container with the names (names) move
  + [maxLimit] is an optional parameter which one is limited the amount in the target container

### CustomHelpers (lights)
+ {lightsign names}}
  + Select lights of the structure with names

### CustomHelpers (lightcolor)
+ {{light color light color}}
  + For light, select the color (color rgb hex)

### CustomHelpers (lightblink)
+ {{lightblink light interval length offset}}
  + In the case of light, set the interval (interval), the interval length (length) and the interval offset (offset)

### CustomHelpers (light intensity)
+ {{light intensity light}
  + Set the light intensity for light

### CustomHelpers (lightrange)
+ {{lightrange light range}}
  + In the case of light, set the light range

### CustomHelpers (lightspotangle)
+ {{lightspotangle light spotangle}}
  + Set the light spot angle (spotangle) for light

### CustomHelpers (lighttype)
+ {{lighttype light type}}
  + For light, set the type of light
  + spot
  + Directional
  + Point
  + Area
  + rectangle
  + disc

### CustomHelpers (devices)
+ {{devices structure customnames}}
  + (structure) (name;name*;*;name)

### CustomHelpers (devicesoftype)
+ {{devicesoftype structure type}}
  + (structure) (type)

### CustomHelpers (setactive)
+ {{setactive block|device active}}

### CustomHelpers (steps)
+ {{steps start end \[step\] \[delay\]}}
  + From (start) to (end) with optional (step)-width and (delay) extends the 1 second per 1 counter add

### CustomHelpers (random)
+ {{random start end}}
   + Deliver a random value between (start) and (end) and submit to the block as {{this}}

### CustomHelpers (split)
+ {split string separator [removeemptyentries]}}
  + (string) split with the delimiter (separator).
  + \[removeemptyentries\] if empty entries should be removed 'true'

### CustomHelpers (bar)
+ {{bar data min max length \[char\] \[bgchar\]}}
  + Displays a bar for (data) in the rage of (min) to (max) with the total bar length of (length)
  + The string for filled signs (char) and background signs (bgchar) are optional 

### CustomHelpers (use)
+ {{use data}}
  + Use this data for direct access
  + the {{else}} case will call when data == null is

### CustomHelpers (math)
+ {{math (lvalue) op (rvalue)}}
  + op = +, -, *, /, %

### CustomHelpers (block)
+ {{block structure x y z}}
  + Returns the block/device of the (structure) at the position (x) (y) (z) 

### CustomHelpers (islocked)
+ {{islocked structure x y z}}
  + Test if the block/device of the (structure) at the position (x) (y) (z) is locked

### CustomHelpers (gettexture)
+ {{gettexture block pos}}
  + Get the TexturId of the block from the side T=Top, B=Bottom,, N=North, S=South, W=West, E=East

### CustomHelpers (settexture)
+ {{settexture block pos textureid}}
  + Set the TexturId of the block at the sides T=Top, B=Bottom,, N=North, S=South, W=West, E=East it could be many sides declared, komma separated

### CustomHelpers (gettext)
+ {{gettext lcddevice}}
  + Gets the text from the  LCD (lcddevice)

### CustomHelpers (settext)
+ {{settext lcddevice text}}
  + Set the text of the LCD (lcddevice) with (text)

## SaveGame scripts
This special form of scripts can be stored in the SaveGame. The basic path for this is the
\[Savegame\]\\Mods\\EmpyrionScripting\\Scripts

in this directory script files with the extension *.hbs are searched for according to the following pattern
* EntityType
* EntityName
* PlayfieldName
* PlayfieldName\\EntityType
* PlayfieldName\\EntityName
* EntityId

Note: EntityType is BA,CV,SV or HV

### CustomHelpers-SaveGameScripts (readfile)
+ {{readfile @root dir filename}}
   + (dir)\\(filename) file content is supplied as a LineArray
   + If the file does not exist, the {{else}} part will be executed

### CustomHelpers-SaveGameScripts (writefile)
+ {{writefile @root dir filename}}
   + (dir)\\(filename) Content of the block is written to the file

### Whats next?


ASTIC/TC
