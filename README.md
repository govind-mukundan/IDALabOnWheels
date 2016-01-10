# IDALabOnWheels
This is a PC application that serves a companion to the Engineers Without Boarders Asia sensor board used for intruducing children to sensors.
# Technology and Libraries
This is a WPF application, with rendering using modern (shader based) OpenGL (via **SharpGL**). The 3D model is loaded using **Assimp.NET**.
Communicaton to sensors is via a COM port.
# How to use
- Power up the board (and add the bluetooth device if not already added)
- Run the software (it's available on the start menu after install)
- Select the COM port corresponding to the Bluetooth module and click Connect.
- Communication should start automatically and you should be able to see any movement on the board reflected on the model in the software. The compass will also rotate to show the current orientation of the board with respect to magnetic North.
- (optional) You can select the "Rotate World" option if you want to move the 3D background instead of the sensor board model.
- (optional) If you want to see a plot of the raw data select the values you want to see via the Accelerometer / magnetometer / Gyroscope options on the top right corner.
    - Static Activity
        - Select the "Static Activity" radio button (selected by default) and click the "Start" button.
        - Wait for the 5 second count down timer.
        - Hold the board still after the count down, and a timer on the right corner will count the seconds.
        - If you move the board by more than 10 degrees with respect to the initial position (when the activity started) the activity will end and you will get a prompt. The timer will show the total time the activity lasted.
        - To restart the activity, click "Start" again.
    - Dynamic Activity
        - Select the "Dynamic Activity" radio button, and click "Start".
        - Wait for a 5 second count down timer.
        - Rotate the board along the Roll Axis (Z axes)
        - The current roll rate in RPM will be displayed on screen and the timer will count the total time for which the Roll rate exceeds 15 RPM.
        
# More Details
[Sensor Board Project](http://www.teacherstryscience.org/lp/sensors-and-wearables)

[EWB-Asia](http://engineeringgood.org/local-programmes/)
