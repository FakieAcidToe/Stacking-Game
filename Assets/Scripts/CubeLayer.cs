using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeLayer : MonoBehaviour
{
	[SerializeField] bool isFrozen;
	[SerializeField] float movementSpeed = 1f;
	[SerializeField] float despawnHeight = -10f;

	[HideInInspector] public bool isX; // movement direction between x and z axis
	[HideInInspector] public float startPos;

	void Update()
	{
		// movement
		if (!isFrozen)
		{
			transform.position += (isX ? transform.right : transform.forward) * Time.deltaTime * movementSpeed;

			// reverse direction
			if ((isX ? transform.position.x : transform.position.z) != Mathf.Clamp(isX ? transform.position.x : transform.position.z, -Mathf.Abs(startPos), Mathf.Abs(startPos)))
				movementSpeed *= -1;
		}
		else if (transform.position.y < despawnHeight) // prepare to despawn
		{
			Destroy(gameObject);
		}
	}

	public bool FreezeSplit(CubeLayer prevCube, float perfectOffset)
	{
		isFrozen = true; // stop movement

		// split up
		float diff = isX ? (transform.position.x - prevCube.transform.position.x) : (transform.position.z - prevCube.transform.position.z);
		if (Mathf.Abs(diff) < perfectOffset) diff = 0; // if it's close enough to a perfect stack, make it perfect 0

		// main cube
		float newSize = (isX ? prevCube.transform.localScale.x : prevCube.transform.localScale.z) - Mathf.Abs(diff);
		float newPos = (isX ? prevCube.transform.position.x : prevCube.transform.position.z) + diff / 2;

		if (Mathf.Abs(diff) < (isX ? transform.localScale.x : transform.localScale.z))
		{
			transform.localScale = new Vector3(
				isX ? newSize : transform.localScale.x,
				transform.localScale.y,
				isX ? transform.localScale.z : newSize);
			transform.position = new Vector3(
				isX ? newPos : transform.position.x,
				transform.position.y,
				isX ? transform.position.z : newPos);
		}
		else
		{
			Rigidbody cubeRigidbody = GetComponent<Rigidbody>();
			cubeRigidbody.isKinematic = false;
			cubeRigidbody.useGravity = true;
			cubeRigidbody.AddForce(Vector3.up, ForceMode.VelocityChange);
			return false;
		}

		// new falling cube
		if (Mathf.Abs(diff) > 0)
		{
			float newFallSize = (isX ? prevCube.transform.localScale.x : prevCube.transform.localScale.z) - newSize;
			float newFallPos = (isX ? transform.position.x : transform.position.z) + (isX ? prevCube.transform.localScale.x : prevCube.transform.localScale.z) / 2 * Mathf.Sign(diff);

			CubeLayer newCube = Instantiate(this, new Vector3(
				isX ? newFallPos : transform.position.x,
				transform.position.y,
				isX ? transform.position.z : newFallPos), Quaternion.identity);
			newCube.transform.localScale = new Vector3(
				isX ? newFallSize : transform.localScale.x,
				transform.localScale.y,
				isX ? transform.localScale.z : newFallSize);
			Rigidbody cubeRigidbody = newCube.GetComponent<Rigidbody>();
			cubeRigidbody.isKinematic = false;
			cubeRigidbody.useGravity = true;
			cubeRigidbody.AddForce(Vector3.up, ForceMode.VelocityChange);
		}
		return true;
	}

	public void SetColHSV(float hue, float sat, float val)
	{
		GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(hue, sat, val);
	}
}
