using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools; //enables referencing of Axe and Sword etc
using StardewValley.Characters; //enables junimo type
using System.Collections.Generic; //enables the List type
using StardewValley.Network; //to use outgoing message

public class NetworkUtility
{
    private Farmer[] farmarray = new Farmer[1];
    private int onlineplayers;
    public const byte uniqueMessageType = 39;//Some random number that the game doesn't use (Check Multiplayer.processIncomingMessage)

    public enum MessageTypes
    {
        TAKE_DAMAGE,
        RELAY_MESSAGE_TO_ANOTHER_PLAYER, //first 4 bytes: MessageTypes, next 8 bytes is player ID then next is the message
    }

    public NetworkUtility()
	{
        this.onlineplayers = 0;
	}

    //i don't know if i need this yet
    public static void RelayMessageFromClientToAnotherPlayer(long targetID, params object[] message)
    {
        var objectsList = new List<object>() { (int)MessageTypes.RELAY_MESSAGE_TO_ANOTHER_PLAYER, targetID };
        objectsList.AddRange(message);

        var relayMessage = new StardewValley.Network.OutgoingMessage(uniqueMessageType, Game1.player, objectsList.ToArray());

        if (Game1.IsServer)
            throw new Exception("Server should not be sending relay instructions");
        else
            Game1.client.sendMessage(relayMessage);
    }

    //message must contain damage amount
    public static void SendDamageToPlayer(int damage, Farmer target, long? damagerID = null) => SendDamageToPlayer(damage, target.UniqueMultiplayerID, damagerID);
    public static void SendDamageToPlayer(int damage, long targetID, long? damagerID = null)
    {
        object[] objects;
        if (damagerID.HasValue)
            objects = new object[] { (int)MessageTypes.TAKE_DAMAGE, damage, damagerID.Value };
        else
            objects = new object[] { (int)MessageTypes.TAKE_DAMAGE, damage };

        if (Game1.IsServer)
            Game1.server.sendMessage(targetID, new OutgoingMessage(uniqueMessageType, Game1.player, objects));
        else
            RelayMessageFromClientToAnotherPlayer(targetID, objects);
    }

    //updates online farmer array and counter information
    public void checkOnline() {
        var players = Game1.getOnlineFarmers();
        int i = 0;
        foreach (Farmer player in players)
        {
            //this.Monitor.Log($"{player.Name} is in game.", LogLevel.Debug);
            farmarray[i] = player;
            i++;
        }
        this.onlineplayers = i;
    }

    //returns number of online players
    public int getOnlineNum() {
        checkOnline();
        return this.onlineplayers; 
    }

    //returns array of all online players
    public Farmer[] getFarmers() {
        checkOnline();
        return farmarray;
    }
}
