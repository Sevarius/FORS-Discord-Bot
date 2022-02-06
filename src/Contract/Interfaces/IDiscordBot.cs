using System.Threading.Tasks;
using Discord;

namespace Contract.Interfaces
{
    public interface IDiscordBot
    {
        void StartBot();

        Task WriteMessageToChannel(ulong channelId, string message);

        IMessageChannel GetChannelById(ulong channelId);
    }
}