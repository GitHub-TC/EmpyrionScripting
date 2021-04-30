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
* https://youtu.be/8MzjdeYlzPU
* https://youtu.be/gPp5CGJusr4
* https://youtu.be/9601vpeLJAI
* https://youtu.be/V1w2A3LAZCs
* https://youtu.be/O89NQJjbQuw
* https://youtu.be/uTgXwrlCfNQ
* https://youtu.be/qhYmJWHk8ec
* https://youtu.be/IbVuzFf_ywI

* https://youtu.be/XzYKNevK0bs
* https://youtu.be/SOnZ_mzytA4
* https://youtu.be/oDOSbllwqSw
* https://youtu.be/qhOnj2D3ejo

* Änderungen mit der A11: https://youtu.be/hxvKs5U1I6I

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
Man kann eine Information auch auf 2 LCD's anzeigen lassen dannsortedeach bei Targets:Name LCD;Name LCD2<br/>
Man kann eine Information auch auf n LCD's anzeigen lassen dann bei Targets:LCDAusgabe*<br/>
Man kann eine Information auch auf n LCD's anzeigen lassen welche schon im ScriptLCD Namen angegeben sind Script:LCDAusgabe*<br/>
Man kann auf einem LCD auch den Inhalt verschiedner Kisten anzeigen lassen!<br/>
 
Eine Debugausgabe erhält man mit einem LCD das den Namen "ScriptDebugInfo" hat.

 ---
## Was ist in der Kiste/Container/ContainerController/MunitionsKiste/Kühlschrank

Eingabe im LCD 1 (alles ohne "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT Optional"
{{items E.S '"Name der Kiste"'}}
{{Count}}{{Name}}
{{/items}}
```
Bsp:
```
Targets:LCD Alle Erze
Meine Erze
{{#items E.S 'Alle Erze'}}
{{Count}}{{i18 Key 'Deutsch'}}
{{/items}}
```
---
## Ausgabe aller Erze in der Basis/Schiff/HV/CV

Eingabe im LCD (alles ohne "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#test ID in '2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
Targets:LCD Alle Erze
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
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#each E.S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
Targets:LCD Barren
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
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
Targets:LCD Alle Barren im Spiel
Alle Barren im Spiel:
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
-----------------------------------------------------
## Anzeige eines bestimmten Produktes in der Basis/Schiff/HV/CV
```
Eingabe im LCD (alles ohne "")
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist E.S.Items '2249'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
Targets:LCD EISEN ERZ
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

## Vordefinierte ID Listen

Diese Listen können geändert werden oder durch neue Einträge erweitert werden.
Dazu kann einfach der Abschnitt "Ids" in der Datei \[EGS\]\Saves\Games\\[SaveGameName\]\Mods\EmpyrionScripting\Configuration.json
geändert werden.

Hinweis: Um den Originalzustand wieder herzustellen kann der Abschnitt "Ids" aus der Datei geöscht werden. Die Mod trägt dann hier die im Programm hinterlegte Standardkonfiguration wieder ein.

Folgende Listen können über "Ids.\[NameDerListe\] im Standard abgerufen werden.

- Ore        
- Ingot      
- BlockL     
- BlockS     
- Medic      
- Food       
- Ingredient 
- Sprout     
- Tools      
- ArmorMod   
- DeviceL    
- DeviceS   
- WeaponPlayer
- WeaponHV   
- WeaponSV   
- WeaponCV   
- WeaponBA   
- AmmoPlayer 
- AmmoHV     
- AmmoSV     
- AmmoCV     
- AmmoBA    

Die Listen beginnen und enden mit einem Komma so das sie einfach mit dem Befehl `concat` kombiniert werden können.
```
(concat Ids.WeaponHV Ids.WeaponSV Ids.WeaponCV)
oder
(concat '1234,5568' Ids.ArmorMod)
```


-----------------------------------------------------
## Welcher Spieler ist auf der Basis/Schiff gerade aktiv

Eingabe im LCD (alles ohne "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"Eingabe im LCD (alles ohne "")
"TEXT optional"
{{#each P.Players}}
 "-" {{Name}}
{{/each}}
```
Bsp.
```
Targets:LCD Info W1
Player:
{{#each P.Players}}
 - {{Name}}
{{/each}}
```
------------------------------------------------------
## Datum und Uhrzeit anzeigen lassen

Eingabe im LCD (alles ohne "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"Eingabe im LCD (alles ohne "")
"TEXT optional"
{{datetime}}

{{datetime 'HH:mm'}}

{{datetime 'dd MMM HH:mm:ss' '+7'}}
```
Bsp.
```
Targets:LCD UHRZEIT
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
+ http://handlebarsjs.com/guide/
+ https://zordius.github.io/HandlebarsCookbook/index.html
+ https://zordius.github.io/HandlebarsCookbook/0014-path.html
+ https://github.com/rexm/Handlebars.Net

## Bedingungen
* {{#test Select Op Value}}
  * Op: eq is =
  * Op: neq is <> or !=
  * Op: leq is <=
  * Op: le is <
  * Op: geq is >=
  * Op: ge is >
  * Op: in  (Trennzeichen sind: ,;#+ )
    * Value: '1,2,3,42'
    * Value: '1-3,42'
    * Value: 'A,xyz,mag'

* {{#ok data}}
  * Block ausführen wenn (data) einen Wert (ungleich 'null') hat oder (data) gleich 'true' oder ungleich 0 ist
  * anderfalls wird der {{else}} Teil ausgeführt

* {{#if data}}
  * Block ausführen wenn (data) einen Wert ungleich 'null' oder 0 hat
  * anderfalls wird der {{else}} Teil ausgeführt

* {{not data}}
  * Negation von (data)

## Inhalte
+ {{#items structure 'box1;box2;fridge*;...'}} = Alle Items aus den Containers (names)='box1;box2;fridge*;...' ermitteln

+ {{#getitems structure 'box1;box2;fridge*;...'}} = Alle Items aus den Containers (names)='box1;box2;fridge*;...' ermitteln und als Liste liefern z.B. für itemlist

* {{#itemlist list 'id1;id2;id3,...'}}
  * Liste der Items (list) auf die Items mit den Ids 'id1;id2;id3,...' filtern. 
    Falls eine Id nicht vorhanden ist wird diese mit einer Anzahl 0 eingefügt.

+ {{configid name}}
  + Liest aus Konfiguration des Block/Items 'name' das Attribut 'id'

+ {{configattr id attrname}}
  + Liest aus Konfiguration des Block/Items 'id' das Attribut 'attrname'

+ {{configattrbyname name attrname}}
  + Liest aus Konfiguration des Block/Items 'name' das Attribut 'attrname'

+ {{configbyid id}}
  + Liest aus Konfiguration den Abschnitt für den Block/Item mit der 'id'

+ {{configbyname name}}
  + Liest aus Konfiguration den Abschnitt für den Block/Item mit dem 'name'

+ {{resourcesforid id}}
  + Liste der benötigten Ressourcen für den Block/Item mit der 'id'

## Verschieben/Auffüllen/Verarbeiten
+ {{move item structure names \[maxLimit\]}}
  + Item (item) in die Struktur (structure) in die Container mit den Namen (names) verschieben
  + \[maxLimit\] ist ein optionaler Parameter der die Anzahl im Zielcontainer begrenzt

+ {{fill item structure tank \[max\]}}
  + Füllt in der Struktur (structure) den Tank (tank) = Fuel/Pxygen/Pentaxid mit dem Item (item) auf. Der prozentuale Füllstand kann mit (max) optional limitiert werden. Standard ist 100.

+ {{deconstruct entity container \[CorePrefix\] \[RemoveItemsIds1,Id2,...\]}}
  + Baut die Struktur 'entity' ab und befördert die Teile in den Container mit dem Namen welcher mit 'container' angegben wird
  + Hinweis: Der Kern der Struktur muss 'Core-Destruct-ID' (wobei ID für die Id der Struktur steht) heißen
  + Mit der Konfigurationseinstellung DeconstructBlockSubstitution kann eine Ersetzung(durch eine anderen BlockTyp)/Löschung (durch 0) von BlockTypen definiert werden

+ {{recycle entity container \[CorePrefix\]}}
  + Baut die Struktur 'entity' ab und befördert die Rohstoffe (der bekannten Rezepten) in den Container mit dem Namen welcher mit 'container' angegben wird
  + Hinweis: Der Kern der Struktur muss 'Core-Recycle-ID' (wobei ID für die Id der Struktur steht) heißen
  + Mit der Konfigurationseinstellung DeconstructBlockSubstitution kann eine Ersetzung(durch eine anderen BlockTyp)/Löschung (durch 0) von BlockTypen definiert werden

## Lichter
+ {{lights structure names}}
  + Lichter der Struktur (structure) mit den Namen (names) auswählen

+ {{lightcolor light color}}
  + Bei Licht (light) die Farbe (color rgb hex) auswählen

+ {{lightblink light interval length offset}}
  + Bei Licht (light) das Intervall (intervall) die Intervalllänge (length) und den Intervalloffset (offset) einstellen

+ {{lightintensity light intensity}}
  + Bei Licht (light) die Lichtintensität (intensity) einstellen

+ {{lightrange light range}}
  + Bei Licht (light) die Lichtreichweite (range) einstellen

+ {{lightspotangle light spotangle}}
  + Bei Licht (light) die Lichtspotwinkel (spotangle) einstellen

+ {{lighttype light type}}
  + Bei Licht (light) die Lichttyp (type) einstellen
	+	Spot
	+	Directional
	+	Point
	+	Area
	+	Rectangle
	+	Disc

## Geräte
+ {{devices structure customnames}}
  + (structure) (name;name*;*;name)

+ {{devicesoftype structure type}}
  + (structure) (type)

+ {{setactive block|device active}}

+ {{islocked structure x y z}}
  + Prüft bei der Struktur (structure) ob das Device (device) oder das Device an der Position (x) (y) (z) gesperrt ist.

## Datenaufbereitung
* {{#intervall sec}}
  * Intervall in (sec) Sekunden

* {{#scroll lines delay \[step\]}}
  * Text scrollen mit (lines) Zeilen und einer Verzögerung von (delay) Sekunden
  * Optional mit (step) Zeilen Schritten

* {{#i18n Select 'Language'}}
  * Language: English,Deutsch,Français,Italiano,Spanish,...
    das Sprachkürzel kann hier, aus der ersten Zeile, entnommen werden \[ESG\]\\Content\\Extras\\Localization.csv

+ {{datetime}} = Datum und Uhrzeit anzeigen
+ {{datetime 'format'}} = gemäß dem 'format' ausgeben
+ {{datetime 'format' '+5'}} = N Stunden addieren
   + DateTime format:<br/>
    https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netframework-4.8#System_DateTime_ToString_System_String_

+ {{format data format}} = Daten (data) gemäß dem Format (format) ausgeben
  + https://docs.microsoft.com/de-de/dotnet/api/system.string.format?view=netframework-4.8#remarks-top

+ {{steps start end \[step\] \[delay\]}}
  + Von (start) nach (end) mit optional einer Schrittweite von (step) und einer (delay)-Sekunden geänderten Zeitbasis

+ {{sortedeach array sortedBy \[reverse\]}}
  + Sortiert das Array nach (sortedBy) und iteriert über die einzelen Element
  + (reverse) = true um die Sortierung umzukehren
  
+ {{sort array sortedBy \[reverse\]}}
  + Sortiert das Array nach (sortedBy)
  + (reverse) = true um die Sortierung umzukehren
  
+ {{orderedeach array '+/-sortedBy1,+/-sortedBy2,...'}}
  + Sortiert das Array nach (sortedBy1) dann nach 'sortedBy2' usw. bei '+' aufsteigend, bei '-' absteigend nach dem jeweiligen Feld 
  + iteriert danach über jedes Element
  
+ {{order array sortedBy \[reverse\]}}
  + Sortiert das Array nach (sortedBy1) dann nach 'sortedBy2' usw. bei '+' aufsteigend, bei '-' absteigend nach dem jeweiligen Feld 
  
+ {{split string separator \[removeemptyentries\] \[trimchars\]}}
  + (string) mit dem Trennzeichen (separator) zerteilen.
  + \[removeemptyentries\] falls leere Einträge entfernt werden sollen 'true'
  + \[trimchars\] Zeichen die vorne und hinten entfernt werden sollen

+ {{trim string \[trimchars\]}}
  + (string) Text
  + \[trimchars\] Zeichen die vorne und hinten entfernt werden sollen

+ {{random start end}}
  + Zufallswert zwischen (start) und (end) liefern und in den Block als {{this}} hereinreichen

+ {{bar data min max length \[char\] \[bgchar\]}}
  + Erzeugt eine Balkenanzeige für (data) in dem Bereich von (min) bis (max) mit der Länge (length)
  + Der Balkensymbole für "gefüllt" (char) und den Hintergrund (bgchar) sind optional 

+ {{use data}}
  + Diesen Datensatz im Inhalt zum direkten Zugriff bereitstellen
  + der {{else}} fall wird aufgerufen wenn data == null ist

+ {{set key data}}
  + Die Daten (data) hinterlegen so dass sie per @root.Data.(key) jederzeit wieder abgerufen werden können

+ {{setblock key}}
  + Die Daten des Blockes hinterlegen so dass sie per @root.Data.(key) jederzeit wieder abgerufen werden können

+ {{setcache key data}}
  + Die Daten (data) hinterlegen so dass sie per @root.CacheData.(key) jederzeit wieder abgerufen werden können. 
  + Diese werden für die Entität gespeichert und stehen beim nächsten Scriptaufruf wieder zur Verfügung können jedoch durch Playfieldwechel oder ähnliches verworfen werden.

+ {{setcacheblock key}}
  + Die Daten des Blockes hinterlegen so dass sie per @root.CacheData.(key) jederzeit wieder abgerufen werden können
  + Diese werden für die Entität gespeichert und stehen beim nächsten Scriptaufruf wieder zur Verfügung können jedoch durch Playfieldwechel oder ähnliches verworfen werden.

+ {{concat a1 a2 a3 ...}}
  + Fügt die Werte a1 .. aN zusammen
  + Wenn ein Wert ein Array von Texten (string\[\]) ist wird der nächste Parameter als Trennzeichen für diese Einträge gewertet

+ {{substring text startindex \[length\]}}
  + Teiltext von dem Text (text) von Index (startindex) mit einer optionalen maximalen Länge von (length) Zeichen

+ {{replace text find replaceto}}
  + Ersetzt die (find) durch (replaceto) in dem Text (text)
  
+ {{startswith text starts \[ignoreCase\]}}
  + Beginnt der Text (text) mit dem Text (starts) optional unabhängige Groß/Kleinschreibung

+ {{endswith text ends \[ignoreCase\]}}
  + Endet der Text (text) mit dem Text (ends) optional unabhängige Groß/Kleinschreibung

+ {{chararray text}}
  + Text als Array von Zeichen liefern

+ {{selectlines lines from to}}
  + Liefert die Zeilen (from) bis (to) aus dem Text (lines)

+ {{lookup array index}} und + {{lookupblock array index}}
  + Liefert das Element an der Position (index) beginnend mit 0

## Block
+ {{block structure x y z}}
  + Liefert den Block/Device der (structure) von der Position (x) (y) (z) 

+ {{gettexture block pos}}
  + Liefert die TexturId des Blocks von der Seite T=Top, B=Bottom,, N=North, S=South, W=West, E=East

+ {{settexture block textureid [pos]}}
  + Setzt die TexturId des Blocks an den Seiten T=Top, B=Bottom,, N=North, S=South, W=West, E=East es können mehrere durch Komma getrennt angegeben werden, wenn keine Position angegeben wird wird der ganze Block gesetzt

+ {{setcolor block colorid [pos]}}
  + Setzt die Farbe des Blocks an den Seiten T=Top, B=Bottom,, N=North, S=South, W=West, E=East es können mehrere durch Komma getrennt angegeben werden, wenn keine Position angegeben wird wird der ganze Block gesetzt

## Rechnen
+ {{math (lvalue) op (rvalue) [digits]}}
  + op = +, -, *, /, %
  + optional [digits] um das Ergebnis auf [digits] Stellen zu runden

+ {{calc (lvalue) op (rvalue) [digits]}}
  + op = +, -, *, /, %
  + optional [digits] um das Ergebnis auf [digits] Stellen zu runden
  + Kann mit () inline in anderen Kommandos benutzt werden

+ {{distance (lVector) (rVector) [format]}}
  + Abstand zwischen (lVector) und (rVector)
  + Optional ein format

+ {{min (lValue) (rValue)}}
  + Liefert den kleineren Wert der beiden

+ {{max (lValue) (rValue)}}
  + Liefert den größeren Wert der beiden

+ {{int (value)}}
  + Liefert den ganzzahligen Anteil des Wertes

## LCD
+ {{gettext lcddevice}}
  + Liefert den Text des LCD (lcddevice)

+ {{settext lcddevice text}}
  + Setzt den Text des LCD (lcddevice) mit dem Text (text)

+ {{settextblock lcddevice}}
  + Setzt den Text des LCD (lcddevice) mit dem Text des innenliegenden Blockes

+ {{setcolor lcddevice (rgb hex)}}
  + Setzt die Farbe des LCD (lcddevice) auf (rgb hex)

+ {{setbrcolor lcddevice (rgb hex)}}
  + Setzt die Hintergrundfarbe des LCD (lcddevice) auf (rgb hex)

## Strukturen
+ {{entitybyname name \[maxdistance\]}}
  + Liefert die Entiäten, in der Nähe und mit der selben Fraktion, mit Name (name) und der, optionalen, maximalen Entfernung \[maxdistance\]

+ {{entitiesbyname names \[maxdistance\] \[types\]}}
  + Liefert die Entiäten, in der Nähe und mit der selben Fraktion, mit Namen in (name;name*;*) und der, optionalen, maximalen Entfernung \[maxdistance\]
  + \[types\] ist nur in 'Elevated Scripten' erlaubt und liefert alle Objkete mit den Typen aus (Z.B. auch Proxy und Asteroid)

+ {{entitybyid id}}
  + Liefert die Entiäten, in der Nähe und mit der selben Fraktion, mit Id (id)

+ {{entitiesbyid ids}}
  + Liefert die Entiäten, in der Nähe und mit der selben Fraktion, mit IDs in (id1;id2;id3)

## Signale
+ {{signalevents names}} 
  + die letzten Signalevents mit den namen (name1;name2...)

+ {{triggerifsignalgoes names boolstate}}
  + triggert wenn eines der Signale 'names' auf den Status 'boolstate' wechselt
  
+ {{signals structure names}}
  + Liefert die Signale (names) der Struktur

+ {{getsignal structure name}}
  + Liefert den Status true/false des Signal (name)

+ {{setswitch structure name state}}
  + Setzt den Schalter (name) auf (state) = true/false. Der Name darf der Name des Schalter im ControlPanel oder der Name seines Signales sein

+ {{getswitch structure name}}
  + Liefert den Status true/false des Schalters (name). Der Name darf der Name des Schalter im ControlPanel oder der Name seines Signales sein

+ {{getswitches structure name}}
  + Liefert alle Schalte die im ControlPanel auf dem Namen (name) passen

+ {{stopwatch startsignal stopsignal \[resetsignal\]}}
  + Eine einfache Stopuhr (für Rennstrecken) mit einem Startsignal, einem Stopsignal und für das Zurücksetzten der Ergebnisse optionalem Resetsignal

## Elevated Scripte (Savegame oder Adm-Strukturen)
+ {{lockdevice structure device|x y z}}
  + Sperrt ein Device

+ {{additems container itemid count}}
  + Fügt (itemid) (count) mal dem container hinzu (dieser sollte gelocked sein)

+ {{removeitems container itemid maxcount}}
  + Entfernt (itemid) (count) aus dem container hinzu (dieser sollte gelocked sein)

+ {{replaceblocks entity RemoveItemsIds1,Id2,... ReplaceId}}
  + Tauscht die Blöcke mit 'RemoveItemsIds1,Id2,...' gegen den Block 'ReplaceId'
  + Replace = 0 entfernt den Block einfach

## SaveGame Scripte
Diese besondere Form von Scripten kann im SaveGame hinterlegt werden. Der BasisPfad dafür ist der
\[SaveGame\]\\Mods\\EmpyrionScripting\\Scripts

Dieser Pfad ist auch über @root.MainScriptPath zu erreichen.

in diesem Verzeichnis werden nach folgendem Muster Scriptdateien mit der Endung *.hbs gesucht
* EntityType
* EntityName
* PlayfieldName
* PlayfieldName\\EntityType
* PlayfieldName\\EntityName
* EntityId
* im Verzeichnis selber

Hinweis: EntityType ist BA,CV,SV or HV

### CustomHelpers-SaveGameScripts
+ {{readfile dir filename}} 
  + (dir)\\(filename) Dateiinhalt wird als ZeilenArray geliefert
  + Falls die Datei nicht existiert wird der {{else}} Teil ausgeführt

+ {{writefile dir filename}} 
  + (dir)\\(filename) Inhalt des Blockes wird in die Datei geschrieben

+ {{fileexists dir filename}}
  + Wenn die Datei existiert dann das Innere ausführen ansonsten den exec Teil auswerten

+ {{filelist dir filename \[recursive\]}}
  + Liste der Dateien (optional rekursiv amit allen Unterverzeichnissen) in dem Verzeichnis (dir) auf, welche dem Pattern (filename) entsprechen

+ {{settype block typeid}}
  + Den Block (austauschen) zu (typeid)

+ {{setdamage block damage}}
  + Schaden des Blockes setzen

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
* https://youtu.be/8MzjdeYlzPU
* https://youtu.be/gPp5CGJusr4
* https://youtu.be/9601vpeLJAI
* https://youtu.be/V1w2A3LAZCs
* https://youtu.be/O89NQJjbQuw
* https://youtu.be/uTgXwrlCfNQ
* https://youtu.be/qhYmJWHk8ec

* https://youtu.be/XzYKNevK0bs
* https://youtu.be/SOnZ_mzytA4
* https://youtu.be/oDOSbllwqSw
* https://youtu.be/qhOnj2D3ejo

* Changes with the A11: https://youtu.be/hxvKs5U1I6I

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
One can also display an information on 2 LCD's then at Targets: "Name LCD"; "Name LCD2"<br/>
You can also display the content of various boxes on an LCD!<br/>

---
## What's in the box / container / container controller / ammo box / refrigerator

Eingabe im LCD 1 (alles ohne "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT Optional"
{{items E.S '"Name der Kiste"'}}
{{Count}}{{Name}}
{{/items}}
```
Bsp:
```
Targets:LCD Alle Erze
Meine Erze
{{#items E.S 'Alle Erze'}}
{{Count}}{{i18 Key 'Deutsch'}}
{{/items}}
```
---
## Output of all ores in the base / ship / HV / CV

Input on the LCD (everything without "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#test ID in '2248,2249,2250,2251,2252,2253,2254,2269,2270,2284,2293,2297'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
Targets:LCD Alle Erze
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
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#each E.S.Items}}
{{#test Id in '2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,2285,2294,2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/test}}
{{/each}}
```
Bsp:
```
Targets:LCD Barren
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
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
Targets:LCD Alle Barren im Spiel
Alle Barren im Spiel:
{{#itemlist E.S.Items '2271;2272;2273;2274;2275;2276;2277;2278;2279;2280;2281;2285;2294;2298'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
-----------------------------------------------------
## Display of a specific product in the base / ship / HV / CV

Input on the LCD (everything without "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"
"TEXT optional"
{{#itemlist E.S.Items '2249'}}
{{Count}} {{i18n Key 'Deutsch'}}
{{/itemlist}}
```
Bsp:
```
Targets:LCD EISEN ERZ
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

## Predefined ID lists

These lists can be changed or expanded with new entries.
To do this, simply use the section "Ids" in the file \[EGS\]\Saves\Games\\[SaveGameName\]\Mods\EmpyrionScripting\Configuration.json
be changed.

Note: To restore the original state, the section "Ids" can be deleted from the file. The mod then enters the standard configuration stored in the program again.

The following lists can be called up via "Ids.\[NameDerListe\] in the standard system.

- Ore
- Ingot
- BlockL
- BlockS
- Medic
- food
- Ingredient
- Sprout
- tools
- ArmorMod
- DeviceL
- DeviceS
- WeaponPlayer
- WeaponHV
- WeaponSV
- WeaponCV
- WeaponBA
- AmmoPlayer
- AmmoHV
- AmmoSV
- AmmoCV
- AmmoBA

The lists begin and end with a comma so that they can be easily combined with the command `concat`.
```
(concat Ids.WeaponHV Ids.WeaponSV Ids.WeaponCV)
or
(concat '1234.5568' Ids.ArmorMod)
```

-----------------------------------------------------
## Which player is currently active on the base / ship

Input on the LCD (everything without "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"Input on the LCD (everything without "")
"TEXT optional"
{{#each P.Players}}
 "-" {{Name}}
{{/each}}
```
Bsp.
```
Targets:LCD Info W1
Player:
{{#each P.Players}}
 - {{Name}}
{{/each}}
```
------------------------------------------------------
## Display date and time

Input on the LCD (everything without "")
```
Targets:"NAME DES ANZUZEIGENDEN LCD"Input on the LCD (everything without "")
"TEXT optional"
{{datetime}}

{{datetime 'HH:mm'}}

{{datetime 'dd MMM HH:mm:ss' '+7'}}
```
Bsp.
```
Targets:LCD UHRZEIT
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

### (test)
* {{#test Select Op Value}}
  * Op: eq is =
  * Op: neq is <> or !=
  * Op: leq is <=
  * Op: le is <
  * Op: geq is >=
  * Op: ge is >
  * Op: in (Delimitters are: ,;#+ )
    * Value: '1,2,3,42'
    * Value: '1-3,42'
    * Value: 'A,xyz,mag'

## Items
+ {{configid name}}
   + Reads the attribute 'id' from the configuration of the block/item 'name'

+ {{configattr id attrname}}
   + Reads the attribute 'attrname' from the configuration of the block/item 'id'

+ {{configattrbyname name attrname}}
   + Reads the attribute 'attrname' from the configuration of the block/item 'name'
   
+ {{configbyid id}}
  + Reads the config section block/item for 'id'

+ {{configbyname name}}
  + Reads the config section block/item for 'name'

+ {{resourcesforid id}}
   + List of the required resources for the block / item with the 'id'

### Logiccheck
* {{#ok data}}
   * Execute block if (data) has a value (not equal to 'zero') or (data) is equal to 'true' or not equal to 0
   * otherwise the {{else}} part is executed

* {{#if data}}
   * Execute block if (data) has a value not equal to 'zero' or 0
   * otherwise the {{else}} part is executed

* {{not data}}
   * Negation of (data)
   
### (intervall)
* {{#intervall sec}}
  * intervall in seconds

### (scroll)
* {{#scroll lines delay \[step\]}}
  * Text scroll block with (lines) od text, scrolls with (delay) seconds
  * optional (step) lines per step

+ {{#getitems structure 'box1; box2; fridge *; ...'}} = Determine all items from the containers (names) = 'box1; box2; fridge *; ...' and deliver them as a list e.g. for itemlist

* {{#itemlist list 'id1;id2;id3'}}
  * Itemlist the the selected items (ids) even if they don't in the list (list)

### (i18n)
* {{#i18n Select 'Language'}}
  * Language: English,Deutsch,Français,Italiano,Spanish,...
    look at \[ESG\]\\Content\\Extras\\Localization.csv at the first line

### (datetime)
+ {{datetime}} = Display the Datetime
+ {{datetime 'format'}} = uses the formatstring
+ {{datetime 'format' '+5'}} = adds N hours

DateTime format:
+ https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netframework-4.8#System_DateTime_ToString_System_String_

### (move)
+ {{move item structure names [maxLimit]}}
  + Item (item) into the structure (structure) in the container with the names (names) move
  + [maxLimit] is an optional parameter which one is limited the amount in the target container

### (lights)
+ {lightsign names}}
  + Select lights of the structure with names

### (lightcolor)
+ {{light color light color}}
  + For light, select the color (color rgb hex)

### (lightblink)
+ {{lightblink light interval length offset}}
  + In the case of light, set the interval (interval), the interval length (length) and the interval offset (offset)

### (light intensity)
+ {{light intensity light}
  + Set the light intensity for light

### (lightrange)
+ {{lightrange light range}}
  + In the case of light, set the light range

### (lightspotangle)
+ {{lightspotangle light spotangle}}
  + Set the light spot angle (spotangle) for light

### (lighttype)
+ {{lighttype light type}}
  + For light, set the type of light
  + spot
  + Directional
  + Point
  + Area
  + rectangle
  + disc

### (devices)
+ {{devices structure customnames}}
  + (structure) (name;name*;*;name)

### (devicesoftype)
+ {{devicesoftype structure type}}
  + (structure) (type)

### (setactive)
+ {{setactive block|device active}}

### (steps)
+ {{steps start end \[step\] \[delay\]}}
  + From (start) to (end) with optional (step)-width and (delay) extends the 1 second per 1 counter add

+ {{sortedeach array sortedBy \[reverse\]}}
  + sorts the array by (sortedBy) and iterates over the individual elements
  + (reverse) = true to reverse the sort order
  
+ {{sort array sortedBy \[reverse\]}}
  + sorts the array by (sortedBy)
  + (reverse) = true to reverse the sort order

+ {{orderedeach array '+/-sortedBy1,+/-sortedBy2,...'}}
  + sorts the array by (sortedBy1) then by 'sortedBy2' etc. with '+' ascending, with '-' descending by the respective field. 
  + iterates over each element
  
+ {{order array sortedBy \[reverse\]}}
  + sorts the array by (sortedBy1) then by 'sortedBy2' etc. at '+' ascending, at '-' descending by the respective field 

+ {{random start end}}
   + Deliver a random value between (start) and (end) and submit to the block as {{this}}

### Stringfunctions
+ {{split string separator [removeemptyentries]}}
  + (string) split with the delimiter (separator).
  + \[removeemptyentries\] if empty entries should be removed 'true'

+ {{substring text startindex [length]}}
  + Substring from the Text (text) from Index (startindex) with optional maximum (length) characters

+ {{startswith text starts \[ignoreCase\]}}
   + If the text (text) begins with the text (starts), optionally independent upper / lower case

+ {{endswith text ends \[ignoreCase\]}}
   + If the text (text) ends with the text (ends), optionally independent upper / lower case
 
+ {{replace text find replaceto}}
  + replace the (find) into the (replaceto) in the (text)
   
### (chararray)
+ {{chararray text}}
  + Split the Text into an array of characters

+ {{selectlines lines from to}}
  + Returns the lines (from) to (to) from the text (lines)

+ {{lookup array index}} and {{lookupblock array index}}
  + Returns the element at the position (index) starting with 0

+ {{bar data min max length \[char\] \[bgchar\]}}
  + Displays a bar for (data) in the rage of (min) to (max) with the total bar length of (length)
  + The string for filled signs (char) and background signs (bgchar) are optional 

+ {{use data}}
  + Use this data for direct access
  + the {{else}} case will call when data == null is

### (set)
+ {{set key data}}
   + The data (data) are stored so that they can be recalled at any time via @root.Data.(Key)

+ {{setblock key}}
   + The data of the block are stored so that they can be recalled at any time via @root.Data.(Key)

+ {{setcache key data}}
   + Store the data (data) so that they can be called up again at any time via @root.CacheData.(Key).
   + These are saved for the entity and are available again the next time the script is called, but can be discarded by changing the playfield or similar.

+ {{setcacheblock key}}
   + Store the data of the block so that it can be called up at any time via @root.CacheData.(Key)
   + These are saved for the entity and are available again the next time the script is called, but can be discarded by changing the playfield or similar.

### (math)
+ {{math (lvalue) op (rvalue) [digits]}}
  + op = +, -, *, /, %
  + digits round number

+ {{calc (lvalue) op (rvalue) [digits]}}
  + op = +, -, *, /, %
  + digits round number
  + Can be used with () inline in other commands

+ {{min (lValue) (rValue)}}
   + Returns the smaller of the two

+ {{max (lValue) (rValue)}}
   + Returns the larger of the two

+ {{int (value)}}
   + Returns the integer part of the value
   
+ {{distance (lVector) (rVector) [format]}}
  + Distance between (lVector) and (rVector)
  + Optional a format

### (block)
+ {{block structure x y z}}

### (concat)
+ {{concat a1 a2 a3 ...}}
  + Concatenate the values of a1 .. aN 
  + If a value is an array of texts (string []), the next parameter is considered the separator for those entries

### (islocked)
+ {{islocked structure device|x y z}}
  + Checks at the structure whether the device (device) or the device is locked at the position (x) (y) (z).

### (gettexture)
+ {{gettexture block pos}}
  + Get the TexturId of the block from the side T=Top, B=Bottom,, N=North, S=South, W=West, E=East

+ {{settexture block textureid [pos]}}
  + Sets the texture ID of the block on the sides T = Top, B = Bottom, N = North, S = South, W = West, E = East several can be specified separated by commas, if no position is given the whole Block set

+ {{setcolor block colorid [pos]}}
  + Sets the color of the block on the sides T = Top, B = Bottom, N = North, S = South, W = West, E = East several can be specified separated by commas, if no position is given the whole Block set

### (gettext)
+ {{gettext lcddevice}}
  + Gets the text from the  LCD (lcddevice)

### (settext)
+ {{settext lcddevice text}}
  + Set the text of the LCD (lcddevice) with (text)

### (settextblock)
+ {{settextblock lcddevice}}
  + Set the text of the LCD (lcddevice) from the nested block

### (setcolor)
+ {{setcolor lcddevice (rgb hex)}}
  + Set the color of the LCD (lcddevice) with (rgb hex)

### (setbgcolor)
+ {{setbrcolor lcddevice (rgb hex)}}
  + Set the bgcolor of the LCD (lcddevice) with (rgb hex)

### (entitybyname)
+ {{entitybyname name \[maxdistance\]}}
  + Returns the entities nearby and with the same fraction, with name (name) and the, optional, maximum distance \[maxdistance\]

+ {{entitiesbyname names \[maxdistance\] \[types\]}}
  + Returns the entities nearby and with the same fraction, with names in (name; name *; *) and the, optional, maximum distance \[maxdistance\]
  + \[types\] is only allowed in 'Elevated Scripts' and delivers all objects with the types (e.g. proxy and asteroid)

### (entitybyid)
+ {{entitybyid id}}
  + Get nearby Entity (with same faction) with (id)

### (entitiesbyid)
+ {{entitiesbyid ids}}
  + Get nearby Entity (with same faction) with IDs in (id1;id2;id3)

### (deconstruct)
+ {{deconstruct entity container \[CorePrefix\] \[RemoveItemsIds1,Id2,...\]}}
   + Deconstruct the entity 'entity' and moves parts to container named as 'container''
   + Note: The core of the structure must be called 'Core-Destruct-ID' (where ID stands for the id of the structure)
   + With the configuration setting DeconstructBlockSubstitution a replacement (by another block type) / deletion (by 0) of block types can be defined

+ {{recycle entity container \[CorePrefix\]}}
   + Dismantles the 'entity' structure and transports the raw materials (of the known recipes) into the container with the name given by 'container'
   + Note: The core of the structure must be called 'Core-Recycle-ID' (where ID stands for the ID of the structure)
   + With the configuration setting DeconstructBlockSubstitution, a replacement (by another block type) / deletion (by 0) of block types can be defined
    
## Elevated scripts (Savegame or Adm structures)
+ {{lockdevice structure device | x y z}}
  + Locks a device

+ {{additems container item id count}}
  + Add (itemid) (count) times to the container (this should be locked)

+ {{removeitems container itemid maxcount}}
  + Removes (itemid) (count) from the container (it should be locked)

+ {{replaceblocks entity RemoveItemsIds1,Id2,... ReplaceId}}
  + Replace the block with the id 'RemoveItemsIds1,Id2,...' to 'ReplaceId'
  + Replace = 0 remove the block

## SaveGame scripts
This special form of scripts can be stored in the SaveGame. The basic path for this is the
\[Savegame\]\\Mods\\EmpyrionScripting\\Scripts

This path is avaiable with @root.MainScriptPath

in this directory script files with the extension *.hbs are searched for according to the following pattern
* EntityType
* EntityName
* PlayfieldName
* PlayfieldName\\EntityType
* PlayfieldName\\EntityName
* EntityId
* in the directory itself

Note: EntityType is BA,CV,SV or HV

### CustomHelpers-SaveGameScripts (readfile)
+ {{readfile dir filename}}
   + (dir)\\(filename) file content is supplied as a LineArray
   + If the file does not exist, the {{else}} part will be executed

### CustomHelpers-SaveGameScripts (writefile)
+ {{writefile dir filename}}
   + (dir)\\(filename) Content of the block is written to the file

### CustomHelpers-SaveGameScripts (fileexists)
+ {{fileexists dir filename}}
  + If exists then templane oterwirse exec else part

### Whats next?


ASTIC/TC
