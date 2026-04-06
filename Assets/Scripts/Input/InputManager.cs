using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace PulseHighway.Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action<InputEvent> OnInput;
        public HashSet<int> PressedLanes { get; private set; } = new HashSet<int>();

        private bool isEnabled;
        private InputBuffer inputBuffer = new InputBuffer();

        // Keyboard mappings
        private readonly KeyCode[] primaryKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
        private readonly KeyCode[] altKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            if (!enabled)
            {
                PressedLanes.Clear();
                inputBuffer.Clear();
            }
        }

        private void Update()
        {
            if (!isEnabled) return;

            HandleKeyboard();
            HandleMouse();
            HandleTouch();
        }

        private void HandleKeyboard()
        {
            for (int lane = 0; lane < 5; lane++)
            {
                if (UnityEngine.Input.GetKeyDown(primaryKeys[lane]))
                    EmitInput(lane, InputEventType.Press, InputSource.Keyboard);
                if (UnityEngine.Input.GetKeyUp(primaryKeys[lane]))
                    EmitInput(lane, InputEventType.Release, InputSource.Keyboard);

                if (UnityEngine.Input.GetKeyDown(altKeys[lane]))
                    EmitInput(lane, InputEventType.Press, InputSource.Keyboard);
                if (UnityEngine.Input.GetKeyUp(altKeys[lane]))
                    EmitInput(lane, InputEventType.Release, InputSource.Keyboard);
            }
        }

        private void HandleMouse()
        {
            // Don't process gameplay input when clicking UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                int lane = ScreenXToLane(UnityEngine.Input.mousePosition.x);
                if (lane >= 0 && lane < 5)
                    EmitInput(lane, InputEventType.Press, InputSource.Mouse);
            }
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                int lane = ScreenXToLane(UnityEngine.Input.mousePosition.x);
                if (lane >= 0 && lane < 5)
                    EmitInput(lane, InputEventType.Release, InputSource.Mouse);
            }
        }

        private void HandleTouch()
        {
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);

                // Don't process gameplay input when touching UI elements
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    continue;

                int lane = ScreenXToLane(touch.position.x);
                if (lane < 0 || lane >= 5) continue;

                if (touch.phase == TouchPhase.Began)
                    EmitInput(lane, InputEventType.Press, InputSource.Touch);
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    EmitInput(lane, InputEventType.Release, InputSource.Touch);
            }
        }

        private void EmitInput(int lane, InputEventType type, InputSource source)
        {
            if (type == InputEventType.Press)
                PressedLanes.Add(lane);
            else
                PressedLanes.Remove(lane);

            var evt = new InputEvent(lane, type, source, Time.time);
            inputBuffer.Add(evt);
            OnInput?.Invoke(evt);
        }

        private int ScreenXToLane(float screenX)
        {
            float normalized = screenX / Screen.width;
            int lane = Mathf.FloorToInt(normalized * 5f);
            return Mathf.Clamp(lane, 0, 4);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) PressedLanes.Clear();
        }
    }
}
