using HMUI;
using UnityEngine;

namespace BetterPause.UI
{
	public class ImageContentBehaviour : MonoBehaviour
	{
		private NoTransitionsButton _button;
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
			if (_image == null)
				_image = transform.Find("BG")?.GetComponent<ImageView>();
			return _button != null && _image != null;
		}

		private void OnEnable()
		{
			if (!InGame || !TryInitialize()) return;
			var pos = _button.transform.localPosition;
			pos.y = yVals.Item2;
			_button.transform.localPosition = pos;
		}

		private void Update()
		{
			if (!TryInitialize()) return;
			_image.color = _button.selectionState == NoTransitionsButton.SelectionState.Highlighted ? Hover : Default;
			if (!InGame) return;
			var pos = _button.transform.localPosition;
			pos.y = Mathf.Lerp(pos.y, _button.selectionState == NoTransitionsButton.SelectionState.Highlighted ? yVals.Item1 : yVals.Item2, Time.deltaTime * 6f);
			_button.transform.localPosition = pos;
		}
	}
}
