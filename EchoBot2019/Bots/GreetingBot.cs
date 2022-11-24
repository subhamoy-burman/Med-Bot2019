using EchoBot2019.Models;
using EchoBot2019.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot2019.Bots
{
    public class GreetingBot : ActivityHandler
    {
        private readonly StateService _stateService;

        public GreetingBot(StateService stateService)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
        }

        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            ConversationData conversationData = await _stateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

            if (!string.IsNullOrWhiteSpace(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(string.Format("Hi {0} . How can I help you today?",
                    userProfile.Name)), cancellationToken);
            }
            else
            {
                if(conversationData.PromptedUserForName)
                {
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    await turnContext.SendActivityAsync
                        (MessageFactory.Text(string.Format("Thanks {0}. How can I help you today?", userProfile.Name)), cancellationToken);

                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"What is your name?"), cancellationToken);
                    conversationData.PromptedUserForName = true;
                }

                await _stateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _stateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

                await _stateService.UserState.SaveChangesAsync(turnContext);
                await _stateService.ConversationState.SaveChangesAsync(turnContext);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach(var member in membersAdded)
            {
                if(member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }
    }
}
