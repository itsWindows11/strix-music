﻿namespace OwlCore.Remoting.Transfer
{
    /// <summary>
    /// Contains information for transporting the calls of a <see cref="RemoteMethodProxy{TResult}"/>.
    /// </summary>
    public class RemoteMethodProxyMessage : IRemoteMemberMessage
    {
        /// <summary>
        /// Creates a new instance of <see cref="RemoteMemberMessageBase"/>.
        /// </summary>
        /// <param name="memberInstanceId">A unique identifier for this instance, consistent between hosts and clients.</param>
        /// <param name="targetName">The name of the target member being changed or invoked.</param>
        /// <param name="result"></param>
        public RemoteMethodProxyMessage(string memberInstanceId, string targetName, object? result)
        {
            MemberInstanceId = memberInstanceId;
            TargetName = targetName;
            Result = result;
        }

        /// <inheritdoc/>
        public string MemberInstanceId { get; set; }

        /// <inheritdoc/>
        public string TargetName { get; set; }

        /// <inheritdoc/>
        public RemotingAction Action { get; set; }

        /// <summary>
        /// The result value
        /// </summary>
        public object? Result { get; set; }
    }
}
