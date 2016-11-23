using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;


namespace botapplication
{
    [Serializable]
    public enum Gender
    {
        Male = 1, Female = 2
    };
    [Serializable]
    public enum Currency
    {
        AUD, BNG, BRL, CAD, CHF, CNY, CZK, DKK, GBP, HKD, HRK, HUF, IDR, ILS, INR, JPY, KRW, MXN, MYR, NOK, NZD, PHP, PLN, RON, RUB, SEK, SGD, THB, TRY, USD, ZAR
    };
    [Serializable]
    public class BankForm
    {
        // these are the fields that will hold the data
        // we will gather with the form
        [Prompt("What is your first name? {||}")]
        public string FirstName;
        [Prompt("What is your last name? {||}")]
        public string LastName;
        [Prompt("What is your gender? {||}")]
        public Gender Gender;
        [Prompt("What is your preferred currency? {||}")]
        public Currency Currency;
        // This method 'builds' the form 
        // This method will be called by code we will place
        // in the MakeRootDialog method of the MessagesControlller.cs file
        public static IForm<BankForm> BuildForm()
        {
            return new FormBuilder<BankForm>()
                    .Message("Please fill out the following details so I can get to know you!")
                    .OnCompletion(async (context, profileForm) =>
                    {
                        // Tell the user that the form is complete
                        await context.PostAsync("Your profile is complete.");
                    })
                    .Build();
        }
    }
    // This enum provides the possible values for the 
    // Gender property in the ProfileForm class
    // Notice we start the options at 1 

}