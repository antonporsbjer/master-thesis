//    DotMatrix - Controller


using UnityEngine;
using System.Collections.Generic;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Class that runs given commands which then change content of the DotMatrix display. Commands sent to controller are
	/// executed one by one in queue.
	/// 
	/// This class doesn't have public constructor. Instance of this class is automatically created when DotMatrix initializes.
	/// After that reference to this class can be get from DotMatrix using GetController() method.
	/// 
	/// If you need to access display dots directly, use DisplayModel instead.
	/// </summary>
	public class Controller {
		
		private DotMatrix dotMatrix;
		private DisplayModel displayModel;
		
		private List<AbsCmd> commands;
		private RawCommand rawCommand;
		
		private float defaultSpeedDotsPerSecond;

		private float timeToConsume;

		private bool paused;

		/// <summary>
		/// Sets or gets the default speed in dots per second. Default speed is used in all moving content such as scrolling texts unless command have its own speed set.
		/// </summary>
		/// <value>
		/// Default speed in dots per second.
		/// </value>
		public float DefaultSpeedDotsPerSecond {
			set {
				if (value<=0f) {
					Debug.LogWarning("Controller.DefaultSpeedDotsPerSecond: Trying to set speed to zero or negative ("+value+"), setting speed to 1.0");
					defaultSpeedDotsPerSecond=1f;
				} else {
					defaultSpeedDotsPerSecond=value;
				}
			}
			get {
				return defaultSpeedDotsPerSecond;
			}
		}
		
		private TextCommand.Fonts defaultTextFont;
		
		/// <summary>
		/// Sets or gets the default text font. Default font is used in TextCommands unless command have its own font defined.
		/// </summary>
		/// <value>
		/// Default text font, one of the values from enum TextCommand.Fonts
		/// </value>
		public TextCommand.Fonts DefaultTextFont {
			set {
				defaultTextFont=value;
			}
			get {
				return defaultTextFont;
			}
		}

		internal Controller(DotMatrix dotMatrix, DisplayModel displayModel) {
			this.dotMatrix=dotMatrix;
			this.displayModel=displayModel;
			commands=new List<AbsCmd>();
			rawCommand=null;
			defaultSpeedDotsPerSecond=42;
			defaultTextFont=TextCommand.Fonts.Normal;
			timeToConsume=0f;
			paused=false;
		}

		internal void update(float deltaTime) {
			// If paused, just act like this this isn't even called
			if (paused) {
				return;
			}
			// Add time to consume
			timeToConsume+=deltaTime;
			// Always run one step
			int cmdCount=commands.Count;
			singleExecute();
			// Run multiple steps if command were instant and queue is getting shorter (so Controller will not get stuck on repeating commands)
			while (rawCommand==null && commands.Count<cmdCount) {
				cmdCount=commands.Count;
				singleExecute();
			}
		}

		private void singleExecute() {
			
			// Get new raw command if nothing is running at the moment
			if (rawCommand==null) {
				// If there isn't anything in queue, nothing to do
				if (commands.Count==0) {
					timeToConsume=0f;
					return;
				}
				// Get command
				AbsCmd command=commands[0];
				commands.RemoveAt(0);
				rawCommand=command.getRawCommand(dotMatrix,displayModel,this);
				// Possibly add back to queue
				if (command.Repeat) {
					commands.Add(command);
				}
			}

			// Run raw command
			timeToConsume=rawCommand.runStep(displayModel,timeToConsume);

			// Clear raw command if finished
			if (rawCommand.isFinished(displayModel)) {
				rawCommand=null;
			}

		}

		/// <summary>
		/// Add new command to controller queue. If queue is empty, instantly start executing this command.
		/// </summary>
		/// <param name="command">
		/// Command to add to queue.
		/// </param>
		public void AddCommand(AbsCmd command) {
			commands.Add(command);
		}
		
		/// <summary>
		/// Remove command from controller queue. If this command is currently being executed it will finish normally.
		/// </summary>
		/// <param name="command">
		/// Command to remove from queue.
		/// </param>
		/// <returns>
		/// True if command was found and removed, false otherwise.
		/// </returns>
		public bool RemoveCommand(AbsCmd command) {
			return commands.Remove(command);
		}

		/// <summary>
		/// Replace command in controller queue. If command that is replaced is currently being executed it will finish normally.
		/// If command to replace doesn't found, this method changes nothing.
		/// </summary>
		/// <param name="oldCommand">
		/// Command to remove from queue.
		/// </param>
		/// <param name="newCommand">
		/// Command to be added to queue to the same index where old command was.
		/// </param>
		/// <returns>
		/// True if command was replaced, false otherwise.
		/// </returns>
		public bool ReplaceCommand(AbsCmd oldCommand, AbsCmd newCommand) {
			int index=commands.IndexOf(oldCommand);
			if (index==-1) {
				return false;
			}
			commands.RemoveAt(index);
			commands.Insert(index,newCommand);
			return true;
		}

		/// <summary>
		/// Clear the controller command queue. Command that is currently being executed (if any) will finish normally but it will not repeat even if Repeat flag is set.
		/// So after finishing current command, controller will be idle unless new commands are added.
		/// </summary>
		public void ClearCommands() {
			commands.Clear();
		}
		
		/// <summary>
		/// Clear the controller command queue and stop any possible currently executing command. Controller will be idle immediately after this.
		/// Display is not cleared so whatever is on display when callign this will remain there.
		/// </summary>
		public void ClearCommandsAndStop() {
			commands.Clear();
			rawCommand=null;
			timeToConsume=0f;
		}
		
		/// <summary>
		/// Check whatever controller is idle.
		/// </summary>
		/// <returns>
		/// True if controller isn't currently executing any command and there is nothing in queue. False otherwise.
		/// </returns>
		public bool IsIdle() {
			return (rawCommand==null && commands.Count==0);
		}

		/// <summary>
		/// Pauses the executions of whatever command controller is currently running, without removing any commands or clearing command queue. Executions can be then
		/// continued by calling ResumeExecution() method.
		/// 
		/// Note that this is rather special command for cases when you need to "freeze" controller at arbitrary time, but still have possibility to continue later like
		/// nothing happened using ResumeExecution() method. If you just need to stop Controller instantly without need for resume, consider using ClearCommandsAndStop()
		/// method instead.
		/// </summary>
		/// <remarks>
		/// Calling this method when execution is already paused doesn't do anything.
		/// </remarks>
		public void PauseExecution() {
			paused=true;
		}

		/// <summary>
		/// Resumes execution of commands. Controller will continue from exactly where it left when PauseExecution() was called.
		/// </summary>
		/// <remarks>
		/// Calling this method when execution is not paused doesn't do anything.
		/// </remarks>
		public void ResumeExecution() {
			paused=false;
		}

		/// <summary>
		/// Check whatever controller is currently set to paused state. If paused, no commands are executed until ResumeExecution() is called.
		/// It is still possible to add/remove/replace new commands like normally, but controller will not continue running them until resume is called.
		/// </summary>
		/// <returns>
		/// True if controller command execution is currently paused. False otherwise.
		/// </returns>
		public bool IsExecutionPaused() {
			return paused;
		}

	}

}
