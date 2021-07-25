using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Timers;

namespace Mario {
	public class Pipe {

		// Pipe settings
		private readonly Process _targetProcess;
		private readonly bool _isBidirectional; // Otherwise it only acts as an input
		private readonly Action<string[]> _flowInCallback;

		// Pipe infrastructure
		private AnonymousPipeServerStream _pipeIn;
		private AnonymousPipeServerStream _pipeOut;
		private StreamReader _pipeInReader;
		private StreamWriter _pipeOutWriter;
		private readonly Timer _readPipeTimer = new();
		private Task _readPipeTask;
		
		// Constructor
		public Pipe(Process targetProcess, bool isBidirectional, int flowInterval, Action<string[]> flowInCallback) {
			
			// Set pipe settings
			_targetProcess = targetProcess;
			_isBidirectional = isBidirectional;
			_flowInCallback = flowInCallback;

			// Initialize pipe infrastructure
			_pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
			_pipeOutWriter = new StreamWriter(_pipeOut);
			if(_isBidirectional) {
				_pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
				_pipeInReader = new StreamReader(_pipeIn);


				// Set up timer that reads pipe every ReadPipeInterval milliseconds
				_readPipeTimer.Interval = flowInterval;
				_readPipeTimer.Elapsed += FlowIn;
				_readPipeTimer.AutoReset = true;
			}
		}

		// Starts the target process and the transfer of data
		public void Start() {
			_targetProcess.Start();
			_readPipeTimer.Start();
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

			_flowInCallback(pipeMessageLines.ToArray());
		}

		// Write to the pipe
		private void FlowOut() {
			
		}

	}
}