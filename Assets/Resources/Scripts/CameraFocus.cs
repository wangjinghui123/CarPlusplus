using UnityEngine;
namespace UnityStandardAssets.ImageEffects{
public sealed class CameraFocus : MonoBehaviour
{
	[SerializeField]
	private Camera m_camera;

	[SerializeField]
	private DepthOfField m_depthOfField;

	[SerializeField]
	private float m_focusDelay;

	#region Unity core events.
	private void Update()
	{
		Transform cameraTransform = m_camera.transform;

		RaycastHit hitInfo;
		if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo))
		{
			m_depthOfField.focalLength = Mathf.Lerp(m_depthOfField.focalLength, hitInfo.distance, Time.deltaTime * m_focusDelay);
		}
	}
	#endregion //Unity core events.

	#region Class functions.
	#endregion //Class functions.
}
}
