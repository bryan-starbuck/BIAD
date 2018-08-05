﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace QnaBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            try
            {
                await context.Forward(new QnaDialog(), AfterQnA, activity, CancellationToken.None);
            }
            catch (Exception e)
            {
                // If an error occurred with QnA Maker Service, post it out to the user
                await context.PostAsync(e.Message);

                // Wait for the next message from the user
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterTrivia(IDialogContext context, IAwaitable<string> result)
        {
            await context.PostAsync("Thanks for playing!");
            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterQnA(IDialogContext context, IAwaitable<object> result)
        {
            string message = null;

            try
            {
                message = (string)await result;
            }
            catch (Exception e)
            {
                await context.PostAsync($"QnAMaker: {e.Message}");
                // Wait for the next message
                context.Wait(MessageReceivedAsync);
            }

            // If the message summary - NOT_FOUND, then it's time to echo
            if (String.IsNullOrEmpty(message))
            {
                message = HttpUtility.UrlEncode(((IMessageActivity)context.Activity).Text);

                if (message.ToLowerInvariant().Contains("trivia"))
                {
                    // Since we are not needing to pass any message to start trivia, we can use call instead of forward
                    context.Call(new TriviaDialog(), AfterTrivia);
                }
                else
                {
                    // Otherwise, echo...
                    await context.PostAsync($"You said: \"{message}\"");
                    // Wait for the next message
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // Display the answer from QnA Maker Service
                var answer = message;

                if (!string.IsNullOrEmpty(answer))
                {
                    Activity reply = ((Activity)context.Activity).CreateReply();

                    string[] qnaAnswerData = answer.Split('|');
                    string title = qnaAnswerData[0];
                    string description = qnaAnswerData[1];
                    string url = qnaAnswerData[2];
                    string imageURL = qnaAnswerData[3];

                    if (title == "")
                    {
                        char charsToTrim = '|';
                        await context.PostAsync(answer.Trim(charsToTrim));
                    }

                    else
                    {
                        HeroCard card = new HeroCard
                        {
                            Title = title,
                            Subtitle = description,
                        };
                        card.Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
                    };
                        card.Images = new List<CardImage>
                    {
                        new CardImage( url = imageURL)
                    };
                        reply.Attachments.Add(card.ToAttachment());
                        await context.PostAsync(reply);
                    }
                }
                else
                {
                    await context.PostAsync("Sorry, I am having trouble finding the answer to that. Try again later.");
                }
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}
