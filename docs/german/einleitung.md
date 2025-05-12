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