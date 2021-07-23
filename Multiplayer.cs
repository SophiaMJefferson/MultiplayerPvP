using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools; //enables referencing of Axe and Sword etc
using StardewValley.Characters; //enables junimo type
using System.Collections.Generic; //enables the List type

public class Multiplayer
{
    private Farmer[] farmarray = new Farmer[4];
    private int onlineplayers;

    public Multiplayer()
	{
        this.onlineplayers = 0;
	}

    //updates online farmer information
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
