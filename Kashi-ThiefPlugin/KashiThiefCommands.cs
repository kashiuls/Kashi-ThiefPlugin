using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using UnityEngine;
using SDG.Unturned;

public class KashiThiefCommands : IRocketCommand
{
    public AllowedCaller AllowedCaller => AllowedCaller.Player;

    public string Name => "cebiniara";

    public string Help => "Bir oyuncunun cebini ara";

    public string Syntax => "";

    public List<string> Aliases => new List<string>();

    public List<string> Permissions => new List<string> { "kashithief.cebiniara" };

    private Dictionary<UnturnedPlayer, bool> playerLastAttemptWasRightPocket = new Dictionary<UnturnedPlayer, bool>();

    public void Execute(IRocketPlayer caller, string[] command)
    {
        UnturnedPlayer thief = (UnturnedPlayer)caller;

        UnturnedPlayer target = GetTarget(thief);
        if (target == null)
        {
            UnturnedChat.Say(thief, $"{KashiThiefPlugin.Instance.GetPrefix()} Önünüzde bir allahsız bulunmakta.", KashiThiefPlugin.Instance.PrefixColor);
            return;
        }

        if (target == thief)
        {
            UnturnedChat.Say(thief, $"{KashiThiefPlugin.Instance.GetPrefix()} Kendinden eşya çalamazsın.", KashiThiefPlugin.Instance.PrefixColor);
            return;
        }

        if (!KashiThiefPlugin.Instance.IsPlayerFacingAway(target, thief))
        {
            UnturnedChat.Say(thief, $"{KashiThiefPlugin.Instance.GetPrefix()} Soymak için oyuncunun arkası dönük olmalı.", KashiThiefPlugin.Instance.PrefixColor);
            return;
        }

        if (!KashiThiefPlugin.Instance.CanAttemptTheft(thief))
        {
            UnturnedChat.Say(thief, $"{KashiThiefPlugin.Instance.GetPrefix()} {KashiThiefPlugin.Instance.Configuration.Instance.CooldownMessage}", KashiThiefPlugin.Instance.PrefixColor);
            return;
        }

        bool isLeftPocket = false;
        if (playerLastAttemptWasRightPocket.ContainsKey(thief) && playerLastAttemptWasRightPocket[thief])
        {
            isLeftPocket = true;
        }
        else
        {
            playerLastAttemptWasRightPocket[thief] = true;
        }

        KashiThiefPlugin.Instance.RegisterTheft(thief, target, isLeftPocket);
        KashiThiefPlugin.Instance.UpdateTheftCooldown(thief);
    }

    private UnturnedPlayer GetTarget(UnturnedPlayer thief)
    {
        float maxDistance = 3.0f;
        UnturnedPlayer target = null;

        foreach (var player in Provider.clients)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(player);
            if (unturnedPlayer != thief)
            {
                float distance = Vector3.Distance(thief.Position, unturnedPlayer.Position);
                if (distance <= maxDistance && KashiThiefPlugin.Instance.IsPlayerFacingAway(unturnedPlayer, thief))
                {
                    target = unturnedPlayer;
                    break;
                }
            }
        }

        return target;
    }
}

public class TakeStolenItemCommand : IRocketCommand
{
    public AllowedCaller AllowedCaller => AllowedCaller.Player;

    public string Name => "esyayical";

    public string Help => "Çalınan eşyayı al";

    public string Syntax => "";

    public List<string> Aliases => new List<string>();

    public List<string> Permissions => new List<string> { "kashithief.esyayical" };

    public void Execute(IRocketPlayer caller, string[] command)
    {
        UnturnedPlayer thief = (UnturnedPlayer)caller;

        KashiThiefPlugin.Instance.CompleteTheft(thief);
    }
}
