# Project Description
This project is a virtual reconstruction of a Stockholm subway station. It will be used in a reasearch project that aims to understand how people react to different elements in enclosed spaces such as subway stations. 
The reconstruction of the subway station is based of Rådmansgatans subway station in Stockholm, but various elements have been added to alter the appearance, so it isn't an exact replica. See below:

<img width="615" alt="image" src="https://github.com/JulianLey/MetropedInteractive/assets/146943186/eaa12f23-9702-435f-8f7c-b9900cec77d7">

A menu has also been added so that various elements (such as the glass walls lining the rails) can be removed and added for comparison. In the same menu the user also has the possibility to rate the current setup:

![image](https://github.com/JulianLey/MetropedInteractive/assets/146943186/d078cfb4-b84c-4bf1-a09f-6e94f1f2a1de)

The results are then saved locally to a JSON file (The adress of this file is written out in the console when you rate and save a setup. See console for the path if you have trouble finding this file). This will look something like this:

![image](https://github.com/JulianLey/MetropedInteractive/assets/146943186/3c9208cd-8a74-423c-bc98-7a52dda075b2)

The participant will be able to move through the scene with the WASD keys and in VR (only the WASD version has been tested on this version of the project).
There are also moving trains and a virtual crowd that move through the scene.

![image](https://github.com/JulianLey/MetropedInteractive/assets/146943186/760bb78e-7ad5-4149-836d-aaea4105e30f)

# Installation Instructions
- Install Unity Hub on your computer.
- Install editor version 2022.3.18f1
- Clone the repository to your local computer
- Open the cloned project folder (MetropedInteractive) in Unity Hub using the "Add from disk" button
- Open the project with the 2022.3.18f1 editor
- In the project window navigate to "Assets" and doubleclick "FinalScenePVK" to open the scene

# Running a basic version of the project
- Press on the play button in the Editor
- Navigate to the ribbon at the top of the Game View window and make sure Display 1 is selected to try the WASD implementation.


# Issues with the Crowd
Currently the project isn't buildable (we can't create an executable/application out of it) due to the Crowd giving certain errors (The project is buildable without the Crowd assets).
Errors:
- "Cannot build player while Editor is importing Assets or compiling scripts" --> This could be due to the custom Editor for the Crowd. Assets --> AssetsCrowd --> Editor --> CustomEditor    && Assets --> Scripts --> CustomEditor
- "Assets\Scripts\CustomEditor.cs(5,37): error CS0246: The type or namespace name 'Editor' could not be found (are you missing a using directive or an assembly reference?)"
- "Assets\Scripts\CustomEditor.cs(4,2): error CS0246: The type or namespace name 'CustomEditorAttribute' could not be found (are you missing a using directive or an assembly reference?)"
- "Assets\Scripts\CustomEditor.cs(4,2): error CS0246: The type or namespace name 'CustomEditor' could not be found (are you missing a using directive or an assembly reference?)"
- "Assets\Scripts\CustomEditor.cs(4,30): error CS0246: The type or namespace name 'CanEditMultipleObjectsAttribute' could not be found (are you missing a using directive or an assembly reference?)"
- "Assets\Scripts\CustomEditor.cs(4,30): error CS0246: The type or namespace name 'CanEditMultipleObjects' could not be found (are you missing a using directive or an assembly reference?)"
- "Assets\Scripts\CustomEditor.cs(7,9): error CS0246: The type or namespace name 'SerializedProperty' could not be found (are you missing a using directive or an assembly reference?)"

As you can see these errors all are related to the Custom Editor.

There are also many warnings that appear that are related to the Crowd files when you try to build the application. Since there are several different warnings, I will not display them all here. To find them go to [File --> BuildSettings --> Build ] and choose a destination folder for the build. The warnings will appear in the console window of Unity.

However there are also warnings that appear when you run the project:
  - "The referenced script (Unknown) on this Behaviour is missing!
UnityEngine.Resources:LoadAll (string)
Spawner:init (UnityEngine.GameObject&,UnityEngine.GameObject&,Agent&,MapGen/map&,System.Collections.Generic.List`1<Agent>&,UnityEngine.Vector2,UnityEngine.Vector2,single,bool,bool,single,single,single,single) (at Assets/Scripts/Spawner.cs:60)
Main:Start () (at Assets/Scripts/Main.cs:153)"

