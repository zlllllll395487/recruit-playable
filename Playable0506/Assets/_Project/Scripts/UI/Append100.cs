using TMPro;
using UnityEngine;

namespace RecruitPlayable {
    /// <summary>Watches a TextMeshProUGUI score field and appends a "/100"
    /// suffix in smaller text. Avoids touching UIManager.SetStatValue.</summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DisallowMultipleComponent]
    public class Append100 : MonoBehaviour {
        TextMeshProUGUI _tmp;
        string _lastRaw;

        void Awake() { _tmp = GetComponent<TextMeshProUGUI>(); }

        void LateUpdate() {
            if (_tmp == null) return;
            string text = _tmp.text;
            if (text == _lastRaw) return;
            // Skip if already formatted (avoid feedback loop).
            if (text.Contains("/100") || text.Contains("<size=")) {
                _lastRaw = text;
                return;
            }
            // Skip when the value is the placeholder "--".
            if (string.IsNullOrEmpty(text) || text == "--") return;
            _lastRaw = text;
            // Single TMP element keeps the number+suffix as one unit, so
            // TextAlignmentOptions.Center centres the whole "88/100" block,
            // not just the digits. The /100 is rendered at 45 % size on the
            // same baseline as the digits.
            _tmp.text = $"{text}<size=45%>/100</size>";
        }
    }
}
