using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using System;
using Zenject;

namespace BetterPause.UI
{
	internal class BetterPauseFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
	{
		private BetterPauseView _view;
		private MenuButton _menuButton;

		public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
		{
			if (firstActivation)
			{
				SetTitle("Better Pause");
				showBackButton = true;
				ProvideInitialViewControllers(_view);
			}
		}

		[Inject]
		internal void Construct(BetterPauseView view)
		{
			_view = view;
		}

		public void Initialize()
		{
			_menuButton = new MenuButton("Better Pause", () => BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(this));
			MenuButtons.Instance.RegisterButton(_menuButton);
		}

		public void Dispose()
		{
			if (_menuButton != null)
				MenuButtons.Instance.UnregisterButton(_menuButton);
		}

		public override void BackButtonWasPressed(ViewController topViewController)
		{
			BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
		}
	}
}
