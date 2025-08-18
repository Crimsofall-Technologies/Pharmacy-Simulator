using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedPeopleSystem
{
    public class UICharacterRotate : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UIControllerDEMO uIController;

        public float mouseRotateCharacterPower = 8f;

        bool toogle = false;
        public void OnPointerDown(PointerEventData eventData)
        {
            toogle = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            toogle = false;
            Cursor.lockState = CursorLockMode.None;
        }

        void Update()
        {
            if (toogle)
            {
                uIController.CharacterCustomization.transform.localRotation =
                Quaternion.Euler(uIController.CharacterCustomization.transform.localEulerAngles +
                (Vector3.up * -Input.GetAxis("Mouse X") * mouseRotateCharacterPower)
                );
            }
        }

    }
}