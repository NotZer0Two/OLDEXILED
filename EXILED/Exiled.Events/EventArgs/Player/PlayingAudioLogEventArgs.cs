// -----------------------------------------------------------------------
// <copyright file="PlayingAudioLogEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using API.Features;

    using Interfaces;

    /// <summary>
    /// Contains all information before a player plays the AudioLog.
    /// </summary>
    public class PlayingAudioLogEventArgs : IPlayerEvent, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayingAudioLogEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="colliderId">
        /// <inheritdoc cref="byte" />
        /// </param>
        /// <param name="isAllowed">
        /// <inheritdoc cref="IsAllowed" />
        /// </param>
        public PlayingAudioLogEventArgs(Player player, byte colliderId, bool isAllowed = true)
        {
            Player = player;
            Type = colliderId;
            IsAllowed = isAllowed;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the audio will start.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets a value indicating the type of AudioLog.
        /// </summary>
        public byte Type { get; }

        /// <summary>
        /// Gets the player who started the AudioLog.
        /// </summary>
        public Player Player { get; }
    }
}