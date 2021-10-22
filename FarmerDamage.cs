using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools; //enables referencing of Axe and Sword etc
using StardewValley.Characters; //enables junimo type
using System.Collections.Generic; //enables the List type
using StardewValley.Monsters; //allows greenslime and other monster types
using MultiplayerPvP;//allows me to access other files in the solution

public class FarmerDamage	
{
	RockCrab damager = new RockCrab(); //use as default farmer damage receiver and dealer

	//called when farmer is damanged
	//constructor
	public FarmerDamage() {
	}

	//TODO: implement bounding box implementation here.

	//called by attacker to see if damages another farmer
	//Farmer, rectangle -> bool
	public bool DamageFromHitbox(Farmer who, Rectangle areaOfEffect) {
		//adjust bounding box position
		int bwidth = Game1.player.GetBoundingBox().Width;
		int bheight = Game1.player.GetBoundingBox().Height;
		int bx = Game1.player.GetBoundingBox().X + 256; //x adjustment to match game weirdness
		int by = Game1.player.GetBoundingBox().Y - 56; //y adjustment
		Rectangle newBB = new Rectangle(bx, by, bwidth, bheight);
		return who.GetBoundingBox().Intersects(areaOfEffect);
	}

	//referenced whenever a player is damaged
	//int, bool -> Null
	public void Damaged(int damage, bool parry) {
		Game1.player.takeDamage(damage,parry,damager); //automatically gives invincibility for a time
	}

	//calculate damage based on weapon and rng
	//weapon -> int
	public int CalcDamage(MeleeWeapon weapon, Farmer who, Farmer monster){
		//monsterbox, daggerspecial stun time, knockback, addedPrecision, should play miss when damage parried by opponent?
		int minDamage = weapon.minDamage;
		int maxDamage = weapon.maxDamage;
		float knockBackModifier = weapon.knockback;
		Double critChance = weapon.critChance;
		float critMultiplier = weapon.critMultiplier;
		int addedPrecision = weapon.addedPrecision; 

		GameLocation currLocation = Game1.player.currentLocation; //should work bc only called when player is loaded in
		StardewValley.Multiplayer gamemultiplayer = new StardewValley.Multiplayer();

		//var multiplayer = ModEntry.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue(); //probably need to set this up in modentry with helper

		//KNOCKBACK
		Microsoft.Xna.Framework.Rectangle monsterBox = monster.GetBoundingBox();
		Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(monsterBox, who);
		if (knockBackModifier > 0f)
		{
			trajectory *= knockBackModifier;
		}
		else
		{
			trajectory = new Vector2(monster.xVelocity, monster.yVelocity);
		}
		//if (monster.Slipperiness == -1)
		//{
		//	trajectory = Vector2.Zero;
		//}

		bool crit = false; //initializing
		int damageAmount = 0;

		//Check crit profession
		if (who.professions.Contains(25)) //if a combat profession increases crit chance
		{
			critChance += critChance * 0.5f;
		}

		//IF DAMAGE DEALT
		//if damge was dealt
		if (maxDamage >= 0)
		{
			damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
			if (who != null && Game1.random.NextDouble() < (double)(critChance + (float)who.LuckLevel * (critChance / 40f))) //check if crit
			{
				crit = true;
				Game1.playSound("crit");
			}
			damageAmount = (crit ? ((int)((float)damageAmount * critMultiplier)) : damageAmount); //calculate damage amount including crit
			damageAmount = Math.Max(1, damageAmount + ((who != null) ? (who.attack * 3) : 0));

			if (who != null && who.professions.Contains(24)) //readjust if player has profession
			{
				damageAmount = (int)Math.Ceiling((float)damageAmount * 1.1f);
			}
			if (who != null && who.professions.Contains(26))
			{
				damageAmount = (int)Math.Ceiling((float)damageAmount * 1.15f);
			}
			if (who != null && crit && who.professions.Contains(29))
			{
				damageAmount = (int)((float)damageAmount * 2f);
			}
			if (who != null)
			{
				foreach (BaseEnchantment enchantment in who.enchantments) //adjust for enchantments 
				{
					enchantment.OnCalculateDamage(damager, currLocation, who, ref damageAmount); //give opponent farmer a default monster type rock crab (not bug or undead, shadows, or mummies), changed this to currlocation
				}
			}
			//below line modifies based on monster resilience and miss chance TURNS DAMAGEAMOUNT TO NEG1 IF MISSED
			//damageAmount = damager.takeDamage(damageAmount, (int)trajectory.X, (int)trajectory.Y, isBomb: false , (double)addedPrecision / 10.0, who); //send calculated damage to monster, replaces isBomb with false, replace monster with damger

			//takeDamge function takes care of parries and resilience and rings
			//if (damageAmount == -1) //sometimes will miss if not giving damage (CONTROLLED BY COMMENTED OUT LINE ABOVE)
			//{
				//currLocation.debris.Add(new Debris("Miss", 1, new Vector2(monsterBox.Center.X, monsterBox.Center.Y), Color.LightGray, 1f, 0f));
			//}
			
			
		//currLocation.removeDamageDebris(damager); //replaced this with currlocation and monster with damager
		//currLocation.debris.Add(new Debris(damageAmount, new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y), crit ? Color.Yellow : new Color(255, 130, 0), crit ? (1f + (float)damageAmount / 300f) : 1f, monster));
		if (who != null)
		{
			foreach (BaseEnchantment enchantment2 in who.enchantments)
			{
				enchantment2.OnDealDamage(damager, currLocation, who, ref damageAmount); //damager type rock crab, changed this to currlocation
			}
		}
			
		}
		else //if no damage, send monster sliding backward, slipperiness
		{
			damageAmount = -2;
			monster.setTrajectory(trajectory);
			//monster.xVelocity /= 2f;
			//monster.yVelocity /= 2f;
		}
		if (who != null && who.CurrentTool != null && who.CurrentTool.Name.Equals("Galaxy Sword")) //galaxy sword animation
		{
			//removed Game1. prefix (from all multiplayer references)
			gamemultiplayer.broadcastSprites(currLocation, new TemporaryAnimatedSprite(362, Game1.random.Next(50, 120), 6, 1, new Vector2(monsterBox.Center.X - 32, monsterBox.Center.Y - 32), flicker: false, flipped: false));
		}

		else if (damageAmount > 0) //if given damage is positive
		{
			//monster.shedChunks(Game1.random.Next(1, 3)); //changed monster to damager
			//creates number throwing animations for when crit the monster and foot wind animations
			if (crit)
			{
				Vector2 standPos = monster.getStandingPosition();
				gamemultiplayer.broadcastSprites(currLocation, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32f, 32f), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
				{
					scale = 0.75f,
					alpha = (crit ? 0.75f : 0.5f)
				});
				gamemultiplayer.broadcastSprites(currLocation, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
				{
					scale = 0.5f,
					delayBeforeAnimationStart = 50,
					alpha = (crit ? 0.75f : 0.5f)
				});
				gamemultiplayer.broadcastSprites(currLocation, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
				{
					scale = 0.5f,
					delayBeforeAnimationStart = 100,
					alpha = (crit ? 0.75f : 0.5f)
				});
				gamemultiplayer.broadcastSprites(currLocation, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
				{
					scale = 0.5f,
					delayBeforeAnimationStart = 150,
					alpha = (crit ? 0.75f : 0.5f)
				});
				gamemultiplayer.broadcastSprites(currLocation, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
				{
					scale = 0.5f,
					delayBeforeAnimationStart = 200,
					alpha = (crit ? 0.75f : 0.5f)
				});
			}
		}

		return damageAmount;
	}


	//calculate defense from equipment and buffs
	//weapon -> int
	public int CalcDefense(MeleeWeapon weapon) {
		return 0;
	}

}
