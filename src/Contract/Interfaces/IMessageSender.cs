using System.Threading.Tasks;

namespace Contract.Interfaces
{
    public interface IMessageSender
    {
        Task SendMessageAsync(string message);
    }
}