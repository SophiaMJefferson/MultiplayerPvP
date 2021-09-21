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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace MultiplayerPvP
{
    /// <summary>The mod entry point.</summary>
    /// testing
    public class ModEntry : Mod
    {
        //TODO: Use unique multiplayer id to communicate between players
        //TODO: Knockback on hit
        //TODO: Bounding box and area of effect combat implementation
        //TODO: Fix requirement that a button must be pushed to update online players. Update every tick instead.


        public static IModHelper Helper { get; private set; }
        FarmerDamage DamageMan = new FarmerDamage(); //used to control what happens when a farmer is damaged.
        NetworkUtility MManager = new NetworkUtility(); //Manage multiplayer interactions
        static bool UsingToolOnPreviousTick = false;
        static bool gameloaded = false;
        MeleeWeapon currWeapon;
        //int playernum; //number of online players
        Farmer[] farmarray = new Farmer[4]; //array of online players (Could use .getOnlineFarmers() I think)
        static int frametime = 1000; //frametime is unused so far
        SpriteBatch spriteBatch = new SpriteBatch(GameRunner.instance.GraphicsDevice);
        PresentationParameters pp = GameRunner.instance.GraphicsDevice.PresentationParameters;

        /*********
        ** Public methods
        *********/
        // The mod entry point, called after the mod is first loaded.
        public override void Entry(IModHelper helper)
        {
            Helper = helper;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Player.InventoryChanged += this.Player_InventoryChanged;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.GameLoop.UpdateTicked += (o,e) => UpdateTime(Game1.currentGameTime);
            helper.Events.GameLoop.UpdateTicked += this.PlayerUsedTool;
            helper.Events.GameLoop.SaveLoaded += (o,e) => OnSaveLoaded();
            helper.Events.Display.Rendered += (o,e) => OnRendered();
        }

        /*********
        ** Private methods
        *********/
        //Raised after the player presses a button on the keyboard, controller, or mouse.
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        /// These are all the methods evoked on response to an event. Every helper method should be implemented in another class

        //every time a new scene is loaded this top square persists, next use to render AOE and BB
        private System.EventHandler<StardewModdingAPI.Events.RenderedEventArgs> OnRendered() {
            //int width = pp.BackBufferWidth;
            //int height = pp.BackBufferHeight;
            SurfaceFormat format = pp.BackBufferFormat;
            RenderTarget2D texture = new RenderTarget2D(GameRunner.instance.GraphicsDevice, 100, 100, mipMap: false, format, DepthFormat.None);
            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, null);
            this.spriteBatch.Draw(texture, new Rectangle(0, 0, 100, 100), Color.White);
			this.spriteBatch.End();
            return null;
        }

        //For debugging, draw AOE and BB
        private void DrawIntersection(Rectangle areaofeffect, Farmer who) {
            int width = who.GetBoundingBox().Width;
            int height = who.GetBoundingBox().Height;
            SurfaceFormat format = pp.BackBufferFormat;
            RenderTarget2D texture = new RenderTarget2D(GameRunner.instance.GraphicsDevice, 100, 100, mipMap: false, format, DepthFormat.None);
            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, null);
            this.spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            this.spriteBatch.End();
        }

        private void OnSaveLoaded() {
            gameloaded = true;
        }

        //Used to determine time to show a frame for
        private void UpdateTime(GameTime gameTime) {
            if (gameloaded) {
                frametime -= 100;
            }
        }
                                                                                                                     
        //Raised after player makes a change to their inventory.
        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            this.Monitor.Log($"{Game1.player.Name} has updated their inventory.", LogLevel.Debug);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //testing features can go here
            if (e.Button == SButton.N) {
                //for calculating damage (need the try bc must send valid type to method)
                try //try to cast current item to type meleeweapon if possible
                {
                    //calculate damage to give to player
                    currWeapon = (MeleeWeapon)Game1.player.CurrentTool;
                    this.Monitor.Log($"Calculated damage: {DamageMan.CalcDamage(currWeapon, Game1.player, Game1.player)}", LogLevel.Debug);

                    //testing features
                    //Vector2 tileLocation1 = Vector2.Zero; //never used after this?
                    //Vector2 tileLocation2 = Vector2.Zero; //never used after this?
                    //Farmer lastUser = currWeapon.getLastFarmerToUse();
                    //Vector2 usedOnTile = lastUser.GetToolLocation() / 64f;
                    //Rectangle areaOfEffect = currWeapon.getAreaOfEffect((int)usedOnTile.X, (int)usedOnTile.Y, lastUser.facingDirection, ref tileLocation1, ref tileLocation2, lastUser.GetBoundingBox(), lastUser.FarmerSprite.currentAnimationIndex);
                    //Rectangle areaOfEffect = currWeapon.getAreaOfEffect(1, 1, Game1.player.facingDirection, ref tileLocation1, ref tileLocation2, Game1.player.GetBoundingBox(), Game1.player.FarmerSprite.currentAnimationIndex);
                    //this.Monitor.Log($"Intersection =  {DamageMan.DamageFromHitbox(Game1.player, areaOfEffect)}", LogLevel.Debug);
                    //Farmer lastUser = Game1.player;
                    //Vector2 v = lastUser.getUniformPositionAwayFromBox(lastUser.FacingDirection, 48);
                    //currWeapon.DoDamage(Game1.currentLocation, (int)v.X, (int)v.Y, lastUser.FacingDirection, 1, lastUser);

                    //check intersection
                    //bring intersection = true/false for each online farmer
                    //foreach (Farmer i in Game1.getOnlineFarmers()) {
                    //    this.Monitor.Log($"Area of effect: {areaOfEffect}", LogLevel.Debug);
                    //    this.Monitor.Log($"Player Bounding Box: {Game1.player.GetBoundingBox()}", LogLevel.Debug);
                    //    this.Monitor.Log($"For farmer {i.Name}", LogLevel.Debug);
                    //    this.Monitor.Log($"Intersection =  {DamageMan.DamageFromHitbox(i, areaOfEffect)}", LogLevel.Debug);
                    //}
                }
                catch (InvalidCastException exception)
                {
                    DamageMan.Damaged(0,false); //no damage is given to player
                    this.Monitor.Log($"Caught {exception}",LogLevel.Debug);
                }
            }

        }

        //TODO experimentation for Bounding-Box implementation
        //When swing weapon check for area of effect intersection with opponent bounding box, then call send damage method

        //Method that detects when used weapon on another player and sends a damage mod message (antiquated)
        private void PlayerUsedTool(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.player.UsingTool != UsingToolOnPreviousTick)
            {
                UsingToolOnPreviousTick = Game1.player.UsingTool; //This happens twice, as it encompasses two ticks
                this.Monitor.Log($"Just used a tool", LogLevel.Debug);

                try //try to cast current item to type meleeweapon if possible
                {
                    //calculate damage to give to player
                    currWeapon = (MeleeWeapon)Game1.player.CurrentTool;

                    //testing features
                    Vector2 tileLocation1 = Vector2.Zero; //never used after this?
                    Vector2 tileLocation2 = Vector2.Zero; //never used after this?
                    Farmer lastUser = currWeapon.getLastFarmerToUse();
                    Vector2 usedOnTile = lastUser.GetToolLocation(); // / 64f;
                    Rectangle areaOfEffect = currWeapon.getAreaOfEffect((int)usedOnTile.X, (int)usedOnTile.Y, lastUser.facingDirection, ref tileLocation1, ref tileLocation2, lastUser.GetBoundingBox(), lastUser.FarmerSprite.currentAnimationIndex);

                    //check intersection
                    //bring intersection = true/false for each online farmer
                    foreach (Farmer i in Game1.getOnlineFarmers())
                    {
                        this.Monitor.Log($"Used on tile: {(int)usedOnTile.X},{(int)usedOnTile.Y}", LogLevel.Debug);
                        this.Monitor.Log($"Area of effect: {areaOfEffect}", LogLevel.Debug);
                        this.Monitor.Log($"Player Bounding Box: {i.GetBoundingBox()}", LogLevel.Debug);
                        this.Monitor.Log($"For farmer {i.Name}", LogLevel.Debug);
                        this.Monitor.Log($"Intersection =  {(i.GetBoundingBox()).Intersects(areaOfEffect)}", LogLevel.Debug);
                        DrawIntersection(areaOfEffect, i); //testing draw bounding box
                    }
                }
                catch (InvalidCastException exception)
                {
                    //DamageMan.Damaged(0, false); //no damage is given to player
                    this.Monitor.Log($"Caught {exception}", LogLevel.Debug);
                }
            }

        }
            
            
            /**if (Game1.player.UsingTool != UsingToolOnPreviousTick) {
                UsingToolOnPreviousTick = Game1.player.UsingTool; //This happens twice, as it encompasses two ticks
                this.Monitor.Log($"Just used a tool", LogLevel.Debug);
                if (Game1.player.UsingTool && (Game1.player.CurrentTool is MeleeWeapon)){ 
                    this.Monitor.Log($"Just used Melee Weapon", LogLevel.Debug);

                    farmarray = MManager.getFarmers();
                    playernum = MManager.getOnlineNum();
                    for (int i=1; i<playernum;i++) { //iterate though other players online
                        if (Math.Abs(farmarray[i].getTileX() - Game1.player.getTileX()) <= 1 && farmarray[i].currentLocation == Game1.player.currentLocation) //send damage if in same location and within 1 tile
                        {
                            this.Monitor.Log($"Player {farmarray[i]} is within range.");
                            string message = farmarray[i].Name; //message is the target player
                           // Helper.Multiplayer.SendMessage(message, "Damage"); 
                            this.Monitor.Log($"Sent Damage.", LogLevel.Debug);
                        }
                    }
                }
            }
        }**/
        
        
        //Raised when any mod message is received.
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e) {
            string message = e.ReadAs<string>();
            this.Monitor.Log($"Damage Message sent to {message}", LogLevel.Debug);
            if(Game1.player.Name == message){
                DamageMan.Damaged(10,true);
                this.Monitor.Log($"Received Damage", LogLevel.Debug);
                //System.Threading.Thread.Sleep(100);

            }
           
            
        }


    }
}
