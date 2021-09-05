# GameLib.VersionHistory v.1.0
GameLib.VersionHistory is a module for GameLib which helps you to manage version history of the game. It uses git commit messages for describe changes. Git tags to distinguish version numbers. And prepares data for unity project so you can display it in a convinient way from the debug menu of the game.
Quick start tutorial:

* Copy PrepareChangeLists directory outside Unity project. 
* Run **PrepareCL.py v.0.1.a v.0.2.a** to obtain change list between 2 versions using git tags.
* Edit obtained files in the **edited** directory if needed
* Run edited/copy.cmd to copy your change lists to unity project. Also you may need to edit **copy.cmd**  to change your project path for the first time you run the pipeline

Unity will import new CL into **VersionHistory** scriptable object. And then your ingame debug menu able to show those changes in a good readable way. To do that via the GameLib DevMenu system there is a DbgVersionHistory control and prepared full screen prefab do display changes.

## Commit convention to support ChageLists
The whole team should use current convention to make it possible to automate VersionHistory system and make commit messages in more standard and readable look. Using special script we can gain information about commits from one specific tag to another (for example: from tag v.1.06 to tag 2.0) and automatically form the version history for the game based on parsed commit messages. That version history could be available from the DevMenu, so inside the game we can always see which set of changes comes with that version.

### Commit comment convention
Every commit message should follow the rules below. If it doesn’t follow then information from the commit message will not go to the version history ( will be ignored by script).

The scheme of the commit message:

	[Symbol][Comment][tags]

Symbols:

	*  Shows the class of the action done by commit: any generic changes of functionality, logic, content (majority of commits could be treated as just changing of something)
	!  Bug fix, or fix something. In art, in content, in code
	-  Removing of something. Removed file, removed method, removed functionality 
	+  Something was added. File, feature, art asset, etc.
	~ Tuning, polishing, adjusting, refactoring. Number was changed, configuration was adjusted, something was moved in GUI, etc. Something that doesn't affect the logic
	-_- Forgot something from previous commit and we just add it in the current commit. It’s annoyed emoji 

Tags:

You can specify any amount of tags at the end of commit message. It could help you to show messages in different way in game menu, like different color or filtered by sections.
Examples of commit messages:

	! Menu bug fixed. #gui #content
	+ Art: Added button image #art #gui
	* Changed input logic #code
