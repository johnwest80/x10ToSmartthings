x10ToSmartthings
================

This is a version that works.  You need to set up the hardware stack as shown below...

Smartthings Shield (https://shop.smartthings.com/#!/products/smartthings-shield-arduino)
------------------
Rs232 shield (https://www.sparkfun.com/products/11958)
------------
Netduino Plus V2 (https://www.sparkfun.com/products/11608)

Steps:

Set the Smartthings shield toggle to use D2 and D3 (the switch didn't work for some reason, so I had to jumper d0 and d1 over to d2 and d3).
Plug a Cm11A x10 interface into the rs232 shield.
Plug the Cm11A into the wall.
Plug the netduino plus into power, either a dedicated power adaptor or usb power.
Connect the Smartthings shield to your hub (follow the Shield instructions)

That is your hardware stack.  On the developer portal, you'll change the Smartthings shield to a device type I created, and you'll run a Smartapp I created that allows you to map x10 switch/sensor changes to x10 commands, and another Smartapp I created to map x10 commands to lights and such on Smartthings.

If you get to the point you need the Smartthings device type and Smartapp, ask me and I'll get them to you.  I'm constantly tweaking them and I haven't looked into how to deal with them in source control, since they are edited online.

Hopefully this helps!  I know it seems like quite a bit of work, but it does work.

Caveats...

Sometimes there's a 2-3 second delay between x10 and ST and vice versa.  But I've never once had the events not fire.
Only on and off are supported for x10.
