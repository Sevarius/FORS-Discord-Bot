using System.Threading.Tasks;
using Contract.Interfaces;
using Discord;

namespace DiscordServices
{
    public class MessageSender : IMessageSender
    {
        private readonly IMessageChannel _channel;

        public MessageSender(IMessageChannel channel)
        {
            _channel = channel;
        }
        public async Task SendMessageAsync(string message)
        {
            await _channel.SendMessageAsync(message);
        }
    }
}