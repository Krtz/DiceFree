using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-30)]
    [DisallowMultipleComponent]
    public sealed class EscapeMenuController : MonoBehaviour
    {
        private InventoryPanel inventory;
        private NoviceSkillPanel novice;
        private PhysicalSkillPanel physical;
        private MagicalSkillPanel magical;
        private OptionsPanel options;
        private SkillTargetingController targeting;
        private Interactor interactor;

        private void Awake()
        {
            inventory = GetComponent<InventoryPanel>();
            novice = GetComponent<NoviceSkillPanel>();
            physical = GetComponent<PhysicalSkillPanel>();
            magical = GetComponent<MagicalSkillPanel>();
            options = FindAnyObjectByType<OptionsPanel>();
            targeting = GetComponent<SkillTargetingController>();
            interactor = GetComponent<Interactor>();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // Targeting owns Escape first so one press does one obvious thing.
            if (targeting != null && (targeting.Active || targeting.InputConsumedThisFrame)) return;

            bool closedSomething = false;

            if (inventory != null && inventory.IsOpen)
            {
                inventory.Close();
                closedSomething = true;
            }

            if (novice != null && novice.Open)
            {
                novice.Close();
                closedSomething = true;
            }

            if (physical != null && physical.Open)
            {
                physical.Close();
                closedSomething = true;
            }

            if (magical != null && magical.Open)
            {
                magical.Close();
                closedSomething = true;
            }

            if (options != null && options.Open)
            {
                options.Close();
                closedSomething = true;
            }

            if (interactor != null && interactor.Active != null)
            {
                interactor.Cancel();
                closedSomething = true;
            }

            var hud = HudLayoutManager.Current;
            if (hud != null && hud.EditMode)
            {
                hud.SetEditMode(false);
                closedSomething = true;
            }

            // Deliberately no combat/movement cancellation here.
            // Escape is UI/cancel semantics, not "stop" or "clear target".
        }
    }
}
