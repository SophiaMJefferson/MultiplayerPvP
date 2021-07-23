using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools; //enables referencing of Axe and Sword etc
using StardewValley.Characters; //enables junimo type
using System.Collections.Generic; //enables the List type

public class FarmerDamage	
{	
	//called when farmer is damanged
	//constructor
	public FarmerDamage() {
	}

	public void HitEmote() {
		Game1.player.performPlayerEmote("surprised");
	}
}
