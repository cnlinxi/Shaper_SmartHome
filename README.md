# Shaper SmartHome
A smart-home project. 

This project win the second prize of ImagineCup 2016 China (Shaper).

### Project features

- Come into your house by face detection, your devices will receive the message timely.
- The strangers attempting to get into your house will be prevented and a photo will be uploaded.
- Get information of house.
- Remote control.

![](http://7xrvee.com1.z0.glb.clouddn.com/18-7-15/79133638.jpg)![](http://7xrvee.com1.z0.glb.clouddn.com/18-7-15/67231732.jpg)![](http://7xrvee.com1.z0.glb.clouddn.com/18-7-15/25251786.jpg)![](http://7xrvee.com1.z0.glb.clouddn.com/18-7-15/64726086.jpg)

*On the left is the mobile client interface and on the right is the PC interface*



![](http://7xrvee.com1.z0.glb.clouddn.com/18-7-15/15966571.jpg)

*A simple IoT physical picture :)*

### Get started

We write UWP client which can run on PC&phone. The Raspberry Pi is used as a hardware device on the IoT side.  We set up a server on Microsoft Azure and use its Face Recognization API and Blob storage. Thanks to Microsoft :)

### How use it

Just open your visual studio and add this three projects into the solution. Press the "run" of the toolbar to start them. Three projects are server side, client side and IoT side.

### File Structure

- Client: A UWP application which run on PC or phone. Display data and provide control interface.
- IoT: A UWP application which run on Raspberry Pi. Collect data and send them to server.
- Server: A asp.net application which run on server. Deal with data , save data and send data to client to display them. It connects client and IoT.

### Connect

[cnmengnan@gmail.com](mailto:cnmengnan@gmail.com)

blog: [WinterColor blog](http://www.cnblogs.com/mengnan/)

enjoy it

