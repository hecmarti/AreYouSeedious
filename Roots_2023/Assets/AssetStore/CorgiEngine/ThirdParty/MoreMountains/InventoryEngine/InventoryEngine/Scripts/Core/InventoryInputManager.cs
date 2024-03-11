using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{
	/// <summary>
	/// Example of how you can call an inventory from your game. 
	/// I suggest having your Input and GUI manager classes handle that though.
	/// </summary>
	public class InventoryInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Targets")]
		[MMInformation("Bind here your inventory container (the CanvasGroup that you want to turn on/off when opening/closing the inventory), your main InventoryDisplay, and the overlay that will be displayed under the InventoryDisplay when opened.", MMInformationAttribute.InformationType.Info, false)]
		/// The CanvasGroup containing all the elements you want to show/hide when pressing the open/close inventory button
		public CanvasGroup TargetInventoryContainer;
		/// The main inventory display 
		public InventoryDisplay TargetInventoryDisplay;
		/// The Fader that will be used under it when opening/closing the inventory
		public CanvasGroup Overlay;

		[Header("Start Behaviour")]
		[MMInformation("If you set HideContainerOnStart to true, the TargetInventoryContainer defined right above this field will be automatically hidden on Start, even if you've left it visible in Scene view. Useful for setup.", MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, the inventory container will be hidden automatically on start
		public bool HideContainerOnStart = true;

		[Header("Permissions")]
		[MMInformation("Here you can decide to have your inventory catch input only when open, or not.", MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, the inventory container will be hidden automatically on start
		public bool InputOnlyWhenOpen = true;

		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			[Header("Input System Key Mapping")]
			[MMInformation("Here you need to set the various key bindings you prefer. There are some by default but feel free to change them.", MMInformationAttribute.InformationType.Info, false)]
			/// the key used to open/close the inventory
			public Key ToggleInventoryKey = Key.I;
			/// the alt key used to open/close the inventory
			public Key CancelKey = Key.Escape;
			/// the key used to move an item
			public Key MoveKey = Key.Insert;
			/// the key used to equip an item
			public Key EquipKey = Key.Home;
			/// the key used to use an item
			public Key UseKey = Key.End;
			/// the key used to equip or use an item
			public Key EquipOrUseKey = Key.Space;
			/// the key used to drop an item
			public Key DropKey = Key.Delete;
			/// the key used to go to the next inventory
			public Key NextInvKey = Key.PageDown;
			/// the key used to go to the previous inventory
			public Key PrevInvKey = Key.PageUp;
		#else
		[Header("Key Mapping")]
		[MMInformation("Here you need to set the various key bindings you prefer. There are some by default but feel free to change them.", MMInformationAttribute.InformationType.Info, false)]
		/// the key used to open/close the inventory
		public KeyCode ToggleInventoryKey = KeyCode.I;
		/// the alt key used to open/close the inventory
		public KeyCode ToggleInventoryAltKey = KeyCode.Joystick1Button6;
		/// the alt key used to open/close the inventory
		public KeyCode CancelKey = KeyCode.Escape;
		/// the alt key used to open/close the inventory
		public KeyCode CancelKeyAlt = KeyCode.Joystick1Button7;
		/// the key used to move an item
		public string MoveKey = "insert";
		/// the alt key used to move an item
		public string MoveAltKey = "joystick button 2";
		/// the key used to equip an item
		public string EquipKey = "home";
		/// the alt key used to equip an item
		public string EquipAltKey = "home";
		/// the key used to use an item
		public string UseKey = "end";
		/// the alt key used to use an item
		public string UseAltKey = "end";
		/// the key used to equip or use an item
		public string EquipOrUseKey = "space";
		/// the alt key used to equip or use an item
		public string EquipOrUseAltKey = "joystick button 0";
		/// the key used to drop an item
		public string DropKey = "delete";
		/// the alt key used to drop an item
		public string DropAltKey = "joystick button 1";
		/// the key used to go to the next inventory
		public string NextInvKey = "page down";
		/// the alt key used to go to the next inventory
		public string NextInvAltKey = "joystick button 4";
		/// the key used to go to the previous inventory
		public string PrevInvKey = "page up";
		/// the alt key used to go to the previous inventory
		public string PrevInvAltKey = "joystick button 5";
		#endif

		[Header("Close Bindings")] 
		/// a list of other inventories that should get force-closed when this one opens
		public List<string> CloseList;

		public enum ManageButtonsModes { Interactable, SetActive }
        
		[Header("Buttons")]
		/// if this is true, the InputManager will change the interactable state of inventory control buttons based on the currently selected slot
		public bool ManageButtons = false;
		/// the selected mode to enable buttons with (interactable will change the button's interactable state, SetActive will enable/disable the button's game object
		[MMCondition("ManageButtons", true)] 
		public ManageButtonsModes ManageButtonsMode = ManageButtonsModes.SetActive;
		/// the button used to equip or use an item
		[MMCondition("ManageButtons", true)]
		public Button EquipUseButton;
		/// the button used to move an item
		[MMCondition("ManageButtons", true)]
		public Button MoveButton;
		/// the button used to drop an item
		[MMCondition("ManageButtons", true)]
		public Button DropButton;
		/// the button used to equip an item
		[MMCondition("ManageButtons", true)]
		public Button EquipButton;
		/// the button used to use an item
		[MMCondition("ManageButtons", true)]
		public Button UseButton;
		/// the button used to unequip an item
		[MMCondition("ManageButtons", true)]
		public Button UnEquipButton;
        
		/// returns the active slot
		public InventorySlot CurrentlySelectedInventorySlot { get; set; }

		[Header("State")] 
		/// if this is true, the associated inventory is open, closed otherwise
		[MMReadOnly]
		public bool InventoryIsOpen;

		protected CanvasGroup _canvasGroup;
		protected bool _pause = false;
		protected GameObject _currentSelection;
		protected InventorySlot _currentInventorySlot;
		protected List<InventoryHotbar> _targetInventoryHotbars;
		protected InventoryDisplay _currentInventoryDisplay;
		private bool _isEquipUseButtonNotNull;
		private bool _isEquipButtonNotNull;
		private bool _isUseButtonNotNull;
		private bool _isUnEquipButtonNotNull;
		private bool _isMoveButtonNotNull;
		private bool _isDropButtonNotNull;
		
		protected bool _toggleInventoryKeyPressed;
		protected bool _cancelKeyPressed;
		protected bool _prevInvKeyPressed;
		protected bool _nextInvKeyPressed;
		protected bool _moveKeyPressed;
		protected bool _equipOrUseKeyPressed;
		protected bool _equipKeyPressed;
		protected bool _useKeyPressed;
		protected bool _dropKeyPressed;
		protected bool _hotbarInputPressed = false;

		/// <summary>
		/// On start, we grab references and prepare our hotbar list
		/// </summary>
		protected virtual void Start()
		{
			_isDropButtonNotNull = DropButton != null;
			_isMoveButtonNotNull = MoveButton != null;
			_isUnEquipButtonNotNull = UnEquipButton != null;
			_isUseButtonNotNull = UseButton != null;
			_isEquipButtonNotNull = EquipButton != null;
			_isEquipUseButtonNotNull = EquipUseButton != null;
			_currentInventoryDisplay = TargetInventoryDisplay;
			InventoryIsOpen = false;
			_targetInventoryHotbars = new List<InventoryHotbar>();
			_canvasGroup = GetComponent<CanvasGroup>();
			foreach (InventoryHotbar go in FindObjectsOfType(typeof(InventoryHotbar)) as InventoryHotbar[])
			{
				_targetInventoryHotbars.Add(go);
			}
			if (HideContainerOnStart)
			{
				if (TargetInventoryContainer != null) { TargetInventoryContainer.alpha = 0; }
				if (Overlay != null) { Overlay.alpha = 0; }
				EventSystem.current.sendNavigationEvents = false;
				if (_canvasGroup != null)
				{
					_canvasGroup.blocksRaycasts = false;
				}
			}
		}

		/// <summary>
		/// Every frame, we check for input for the inventory, the hotbars and we check the current selection
		/// </summary>
		protected virtual void Update()
		{
			HandleInventoryInput();
			HandleHotbarsInput();
			CheckCurrentlySelectedSlot();
			HandleButtons();
		}

		/// <summary>
		/// Every frame, we check and store what object is currently selected
		/// </summary>
		protected virtual void CheckCurrentlySelectedSlot()
		{
			_currentSelection = EventSystem.current.currentSelectedGameObject;
			if (_currentSelection == null)
			{
				return;
			}
			_currentInventorySlot = _currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>();
			if (_currentInventorySlot != null)
			{
				CurrentlySelectedInventorySlot = _currentInventorySlot;
			}
		}

		/// <summary>
		/// Will turn inventory controls interactable or not based on the currently selected slot, if ManageButtons is set to true
		/// </summary>
		protected virtual void HandleButtons()
		{
			if (!ManageButtons)
			{
				return;
			}
            
			if (CurrentlySelectedInventorySlot != null)
			{
				if (_isUseButtonNotNull)
				{
					SetButtonState(UseButton, CurrentlySelectedInventorySlot.Usable() && CurrentlySelectedInventorySlot.UseButtonShouldShow());
				}

				if (_isEquipButtonNotNull)
				{
					SetButtonState(EquipButton, CurrentlySelectedInventorySlot.Equippable() && CurrentlySelectedInventorySlot.EquipButtonShouldShow());
				}

				if (_isEquipUseButtonNotNull)
				{
					SetButtonState(EquipUseButton, (CurrentlySelectedInventorySlot.Usable() ||
					                                CurrentlySelectedInventorySlot.Equippable()) && CurrentlySelectedInventorySlot.EquipUseButtonShouldShow());
				}

				if (_isUnEquipButtonNotNull)
				{
					SetButtonState(UnEquipButton, CurrentlySelectedInventorySlot.Unequippable() && CurrentlySelectedInventorySlot.UnequipButtonShouldShow());
				}

				if (_isMoveButtonNotNull)
				{
					SetButtonState(MoveButton, CurrentlySelectedInventorySlot.Movable() && CurrentlySelectedInventorySlot.MoveButtonShouldShow());
				}

				if (_isDropButtonNotNull)
				{
					SetButtonState(DropButton, CurrentlySelectedInventorySlot.Droppable() && CurrentlySelectedInventorySlot.DropButtonShouldShow());
				}
			}
			else
			{
				SetButtonState(UseButton, false);
				SetButtonState(EquipButton, false);
				SetButtonState(EquipUseButton, false);
				SetButtonState(DropButton, false);
				SetButtonState(MoveButton, false);
				SetButtonState(UnEquipButton, false);
			}
		}

		/// <summary>
		/// An internal method used to turn a button on or off
		/// </summary>
		/// <param name="targetButton"></param>
		/// <param name="state"></param>
		protected virtual void SetButtonState(Button targetButton, bool state)
		{
			if (ManageButtonsMode == ManageButtonsModes.Interactable)
			{
				targetButton.interactable = state;
			}
			else
			{
				targetButton.gameObject.SetActive(state);
			}
		}

		/// <summary>
		/// Opens or closes the inventory panel based on its current status
		/// </summary>
		public virtual void ToggleInventory()
		{
			if (InventoryIsOpen)
			{
				CloseInventory();
			}
			else
			{
				OpenInventory();
			}
		}

		/// <summary>
		/// Opens the inventory panel
		/// </summary>
		public virtual void OpenInventory()
		{
			if (CloseList.Count > 0)
			{
				foreach (string playerID in CloseList)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloseRequest, null, "", null, 0, 0, playerID);
				}
			}
            
			// we set the game to pause
			_pause = true;
			if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = true;
			}

			// we open our inventory
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryOpens, null, TargetInventoryDisplay.TargetInventoryName, TargetInventoryDisplay.TargetInventory.Content[0], 0, 0, TargetInventoryDisplay.PlayerID);
			MMGameEvent.Trigger("inventoryOpens");
			InventoryIsOpen = true;

			StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 1f));
			StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, 0.85f));
		}

		/// <summary>
		/// Closes the inventory panel
		/// </summary>
		public virtual void CloseInventory()
		{
			// we unpause the game
			_pause = false;
			if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = false;
			}
			// we close our inventory
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloses, null, TargetInventoryDisplay.TargetInventoryName, null, 0, 0, TargetInventoryDisplay.PlayerID);
			MMGameEvent.Trigger("inventoryCloses");
			InventoryIsOpen = false;

			StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 0f));
			StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, 0f));
		}

		/// <summary>
		/// Handles the inventory related inputs and acts on them.
		/// </summary>
		protected virtual void HandleInventoryInput()
		{
			// if we don't have a current inventory display, we do nothing and exit
			if (_currentInventoryDisplay == null)
			{
				return;
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				_toggleInventoryKeyPressed = Keyboard.current[ToggleInventoryKey].wasPressedThisFrame;
				_cancelKeyPressed = Keyboard.current[CancelKey].wasPressedThisFrame;
				_prevInvKeyPressed = Keyboard.current[PrevInvKey].wasPressedThisFrame;
				_nextInvKeyPressed = Keyboard.current[NextInvKey].wasPressedThisFrame;
				_moveKeyPressed = Keyboard.current[MoveKey].wasPressedThisFrame;
				_equipOrUseKeyPressed = Keyboard.current[EquipOrUseKey].wasPressedThisFrame;
				_equipKeyPressed = Keyboard.current[EquipKey].wasPressedThisFrame;
				_useKeyPressed = Keyboard.current[UseKey].wasPressedThisFrame;
				_dropKeyPressed = Keyboard.current[DropKey].wasPressedThisFrame;
			#else
			_toggleInventoryKeyPressed = Input.GetKeyDown(ToggleInventoryKey) || Input.GetKeyDown(ToggleInventoryAltKey);
			_cancelKeyPressed = (Input.GetKeyDown(CancelKey)) || (Input.GetKeyDown(CancelKeyAlt));
			_prevInvKeyPressed = Input.GetKeyDown(PrevInvKey) || Input.GetKeyDown(PrevInvAltKey);
			_nextInvKeyPressed = Input.GetKeyDown(NextInvKey) || Input.GetKeyDown(NextInvAltKey);
			_moveKeyPressed = (Input.GetKeyDown(MoveKey) || Input.GetKeyDown(MoveAltKey));
			_equipOrUseKeyPressed = Input.GetKeyDown(EquipOrUseKey) || Input.GetKeyDown(EquipOrUseAltKey);
			_equipKeyPressed = Input.GetKeyDown(EquipKey) || Input.GetKeyDown(EquipAltKey);
			_useKeyPressed = Input.GetKeyDown(UseKey) || Input.GetKeyDown(UseAltKey);
			_dropKeyPressed = Input.GetKeyDown(DropKey) || Input.GetKeyDown(DropAltKey);
			#endif
			
			// if the user presses the 'toggle inventory' key
			if (_toggleInventoryKeyPressed)
			{
				// if the inventory is not open
				if (!InventoryIsOpen)
				{
					OpenInventory();
				}
				// if it's open
				else
				{
					CloseInventory();
				}
			}

			if (_cancelKeyPressed)
			{
				if (InventoryIsOpen)
				{
					CloseInventory();
				}
			}

			// if we've only authorized input when open, and if the inventory is currently closed, we do nothing and exit
			if (InputOnlyWhenOpen && !InventoryIsOpen)
			{
				return;
			}

			// previous inventory panel
			if (_prevInvKeyPressed)
			{
				if (_currentInventoryDisplay.GoToInventory(-1) != null)
				{
					_currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(-1);
				}
			}

			// next inventory panel
			if (_nextInvKeyPressed)
			{
				if (_currentInventoryDisplay.GoToInventory(1) != null)
				{
					_currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(1);
				}
			}

			// move
			if (_moveKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					CurrentlySelectedInventorySlot.Move();
				}
			}

			// equip or use
			if (_equipOrUseKeyPressed)
			{
				EquipOrUse();
			}

			// equip
			if (_equipKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					CurrentlySelectedInventorySlot.Equip();
				}
			}

			// use
			if (_useKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					CurrentlySelectedInventorySlot.Use();
				}
			}

			// drop
			if (_dropKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					CurrentlySelectedInventorySlot.Drop();
				}
			}
		}

		/// <summary>
		/// Checks for hotbar input and acts on it
		/// </summary>
		protected virtual void HandleHotbarsInput()
		{
			if (!InventoryIsOpen)
			{
				foreach (InventoryHotbar hotbar in _targetInventoryHotbars)
				{
					if (hotbar != null)
					{
						#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						_hotbarInputPressed = Keyboard.current[hotbar.HotbarKey].wasPressedThisFrame;
						#else
						_hotbarInputPressed = Input.GetKeyDown(hotbar.HotbarKey) || Input.GetKeyDown(hotbar.HotbarAltKey);
						#endif
						
						if (_hotbarInputPressed)
						{
							hotbar.Action();
						}
					}
				}
			}
		}

		/// <summary>
		/// When pressing the equip/use button, we determine which of the two methods to call
		/// </summary>
		public virtual void EquipOrUse()
		{
			if (CurrentlySelectedInventorySlot.Equippable())
			{
				CurrentlySelectedInventorySlot.Equip();
			}
			if (CurrentlySelectedInventorySlot.Usable())
			{
				CurrentlySelectedInventorySlot.Use();
			}
		}

		public virtual void Equip()
		{
			CurrentlySelectedInventorySlot.Equip();
		}

		public virtual void Use()
		{
			CurrentlySelectedInventorySlot.Use();
		}

		public virtual void UnEquip()
		{
			CurrentlySelectedInventorySlot.UnEquip();
		}

		/// <summary>
		/// Triggers the selected slot's move method
		/// </summary>
		public virtual void Move()
		{
			CurrentlySelectedInventorySlot.Move();
		}

		/// <summary>
		/// Triggers the selected slot's drop method
		/// </summary>
		public virtual void Drop()
		{
			CurrentlySelectedInventorySlot.Drop();
		}

		/// <summary>
		/// Catches MMInventoryEvents and acts on them
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.PlayerID != TargetInventoryDisplay.PlayerID)
			{
				return;
			}
            
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloseRequest)
			{
				CloseInventory();
			}
		}

		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}