using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FinanceApp.Hubs
{
    public class FinanceAppHub : Hub
    {
        public async Task SendIncomeVsExpenseData(string userId)
        {
            await Clients.User(userId).SendAsync("ReceiveIncomeVsExpenseData");
        }
    }
}

