# Inhaltsverzeichnis

1. [Einleitung](#einleitung)  
2. [Allgemeines](#allgemeines)  
    2.1 [Der PIC16F84](#der-pic16f84)  
    2.2 [Funktionsweise eines Simulators](#funktionsweise-eines-simulators)  
    2.3 [Vor- und Nachteile einer Simulation](#vor--und-nachteile-einer-simulation)  
    2.4 [Programmoberfläche und deren Handhabung](#programmoberfläche-und-deren-handhabung)
3. [Realisierung](#realisierung)  
   3.1 [Beschreibung des Grundkonzepts](#beschreibung-des-grundkonzepts)  
   3.2 [Beschreibung der Gliederung](#beschreibung-der-gliederung)  
   3.3 [Programmstruktur und Ablaufdiagramme](#programmstruktur-und-ablaufdiagramme)  
   3.4 [Verwendete Programmiersprache](#verwendete-programmiersprache)  
   3.5 [Funktionsbeschreibung ausgewählter Befehle](#funktionsbeschreibung-ausgewählter-befehle)  
   3.6 [Realisierung der Flags](#realisierung-der-flags)  
   3.7 [Implementierung von Interrupts](#implementierung-von-interrupts)  
   3.8 [Realisierung des TRIS-Registers](#realisierung-des-tris-registers)  
   3.9 [EEPROM-State-Machine](#eeprom-state-machine)  
4. [Zusammenfassung](#zusammenfassung)  
   4.1 [Erreichte Funktionalität](#erreichte-funktionalität)  
   4.2 [Fazit und persönliche Erfahrungen](#fazit-und-persönliche-erfahrungen)  
5. [Anhang](#anhang)  
   5.1 [Programmlisting](#programmlisting)  
   5.2 [Projektverlauf und Versionsverwaltung](#projektverlauf-und-versionsverwaltung)

# Einleitung

Der Pic-Sim ist ein Simulator für den Mikrocontroller PIC16F84 der im Rahmen einer benoteten Abgabe für den Kurs "Systemnahe Programmierung 2" im Studiengang Informationstechnik an der Dualen Hochschule Karlsruhe (DHBW KA) entwickelt wurde. 
Die Entwicklung dieses Simulators soll das Verständnis für die Funktionsweise von Mikrocontrollern weitgehend vertiefen.

Das Backend dieses Projektes wurde in C# programmiert und die Oberfläche mit Hilfe von [Avalonia UI](https://avaloniaui.net/) erstellt. 

Der Simulator ermöglicht es eine Software-Simulation des PIC16F84 durchzuführen, um Assembler-Programme im LST-Format zu testen und zu debuggen.
Da es sich jedoch um eine Software-Simulation handelt und um die Sachverhalte zu vereinfachen, wird auf die Einhaltung von CPU-Zyklen und Echtzeitausführung verzichtet.
Ein solcher Simulator kann sowohl zu Lernzwecken als auch zur Fehlersuche und Analyse von Mikrocontroller-Programmen eingesetzt werden. Die Möglichkeit, Code in einem kontrollierten Umfeld zu testen, ohne physische Hardware zu benötigen, bietet erhebliche Vorteile – etwa beim schnellen Testen kleiner Programmabschnitte oder bei der Analyse von Randbedingungen, die in der Praxis schwer reproduzierbar sind.

In dieser Dokumentation wird der Entwicklungsprozess des Simulators beschrieben. Es werden die zugrunde liegenden Konzepte erläutert, die technische Umsetzung dargestellt sowie ausgewählte Programmteile und deren Funktionsweise im Detail erklärt. Ziel ist es, auch für Außenstehende verständlich zu machen, wie der Simulator arbeitet, welche Funktionen implementiert wurden und welche Überlegungen während der Entwicklung eine Rolle spielten.
Die Dokumentation ist so aufgebaut, dass sie unabhängig vom Quellcode und ohne das Ausführen der Software die Funktionsweise und Struktur des Projekts nachvollziehbar macht.

Ein gekürztes und korrigiertes Datenblatt des PIC16F84 ist im docs-Ordner zu finden.

# Allgemeines

## Der PIC16F84

Der PIC16F84 ist ein weit verbreiteter 8-Bit-Mikrocontroller aus der PICmicro™-Serie von Microchip, der vor allem im Bereich der Ausbildung, der Prototypenentwicklung sowie bei kleineren Steuerungsaufgaben Anwendung findet.
Er basiert auf einer fortschrittlichen RISC-Architektur. Diese Architektur ist für ihre hohe Effizienz und Leistung bekannt und ermöglicht, dass nahezu alle Befehle in nur einem einzigen Taktzyklus ausgeführt werden – mit Ausnahme von Sprungbefehlen, die zwei Zyklen benötigen.

### Technische Merkmale

- RISC-Architektur: 35 Befehle (Reduced Instruction Set), mit zwei-stufiger Pipeline für schnelle Ausführung
- Harvard-Architektur: Getrennte Busse für Instruktionen (14 Bit) und Daten (8 Bit), was parallelen Zugriff erlaubt
- Speicher:
  - 68 Byte RAM
  - 64 Byte EEPROM für nichtflüchtige Datenspeicherung
  - 1K x 14 Bit Flash-Programmspeicher
- I/O-Funktionen:
  - Bis zu 13 digitale Ein-/Ausgabepins
  - Timer/Counter integriert
- Taktquellen:
  - Vier Oszillatortypen: RC (einfach und günstig), LP (stromsparend), XT (Standardquarz), HS (Hochgeschwindigkeitsquarz)
- Stromsparfunktionen:
  - SLEEP-Modus
  - Watchdog Timer mit internem RC-Oszillator zur Absicherung gegen Software-Abstürze
- Interruptquellen: Interne und externe Interrupts verfügbar
- Flash-Technologie:
  - Unterstützt In-Circuit-Reprogrammierung, ideal für Prototyping und Updates im eingebauten Zustand
  - Geeignet für Seriennummern, Kalibrierungsdaten oder späte Firmware-Programmierung

### Anwendungen

Der PIC16F84 eignet sich hervorragend für eine Vielzahl von Anwendungen, z. B.:

- Motorsteuerungen in Automobilen und Haushaltsgeräten
- Sicherheits- und Zugangssysteme
- Smartcards
- Energiesparende Sensorik in Remote-Systemen
- Anwendungen mit Platzbeschränkungen

## Funktionsweise eines Simulators

Ein Simulator ist ein Werkzeug, das versucht die Realität eines Systems oder Prozesses in einer kontrollierten Umgebung nachzubilden. 
Je nachdem, wie genau ein Simulator die Realität emuliert, können verschiedene Aspekte nach variierender Genauigkeit simuliert werden. 

Im Falle dieses Projektes wird ein PIC16F84 Mikrocontroller simuliert. Dieser wird mittels Algorithmen und Datenstrukturen in einem Software-Umfeld abgebildet.
Die Simulation erfolgt über eine reine logische Ebene, die die Funktionen des Mikrocontrollers nachbildet. Es werden keine elektrischen Komponenten oder physische Hardware simuliert. 

Somit sind Systemvariablen wie Spannung, Stromstärke, Stromverbrauch, Temperatur und weitere Randbedingungen in diesem Projekt nicht von Relevanz.

## Vor- und Nachteile einer Simulation



## Programmoberfläche und deren Handhabung

Der Simulator kann durch eine GUI bedient werden. Auf der Navigationleiste befinden sich 3 Menus. Mit dem ersten, File, kann eine Simulationsdatei geladen werden. Unter Settings kann
die Simulations geschwindigkeit angepasst werden und der EEPROM-Speicher für test zwecke gelöscht werden. Mit dem Menu Dokumentation wird diese Dokumentation aufgerufen. Auf der linken Seite des Hauptfenstern ist der Speicher des PIC abgebildet, durch klicken kann der gespeicherte Wert geändert werden. Der neue Wert muss im Hexadezimalsystem eingeben werden.Oben rechts im Hauptfenster befindet sich eine übersicht über die wichtigsten Register des PIC's. Dazu gehören die Port A und B Register und die korrespondierenden Tris-Register. Mit hilfe der Checkboxen kann der User die Tris-Register einstellen und die Port's setzen, insofern sie als Input fungieren. Weitere Register sind: Status, Option und Intcon. Auch der Watchdogtimer und Stack sind hier abgebildet. Die Quartzfrequenz kann per Combobox eingestellt werden. Der dritte Block, unten rechts, zeigt die geladene LST Datei hier können links Breakpoints gesetzt werden. Rechts befinden sich Buttons mit der der User die Simulation steuern kann. Run startet das Programm, mit Step Forward kann Schritt für Schritt vorgegangen werden. Mit Skip To Next wird ein Befehl übersprungen ohne ihn auszuführen. Bei Pause stoppt das Programm am Aktuellen Befehl und mit Reset wird der Simulator komplett resetet.
![Pic Simulator GUI](images/PicSimulatorGUI.png)

# Realisierung

## Beschreibung des Grundkonzepts

Die eingelesene LST-Datei wird in ein Array von Befehlen umgewandelt, diese sind als Hexadezimalwerte gespeichert. Der Programmzähler bestimmt den Index des aktuellen Befehls im Array. 
Wird das Programm gestartet, wird der Programmzähler auf 0 gesetzt und der erste Befehl im Array ausgeführt.
Danach wird immer der Befehl mit dem Index des Programmzählers ausgeführt. Der Befehlsdecoder interpretiert, in dem er den Hexwert maskiert, um welchen Befehl es sich handelt und ruft die entsprechende Funktion auf, die den Befehl ausführt. 
In dieser Funktion wird dann der Befehlscode ausgeführt, der die Register des PIC16F84 entsprechend ändert. 
Die meisten Befehle inkrementieren den Programmzähler um 1, sodass der nächste Befehl im Array ausgeführt wird. Außnamen sind Sprungbefehle, die den Programmzähler auf eine andere Adresse setzen, und Befehle wie `BTFSx`, die den nächsten Befehl überspringen, wenn eine Bedingung erfüllt ist.
Ausserdem wird der Timer und Watchdog Timer (falls aktiviert) bei jedem Befehl aktualisiert.

## Beschreibung der Gliederung

## Programmstruktur und Ablaufdiagramme

## Verwendete Programmiersprache

Das Backend des Simualators ist in C# programmiert. C# wurde aufgrund unserer familiarität mit der Programmiersprache gewählt. Zudem bietet C# durch .NET eine zuferlässige Entwicklungsumgebung. Das Frontend wurde Mit Avalonia UI gebaut. Die erste Überlegung war es WPF zu nutzen, WPF ist ein UI Framework für .NET Applicationen. Es war wichtig das der Simulator Platform übergreifend funktioniert, vorallem auf Windows und Linux. Da WPF ein Windows exlusives Framework ist fiel die Entscheidung auf Avalonia UI. Avalonia UI eine platformübergreifende Opensource-Alternative zu WPF. Es ist vom WPF inspiriert und hat sehr ähnliche Features. Als IDE wurde JetBrains Rider verwendet. Bei Rider handelt es sich um eine Platformübergreifende IDE für C# und .NET.

## Funktionsbeschreibung ausgewählter Befehle

- **BTFSx**: Dieser Befehl testet ein bestimmtes Bit und überspringt den nächten Befehl wenn die Bedingung erfüllt ist. Es gibt BTFSS der den nächsten Befehl bei einer 1 überspringt und BTFSC der den nächsten Befehl bei einer 0 überspringt. Mit diesem Befehl können zum Beispiel Schleifen realisiert werden. Der Sprung wird durch ein Inkrement des Program Counter realisiert, sodass der nächste Befehl übersprungen wird.
- **Call**: Beim Call-Befehl wird eine Subroutine aufgerufen. Dabei wird die aktuelle Position im Programm Counter auf den Stack gelegt, sodass nach der Ausführung der Subroutine an dieser Stelle weitergemacht werden kann. Der Befehl springt dann zur Adresse der Subroutine indem er mit dem übergebenen Wert und PCLATH die neue Adresse berechnet und diese in den Programm Counter schreibt.
- **MOVF**: Der MOVF Befehl kopiert den Inhalt eines Registers je nach Destination-Bit entweder in das W-Register oder in das Register selbst. Wenn das Destination-Bit 0 ist, wird der Inhalt des Registers in das W-Register kopiert, andernfalls bleibt das Register unverändert. Dieser Befehl wird häufig verwendet, um Daten zwischen Registern zu übertragen oder um den Inhalt eines Registers zu überprüfen. Dies ist möglich das der Befehl das Zero-Flag setzt, wenn der Inhalt des Registers Null ist. Das Carry-Flag wird nicht beeinflusst.


## Realisierung der Flags

Die Flags sind ein wichtiger Bestandteil des PIC16F84 Mikrocontrollers. Sie dienen dazu, den Zustand des Prozessors zu überwachen und bestimmte Operationen zu steuern. Im Simulator werden die Flags in einem speziellen Register, dem Status-Register, implementiert.
Das Status-Register enthält mehrere Bits, die verschiedene Zustände des Prozessors repräsentieren. Die wichtigsten Flags sind:
- **Z (Zero Flag)**: Dieses Flag wird gesetzt, wenn das Ergebnis einer arithmetischen oder logischen Operation Null ist. Es wird verwendet, um Vergleiche durchzuführen und bedingte Sprünge zu steuern.
- **C (Carry Flag)**: Dieses Flag wird gesetzt, wenn bei einer arithmetischen Operation ein Übertrag auftritt. Es ist wichtig für die Durchführung von Mehrwortarithmetik und für die Überprüfung von Überläufen.
- **DC (Digit Carry Flag)**: Dieses Flag wird gesetzt, wenn bei einer BCD-Operation ein Übertrag auftritt. Es ist speziell für die Arbeit mit BCD-Zahlen relevant.

Es ist wichtig zu beachten, dass das Carry-Flag und das Digit-Carry-Flag, aufgrund eines Hardwarefehlers beim PIC16F84, bei Subtraktionsbefehlen invertiert ist.

## Implementierung von Interrupts

Interrupts sind ein wesentlicher Bestandteil der Funktionalität des PIC16F84 Mikrocontrollers. Sie ermöglichen es dem Prozessor, auf externe Ereignisse zu reagieren, indem sie die normale Programmausführung unterbrechen und eine spezielle Interrupt-Service-Routine (ISR) ausführen. 
Der Simulator prüft vor jedem Befehl ob ein Interrupt ausgelöst wurde. Wenn dies der Fall ist, springt der Programmzähler an Adresse 0x0004, die die Startadresse der Interrupt-Service-Routine ist. 
Die ISR wird dann ausgeführt, und nach Abschluss der ISR wird der Programmzähler auf die Adresse zurückgesetzt, an der die normale Programmausführung unterbrochen wurde. 

## Realisierung des TRIS-Registers

Das TRIS-Register ist ein wichtiges Register im PIC16F84 Mikrocontroller, das die Richtung der I/O-Pins steuert. Es bestimmt, ob ein Pin als Eingang oder Ausgang konfiguriert ist.
Im Simulator bestimmt das TRIS-Register ob die Checkboxen für die Ports A und B aktiviert sind. Nur wenn ein Pin als Eingang konfiguriert ist, kann sein Zustand über die Checkboxen geändert werden. 
Außerdem wird bei einem Pin der als Eingang konfiguriert ist, interne Änderung an der Speicherzelle erst angezeigt, wenn der Pin zum Ausgang wird.


## EEPROM-State-Machine

Das EEPROM, der nichtflüchtige Speicher des Pic's, wird durch eine Textdatei simuliert (`EEPROM.txt`), die im Projektverzeichnis gespeichert ist. Diese Datei enthält die Daten, die im EEPROM des PIC16F84 gespeichert sind.
Der Programmierer kann in das EEPROM schreiben, indem er den Wert in das EEDATA-Register schreibt und die Adresse im EEPROM in das EEADR-Register schreibt. 
Beim Schreiben wird das EECON1-Register verwendet, um den Schreibvorgang zu steuern. Es muss eine bestimmte Sequenz ins EECON2-Register geschrieben werden, um den Schreibvorgang zu starten.
Wird der Schreibvorgang gestartet, wird der Inhalt des EEDATA-Registers in die angegebene Adresse im EEPROM geschrieben. Diese Aktion dauert 1ms (bei einer Quarzfrequenz von 4MHz sind das 1000 Befehle).
Nach dieser Zeit wird das EEPROM beschrieben und ein Interrupt ausgelöst, der den Programmierer darüber informiert, dass der Schreibvorgang abgeschlossen ist. 

Das Lesen des EEPROM's wird auch über das EECON1-Register gesteuert. Der Programmierer kann die Adresse im EEPROM in das EEADR-Register schreiben und dann das EECON1-Register verwenden, um den Lesevorgang zu starten.
Der Inhalt der angegebenen Adresse wird dann in das EEDATA-Register geladen. Dies geschieht sofort, da das Lesen des EEPROMs keine Verzögerung erfordert.

# Zusammenfassung

## Erreichte Funktionalität 

Der PIC Simulator kann in dieser Version Assembler-Programme im LST-Format simulieren und ausführen. 
Es wurden alle Funktionen des PIC16F84 implementiert, die für die Simulation von Assembler-Programmen erforderlich sind.

## Fazit und persönliche Erfahrungen

Die Entwicklung des PIC Simulators war eine lehrreiche Erfahrung, die ein tieferes Verständnis für die Funktionsweise von Mikrocontrollern und deren Programmierung vermittelt hat. 
Auch dinge die in Rechnerarchitekturen 1 nicht behandelt wurden, wie Interrupts, Watchdog und EEPROM, wurden hier klar. Die gegebenen Materialien waren hilfreich, um den PIC16F84 zu verstehen und die Implementierung der verschiedenen Funktionen zu realisieren. 
Hier war es besonders interessant auf die kleinen Details zu achten, die im PIC-Datenblatt beschrieben sind. 

Die Arbeit mit Avalonia UI war anfangs herausfordernd, dies lag vor allem daran, dass wir unsere Probleme mit der MVVM-Design-Pattern hatten. Sobald wir das INotifyPropertyChanged-Interface verstanden hatten, wurde die Arbeit mit Avalonia UI jedoch deutlich einfacher. 
Die Entscheidung, C# für das Backend zu verwenden, erwies sich als vorteilhaft, da es eine robuste und gut unterstützte Sprache ist, die eine schnelle Entwicklung ermöglicht.

Zudem wurden unsere Git-Kenntnisse durch die Arbeit an diesem Projekt verbessert, da wir unsere Versionsverwaltung mit GitHub verwaltet haben.

# Anhang

## Programmlisting

## Projektverlauf und Versionsverwaltung
