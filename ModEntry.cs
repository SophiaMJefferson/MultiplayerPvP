using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools; //enables referencing of Axe and Sword etc
using StardewValley.Characters; //enables junimo type

using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace SDVMod1
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        //TODO: Knockback and invincibility on hit
        //TODO: Number animations on damage
        //TODO: Must face direction of opponent to hit (Bounding box and area of effect implementation perhaps)
        //TODO: Fix location bug in farm cabins and mines (maybe do bounding box intersections?)
        //TODO: Fix requirement that button must be pushed to update online players. Update every tick instead.
        //TODO: Calculate amount of damage to send(need weapon stats)
        //TODO: Cannot stop showing frame on timer

        int playernum; //number of online players
        Farmer[] farmarray = new Farmer[4]; //array of online players
        //Junimo jun = new Junimo(new Vector2(Game1.player.getTileX(), Game1.player.getTileY() + 2) * 64f, 2, temporary: true);
        //^ YOU WERE CRASHING MY GAME
        //GameLocation g = default;
        static bool UsingToolOnPreviousTick = false;
        int j = 0;
        static bool gameloaded = false;
        static int frametime = 1000;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Player.InventoryChanged += this.Player_InventoryChanged;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.GameLoop.UpdateTicked += (o,e) => UpdateTime(Game1.currentGameTime);
            helper.Events.GameLoop.UpdateTicked += this.PlayerUsedTool;
            helper.Events.GameLoop.SaveLoaded += (o,e) => OnSaveLoaded();

        }
        private void OnSaveLoaded() {
            gameloaded = true;
        }

        private void UpdateTime(GameTime gameTime) {
            if (gameloaded) {
                //this.Monitor.Log($"{gameTime.ElapsedGameTime.Milliseconds} elapsed", LogLevel.Debug);
                frametime -= 100;
            }
            if (frametime == 0) {
                Game1.player.stopShowingFrame();
            }
        }
                                                                                                                     
        //Raised after player makes a change to their inventory.
        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            this.Monitor.Log($"{Game1.player.Name} has updated their inventory.", LogLevel.Debug);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console
            Game1.playSound("coin");
            Game1.player.stamina = 100;
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
            //Game1.player.stopShowingFrame();

            //Check who is in the farm and add them to an array
            var players = Game1.getOnlineFarmers();
            int i = 0;
            foreach (Farmer player in players)
            {
                this.Monitor.Log($"{player.Name} is in game.", LogLevel.Debug);
                farmarray[i] = player;
                i++;

                //DEBUGGING PRINT STATEMENTS:
                //this.Monitor.Log($"I am at{Game1.player.currentLocation}, and you are {player.currentLocation}", LogLevel.Debug);
                //this.Monitor.Log($"I am at {Game1.player.getTileX()}, and you are at {player.getTileX()}", LogLevel.Debug);
            }
            playernum = i;
            if (e.Button == SButton.N) {
                Game1.player.showFrame(j, true); //these two lines mimic multiplayer damage code
                frametime = 1000;
                //Game1.player.changeShirt(j);
                //Game1.player.changeHairStyle(j);
                //Game1.player.changeSkinColor(j);
                //System.Threading.Thread.Sleep(100);
                this.Monitor.Log($"Showing Frame {j}", LogLevel.Debug); //There are 125 frames
                j++;
                //g.characters.Add(jun);
                //FarmerSprite.AnimationFrame(int frame, int millisecond, int positionOffset, bool secondaryArm, bool flip, endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false, int xOffset = 0)
            }

            //this.Monitor.Log($"getToolLocation gives {Game1.player.GetToolLocation()}", LogLevel.Debug);
        }

        //Method that detects when used weapon
        private void PlayerUsedTool(object sender, UpdateTickedEventArgs e) {
            if (Game1.player.UsingTool != UsingToolOnPreviousTick) {
                UsingToolOnPreviousTick = Game1.player.UsingTool; //This happens twice, as it encompasses two ticks
                this.Monitor.Log($"Just used a tool", LogLevel.Debug);
                if (Game1.player.UsingTool && (Game1.player.CurrentTool is MeleeWeapon)){ 
                    this.Monitor.Log($"Just used Melee Weapon", LogLevel.Debug);

                    for (int i=1; i<playernum;i++) { //iterate though other players online
                        if (Math.Abs(farmarray[i].getTileX() - Game1.player.getTileX()) <= 1 && farmarray[i].currentLocation == Game1.player.currentLocation) //send damage if in same location and within 1 tile
                        {
                            string message = farmarray[i].Name; //message is the target player
                            this.Helper.Multiplayer.SendMessage(message, "Damage"); 
                            this.Monitor.Log($"Sent Damage.", LogLevel.Debug);
                        }
                    }
                }
            }
        }

        
        //Raised when any mod message is received.
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e) {
            string message = e.ReadAs<string>();
            this.Monitor.Log($"Damage Message sent to {message}", LogLevel.Debug);
            if(Game1.player.Name == message){
                Game1.player.health -= 10;
                Game1.player.showFrame(99,true);
                frametime = 1000;
                this.Monitor.Log($"Received Damage", LogLevel.Debug);
                
            }
           
            
        }


    }
}
