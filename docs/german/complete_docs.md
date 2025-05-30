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
   3.9 [Hardwareansteuerung (optional)](#hardwareansteuerung-optional)  
   3.10 [EEPROM-State-Machine](#eeprom-state-machine)  
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



## Beschreibung der Gliederung

## Programmstruktur und Ablaufdiagramme

## Verwendete Programmiersprache

Das Backend des Simualators ist in C# programmiert. C# wurde aufgrund unserer familiarität mit der Programmiersprache gewählt. Zudem bietet C# durch .NET eine zuferlässige Entwicklungsumgebung. Das Frontend wurde Mit Avalonia UI gebaut. Die erste Überlegung war es WPF zu nutzen, WPF ist ein UI Framework für .NET Applicationen. Es war wichtig das der Simulator Platform übergreifend funktioniert, vorallem auf Windows und Linux. Da WPF ein Windows exlusives Framework ist fiel die Entscheidung auf Avalonia UI. Avalonia UI eine platformübergreifende Opensource-Alternative zu WPF. Es ist vom WPF inspiriert und hat sehr ähnliche Features. Als IDE wurde JetBrains Rider verwendet. Bei Rider handelt es sich um eine Platformübergreifende IDE für C# und .NET.

## Funktionsbeschreibung ausgewählter Befehle

## Realisierung der Flags

## Implementierung von Interrupts

## Realisierung des TRIS-Registers

## Hardwareansteuerung (optional)

## EEPROM-State-Machine

# Zusammenfassung

## Erreichte Funktionalität

## Fazit und persönliche Erfahrungen

# Anhang

## Programmlisting

## Projektverlauf und Versionsverwaltung
