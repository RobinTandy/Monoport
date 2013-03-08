﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformerMonoPort
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    public class EnemyNewA : PlatformerEntity
    {
        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 64.0f;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public EnemyNewA()
        {
            //NOTE: Load content should really be sepererate from creation
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public override void LoadContent()//string spriteSet)
        {
            // Load animations.
            string spriteSet = "Sprites/" + "MonsterA" + "/";
            runAnimation = new Animation(Manager.Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Manager.Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }


        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public override void Update()
        {
            if (Manager.Level.Player.IsAlive)
            {
                float elapsed = (float)Manager.GameTime.ElapsedGameTime.TotalSeconds;

                // Calculate tile position based on the side we are walking towards.
                float posX = Position.X + localBounds.Width / 2 * (int)direction;
                int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
                int tileY = (int)Math.Floor(Position.Y / Tile.Height);

                if (waitTime > 0)
                {
                    // Wait for some amount of time.
                    waitTime = Math.Max(0.0f, waitTime - (float)Manager.GameTime.ElapsedGameTime.TotalSeconds);
                    if (waitTime <= 0.0f)
                    {
                        // Then turn around.
                        direction = (FaceDirection)(-(int)direction);
                    }
                }
                else
                {
                    // If we are about to run into a wall or off a cliff, start waiting.
                    if (Manager.Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                        Manager.Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                    {
                        waitTime = MaxWaitTime;
                    }
                    else
                    {
                        // Move in the current direction.
                        Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                        Position = Position + velocity;
                    }
                }

                // Touching an enemy instantly kills the player
                if (this.BoundingRectangle.Intersects(Manager.Level.Player.BoundingRectangle))
                {
                    Manager.Level.Player.OnKilled(this);
                }
            }
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public override void Draw()
        {
            // Stop running when the game is paused or before turning around.
            if (!Manager.Level.Player.IsAlive ||
                Manager.Level.ReachedExit ||
                Manager.Level.TimeRemaining == TimeSpan.Zero ||
                waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }


            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(Manager.GameTime, Manager.SpriteBatch, Position, flip);
        }

        public override void Dispose()
        {
        }
    }
}