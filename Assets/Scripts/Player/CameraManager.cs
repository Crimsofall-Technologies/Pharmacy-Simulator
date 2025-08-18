using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public GameObject CamGO;
	public Transform[] CamPoints;
	
	public Camera Camera;
	public CameraType CamType { get; private set; }
	
	private void Start()
	{
		CamType = CameraType.Shop;
	}
	
	public void SwitchCamera(CameraType camType)
	{
		CamGO.transform.position = CamPoints[(int)camType].position;
		CamGO.transform.rotation = CamPoints[(int)camType].rotation;
		CamType = camType;
		
		if(CamType == CameraType.Shop)
		{
			UIManager.Instance.shopScreen.SetActive(true);
			UIManager.Instance.warehouseScreen.SetActive(false);
		}
		else if(CamType == CameraType.Warehouse)
		{
			UIManager.Instance.shopScreen.SetActive(false);
			UIManager.Instance.warehouseScreen.SetActive(true);
		}
		else
		{
			UIManager.Instance.warehouseScreen.SetActive(false);
			UIManager.Instance.shopScreen.SetActive(false);
		}

		UIManager.Instance.UpdateTimersOnCameraChange(CamType);
	}
}

public enum CameraType
{
	Warehouse, Shop, Buildings
}
