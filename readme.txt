GameLib.VersionHistory v.1.0

* Copy PrepareChangeLists outside Unity project
* Run PrepareCL v.0.1.a v.0.2.a to obtain CL using git tags.
* Edit files in edited directory if needed
* Run edited/copy.cmd to copy your change lists to unity project. Also you may need to edit your project path for the first time

Unity will import new CL into VersionHistory scriptable object. And then your ingame debug menu able to show those changes in a good readable way.