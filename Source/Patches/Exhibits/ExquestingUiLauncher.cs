using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using lvalonmima.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using lvalonmima.Exhibits;
using lvalonmima.Source.Patches;
using System.Linq;
using LBoL.Base;

namespace lvalonmima.Patches.Exhibits;

public static class ExquestingUiLauncher
{
	public static void RefreshCustomSoldOutSlots(ShopPanel panel)
	{
		if (panel?.ShopStation?.ShopCards == null || panel.shopCardList == null)
			return;

		exquesting exhibit = panel.GameRun?.Player?.GetExhibit<exquesting>();

		int max = Mathf.Min(panel.ShopStation.ShopCards.Count, panel.shopCardList.Count);
		for (int i = 0; i < max; i++)
		{
			ShopCard shopCard = panel.shopCardList[i];
			if (shopCard == null)
			{
				continue;
			}

			ShopItem<Card> item = panel.ShopStation.ShopCards[i];
			if (item != null && item.IsSoldOut)
			{
				RefreshCustomSlotVisual(panel, i);
			}
			else
			{
				bool accepted = exhibit != null && exhibit.IsQuestSlotAccepted(i);
				TryRefreshCardWidgetVisual(shopCard, item, accepted);
				ExquestingPanelController.SetShopCardInteractionEnabled(shopCard, true);
			}
		}
	}

	private static void RefreshCustomSlotVisual(ShopPanel panel, int slotIndex)
	{
		if (panel?.shopCardList == null || slotIndex < 0 || slotIndex >= panel.shopCardList.Count)
			return;

		ShopCard shopCard = panel.shopCardList[slotIndex];
		if (shopCard == null)
			return;

		ExquestingPanelController.SetShopCardInteractionEnabled(shopCard, false);

		StoreSetActive(shopCard.transform.Find("Content")?.gameObject, false);
		StoreSetActive(shopCard.transform.Find("Price")?.gameObject, false);
		StoreSetActive(shopCard.transform.Find("SoldOut")?.gameObject, true);
		StoreSetActive(shopCard.transform.Find("Content/Price")?.gameObject, false);
		StoreSetActive(shopCard.transform.Find("Content/GoldIcon")?.gameObject, false);
		if (shopCard.price != null)
			StoreSetActive(shopCard.price.gameObject, false);

		TrySetCardWidgetEdge(shopCard, CardWidget.EdgeStatus.None);
	}

	private static void RefreshAcceptedSlotVisual(ShopPanel panel, int slotIndex)
	{
		if (panel?.shopCardList == null || panel.ShopStation?.ShopCards == null || slotIndex < 0 || slotIndex >= panel.shopCardList.Count || slotIndex >= panel.ShopStation.ShopCards.Count)
			return;

		ShopCard shopCard = panel.shopCardList[slotIndex];
		ShopItem<Card> item = panel.ShopStation.ShopCards[slotIndex];
		if (shopCard == null || item == null)
			return;

		item.IsSoldOut = false;
		TryRefreshShopCardBinding(panel, shopCard, item);
		TryRefreshCardWidgetVisual(shopCard, item, accepted: true);
		ExquestingPanelController.SetShopCardInteractionEnabled(shopCard, true);

		StoreSetActive(shopCard.transform.Find("Content")?.gameObject, true);
		StoreSetActive(shopCard.transform.Find("Price")?.gameObject, false);
		StoreSetActive(shopCard.transform.Find("SoldOut")?.gameObject, false);
		StoreSetActive(shopCard.transform.Find("Content/Price")?.gameObject, false);
		StoreSetActive(shopCard.transform.Find("Content/GoldIcon")?.gameObject, false);
		if (shopCard.price != null)
			StoreSetActive(shopCard.price.gameObject, false);

		RectTransform rect = shopCard.GetComponent<RectTransform>();
		if (rect != null)
			LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
	}

	private static void TryRefreshCardWidgetVisual(ShopCard shopCard, ShopItem<Card> item, bool accepted)
	{
		if (shopCard == null || item?.Content == null)
			return;

		CardWidget cardWidget = TryGetCardWidget(shopCard);
		if (cardWidget == null)
			return;

		cardWidget.Card = item.Content;
		cardWidget.RefreshStatus();
		cardWidget.SetProperties();

		CardWidget.EdgeStatus edgeStatus = accepted ? (cardWidget.Card.Config.Rarity == Rarity.Rare ? CardWidget.EdgeStatus.HighKicker : CardWidget.EdgeStatus.AffordKicker) : CardWidget.EdgeStatus.None;
		TrySetCardWidgetEdge(shopCard, edgeStatus);
	}

	internal static void ResetCardWidgetEdge(ShopCard shopCard)
	{
		TrySetCardWidgetEdge(shopCard, CardWidget.EdgeStatus.None);
	}

	private static void TrySetCardWidgetEdge(ShopCard shopCard, CardWidget.EdgeStatus edge)
	{
		CardWidget cardWidget = TryGetCardWidget(shopCard);
		if (cardWidget != null)
			cardWidget.SetCardEdge(edge);
	}

	private static CardWidget TryGetCardWidget(ShopCard shopCard)
	{
		if (shopCard == null)
			return null;

		CardWidget cardWidget = shopCard.GetComponentInChildren<CardWidget>(true);
		if (cardWidget != null)
			return cardWidget;

		return shopCard.cardWidget;
	}

	// Was reflection probing for SetShopItem/SetData/Refresh methods that don't
	// exist on vanilla ShopCard (confirmed via decompile: only SetCard/SetPrice
	// do), so this always fell through to the OnPointerExit(null) hack. Replaced
	// with vanilla's own rebind pattern (see ShopPanel.SetShopAfterBuying, which
	// calls SetCard when the card changed and SetPrice otherwise).
	private static void TryRefreshShopCardBinding(ShopPanel panel, ShopCard shopCard, ShopItem<Card> item)
	{
		if (shopCard == null || item == null)
			return;

		int money = panel.ShopStation.GameRun.Money;
		bool canAfford = money >= item.Price;
		if (item.Content != shopCard.Card)
			shopCard.SetCard(item.Content, item.Price, canAfford, item.IsDiscounted);
		else
			shopCard.SetPrice(item.Price, canAfford, item.IsDiscounted);
	}

	private static void StoreSetActive(GameObject target, bool isActive)
	{
		if (target != null)
			target.SetActive(isActive);
	}

	public static IEnumerator CoHandleCustomCardClick(ShopPanel panel, int index)
	{
		if (panel == null)
			yield break;

		ShopPanel shopPanel = null;
		bool lockedShop = false;
		Coroutine keepSelectPanelOnTopCoroutine = null;

		try
		{
			if (!panel.IsVisible || panel.ShopStation == null)
				yield break;

			if (index < 0 || index >= panel.ShopStation.ShopCards.Count)
				yield break;

			ShopItem<Card> item = panel.ShopStation.ShopCards[index];
			if (item == null || item.IsSoldOut)
				yield break;

			Card card = item.Content;
			if (card == null)
				yield break;

			shopPanel = UiManager.GetPanel<ShopPanel>();
			if (shopPanel != null)
			{
				shopPanel.LockedByInteractionMinimized = true;
				lockedShop = true;
			}

			keepSelectPanelOnTopCoroutine = panel.StartCoroutine(CoKeepSelectPanelOnTop(panel));

			if (!GameMaster.Instance.CurrentGameRun.Player.HasExhibit<exquesting>())
				yield break;

			exquesting exhibit = GameMaster.Instance.CurrentGameRun.Player.GetExhibit<exquesting>();
			if (exhibit == null)
				yield break;

			Card rolledCard = exhibit.GetRolledQuestCard(index);
			if (rolledCard != null)
				card = rolledCard;

			bool alreadyAccepted = exhibit.IsQuestSlotAccepted(index);

			string cardId = card?.Id ?? item.Content?.Id ?? "";
			bool canAbandon = true;
			if (cardId == nameof(cardquest11))
				canAbandon = false;

			SelectCardInteraction interaction = new(0, (!canAbandon && alreadyAccepted) ? 0 : 1, new[] { card })
			{
				Source = null,
				CanCancel = true,
				Description = alreadyAccepted ? GetLoc("AbandonQuest") : GetLoc("AcceptQuest")
			};

			yield return panel.GameRun.InteractionViewer.View(interaction);

			if (!interaction.IsCanceled && interaction.SelectedCards.Count > 0)
			{
				if (alreadyAccepted)
				{
					switch (interaction.SelectedCards[0].Id)
					{
						case nameof(cardquest4):
							Card toRmv = panel.GameRun.BaseDeck.FirstOrDefault(c => c.Id == nameof(cardgenji));
							if (toRmv != null)
							{
								panel.GameRun.RemoveDeckCard(toRmv);
							}
							break;
						case nameof(cardquest10):
							List<Card> toRmv2 = [];
							for (int i = 0; i < Library.CreateCard<cardquest10>().Value20; i++)
							{
								toRmv2.Add(panel.GameRun.BaseDeck.FirstOrDefault(c => c.Id == nameof(LBoL.EntityLib.Cards.Neutral.Black.Shadow) && !toRmv2.Contains(c)));
							}
							if (toRmv2.Count > 0)
								panel.GameRun.RemoveDeckCards(toRmv2.Where(c => c != null && c is LBoL.EntityLib.Cards.Neutral.Black.Shadow).ToArray());
							break;
						default:
							break;
					}
					exhibit.PendingQuestProgress.Remove(cardId);
					exhibit.ClearQuestRequirement(cardId);
					exhibit.ClearQuestCompleted(cardId);
					exhibit.MarkQuestSlotSoldOut(index);
					item.IsSoldOut = true;
					RefreshCustomSlotVisual(panel, index);
				}
				else
				{
					switch (interaction.SelectedCards[0].Id)
					{
						case nameof(cardquest4):
							panel.GameRun.AddDeckCard(Library.CreateCard<cardgenji>(), true);
							break;
						case nameof(cardquest10):
							cardquest10 quest10 = Library.CreateCard<cardquest10>();
							panel.GameRun.AddDeckCards(Library.CreateCards<LBoL.EntityLib.Cards.Neutral.Black.Shadow>(quest10.Value20), true);
							break;
						case nameof(cardquest11):
							panel.GameRun.GainMoney(Library.CreateCard<cardquest11>().Value440);
							break;
						case nameof(cardquest13):
							panel.GameRun.AddDeckCard(panel.GameRun.GetRandomCurseCard(panel.GameRun.CardRng), true);
							break;
						default:
							break;
					}
					exhibit.ClearQuestCompleted(cardId);
					if (!exhibit.PendingQuestProgress.ContainsKey(cardId))
					{
						exhibit.PendingQuestProgress[cardId] = 0;
					}

					exhibit.EnsureRequirementLockedForQuest(cardId);

					RefreshAcceptedSlotVisual(panel, index);
				}

				exhibit.CleanupStaleQuestRequirements();
				ShopModHandlers.PersistQuestProgress(GameMaster.Instance?.CurrentGameRun, exhibit.PendingQuestProgress, syncToLiteShop: false, saveToDisk: false, questRequirements: exhibit.QuestRequirements, completedQuestCards: exhibit.CompletedQuestCards, writeToRunFlags: false, questModifiers: exhibit.PendingQuestModifiers);
			}
		}
		finally
		{
			if (keepSelectPanelOnTopCoroutine != null && panel != null)
				panel.StopCoroutine(keepSelectPanelOnTopCoroutine);

			if (lockedShop && shopPanel != null)
				shopPanel.LockedByInteractionMinimized = false;

			ExquestingPanelController.UnlockInteraction(panel);
		}
	}

	private static IEnumerator CoKeepSelectPanelOnTop(ShopPanel shopPanel)
	{
		while (shopPanel != null && shopPanel.IsVisible)
		{
			SelectCardPanel selectCardPanel = UiManager.GetPanel<SelectCardPanel>();
			if (selectCardPanel != null && selectCardPanel.IsVisible && selectCardPanel.transform.parent != null)
			{
				selectCardPanel.transform.SetAsLastSibling();
			}

			yield return null;
		}
	}

	public static void OnExhibitClicked()
	{
		GameMaster instance = Singleton<GameMaster>.Instance;
		object battleObj = instance?.CurrentGameRun?.Battle;
		if (battleObj != null)
			return;

		if (IsInteractionActive())
			return;

		ExquestingPanelController.SetInitialState();
		GameRunController run = instance?.CurrentGameRun;
		if (run == null)
			return;

		exquesting exhibit = run.Player?.GetExhibit<exquesting>();
		if (exhibit == null)
			return;

		ShopPanel panel = UiManager.GetPanel<ShopPanel>();
		if (panel == null)
			return;

		if (panel.IsVisible && panel.GetComponent<ExquestingPanelMarker>() == null)
			return;

		if (panel.IsVisible)
			return;

		exhibit.EnsureRolledQuestCards();
		List<ShopItem<Card>> shopCards = exhibit.BuildRolledShopCards(run);

		ShopStation station = new()
		{
			GameRun = run,
			ShopCards = shopCards,
			ShopExhibits = [],
			CanUseCardService = false
		};

		while (station.ShopExhibits.Count < 3)
			station.ShopExhibits.Add(null);

		ExquestingPanelController.RegisterExquestingStation(station);

		if (panel.GetComponent<ExquestingPanelMarker>() == null)
			panel.gameObject.AddComponent<ExquestingPanelMarker>();

		if (panel is UiPanel<ShopStation> v)
			v.Show(station);
	}

	private static bool IsInteractionActive()
	{
		SelectCardPanel selectCardPanel = UiManager.GetPanel<SelectCardPanel>();
		if (selectCardPanel != null && selectCardPanel.IsVisible)
			return true;

		ShopPanel shopPanel = UiManager.GetPanel<ShopPanel>();
		return shopPanel != null && shopPanel.IsVisible;
	}
	private static string GetLoc(string key) => LocalisationKeys.Get(key, "<{0}>");
}
