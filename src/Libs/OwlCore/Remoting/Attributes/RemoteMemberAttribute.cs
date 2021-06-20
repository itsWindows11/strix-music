﻿using System;

namespace OwlCore.Remoting.Attributes
{
    /// <summary>
    /// Attribute used in conjuction with <see cref="MemberRemote"/>.
    /// Mark any member with this to control the data flow direction when changes are relayed remotely.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class)]
    public class RemoteMemberAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="RemoteMethodAttribute"/>.
        /// </summary>
        /// <param name="direction">The remoting direction to use when relaying class changes remotely.</param>
        public RemoteMemberAttribute(RemotingDirection direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// The remoting direction to use when relaying class changes remotely.
        /// </summary>
        public RemotingDirection Direction { get; }
    }
}
