//    Example - Clock with seconds (showing system time)


using UnityEngine;
using System;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_ClockWithSeconds : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private float updateCounter;

		void Start() {
			updateCounter = 0f;
		}

		void Update() {
			if (Time.timeScale>0f) {
				updateCounter -= Time.deltaTime/Time.timeScale; // Update once per second no matter what timescale is (since we want to show real time in this case)
				if (updateCounter<=0f) {
					// Update current time to display
					DateTime dt = DateTime.Now;
					string str = dt.Hour.ToString("00")+":"+dt.Minute.ToString("00")+":"+dt.Second.ToString("00");
					TextCommand text = new TextCommand(str);
					dotMatrix.GetController().AddCommand(text);
					updateCounter += 1f;
				}
			}
		}

	}

}
