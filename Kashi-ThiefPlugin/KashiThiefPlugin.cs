using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using System.Linq;

public class KashiThiefPlugin : RocketPlugin<KashiThiefConfiguration>
{
    public static KashiThiefPlugin Instance { get; private set; }
    private Dictionary<UnturnedPlayer, StolenItemData> playerTheftItems = new Dictionary<UnturnedPlayer, StolenItemData>();
    private Dictionary<UnturnedPlayer, float> playerCooldowns = new Dictionary<UnturnedPlayer, float>();
    private System.Random random = new System.Random();
    private Dictionary<UnturnedPlayer, Coroutine> activeThefts = new Dictionary<UnturnedPlayer, Coroutine>();

    public string GetPrefix()
    {
        return Configuration.Instance.Prefix;
    }

    public Color PrefixColor { get; private set; }
    public Color MessageColor { get; private set; }
    public string AlertPrefix => Configuration.Instance.AlertPrefix;

    protected override void Load()
    {
        Instance = this;
        PrefixColor = UnturnedChat.GetColorFromName(Configuration.Instance.PrefixColor, Color.yellow);
        MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.yellow);
        U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdatePosition += OnPlayerUpdatePosition;
        Rocket.Core.Logging.Logger.Log("gaspçı plsi online baba.");
    }

    protected override void Unload()
    {
        U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
        Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdatePosition -= OnPlayerUpdatePosition;
        Rocket.Core.Logging.Logger.Log("gaspçı plsi devredisi problem yok devam.");
    }

    private void OnPlayerDisconnected(UnturnedPlayer player)
    {
        if (playerTheftItems.ContainsKey(player))
        {
            playerTheftItems.Remove(player);
        }
        if (activeThefts.ContainsKey(player))
        {
            StopCoroutine(activeThefts[player]);
            activeThefts.Remove(player);
        }
    }

    private void OnPlayerUpdatePosition(UnturnedPlayer player, Vector3 position)
    {
        if (activeThefts.ContainsKey(player))
        {
            UnturnedPlayer target = playerTheftItems[player].Target;
            if (Vector3.Distance(player.Position, target.Position) > Configuration.Instance.Radius)
            {
                UnturnedChat.Say(player, Configuration.Instance.TheftCancelledMessage, MessageColor);
                StopCoroutine(activeThefts[player]);
                activeThefts.Remove(player);
                playerTheftItems.Remove(player);
            }
        }
    }

    public void RegisterTheft(UnturnedPlayer thief, UnturnedPlayer target, bool isLeftPocket)
    {
        string pocket = isLeftPocket ? "sol" : "sağ";
        UnturnedChat.Say(thief, $"{GetPrefix()} {target.CharacterName} adlı vatandaşın {pocket} cebini karıştırıyorsun sabırlı ol...", PrefixColor);

        if (target.Inventory == null || !HasItems(target))
        {
            UnturnedChat.Say(thief, $"{GetPrefix()} Kişinin üzeri boş gibi gözüküyor, hemen oradan uzaklaş!", PrefixColor);
            return;
        }

        Coroutine theftCoroutine = StartCoroutine(AttemptPocketTheft(thief, target, isLeftPocket));
        activeThefts[thief] = theftCoroutine;
    }

    private IEnumerator AttemptPocketTheft(UnturnedPlayer thief, UnturnedPlayer target, bool isLeftPocket)
    {
        yield return new WaitForSeconds(3);

        if (random.NextDouble() < 0.5 || isLeftPocket)
        {
            var item = GetRandomItem(target);
            if (item != null)
            {
                playerTheftItems[thief] = item;
                ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, item.ItemJar.item.id);
                UnturnedChat.Say(thief, $"{GetPrefix()} Eşya: {itemAsset.itemName}. /esyayical ile eşyayı çal.", PrefixColor);
                KashiThiefPlugin.Instance.StartCoroutine(AlertTarget(target));
                activeThefts.Remove(thief);
                yield break;
            }
        }

        if (!isLeftPocket)
        {
            UnturnedChat.Say(thief, $"{GetPrefix()} Sağ cepte bir şey bulamadın, sol tarafı dene!", PrefixColor);
        }
        else
        {
            UnturnedChat.Say(thief, $"{GetPrefix()} Sol cepte bir şey bulamadın, soygun iptal edildi!", PrefixColor);
        }

        KashiThiefPlugin.Instance.StartCoroutine(AlertTarget(target));
        activeThefts.Remove(thief);
    }

    private IEnumerator AlertTarget(UnturnedPlayer target)
    {
        yield return new WaitForSeconds(3);
        UnturnedChat.Say(target, $"{AlertPrefix} {Configuration.Instance.TargetAlertMessage}", MessageColor);
    }

    public void CompleteTheft(UnturnedPlayer thief)
    {
        if (!playerTheftItems.ContainsKey(thief))
        {
            UnturnedChat.Say(thief, $"{GetPrefix()} {Configuration.Instance.NoTheftMessage}", PrefixColor);
            return;
        }

        StolenItemData stolenItem = playerTheftItems[thief];
        UnturnedPlayer target = stolenItem.Target;

        if (Vector3.Distance(thief.Position, target.Position) > Configuration.Instance.Radius)
        {
            UnturnedChat.Say(thief, $"{GetPrefix()} {Configuration.Instance.TheftCancelledMessage}", PrefixColor);
            playerTheftItems.Remove(thief);
            return;
        }

        KashiThiefPlugin.Instance.StartCoroutine(GiveStolenItem(thief, target, stolenItem));
    }

    private IEnumerator GiveStolenItem(UnturnedPlayer thief, UnturnedPlayer target, StolenItemData item)
    {
        for (int i = 5; i > 0; i--)
        {
            UnturnedChat.Say(thief, $"{GetPrefix()} Eşyayı çalıyorsun, biraz bekle! [{i} sn]", PrefixColor);
            yield return new WaitForSeconds(1);
        }

        thief.GiveItem(item.ItemJar.item.id, item.ItemJar.item.amount);
        target.Inventory.removeItem(item.Page, (byte)item.Index);

        UnturnedChat.Say(thief, $"{GetPrefix()} {Configuration.Instance.TheftSuccessMessage}", PrefixColor);
        UnturnedChat.Say(target, $"{AlertPrefix} {Configuration.Instance.TargetAlertMessage}", PrefixColor);

        playerTheftItems.Remove(thief);
    }

    public StolenItemData GetRandomItem(UnturnedPlayer player)
    {
        if (player.Inventory == null)
        {
            return null;
        }

        List<StolenItemData> items = new List<StolenItemData>();

        for (byte page = 0; page < PlayerInventory.PAGES; page++)
        {
            if (player.Inventory.items[page] == null)
            {
                continue;
            }

            var pageItems = player.Inventory.items[page].items;
            for (int i = 0; i < pageItems.Count; i++)
            {
                items.Add(new StolenItemData
                {
                    ItemJar = pageItems[i],
                    Page = page,
                    Index = i,
                    Target = player
                });
            }
        }

        if (items.Count == 0) return null;

        System.Random rand = new System.Random();
        int index = rand.Next(items.Count);

        return items[index];
    }

    public bool IsPlayerFacingAway(UnturnedPlayer player, UnturnedPlayer target)
    {
        Vector3 directionToTarget = (target.Position - player.Position).normalized;
        float dotProduct = Vector3.Dot(player.Player.transform.forward, directionToTarget);
        return dotProduct < 0;
    }

    public bool CanAttemptTheft(UnturnedPlayer thief)
    {
        if (playerCooldowns.ContainsKey(thief))
        {
            float lastAttemptTime = playerCooldowns[thief];
            float currentTime = Time.time;
            if (currentTime - lastAttemptTime < Configuration.Instance.TheftCooldown)
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateTheftCooldown(UnturnedPlayer thief)
    {
        playerCooldowns[thief] = Time.time;
    }

    private bool HasItems(UnturnedPlayer player)
    {
        for (byte page = 0; page < PlayerInventory.PAGES; page++)
        {
            if (player.Inventory.items[page] != null && player.Inventory.items[page].items.Count > 0)
            {
                return true;
            }
        }
        return false;
    }
}

public class StolenItemData
{
    public ItemJar ItemJar { get; set; }
    public byte Page { get; set; }
    public int Index { get; set; }
    public UnturnedPlayer Target { get; set; }
}
