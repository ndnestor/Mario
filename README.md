# Mario
This project is a C# library that abstracts inter-process communication using anonymous pipes. This keeps things simple as you only need to deal with two classes in order to get things running. Who better to do deal with pipes than Mario himself?

## Usage
### Starting the pipe
Creating a pipe between two applications is simple. One program, known as the server, will request the IPC. The other, known as the client, will accept this request. This is the setup for the server:
```c#
// Create a process object for the other process we want to communicate with
Process myProcess = new();
myProcess.StartInfo.FileName = @"path\to\other\executable";

// Create the pipe settings with our process
PipeSettings mySettings = new();
mySettings.TargetProcess = myProcess;

// Create a pipe using our settings
Pipe myPipe = new(mySettings);

// Start the target process and facilitate IPC
myPipe.Start();
```

Accepting the servers request from the client is even simpler.
```c#

// Create the pipe
// When you don't specify your own pipe settings, default values will be used
// The client process should not specify a target process
Pipe myPipe = new();

// Accept the servers request for IPC
myPipe.Start();
```

### Sending messages
Sending a message to the other process from either the server or client is a simple one liner:
```c#
// This message will be passed from the current process to the other
myPipe.FlowOut("This is a pipe messaage!");

// You can also send multiple messages at once
// by passing anything that is derived from IList<string> such as an array of strings
myPipe.FlowOut(new string[] { "First message", "Second message", "Third message" });
```

### Receiving messages
There are two ways to receive incoming pipe messages. The simplest way goes like this:
```c#

// Store the received pipe messages in a variable
string[] incomingMessages = myPipe.GetContents();

// Pipe contents will accumulate unless it is specified that it should be cleared after reading its contents
// by using the Pipe.GetContents(bool clearContents) overload. When set to true, the contents will be cleared
// The pipe contents that were present before clearing will still be returned
incomingMessages = myPipe.GetContents(true);
```

The second way to read messages is using a callback method which must be set in the pipe settings **before** calling `Pipe.Start()`. It would look like this:
```c#

// Set which method to use as the callback
PipeSettings mySettings = PipeSettings.new();
mySettings.FlowInCallback = MyCallback;

// The callback must accept one parameter only of type string[]
// This method will be called whenever one or more messages have been received
void MyCallback(string[] messages) {
  
    // You can do whatever you want with the new messages in here
    Console.WriteLine(messages);
}
```
There are some notable differences in operation between these two ways. Firstly, `GetContents()` will return the history of *all* messages ever received by the pipe unless it is cleared whereas using the callback method will only pass through *new* messages that have arrived since the last time it was called. Secondly, `GetContents()` executes on whatever thread it was called on but the callback method will run on a different thread each time.

### Pipe Settings
All pipe settings must be set before starting the pipe in order for them to have any effect. There are a number of pipe settings that weren't covered in the examples above so here are all of them and their default values:
```c#

// How often the pipe checks for new incoming messages in milliseconds
int FlowInterval = 33;

// Set true if you intend to use the GetContents() method. Set false otherwise. This is for optimization reasons
bool SaveContents = true;

// The client process that the server process will create. Must be left as null by the client process
Process TargetProcess = null;

// The action to call when one or more messages are received
Action<string[]> FlowInCallback = (string[] _) => { /* Intentionally empty */ };

// For now, don't touch this. No other connection types are implemented yet
Pipe.ConnectionType ConnectionType = Pipe.ConnectionType.CmdArgs;

// Sets whether the pipe can send messages, receives messages, or both
Pipe.FlowDirection FlowDirection = Pipe.FlowDirection.Bidirectional;
```

## Sending complex objects
Passing non-string objects through a pipe can effectively be done by using something like Newtonsoft.Json which has tools to convert objects into strings and vice versa.

## Limitations
As it currently stands, command line arguments 1 and 2 are reserved by Mario on the client process. I will be looking into a way around this in the future!
