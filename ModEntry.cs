using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools; //enables referencing of Axe and Sword etc

namespace SDVMod1
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        //TODO: Fix location bug in farm cabins and mines (maybe do bounding box intersections?)
        //TODO: Fix requirement that button must be pushed to update online players. Update every tick instead.
        //TODO: Calculate amount of damage to send(need weapon stats)

        int playernum; //number of online players
        Farmer[] farmarray = new Farmer[4]; //array of online players

        static bool UsingToolOnPreviousTick = false;

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
            //helper.Events.Player.UpdateTicked += PlayerUsedTool;
            helper.Events.GameLoop.UpdateTicked += this.PlayerUsedTool;
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
                this.Monitor.Log($"Received Damage", LogLevel.Debug);
            }
           
            
        }


    }
}
