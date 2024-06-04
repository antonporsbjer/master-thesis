+--------------------------+
|  DotMatrix - ReadMe.txt  |
+--------------------------+



Introduction
============


DotMatrix is low resolution display for example to clocks, elevators, roadsigns or to show scrolling texts.

Normally each dot in display is object that changes color (or rotation/texture in case of 3D display) to show wanted content. Content can be added to
display with simple commands or dot by dot. Additionally there's also texture based display where whole display is single object.



How to use
==========


Choosing prefab
---------------


Prefabs folder have 4 subfolders. Choose the one that is suitable for your purposes:

- "Sprite-based_for_3D_or_2D_world" contains DotMatrix display that can be used in 3D or 2D worlds as part of the scene. Dots are made of sprites so
  display is totally flat and is typically embedded to another object that provides background and borders for the display. Typically, this is the one
  to use, unless you wish to add DotMatrix display to your UI.

- "Object-based_for_3D_world" contains DotMatrix where dots are actual 3D gameobjects that rotates or change texture when dots in display turns on or off.
  This is for you if you want everything in your scene to be real 3D.

- "Image-based_for_UI" contains DotMatrix that is meant to be used as part of UI. Dots in display are images that change colors when turning on or off.

- "Texture-based" contains special DotMatrix version that doesn't create any dot objects, but display is generated to Texture2D. This is for you if you
   need extra big displays or resources are limited. On downside, this display doesn't have as much customization as others and setting up also requires
   some extra steps.

Examples folder contains lots of scenes that show examples of all these four types.



Add DotMatrix display to scene (other than texture based)
---------------------------------------------------------


Simply drag prefab to your scene.

Note, if you are using UI-version of prefab: When dragging prefab to hierarchy or scene, prefab should automatically set UI Canvas as its parent. If there is
no other UI and no UI Canvas in scene yet, one is created automatically. However, it's recommended to first create UI Canvas using normal Unity Editor tools
before adding UI-version of DotMatrix to the Scene.

At this point display should be already visible on scene, though its size and colors may be totally wrong. Choose created DotMatrix gameobject and check
inspector window. It contains multiple settings where you can choose display size, dot style, sizes and colors.

Choose desired parameters and position DotMatrix display to your scene/UI.

DotMatrix dots are created from prefabs. There are multiple ready made simple prefabs (different shape of dots). If you wish to have more control on how
display looks, you can make your own dot prefabs. After creating your own dot-object prefab, just drag that to "Dot Prefab" field in Unity inspector window.



Add DotMatrix display to scene (texture based)
----------------------------------------------


Drag prefab to your scene. You won't directly see anything as this type of display won't generate dot objects.

Create your own object where you want to show the display. In simplest case this can be Quad. Create new material and add that to your gameobject.
Select DotMatrix object from scene and drag your new material (from Project window) to Target Material field visible in Inspector. After that you
can click "Create Dots in Editor". DotMatrix will then create texture and set it to the target material. Display should be visible in the gameobject
using this material.

You can then do the same setting up as in previous chapter above.

To make display look prettier, you can also set shader for the material. Just choose shader "DotMatrix / DotMatrix DotDisplay" for the material.
Shader have several properties (sliders) to change the appearance of dots displayed.



Accessing DotMatrix
-------------------


Create new script that will be used to control DotMatrix display. Use any way you prefer to give your script reference to DotMatrix script in gameobject
created earlier. Make sure your own script is "using Leguar.DotMatrix;"

You can then get Controller of DotMatrix display using:

Controller controller = myDotMatrix.GetController();

Controller can be used to feed new commands to display. For example:

TextCommand textCommand = new TextCommand("Hello world!");
controller.AddCommand(textCommand);

This will cause text "Hello world!" to appear on display. By default it will be on center of the display.



Controller commands
-------------------


Commands can be added to controller before previous one is finished. All commands go to queue and they'll run in order they were added, one by one.

Basic commands are following:

- "TextCommand" adds text to display. Most common command to use.
- "PauseCommand" adds delay before Controller continues from next command. Whatever is currently on display will stay there.
- "ClearCommand" clears (or fills) the display. There are different types of methods to clear the screen.

For example, following script would scroll text from left to center of display, wait for 2 seconds and then scroll text away:

TextCommand textCommand = new TextCommand("Hello and bye") {
    HorPosition = TextCommand.HorPositions.Center,
    Movement = TextCommand.Movements.MoveLeftAndStop
};
controller.AddCommand(textCommand);
controller.AddCommand(new PauseCommand(2f));
controller.AddCommand(new ClearCommand() {
    Method = ClearCommand.Methods.MoveLeft
});

Some more advanced commands:

- "ContentCommand" adds any free content to display, defined by two-dimensional integer array.
- "CallbackCommand" will call Action given as parameter when this command is reached in Controller queue.



Direct access
-------------


If you wish to access display directly, you can use DisplayModel:

DisplayModel displayModel = myDotMatrix.GetDisplayModel();
displayModel.SetDot(0,0,1);

That will set upper left corner dot (0,0) to "on state" (1) during this or next Update loop (depending on script execution order).



Complicated content
-------------------


Typically display like this shows just some simple text. If you want to add for example some more complicated image to display,
you can use ImageToContent script. That script contains static method to turn 2D texture (like sprite) to content that can be then
added to display using ContentCommand or added directly using DisplayModel.

However, this is just extra functionality and there are multiple things to note:

1. DotMatrix style display resolution is typically very low but even more limited is its colors. ImageToContent script does
   its best to match texture pixel colors to dotmatrix dot colors, but you will definitely get the best results if source
   texture contains only few, clear colors and DotMatrix where it is added is using exactly same color palette as source texture.
2. Make sure that source texture have "Read/Write Enabled" in texture import settings so ImageToContent script can read the
   pixels of the texture.
3. Also it is recommended to disable any compression of the source texture (set it to use true RGB 24bit). Compression
   may change colors of some pixels resulting broken looking content.
4. ImageToContent can not be used for 3D style DotMatrix displays since they do not have any dot colors defined.



Additional documents
====================


Inline c# documents are included to all public classes. They are also available online in html format:

http://www.leguar.com/unity/dotmatrix/apidoc/2.5/


There is also multiple example scenes containing lots of example displays and their scripts included in this package.
Take a look and feel free to modify and use any parts of the examples in your own projects.



Feedback
========


If you are happy with this package, please rate us or leave feedback in Unity Asset Store:

https://assetstore.unity.com/packages/slug/75420


If you have any problems, or maybe suggestions for future versions, feel free to contact:

http://www.leguar.com/contact/?about=dotmatrix
