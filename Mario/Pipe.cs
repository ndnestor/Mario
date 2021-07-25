using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Timers;

namespace Mario {
	public class Pipe {

		// Pipe settings
		// TODO: Set defaults
		public class PipeSettings {
			public Process targetProcess;
			public int flowInterval; // In milliseconds
			public Action<string[]> flowInCallback;
			public ConnectionType connectionType;
			public FlowDirection flowDirection;
		}

		private PipeSettings _pipeSettings;

		public enum ConnectionType {
			CmdArgs
		}

		public enum FlowDirection {
			In,
			Out,
			Bidirectional
		}

		// Pipe infrastructure
		private PipeStream _pipeIn;
		private PipeStream _pipeOut;
		private StreamReader _pipeInReader;
		private StreamWriter _pipeOutWriter;
		private readonly Timer _readPipeTimer = new();
		
		// Constructor
		public Pipe(PipeSettings pipeSettings) {
			
			// Set pipe settings
			_pipeSettings = pipeSettings;

			// Initialize pipe infrastructure
			/*void InitFlowIn() {
				_pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
				_pipeInReader = new StreamReader(_pipeIn);
				
				// Set up timer that reads pipe every ReadPipeInterval milliseconds
				_readPipeTimer.Interval = _pipeSettings.flowInterval;
				_readPipeTimer.Elapsed += FlowIn;
				_readPipeTimer.AutoReset = true;
			}

			void InitFlowOut() {
				_pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
				_pipeOutWriter = new StreamWriter(_pipeOut);
			}
			
			switch(pipeSettings.flowDirection) {
				case FlowDirection.In:
					InitFlowIn();
					break;
				case FlowDirection.Out:
					InitFlowOut();
					break;
				case FlowDirection.Bidirectional:
					InitFlowOut();
					InitFlowIn();
					break;
			}*/
		}

		// Starts the transfer of data
		public void Start() {
			
			Action startInFlow;
			Action startOutFlow;

			switch(_pipeSettings.connectionType) {
				case ConnectionType.CmdArgs:

					string[] cmdArgs = Environment.GetCommandLineArgs();
					
					startInFlow = () => {
						if(_pipeSettings.targetProcess == null) {
							
							// This is the client process
							string pipeInHandle = cmdArgs[1];
							_pipeIn = new AnonymousPipeClientStream(PipeDirection.In, pipeInHandle);
						} else {
							
							// This is the server process
							_pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
							_pipeInReader = new StreamReader(_pipeIn);
						}
						
						_readPipeTimer.Start();
					};

					startOutFlow = () => {
						if(_pipeSettings.targetProcess == null) {
							
							// This is the client process
							string pipeOutHandle = cmdArgs[0];
							_pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, pipeOutHandle);
						} else {
							
							// This is the server process
							_pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

							// TODO: Figure how to shorten this long command
							_pipeSettings.targetProcess.StartInfo.Arguments =
								$"{((AnonymousPipeServerStream)_pipeIn).GetClientHandleAsString()} " +
								$"{((AnonymousPipeServerStream)_pipeOut).GetClientHandleAsString()}";
							
							_pipeSettings.targetProcess.Start();
						}
					};
					break;
				default:
					// TODO: This shouldn't happen. Log a warning or throw an error
					return;
			}
			
			// Create the pipe writer and reader
			_pipeInReader = new StreamReader(_pipeIn);
			_pipeOutWriter = new StreamWriter(_pipeOut);
			
			switch(_pipeSettings.flowDirection) {
				case FlowDirection.In:
					startInFlow();
					
					// Set up timer that reads pipe every ReadPipeInterval milliseconds
					_readPipeTimer.Interval = _pipeSettings.flowInterval;
					_readPipeTimer.Elapsed += FlowIn;
					_readPipeTimer.AutoReset = true;
					break;
				case FlowDirection.Out:
					startOutFlow();
					break;
				case FlowDirection.Bidirectional:
					startInFlow();
					startOutFlow();
					
					// Set up timer that reads pipe every ReadPipeInterval milliseconds
					_readPipeTimer.Interval = _pipeSettings.flowInterval;
					_readPipeTimer.Elapsed += FlowIn;
					_readPipeTimer.AutoReset = true;
					break;
			}
		}
		// Read from the pipe
		private void FlowIn(object sender, ElapsedEventArgs e) {
			
			// Check for sync message
			string pipeMessage = _pipeInReader.ReadLine();
			if(pipeMessage == null || !pipeMessage.StartsWith("SYNC")) {
				
				// No message was found
				return;
			}
			
			// Get all messages in the pipe
			List<string> pipeMessageLines = new();
			do {
				pipeMessage = _pipeInReader.ReadLine();
				pipeMessageLines.Add(pipeMessage);
			} while(pipeMessage != null && !pipeMessage.StartsWith("END"));

			_pipeSettings.flowInCallback(pipeMessageLines.ToArray());
		}

		// Write to the pipe
		private void FlowOut() {
			
		}

	}
}