using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace SDVMod1
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        //TODO: Fix location bug in farm cabins and mines (maybe do bounding box intersections?)

        Farmer farmhand;
        Farmer farmhand2;
        Farmer farmhand3;

        Farmer[] farmarray = new Farmer[4];
        int i = 0;

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
        }

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

            // print button presses to the console window
            Game1.playSound("coin");
            Game1.player.stamina = 100;
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            //To attack press N
            if (e.Button == SButton.N) 
            {
                this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

                //TODO: Add all farmhands
                //Maximum four farmhands are in a farm
                var players = Game1.getOnlineFarmers();
                i = 0;
                foreach (Farmer player in players)
                {
                    this.Monitor.Log($"{player.Name} is in game.", LogLevel.Debug);
                    farmhand = player;
                    farmarray[i] = player;
                    i++;

                    this.Monitor.Log($"I am at{Game1.player.currentLocation}, and you are {player.currentLocation}", LogLevel.Debug);
                    this.Monitor.Log($"I am at {Game1.player.getTileX()}, and you are at {player.getTileX()}", LogLevel.Debug);
                }
                farmhand = farmarray[1];

                //send damage if in same location and within 1 tile
                //TODO: Recognize using melee weapon to do damage
                this.Monitor.Log($"getToolLocation gives {Game1.player.GetToolLocation()}", LogLevel.Debug);

                if (Math.Abs(farmhand.getTileX() - Game1.player.getTileX()) <= 1 && farmhand.currentLocation == Game1.player.currentLocation) {
                    int message = 10;
                    this.Helper.Multiplayer.SendMessage(message, "Damage");
                    this.Monitor.Log($"Sent Damage.", LogLevel.Debug);
                }                
            }
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e) {
            Game1.player.health -= 10;
            this.Monitor.Log($"Received Damage", LogLevel.Debug);
            
        }


    }
}
