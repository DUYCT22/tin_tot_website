using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Messages;

namespace TinTot.Application.Interfaces.Messages
{
    public interface IMessageRealtimePublisher
    {
        Task PublishToUsersAsync(int senderId, int receiverId, ChatMessageDto message);
    }
}
