using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace lvalonmima
{
    public class shopplugin
    {
        public static IEnumerator shopCoroutinePassive()
        {
            if (UiManager.GetPanel<ShopPanel>().IsVisible)
            {
                yield break;
            }
            GameMaster.Instance?.CurrentGameRun.LoseMoney(10);
            GameMaster.Instance.StartCoroutine(adventureCoroutine());
            GameMaster.Instance.StartCoroutine(battleCoroutine());
            StationType stationType = GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode.StationType;
            GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode.StationType = StationType.Shop;
            ShopStation station = (ShopStation)GameMaster.Instance.CurrentGameRun.CurrentStage.CreateStation(GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode);
            station.CanUseCardService = false;
            GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode.StationType = stationType;
            List<ShopItem<Card>> list = new List<ShopItem<Card>>();
            List<Card> mimapassivelist = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), 8, null, true, false, false, false, (Card card) => card is mimaextensions.mimacard mimascard && mimascard.ispassive).ToList<Card>();
            List<Card> mimamonsterlist = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), 2, null, true, false, false, false, (Card card) => card is mimaextensions.mimacard mimascard && mimascard.ismonster).ToList<Card>();

            if (mimapassivelist.Count > 0)
            {
                foreach (Card card in mimapassivelist)
                {
                    System.Random random = new System.Random();
                    int posorneg = random.Next(0, 3);
                    int cardcost = 0;
                    switch (card.Config.Rarity)
                    {
                        case Rarity.Common:
                            cardcost = 25;
                            if (posorneg == 2) { cardcost += random.Next(0, 6); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 6); }
                            break;
                        case Rarity.Uncommon:
                            cardcost = 50;
                            if (posorneg == 2) { cardcost += random.Next(0, 11); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 11); }
                            break;
                        case Rarity.Rare:
                            cardcost = 100;
                            if (posorneg == 2) { cardcost += random.Next(0, 21); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 21); }
                            break;
                    }
                    int discount02 = random.Next(0, 100);
                    bool discountpls = false;
                    if (discount02 > 89) { discountpls = true; cardcost /= 2; }
                    list.Add(new ShopItem<Card>(GameMaster.Instance.CurrentGameRun, card, cardcost, false, discountpls));
                }
                foreach (Card card in mimamonsterlist)
                {
                    System.Random random = new System.Random();
                    int posorneg = random.Next(0, 3);
                    int cardcost = 0;
                    switch (card.Config.Rarity)
                    {
                        case Rarity.Common:
                            cardcost = 50;
                            if (posorneg == 2) { cardcost += random.Next(0, 11); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 11); }
                            break;
                        case Rarity.Uncommon:
                            cardcost = 100;
                            if (posorneg == 2) { cardcost += random.Next(0, 21); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 21); }
                            break;
                        case Rarity.Rare:
                            cardcost = 200;
                            if (posorneg == 2) { cardcost += random.Next(0, 41); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 41); }
                            break;
                    }
                    int discount02 = random.Next(0, 100);
                    bool discountpls = false;
                    if (discount02 > 89) { discountpls = true; cardcost /= 2; }
                    list.Add(new ShopItem<Card>(GameMaster.Instance.CurrentGameRun, card, cardcost, false, discountpls));
                }
            }

            station.DiscountCardNo = 0;
            station.ShopCards = list;
            List<ShopItem<Exhibit>> list2 = new List<ShopItem<Exhibit>>();
            for (int j = 0; j < 3; j++)
            {
                Exhibit shopExhibit = GameMaster.Instance.CurrentGameRun.CurrentStage.GetSentinelExhibit();
                list2.Add(new ShopItem<Exhibit>(GameMaster.Instance.CurrentGameRun, shopExhibit, station.GetPrice(shopExhibit), true, false));
            }
            station.ShopExhibits = list2;
            UiManager.GetPanel<ShopPanel>().Show(station);
            GameMaster.Instance.StartCoroutine(rewardCoroutine());
            yield break;
        }
        public static IEnumerator shopCoroutineBlitz()
        {
            if (UiManager.GetPanel<ShopPanel>().IsVisible)
            {
                yield break;
            }
            GameMaster.Instance?.CurrentGameRun.LoseMoney(10);
            GameMaster.Instance.StartCoroutine(adventureCoroutine());
            GameMaster.Instance.StartCoroutine(battleCoroutine());
            StationType stationType = GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode.StationType;
            GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode.StationType = StationType.Shop;
            ShopStation station = (ShopStation)GameMaster.Instance.CurrentGameRun.CurrentStage.CreateStation(GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode);
            station.CanUseCardService = false;
            GameMaster.Instance.CurrentGameRun.CurrentMap.StartNode.StationType = stationType;
            List<ShopItem<Card>> list = new List<ShopItem<Card>>();
            List<Card> mimapassivelist = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), 8, null, true, false, false, false, (Card card) => card is mimaextensions.mimacard mimascard && mimascard.isblitz).ToList<Card>();
            List<Card> mimamonsterlist = toolbox.RollCardsCustom(GameMaster.Instance.CurrentGameRun.BattleCardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot), 2, null, true, false, false, false, (Card card) => card is mimaextensions.mimacard mimascard && mimascard.isbm).ToList<Card>();

            if (mimapassivelist.Count > 0)
            {
                foreach (Card card in mimapassivelist)
                {
                    System.Random random = new System.Random();
                    int posorneg = random.Next(0, 3);
                    int cardcost = 0;
                    switch (card.Config.Rarity)
                    {
                        case Rarity.Common:
                            cardcost = 50;
                            if (posorneg == 2) { cardcost += random.Next(0, 11); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 11); }
                            break;
                        case Rarity.Uncommon:
                            cardcost = 100;
                            if (posorneg == 2) { cardcost += random.Next(0, 21); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 21); }
                            break;
                        case Rarity.Rare:
                            cardcost = 200;
                            if (posorneg == 2) { cardcost += random.Next(0, 41); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 41); }
                            break;
                    }
                    int discount02 = random.Next(0, 100);
                    bool discountpls = false;
                    if (discount02 > 89) { discountpls = true; cardcost /= 2; }
                    list.Add(new ShopItem<Card>(GameMaster.Instance.CurrentGameRun, card, cardcost, false, discountpls));
                }
                foreach (Card card in mimamonsterlist)
                {
                    System.Random random = new System.Random();
                    int posorneg = random.Next(0, 3);
                    int cardcost = 0;
                    switch (card.Config.Rarity)
                    {
                        case Rarity.Common:
                            cardcost = 100;
                            if (posorneg == 2) { cardcost += random.Next(0, 21); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 21); }
                            break;
                        case Rarity.Uncommon:
                            cardcost = 200;
                            if (posorneg == 2) { cardcost += random.Next(0, 41); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 41); }
                            break;
                        case Rarity.Rare:
                            cardcost = 400;
                            if (posorneg == 2) { cardcost += random.Next(0, 81); }
                            if (posorneg == 0) { cardcost -= random.Next(0, 81); }
                            break;
                    }
                    int discount02 = random.Next(0, 100);
                    bool discountpls = false;
                    if (discount02 > 89) { discountpls = true; cardcost /= 2; }
                    list.Add(new ShopItem<Card>(GameMaster.Instance.CurrentGameRun, card, cardcost, false, discountpls));
                }
            }

            station.DiscountCardNo = 0;
            station.ShopCards = list;
            List<ShopItem<Exhibit>> list2 = new List<ShopItem<Exhibit>>();
            for (int j = 0; j < 3; j++)
            {
                Exhibit shopExhibit = GameMaster.Instance.CurrentGameRun.CurrentStage.GetSentinelExhibit();
                list2.Add(new ShopItem<Exhibit>(GameMaster.Instance.CurrentGameRun, shopExhibit, station.GetPrice(shopExhibit), true, false));
            }
            station.ShopExhibits = list2;
            UiManager.GetPanel<ShopPanel>().Show(station);
            GameMaster.Instance.StartCoroutine(rewardCoroutine());
            yield break;
        }
        public static IEnumerator adventureCoroutine()
        {
            if (UiManager.GetPanel<VnPanel>().root.activeSelf)
            {
                bool nextButton = UiManager.GetPanel<VnPanel>().nextButton.isActiveAndEnabled;
                UiManager.Hide<BackgroundPanel>(true);
                UiManager.GetPanel<VnPanel>().enemyTitleRoot.SetActive(false);
                UiManager.GetPanel<VnPanel>().adventureTitle.SetActive(false);
                UiManager.GetPanel<VnPanel>().adventureFrame.gameObject.SetActive(false);
                UiManager.GetPanel<VnPanel>().optionsRoot.SetActive(false);
                UiManager.GetPanel<VnPanel>().tempArtText.gameObject.SetActive(false);
                UiManager.GetPanel<VnPanel>().root.SetActive(false);
                yield return new WaitUntil(() => UiManager.GetPanel<VnPanel>().nextButton.gameObject.activeSelf);
                if (!nextButton)
                {
                    UiManager.GetPanel<VnPanel>().SetNextButton(false, 0, null);
                }
                UiManager.GetPanel<BackgroundPanel>().Show("Adventure");
                UiManager.GetPanel<VnPanel>().enemyTitleRoot.SetActive(true);
                UiManager.GetPanel<VnPanel>().adventureTitle.SetActive(true);
                UiManager.GetPanel<VnPanel>().adventureFrame.gameObject.SetActive(true);
                UiManager.GetPanel<VnPanel>().optionsRoot.SetActive(true);
                UiManager.GetPanel<VnPanel>().tempArtText.gameObject.SetActive(true);
                UiManager.GetPanel<VnPanel>().root.SetActive(true);
            }
            yield break;
        }
        public static IEnumerator battleCoroutine()
        {
            if (UiManager.GetPanel<PlayBoard>().IsVisible)
            {
                UiManager.GetPanel<PlayBoard>().Hide();
                yield return new WaitUntil(() => UiManager.GetPanel<VnPanel>().nextButton.gameObject.activeSelf);
                UiManager.GetPanel<VnPanel>().SetNextButton(false, 0, null);
                UiManager.GetPanel<PlayBoard>().Show();
            }
            yield break;
        }
        public static IEnumerator rewardCoroutine()
        {
            if (UiManager.GetPanel<RewardPanel>().IsVisible)
            {
                GameMaster.Instance.CurrentGameRun.CurrentStation.Status = StationStatus.Battle;
                UiManager.GetPanel<RewardPanel>().gameObject.SetActive(false);
                yield return new WaitUntil(() => UiManager.GetPanel<VnPanel>().nextButton.gameObject.activeSelf);
                GameMaster.Instance.CurrentGameRun.CurrentStation.Status = StationStatus.Finished;
                UiManager.GetPanel<RewardPanel>().gameObject.SetActive(true);
                Station station = GameMaster.Instance.CurrentGameRun.CurrentStation;
                yield return new WaitUntil(() => station != GameMaster.Instance.CurrentGameRun.CurrentStation);
                UiManager.GetPanel<RewardPanel>().gameObject.SetActive(false);
            }
            yield break;
        }
    }
}