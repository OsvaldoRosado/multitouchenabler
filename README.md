MultiTouchEnabler
=========

Multitouch Gesture Support on Single-finger Synaptics Touchpads  
(and all types of Synaptics touchpads on software like VMware)

SynapticsMultiTouchEnabler is a Windows tool to get two-finger scrolling (and potentially other gestures) working on Synaptics touchpads that can only detect one finger. Absolutely no hardware-level multi-finger support is necessary for this tool to work.

Alternatively, SynapticsMultiTouchEnabler can be used to support touchpad scrolling in software that does not otherwise recognize touchpad events, like VMware.

SynapticsMultiTouchEnabler uses touchpad information supplied by the official Synaptics drivers to infer when multiple fingers are on the touchpad (through the width value of the touch event) and then uses this information to enable gesture support.

This program always runs in the taskbar, unless the icon is right-clicked and the user selects to close the program. Left-clicking on the taskbar icon shows a configuration window that allows setting the scroll inversion (scrolling is inverted "natural scrolling" by default) and the scroll speed, as well as viewing of about information. Any setting changes are saved for future runs.

For maximum utility, it is suggested that a shortcut to wherever you save the program is placed in your startup folder, so that the program need not be manually started to enable gesture support.

Note: This software requires .NET Framework 4.0 to be installed.  
.NET Framework 4 Client Profile is sufficient.

A binary can be found at: http://osvaldojr.com/wp-content/uploads/2012/07/SynapticsMultiTouchEnabler.exe

This tool is third-party and unofficial. As such, it is not supported or endorsed by Synaptics Incorporated.