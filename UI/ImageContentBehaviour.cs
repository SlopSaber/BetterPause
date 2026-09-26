using HMUI;
using UnityEngine;

namespace BetterPause.UI
{
	public class ImageContentBehaviour : MonoBehaviour
	{
		private NoTransitionsButton _button;
		private Transform _buttonTransform;
		private ImageView _image;
		internal Color Default { get; set; }
		internal Color Hover { get; set; }
		internal bool InGame { get; set; }
		private (float, float) yVals = (1f, 0f);
		private void Start()
		{
			TryInitialize();
		}

		private bool TryInitialize()
		{
			if (_button == null)
				_button = GetComponent<NoTransitionsButton>();
			if (_button != null && _buttonTransform == null)
				_buttonTransform = _button.transform;
			if (_image == null)
				_image = transform.Find("BG")?.GetComponent<ImageView>();
			return _button != null && _buttonTransform != null && _image != null;
		}

		private void OnEnable()
		{
			if (!InGame || !TryInitialize()) return;
			var pos = _buttonTransform.localPosition;
			pos.y = yVals.Item2;
			_buttonTransform.localPosition = pos;
		}

		private void Update()
		{
			if (!TryInitialize()) return;
			var highlighted = _button.selectionState == NoTransitionsButton.SelectionState.Highlighted;
			var color = highlighted ? Hover : Default;
			if (_image.color != color)
				_image.color = color;
			if (!InGame) return;
			var pos = _buttonTransform.localPosition;
			var target = highlighted ? yVals.Item1 : yVals.Item2;
			var next = Mathf.Lerp(pos.y, target, Time.deltaTime * 6f);
			if (Mathf.Abs(next - target) < 0.001f)
				next = target;
			if (pos.y == next) return;
			pos.y = next;
			_buttonTransform.localPosition = pos;
		}
	}
}
