# How to use OpenNerve Gen2 Development Board
Alex Baldwin
Jan 21, 2026

## Instructions
1.	Attach jumper between VRECT and the outside pin of VBAT2R (see picture below) to avoid needing magnet to wake up device
2.	Carefully insert USB-C port for power
3.	Open ONrecorder.sln in Visual Studios
4.	Press the green play button at the top of the GUI to start the program
5.	Device will automatically start a Bluetooth handshake that will last several seconds, with steps visible in the bottom left corner of the UI
a.	During the handshake you will see messages transmitted and received in the “Messages” area of the form (see image below). Once the handshake is complete, you should see “Connected” or “Connected+” in both TX and RX. 
b.	If no form appears, double check that the jumper is placed and that it is in the correct position (see image below)
c.	If the Bluetooth handshake does not work, press Quit or the red Stop button on Visual Studios, press the reset button on the OpenNerve board, and try to start the program again
i.	It is often necessary to press the reset button just before pressing the green Play button
6.	Press “Get Params” to read the current stimulation settings off of the OpenNerve board
a.	Note: this will take several seconds, and may stop if another message (such as Connect+) is sent during parameter reading. If so, press Get Params again, or press “Get” next to the specific parameters you are interested in.
b.	Parameter explanation:
i.	Cathode – the first electrode under test
ii.	Anode – the second electrode under test
iii.	Amp – current amplitude in milliAmps
iv.	Width – pulse width in microseconds
v.	Freq – frequency in Hz
vi.	Ramp – the time in which current will ramp from zero to the target amplitude. Set to 0 for testing.
vii.	Train On – the time in seconds at which stimulation will continue. Set to any number for testing
viii.	Train Off – the time in seconds at which stimulation will pause between train cycles. Set to zero for testing
7.	To change a parameter, change the value in the text box and then press “Set”. 
a.	Note: parameters cannot be changed during stimulation. You may adjust parameters when stimulation is ongoing, but you will need to press Stop Stimulation and then Start Stimulation for the changes to take effect.
b.	It is often useful to press “Get” after changing a setting to make sure that the OpenNerve board received it correctly.
8.	To start stimulation, press “Start Stimulation”. It will take 2-3 seconds for stimulation to start.
9.	To stop stimulation, press “Stop Stimulation”. Stimulation should stop within 1 second.
