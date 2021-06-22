﻿using Microsoft.Toolkit.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace OwlCore.Remoting.Transfer.MessageConverters
{
    /// <inheritdoc/>
    public class NewtonsoftRemoteMessageConverter : IRemoteMessageConverter
    {
        /// <inheritdoc/>
        public Task<IRemoteMessage> DeserializeAsync(byte[] message)
        {
            var jsonStr = Encoding.UTF8.GetString(message);

            var deserializedBase = JsonConvert.DeserializeObject<RemoteMemberMessageBase>(jsonStr);

            IRemoteMessage result = deserializedBase.Action switch
            {
                RemotingAction.None => deserializedBase,
                RemotingAction.MethodCall => JsonConvert.DeserializeObject<RemoteMethodCallMessage>(jsonStr),
                RemotingAction.PropertyChange => JsonConvert.DeserializeObject<RemotePropertyChangeMessage>(jsonStr),
                RemotingAction.RemoteMethodProxy => JsonConvert.DeserializeObject<RemoteMethodProxyMessage>(jsonStr),
                RemotingAction.ExceptionThrown => JsonConvert.DeserializeObject<RemoteExceptionDataMessage>(jsonStr),
                _ => ThrowHelper.ThrowNotSupportedException<IRemoteMemberMessage>(),
            };

            if (result is RemoteMethodCallMessage memberMessage)
                memberMessage.TargetMemberSignature = memberMessage.TargetMemberSignature.Replace("TARGETNAME_", "");

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<byte[]> SerializeAsync(IRemoteMessage message)
        {
            var methodCallMessage = message as RemoteMethodCallMessage;

            // Newtonsoft won't serialize a string containing a method signature.
            if (methodCallMessage != null)
                methodCallMessage.TargetMemberSignature = $"TARGETNAME_{methodCallMessage.TargetMemberSignature}";

            var jsonStr = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);

            // Newtonsoft won't serialize a string containing a method signature.
            if (methodCallMessage != null)
                methodCallMessage.TargetMemberSignature = methodCallMessage.TargetMemberSignature.Replace("TARGETNAME_", "");

            return Task.FromResult(bytes);
        }
    }
}
