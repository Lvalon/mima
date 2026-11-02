using UnityEngine;
using LBoL.Presentation;

namespace lvalonmima.Source.Patches;

public class ShopLabelUpdater : MonoBehaviour
{
	private float _timer;
	private const float Timeout = 3f;

	private void Update()
	{
		if (GameMaster.Instance?.CurrentProfile != null)
		{
			LiteShopButton.RefreshMainMenuButtonLabel();
			Destroy(this);
			return;
		}

		_timer += Time.deltaTime;
		if (_timer > Timeout)
			Destroy(this);
	}
}
