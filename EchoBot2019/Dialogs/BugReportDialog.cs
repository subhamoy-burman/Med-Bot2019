﻿using EchoBot2019.Models;
using EchoBot2019.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot2019.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion  


        public BugReportDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                //DescriptionStepAsync,
                //CallbackTimeStepAsync,
                //PhoneNumberStepAsync,
                //BugStepAsync,
                ConfirmSerialNumberStepAsync,
                ConfirmPrimaryIssueTypeStepAsync,
                ConfirmSecondaryIssueTypeStepAsnc,
                SummaryStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.isSerialNumberConfirmed"));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.primaryIssueType"));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.secondaryIssueType"));


            // Set the starting Dialog
            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> ConfirmSecondaryIssueTypeStepAsnc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["primaryIssueType"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.secondaryIssueType",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select the issue type from below"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Hardware preventing the use of software",
                        "Incompatible software", "No therapy screen coming up", "Neurotransmistter Reset", "Other" })
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmPrimaryIssueTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["isSerialNumberConfirmed"] = ((FoundChoice)stepContext.Result).Value;

            //return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.primaryIssueType",
            //    new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text("Please enter the type of issue you want to report."),
            //        Choices = ChoiceFactory.ToChoices(new List<string> { "Accessory", "Telemetry", "Programmer/Controller", "Recharger", "Patient App Issue" })
            //    }, cancellationToken);

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.primaryIssueType",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter the type of issue you want to report"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Accessory", "Telemetry", "Programmer", "Recharger", "App Issue" })
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmSerialNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.isSerialNumberConfirmed",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please confirm your device serial number: RNT8019919"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
                }, cancellationToken) ;
        }



        #region Waterfall Steps
        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a description for your report")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["description"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter in a callback time"),
                    RetryPrompt = MessageFactory.Text("The value entered must be between the hours of 9 am and 5 pm."),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["callbackTime"] = Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value);

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter in a phone number that we can call you back at"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid phone number"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phoneNumber"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.bug",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter the type of bug."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["secondaryIssueType"] = ((FoundChoice)stepContext.Result).Value;


            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            userProfile.SecondaryIssueType = (string)stepContext.Values["secondaryIssueType"];
            userProfile.PrimaryIssueType = (string)stepContext.Values["primaryIssueType"];

            await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(userProfile.SecondaryIssueType)
                }, cancellationToken);

            // Show the summary to the user
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summary of your bug report:"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Description: Primary issue :{0} \n Details: {1}", userProfile.PrimaryIssueType, userProfile.SecondaryIssueType)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Thanks for reporting the issue. Your RFR code #67891001 - note it for future reference")), cancellationToken);

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Callback Time: {0}", userProfile.CallbackTime.ToString())), cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Phone Number: {0}", userProfile.PhoneNumber)), cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Bug: {0}", userProfile.Bug)), cancellationToken);

            // Get the current profile object from user state.
            /* var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

             // Save all of the data inside the user profile
             userProfile.Description = (string)stepContext.Values["description"];
             userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
             userProfile.PhoneNumber = (string)stepContext.Values["phoneNumber"];
             userProfile.Bug = (string)stepContext.Values["bug"];

             // Show the summary to the user
             await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summary of your bug report:"), cancellationToken);
             await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Description: {0}", userProfile.Description)), cancellationToken);
             await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Callback Time: {0}", userProfile.CallbackTime.ToString())), cancellationToken);
             await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Phone Number: {0}", userProfile.PhoneNumber)), cancellationToken);
             await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Bug: {0}", userProfile.Bug)), cancellationToken);

             // Save data in userstate
             await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

             // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.*/
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion

        #region Validators
        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                var resolution = promptContext.Recognized.Value.First();
                DateTime selectedDate = Convert.ToDateTime(resolution.Value);
                TimeSpan start = new TimeSpan(9, 0, 0); //9 o'clock
                TimeSpan end = new TimeSpan(17, 0, 0); //5 o'clock
                if ((selectedDate.TimeOfDay >= start) && (selectedDate.TimeOfDay <= end))
                {
                    valid = true;
                }
            }
            return Task.FromResult(valid);
        }

        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value, @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$").Success;
            }
            return Task.FromResult(valid);
        }

        #endregion
    }
}

