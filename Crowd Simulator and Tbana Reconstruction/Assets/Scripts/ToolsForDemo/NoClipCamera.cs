/* NoClipCamera v1.0, author: Crystopher William Mariño Ross, created 2024-03-05
 * This is a script meant to be placed on the 'Main Camera' GameObject. It allows moving the camera while  
 * in the 'Play' mode in the editor. The camera moves like a noclip camera. Controls:
 * - Move mouse sideways to turn around.
 * - Move mouse up and down to look up or down.
 * - Press 'W' to move camera forward.
 * - Press 'S' to move camera backward.
 * - Press 'A' to move camera to the left.
 * - Press 'D' to move camera to the right.
 * - Scroll the mouse wheel up to increase camera movement speed.
 * - Scroll the mouse wheel down to decrease camera movement speed.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoClipCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private float speed = 20.0f;
    private float turnSpeed = 45.0f;
    private float horizontalInput;
    private float verticalInput;
    private float mouseXInput;
    private float mouseYInput;
    private float mouseScrollInput;
    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseXInput = Input.GetAxis("Mouse X");
        mouseYInput = Input.GetAxis("Mouse Y");
        mouseScrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScrollInput > 0)
            speed++;
        else if (mouseScrollInput < 0 && speed>0)
            speed--;

        //Move camera back and forth
        transform.Translate(Vector3.forward * Time.deltaTime * speed * verticalInput);
        //Move camera sideways
        transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput);
        // Turn camera. Inspired by StackOverflow post https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio
        transform.localEulerAngles = transform.localEulerAngles + new Vector3(-Time.deltaTime * turnSpeed * mouseYInput, Time.deltaTime * turnSpeed * mouseXInput, 0);

    }
}
