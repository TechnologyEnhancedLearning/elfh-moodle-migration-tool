Running the application with arguments from Visual Studio
1.	Right-click on the Moodle-Migration project in the Solution Explorer and select Properties.
2.	Go to the Debug tab & select Open debug launch profiles UI 
3.	In the Command line arguments field, enter the arguments you want to pass.
	For example: /help

Or open a terminal in the \bin\Debug\net8.0 folderand running the following command:
	dotnet Moodle-Migration {option}
For example:
	dotnet Moodle-Migration /help

Running the project wothout any arguments will run the application in interactive mode.