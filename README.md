# OpenNerve-Control-Software
The OpenNerve control software (“ONrecorder”) is a Windows-based desktop application written in C# (.NET, WinForms) for communicating with and controlling the OpenNerve implantable pulse generator over Bluetooth Low Energy (BLE). The application runs on 32-bit Windows (x86) and is built using Visual Studio 2022 with the .NET SDK; no additional third-party runtime dependencies are required beyond standard Windows BLE support. The software discovers and connects to the target device, performs an authentication handshake, and then exchanges control and data messages using a custom BLE protocol. To build the project, clone the repository, open the provided solution file (ONrecorder.sln) in Visual Studio, select the x86 build configuration, restore NuGet packages if prompted, and build/run the application. For instructions on how to get started applying stimulation using the App, see [user-guide.md](user-guide.md).

Admin authentication require adding a private key in a text file to a separate folder; contact us at carss@usc.edu to get the text file. We have also included instructions on how to generate your own private and public keys and to modify this software, along with the OpenNerve MCU firmware, to use them. You can find instructions in the file [encryption-instructions.md](encryption-instructions.md).

This software is optimized for the Gen2 development board, but is able to be used for both the Gen1 and Gen2 boards. To use a Gen1 board, set the “BoardGen” constant at the top of the Form1.cs file to “1” instead of “2”.

**Note:** at present time, the software is designed such that, if no OpenNerve board is present and broadcasting over BLE, the software will not open up its user interface. If you are experiencing trouble try 1) pressing the reset button on OpenNerve and 2) ensuring that, if you are using a development board, that it is powered correctly. If you are powering a development board off of USB-C, a jumper may be connected between VRECT and the outside pin of VBAT2R to bypass the magnetic switch. See instructions for use of the Gen2 board for more details.

All code is released under the CC-BY 4.0 open source license. If you use this code in whole or in part, you must give us credit!

## Disclaimer

The contents of this repository are subject to revision. No representation or warranty, express or implied, is provided in relation to the accuracy, correctness, completeness, or reliability of the information, opinions, or conclusions expressed in this repository.

The contents of this repository (the “Materials”) are experimental in nature and should be used with prudence and appropriate caution. Any use is voluntary and at your own risk.

The Materials are made available “as is” by USC (the University of Southern California and its trustees, directors, officers, employees, faculty, students, agents, affiliated organizations and their insurance carriers, if any), its collaborators Med-Ally LLC and Medipace Inc., and any other contributors to this repository (collectively, “Providers”). Providers expressly disclaim all warranties, express or implied, arising in connection with the Materials, or arising out of a course of performance, dealing, or trade usage, including, without limitation, any warranties of title, noninfringement, merchantability, or fitness for a particular purpose.

Any user of the Materials agrees to forever indemnify, defend, and hold harmless Providers from and against, and to waive any and all claims against Providers for any and all claims, suits, demands, liability, loss or damage whatsoever, including attorneys' fees, whether direct or consequential, on account of any loss, injury, death or damage to any person or (including without limitation all agents and employees of Providers and all property owned by, leased to or used by either Providers) or on account of any loss or damages to business or reputations or privacy of any persons, arising in whole or in part in any way from use of the Materials or in any way connected therewith or in any way related thereto.

The Materials, any related materials, and all intellectual property therein, are owned by Providers.
