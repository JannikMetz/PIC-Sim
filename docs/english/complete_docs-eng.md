# Table of Contents

1. [Introduction](#introduction)  
2. [General Information](#general-information)  
    2.1 [The PIC16F84](#the-pic16f84)  
    2.2 [Functionality of a Simulator](#functionality-of-a-simulator)  
    2.3 [Advantages and Disadvantages of a Simulation](#advantages-and-disadvantages-of-a-simulation)  
    2.4 [Program Interface and Usage](#program-interface-and-usage)  
3. [Implementation](#implementation)  
   3.1 [Description of the Basic Concept](#description-of-the-basic-concept)  
   3.2 [Programming Language Used](#programming-language-used)  
   3.3 [Function Description of Selected Instructions](#function-description-of-selected-instructions)  
   3.4 [Flag Implementation](#flag-implementation)  
   3.5 [Interrupt Implementation](#interrupt-implementation)  
   3.6 [Implementation of the TRIS Register](#implementation-of-the-tris-register)  
   3.7 [EEPROM State Machine](#eeprom-state-machine)  
4. [Summary](#summary)  
   4.1 [Achieved Functionality](#achieved-functionality)  
   4.2 [Conclusion and Personal Experiences](#conclusion-and-personal-experiences)  


# Important Note
This documentation is a translation of the original German documentation. 
This translation was 100% done by [OPENAI's ChatGPT](https://chatgpt.com/) and has **NOT** been manually checked.

If you find any mistakes, please report them to the author of this project.

If you want to read the original German documentation, you can find it [here](../german/complete_docs-ger.md).

# Introduction

The Pic-Sim is a simulator for the PIC16F84 microcontroller, developed as part of a graded submission for the course "System-Level Programming 2" in the Information Technology program at the Cooperative State University Karlsruhe (DHBW KA).  
The purpose of developing this simulator is to deepen understanding of how microcontrollers work.

The backend of this project was programmed in C#, and the interface was created using [Avalonia UI](https://avaloniaui.net/).  

The simulator allows for software simulation of the PIC16F84 to test and debug assembler programs in LST format.  
Since this is a software simulation and to simplify the concepts, CPU cycles and real-time execution are not enforced.  
Such a simulator can be used for educational purposes as well as for debugging and analysis of microcontroller programs. The ability to test code in a controlled environment without the need for physical hardware offers significant benefits—for example, for quickly testing small program sections or analyzing edge conditions that are difficult to reproduce in real life.

This documentation describes the simulator's development process. It explains the underlying concepts, technical implementation, and selected program parts and their functionality in detail. The goal is to make it understandable for outsiders how the simulator works, what features were implemented, and which considerations were made during development.  
The documentation is structured so that the functionality and structure of the project can be understood independently of the source code and without running the software.

A shortened and corrected datasheet of the PIC16F84 can be found in the `docs` folder.

# General Information

## The PIC16F84

The PIC16F84 is a widely used 8-bit microcontroller from Microchip's PICmicro™ series, commonly used in education, prototyping, and small control tasks.  
It is based on an advanced RISC architecture known for its efficiency and performance, enabling nearly all instructions to be executed in a single clock cycle—except for jump instructions, which require two cycles.

### Technical Features

- RISC Architecture: 35 instructions (Reduced Instruction Set), with a two-stage pipeline for fast execution
- Harvard Architecture: Separate buses for instructions (14-bit) and data (8-bit), allowing parallel access
- Memory:
  - 68 bytes RAM
  - 64 bytes EEPROM for non-volatile data storage
  - 1K x 14-bit Flash program memory
- I/O Functions:
  - Up to 13 digital input/output pins
  - Integrated Timer/Counter
- Clock Sources:
  - Four oscillator types: RC (simple and cheap), LP (low power), XT (standard crystal), HS (high-speed crystal)
- Power-Saving Features:
  - SLEEP mode
  - Watchdog timer with internal RC oscillator as a safeguard against software crashes
- Interrupt Sources: Both internal and external interrupts available
- Flash Technology:
  - Supports in-circuit reprogramming, ideal for prototyping and updates in installed state
  - Suitable for serial numbers, calibration data, or late firmware programming

### Applications

The PIC16F84 is well-suited for a variety of applications, such as:

- Motor control in automotive and home appliances
- Security and access systems
- Smart cards
- Power-saving sensor technology in remote systems
- Space-constrained applications

## Functionality of a Simulator

A simulator is a tool that attempts to replicate the reality of a system or process in a controlled environment.  
Depending on the accuracy of the simulation, different aspects of the system can be emulated with varying precision.

In this project, a PIC16F84 microcontroller is simulated using algorithms and data structures in a software environment.  
The simulation operates purely on a logical level that mimics the microcontroller's functions. No electrical components or physical hardware are simulated.

Thus, system variables such as voltage, current, power consumption, temperature, and other conditions are irrelevant in this project.

## Advantages and Disadvantages of a Simulation

A simulation brings both advantages and disadvantages, depending on the context and purpose of its use. Here's a clear comparison for this project:

### Advantages
| Advantage | Reason |
| --------- | ------ |
| **Cost savings** | No hardware required |
| **Error analysis** | Breakpoints, step-by-step execution, register monitoring enable precise debugging |
| **Flexibility** | Fast code testing without hardware modifications (e.g., flashing) |
| **Portability** | Simulations can be run on different computers regardless of physical hardware |

### Disadvantages
| Disadvantage | Reason |
| ------------ | ------ |
| **Limited accuracy** | Hardware-specific details like timing, signal behavior, and interference are not considered |
| **Lack of interaction** | Physical components like sensors, motors, or actuators cannot be tested directly |
| **Performance differences** | Timing behavior may vary significantly from the real microcontroller—critical timing tests are difficult |
| **Hardware-related problems not detected** | Issues like soldering errors, voltage drops, electrical noise, or thermal effects remain hidden |

## Program Interface and Usage

The simulator is operated via a GUI. The navigation bar contains 3 menus. The first, **File**, allows loading a simulation file. Under **Settings**, simulation speed can be adjusted and EEPROM memory can be cleared for testing purposes. The **Documentation** menu opens this documentation.  
On the left side of the main window is the PIC memory view; clicking a cell allows changing the stored value (entered in hexadecimal).  
The top right of the window shows key registers of the PIC, including Port A and B and their corresponding TRIS registers. With the help of checkboxes, users can configure TRIS and set ports, provided they are inputs.  
Other displayed registers include Status, Option, and Intcon. The Watchdog timer and Stack are also shown. The quartz frequency can be selected via a combo box.  
The third block, bottom right, displays the loaded LST file. Breakpoints can be set on the left side. On the right are buttons to control simulation:  
- **Run** starts execution  
- **Step Forward** executes the next instruction  
- **Skip To Next** skips the next instruction without executing it  
- **Pause** halts at the current instruction  
- **Reset** fully resets the simulator  

![Pic Simulator GUI](images/PicSimulatorGUI.png)

# Implementation

## Description of the Basic Concept

The loaded LST file is converted into an array of instructions, stored as hexadecimal values. The program counter determines the current instruction’s index.  
When execution starts, the program counter is set to 0 and the first instruction is executed.  
The decoder interprets the instruction by masking the hex value to identify the command and then calls the corresponding function. This function modifies the PIC16F84 registers accordingly.  
Most instructions increment the program counter by 1. Exceptions are jump instructions and `BTFSx` instructions, which may skip or change the counter.  
The Timer and Watchdog Timer (if enabled) are updated with every instruction.

## Programming Language Used

The simulator’s backend is written in C#. C# was chosen due to our familiarity and the robust .NET development environment.  
The frontend was built using Avalonia UI. While WPF was initially considered, it is Windows-exclusive. Avalonia UI is a cross-platform open-source alternative inspired by WPF.  
JetBrains Rider was used as the IDE, a cross-platform IDE for C# and .NET.

## Function Description of Selected Instructions

- **BTFSx**: Tests a specific bit and skips the next instruction if a condition is met.  
  - `BTFSS`: skip if bit is 1  
  - `BTFSC`: skip if bit is 0  
  Useful for implementing loops.
- **Call**: Calls a subroutine. The current program counter is pushed to the stack, and control jumps to the subroutine address.
- **MOVF**: Copies a register's value to W or back to itself depending on the destination bit.  
  Sets the Zero flag if the result is 0; does not affect the Carry flag.

## Flag Implementation

Flags are part of the Status register and indicate processor states:

- **Z (Zero Flag)**: Set if result is zero; used for comparisons and jumps.
- **C (Carry Flag)**: Set if overflow occurs during arithmetic.
- **DC (Digit Carry Flag)**: Set during BCD operations.

**Note:** Due to a hardware bug in the PIC16F84, Carry and Digit Carry are inverted for subtraction operations.

## Interrupt Implementation

Interrupts allow responding to external events.  
Before each instruction, the simulator checks if an interrupt occurred. If so, the program counter jumps to 0x0004 (ISR start).  
After executing the ISR, the program resumes at the interrupted instruction.

## Implementation of the TRIS Register

The TRIS register controls I/O direction.  
In the simulator, if a pin is configured as input, its value can be set using checkboxes.  
Changes to internal memory for input pins are only reflected when the pin becomes an output.

## EEPROM State Machine

EEPROM is simulated via a text file (`EEPROM.txt`).  
Writing involves setting EEDATA and EEADR, then starting the write via a special sequence in EECON2 and a flag in EECON1.  
The write process takes 1ms (1000 instructions at 4 MHz).  
Once complete, data is written, and an interrupt signals completion.

Reading is immediate—set EEADR, trigger via EECON1, and data is loaded into EEDATA.

# Summary

## Achieved Functionality

This version of the PIC Simulator can simulate and execute assembler programs in LST format.  
All necessary functions of the PIC16F84 for such simulations have been implemented.

## Conclusion and Personal Experiences

Developing the PIC Simulator was an educational experience that deepened our understanding of microcontrollers and their programming.  
Topics not covered in prior coursework (e.g., interrupts, watchdog, EEPROM) became clear during implementation.  
Working with Avalonia UI was initially challenging, particularly due to the MVVM pattern. Once we understood `INotifyPropertyChanged`, UI development became smoother.  
Choosing C# for the backend was beneficial due to its robust ecosystem.

Our Git skills also improved through version control using GitHub.
