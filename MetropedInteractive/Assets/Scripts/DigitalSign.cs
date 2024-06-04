using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leguar.DotMatrix;




namespace Leguar.DotMatrix.Example
{
	public static class Global
	{
		public static int counter = 0;

	}
	public class DigitalSign : MonoBehaviour
	{

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector
		private Controller controller;
		private TextCommand nextStopMessage;


		void Start()
		{

			// Get controller

			controller = dotMatrix.GetController();

			controller.DefaultSpeedDotsPerSecond = 45f;

			// Set basic loop that is repeated forever
			PauseCommand pause = new PauseCommand(30f)
			{
				Repeat = true
			};
			ClearCommand clear = new ClearCommand()
			{
				Method = ClearCommand.Methods.Instant,
				DotsPerSecond = 25f,
				Repeat = true
			};
			nextStopMessage = new TextCommand("(placeholder)");
			controller.AddCommand(nextStopMessage);
			//controller.AddCommand(text);
			controller.AddCommand(pause);
			//controller.AddCommand(clear);
			setRandomNextStop();

			for (int i = 0; i < 60; i++)
			{
			Global.counter++;
			controller.AddCommand(nextStopMessage);
			controller.AddCommand(pause);
			setRandomNextStop();
			}

			//InvokeRepeating("setRandomNextStop", 0f, 5f);
			//InvokeRepeating("Counter", 0f, 5f);

		}

		/*void Update()
		{
			// Just randomly change next stop
			if (Random.value < 0.0005f)
			{



				Global.counter++;
				setRandomNextStop();
			}
		}
		
		private float timer = 0f;
		private float interval = 5f; // Interval in seconds

		// Your other class members and methods...

		void Update()
		{
			// Update the timer
			timer += Time.deltaTime;

			// Check if the interval has elapsed
			if (timer >= interval)
			{
				// Call setRandomNextStop() method
				Global.counter++;
				setRandomNextStop();

				// Reset the timer
				timer = 0f;
			}
		}
		
		// Your other class members and methods...
		private void Counter()
        {
			Global.counter++; 
        }
	*/
		private void setRandomNextStop()
		{
			// Create next text command which is displaying next stop
			TextCommand newNextStopMessage = new TextCommand(getRandomNextStop())
			{
				Movement = TextCommand.Movements.None,
				Repeat = true
			};
			// Replace old next stop text in queue with new one
			controller.ReplaceCommand(nextStopMessage, newNextStopMessage);
			nextStopMessage = newNextStopMessage;

		}

		private string getRandomNextStop()
		{
			string[] stops = new string[] { "Farsta Strand        Nu", "Hagsätra           Nu", "Skarpnäck          Nu", };
			//int rnd = Random.Range(0, stops.Length);
			int index = Global.counter % 3;

			return stops[index];
		}

	}
}
