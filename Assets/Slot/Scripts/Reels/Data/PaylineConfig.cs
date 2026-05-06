using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    [CreateAssetMenu(menuName = "Slot/Payline Config")]
    public class PaylineConfig : ScriptableObject
    {
        [SerializeField] private List<PaylineDefinition> paylines = new List<PaylineDefinition>();

        public IReadOnlyList<PaylineDefinition> Paylines => paylines;

        [ContextMenu("Create Default 20 Paylines")]
        private void CreateDefault20Paylines()
        {
            paylines.Clear();

            paylines.Add(new PaylineDefinition(1, "Middle", new[] { 1, 1, 1, 1, 1 }));
            paylines.Add(new PaylineDefinition(2, "Top", new[] { 0, 0, 0, 0, 0 }));
            paylines.Add(new PaylineDefinition(3, "Bottom", new[] { 2, 2, 2, 2, 2 }));

            paylines.Add(new PaylineDefinition(4, "V Down", new[] { 0, 1, 2, 1, 0 }));
            paylines.Add(new PaylineDefinition(5, "V Up", new[] { 2, 1, 0, 1, 2 }));

            paylines.Add(new PaylineDefinition(6, "Top Small V", new[] { 0, 0, 1, 0, 0 }));
            paylines.Add(new PaylineDefinition(7, "Bottom Small V", new[] { 2, 2, 1, 2, 2 }));

            paylines.Add(new PaylineDefinition(8, "Mid Top Arc", new[] { 1, 0, 0, 0, 1 }));
            paylines.Add(new PaylineDefinition(9, "Mid Bottom Arc", new[] { 1, 2, 2, 2, 1 }));

            paylines.Add(new PaylineDefinition(10, "Top Mid Arc", new[] { 0, 1, 1, 1, 0 }));
            paylines.Add(new PaylineDefinition(11, "Bottom Mid Arc", new[] { 2, 1, 1, 1, 2 }));

            paylines.Add(new PaylineDefinition(12, "Step Down", new[] { 0, 1, 1, 1, 2 }));
            paylines.Add(new PaylineDefinition(13, "Step Up", new[] { 2, 1, 1, 1, 0 }));

            paylines.Add(new PaylineDefinition(14, "Zig Top", new[] { 0, 1, 0, 1, 0 }));
            paylines.Add(new PaylineDefinition(15, "Zig Bottom", new[] { 2, 1, 2, 1, 2 }));

            paylines.Add(new PaylineDefinition(16, "W Top", new[] { 0, 2, 0, 2, 0 }));
            paylines.Add(new PaylineDefinition(17, "W Bottom", new[] { 2, 0, 2, 0, 2 }));

            paylines.Add(new PaylineDefinition(18, "Small Down", new[] { 1, 0, 1, 2, 1 }));
            paylines.Add(new PaylineDefinition(19, "Small Up", new[] { 1, 2, 1, 0, 1 }));
            paylines.Add(new PaylineDefinition(20, "Mixed", new[] { 0, 2, 1, 0, 2 }));

            Debug.Log($"[{name}] Created {paylines.Count} paylines.");
        }
    }
}