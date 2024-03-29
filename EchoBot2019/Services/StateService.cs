﻿using EchoBot2019.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot2019.Services
{
    public class StateService
    {
        public UserState UserState { get; set; }
        public ConversationState ConversationState { get; set; }

        public static string UserProfileId { get; } = $"{nameof(StateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(StateService)}.ConversationData";

        public static string DialogStateId { get; } = $"{nameof(StateService)}.DialogState";

        /// <summary>
        /// User profile accessor
        /// </summary>
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public StateService(UserState userState, ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));

            InitializeAccessors();
        }

        public void InitializeAccessors()
        {
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
            ConversationDataAccessor = UserState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);

        }
    }
}
