FunScript.Tutorial
==================

Commented examples for [FunScript](http://funscript.info/) (F# to JavaScript compiler).

The examples have been tested in Windows and Mac. There's no project, just F# script files (.fsx) which you can execute with Visual Studio (Windows), Xamarin Studio (Windows and Mac) or even with the stand-alone F# interactive. I haven't tested the scripts with Linux or other IDEs but they should work as well.

You can use any server to run the examples, but for convenience there are two simple node.js and .NET/Mono servers included.

- *[First time only]* [Clone](github-windows://openRepo/https://github.com/alfonsogarciacaro/FunScript.Tutorial) or [Download](https://github.com/alfonsogarciacaro/FunScript.Tutorial/archive/master.zip) the FunScript.Tutorial project.

To run the node.js server:

1. *[First time only]* If necessary, install [node.js](http://nodejs.org/) in your computer to run the server.
2. Open a terminal window.
3. Go to the directory where you copied the project and then to the ```/tutorials``` folder.
4. *[First time only]* Run the following command in the terminal: ```npm install finalhandler```
5. *[First time only]* Run the following command in the terminal: ```npm install serve-static```
6. Run the server with the command ```node nodeserver.js```. If you don't pass a port number, it will default to 8080.

To run the .NET/Mono server instead:

1. Open ```/tutorials/netserver.fsx``` with Xamarin Studio or Visual Studio and send it to F# interactive. You can change the ```port``` value (8080 by default) on top of the script if necessary.
2. To close the server later, just type ```server.Stop();;``` in F# interactive or finish/restart the F# interactive session.

To run the examples:

1. Open ```http://localhost:8080/``` in your browser. There will be nothing the first time.
2. Now you can run any example. Let's start with HelloWorld. Open ```/1_HelloWorld/Program.fsx``` with Xamarin Studio or Visual Studio and send it to F# interactive. The script will be compiled to JavaScript, and then the generated code together with the accompanying HTML file will be copied to the parent directory (```/tutorials```).
3. Refresh the browser to see the example in action.

See the comments in each script for more details. Please open an issue if you have a problem or any suggestion. You can also tweet @alfonsogcnunez if you find the project useful.

NOTE: More examples to come but for the moment, please don't expect them to be in a particular order. Also, make sure the assemblies in ```/lib``` are synced with the repository when trying new examples.

Have fun(script)!


