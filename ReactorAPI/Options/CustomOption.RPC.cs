using ReactorAPI.Helpers;
using Hazel;
using Reactor;
using Reactor.Networking;
using System;
using System.Linq;

namespace ReactorAPI.Options
{
    public partial class CustomOption
    {

        [RegisterCustomRpc(0)]
        private protected class RPC : PlayerCustomRpc<APIPlugin, (byte[], CustomOptionType, object)>
        {
            public static RPC Instance { get { return Rpc<RPC>.Instance; } }

            public RPC(APIPlugin plugin, uint id) : base(plugin, id)
            {
            }

            public override RpcLocalHandling LocalHandling { get { return RpcLocalHandling.None; } }

            public override void Write(MessageWriter writer, (byte[], CustomOptionType, object) option)
            {
                writer.Write(option.Item1);
                writer.Write((byte)option.Item2);
                if (option.Item2 == CustomOptionType.Toggle) writer.Write((bool)option.Item3);
                else if (option.Item2 == CustomOptionType.Number) writer.Write((float)option.Item3);
                else if (option.Item2 == CustomOptionType.String) writer.Write((int)option.Item3);
            }

            public override (byte[], CustomOptionType, object) Read(MessageReader reader)
            {
                byte[] sha1 = reader.ReadBytes(SHA1Helper.Length);
                CustomOptionType type = (CustomOptionType)reader.ReadByte();
                object value = null;
                if (type == CustomOptionType.Toggle) value = reader.ReadBoolean();
                else if (type == CustomOptionType.Number) value = reader.ReadSingle();
                else if (type == CustomOptionType.String) value = reader.ReadInt32();

                return (sha1, type, value);
            }

            public override void Handle(PlayerControl sender, (byte[], CustomOptionType, object) option)
            {
                if (sender?.Data == null) return;

                byte[] sha1 = option.Item1;
                CustomOptionType type = option.Item2;
                CustomOption customOption = Options.FirstOrDefault(o => o.Type == type && o.SHA1.SequenceEqual(sha1));

                if (customOption == null)
                {
                    APIPlugin.Logger.LogWarning($"Received option that could not be found, sha1: \"{string.Join("", sha1.Select(b => $"{b:X2}"))}\", type: {type}.");

                    return;
                }

                object value = option.Item3;

                if (Debug) APIPlugin.Logger.LogInfo($"\"{customOption.ID}\" type: {type}, value: {value}, current value: {customOption.Value}");

                customOption.SetValue(value, true);

                if (Debug) APIPlugin.Logger.LogInfo($"\"{customOption.ID}\", set value: {customOption.Value}");
            }
        }

        public static implicit operator (byte[] SHA1, CustomOptionType Type, object Value)(CustomOption option)
        {
            return (option.SHA1, option.Type, option.GetValue<object>());
        }
    }
}