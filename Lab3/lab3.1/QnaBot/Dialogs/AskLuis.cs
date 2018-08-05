using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace QnaBot.Dialogs
{
    [Serializable]
    public class AskLuis : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            IMessageActivity activity = (IMessageActivity)await result;
            await context.Forward(new LuisDialog(), AfterLuis, activity, CancellationToken.None);
        }

        public async Task AfterLuis(IDialogContext context, IAwaitable<string> message)
        {
            string val = await message;
            if (val == "none")
            {
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done("");
            }
        }
    }
}
