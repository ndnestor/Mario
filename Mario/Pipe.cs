using System;
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
		private readonly int _flowInterval; // In milliseconds

		// Pipe infrastructure
		private AnonymousPipeServerStream _pipeIn;
		private AnonymousPipeServerStream _pipeOut;
		private StreamReader _pipeInReader;
		private StreamWriter _pipeOutWriter;
		private readonly Timer _readPipeTimer = new();
		private Task _readPipeTask;
		
		// Constructor
		public Pipe(Process targetProcess, bool isBidirectional, int flowInterval) {
			
			// Set pipe settings
			_targetProcess = targetProcess;
			_isBidirectional = isBidirectional;
			_flowInterval = flowInterval;
			
			// Initialize pipe infrastructure
			_pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
			_pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
			_pipeInReader = new StreamReader(_pipeIn);
			_pipeOutWriter = new StreamWriter(_pipeOut);

			// Set up timer that reads pipe every ReadPipeInterval milliseconds
			_readPipeTimer.Interval = _flowInterval;
			_readPipeTimer.Elapsed += Flow;
			_readPipeTimer.AutoReset = true;
			_readPipeTimer.Start();
		}

		// Starts the target process and the transfer of data
		public void Start() {
			_targetProcess.Start();
			
			// TODO: Call Flow() periodically
			
		}

		private void Flow() {
			FlowOut();
			if(_isBidirectional) {
				FlowIn();
			}
		}

		private void FlowIn() {
			
		}

		private void FlowOut() {
			
		}

	}
}