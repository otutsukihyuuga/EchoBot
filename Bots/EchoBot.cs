// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using AdaptiveCards.Templating;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Builder.Teams;
using Newtonsoft.Json.Linq;
using System;

namespace EchoBot1.Bots
{
    class Series
    {
        public string name;
        public string genre;
    }
    public class EchoBot : TeamsActivityHandler
    {
        string helloImage;
        private IConfiguration configuration;
        public EchoBot(IConfiguration iconfig)
        {
            configuration = iconfig;
            helloImage = configuration.GetSection("BaseUrl").Value + configuration.GetSection("HelloImage").Value;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string msg = turnContext.Activity.Text;
            string[] words = msg.Split(' ');
            //string replyText;
            if (words.Length == 3 && words[0].Equals("I") && words[1].Equals("am"))
            {
                string name = words[2];
                HeroCard card = new()
                {
                    Title = $"Hello {name}",
                    Text = "lorem ipsum blah blah\n\nmore blah blah\n\neven more blah blah",
                    Images = new List<CardImage>() { new CardImage(helloImage) },
                    Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.OpenUrl, "Visit Google", null, null, null,"https://google.com"),
                    new CardAction(ActionTypes.OpenUrl, "Visit Bing", null, null, null, "https://bing.com"),
                }
                };

                //var response = MessageFactory.Attachment(card.ToAttachment());
                var response = MessageFactory.Attachment(new List<Attachment>());
                response.Attachments.Add(card.ToAttachment());
                await turnContext.SendActivityAsync(response, cancellationToken);
            }
            else if(words.Length == 4 && words[0].Equals("I") && words[1].Equals("am"))
            {
                string name = words[2];
                string query = words[3];
                var response = MessageFactory.Attachment(new List<Attachment>());
                switch (query)
                {
                    case "Hero":
                        response.Attachments.Add(GetHeroCard(name).ToAttachment());
                        break;
                    case "Thumbnail":
                        response.Attachments.Add(GetThumbnailCard(name).ToAttachment());
                        break;
                    case "Signin":
                        response.Attachments.Add(GetSigninCard(name).ToAttachment());
                        break;
                    case "Adaptive":
                        response.Attachments.Add(GetAdaptiveCard(name));
                        break;
                    default:
                        break;
                }
                await turnContext.SendActivityAsync(response, cancellationToken);
            }
            else
            {
                //Activity reply = activity.CreateReply();
                //reply.Type = ActivityTypes.Typing;
                //reply.Text = null;
                //ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //await connector.Conversations.ReplyToActivityAsync(reply);


                string replyText = $"Echo: {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(replyText, cancellationToken: cancellationToken);
            }
        }
        public static Attachment GetAdaptiveCard(string name)
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            Series[] series = new Series[]
            {
                new Series{name = "One Piece", genre="Pirates World-Building"},
                new Series{name = "The Boys", genre="Sci-Fi Super Hero"},
                new Series{name = "Money Heist", genre="Thriller"},
            };
            AdaptiveCardTemplate adaptiveCardTemplate = new AdaptiveCardTemplate(adaptiveCardJson);
            var Data = new
            {
                name = name,
                series = series
            };
            adaptiveCardJson = adaptiveCardTemplate.Expand(Data);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
        private SigninCard GetSigninCard(string name)
        {
            var signinCard = new SigninCard
            {
                Text = $"Hello {name} this is Sign-in Card",
                Buttons = new List<CardAction> { 
                    new CardAction(ActionTypes.Signin, "Sign-in to microsoft", value: "https://login.microsoftonline.com/"), 
                    new CardAction(ActionTypes.Signin, "Sign-in to google", value: "https://accounts.google.com/")
                },
            };

            return signinCard;
        }
        private ThumbnailCard GetThumbnailCard(string name)
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = $"Hello {name} this is Thumbnail Card",
                Subtitle = "Thumbnail Card",
                Text = "lorem ipsum blah blah\n\nmore blah blah\n\neven more blah blah",
                Images = new List<CardImage> { new CardImage(helloImage) },
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.OpenUrl, "Visit Google", null, null, null,"https://google.com"),
                    new CardAction(ActionTypes.OpenUrl, "Visit Bing", null, null, null, "https://bing.com"),
                },
            };

            return thumbnailCard;
        }
        private HeroCard GetHeroCard(string name)
        {
            var heroCard = new HeroCard
            {
                Title = $"Hello {name} this is Hero Card",
                Subtitle = "Hero Card",
                Text = "lorem ipsum blah blah\n\nmore blah blah\n\neven more blah blah",
                Images = new List<CardImage>() { new CardImage(helloImage) },
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.OpenUrl, "Visit Google", null, null, null,"https://google.com"),
                    new CardAction(ActionTypes.OpenUrl, "Visit Bing", null, null, null, "https://bing.com"),
                }
            };

            return heroCard;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var welcomeText = $"Hello and welcome! {turnContext.Activity.GetLocale()}";
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(welcomeText,cancellationToken: cancellationToken);
                }
            }
        }
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var text = query?.Parameters?[0]?.Value as string ?? string.Empty;

            var packages = await GetSuggestions(text);

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
            var attachments = packages.Select(package =>
            {
                var previewCard = new ThumbnailCard { Title = package.Item1, Tap = new CardAction { Type = "invoke", Value = package } };
                

                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard { Title = "Main title Hero" },
                    Preview = previewCard.ToAttachment()
                };

                return attachment;
            }).ToList();


            //this returns a list dropdown which rn at the moment would be empty until below function isnt done
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };
        }

        private Task<IEnumerable<(string,string)>> GetSuggestions(string text)
        {
            //logic here remaining
            var suggestions = new List<(string,string)>
            {
                ($"Search All for {text}",text),
                ($"Search Article for {text}",text),
                ($"Search Project Document for {text}",text),
                ($"Search Lessons Learnt for {text}",text),
                ($"Search Procedure for {text}",text),
                ($"Search Templates for {text}",text),
                ($"Search Situations for {text}",text),
            };
            return Task.FromResult<IEnumerable<(string,string)>>(suggestions);
        }
        private List<CardAction> getResults(string query)
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            Random rnd = new Random((int)t.TotalSeconds);
            int n = rnd.Next(1, 10);

            List<CardAction> Results = new List<CardAction>();
            for(int i=1;i<=n;i++)
            {
                CardAction result = new CardAction
                {
                    Type = ActionTypes.OpenUrl,
                    Title = $"Google Search Result {i}",
                    Value = $"https://www.google.com/search?q={query}"
                };
                Results.Add(result);
            }


            return Results;
        }
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            var (choosedSuggetion,q) = query.ToObject<(string,string)>();

            var card = new ThumbnailCard
            {
                Title = $"{choosedSuggetion}",
                Subtitle = $"You chose {choosedSuggetion}",
                Buttons = getResults(choosedSuggetion),
            };
            
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            MessagingExtensionActionResponse result = new()
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = GetAdaptiveCardAttachmentFromFile("fetchTaskCard.json"),
                        Width = 400,
                        Title = "Welcome {memberName}",
                    },
                },
            };
            return Task.FromResult(result);
        }
        private static Attachment GetAdaptiveCardAttachmentFromFile(string fileName)
        {
            //Read the card json and create attachment.
            string[] paths = { ".", "Resources", fileName };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            string choosedSuggetion = JsonConvert.DeserializeObject<IDictionary<string, string>>(action.Data.ToString())["query"];
            string button = JsonConvert.DeserializeObject<IDictionary<string, string>>(action.Data.ToString())["id"];
            var card = new ThumbnailCard();
            switch (button)
            {
                case "google":
                    card = new ThumbnailCard
                    {
                        Title = $"{choosedSuggetion}",
                        Subtitle = $"Google result {choosedSuggetion}",
                        Buttons = new List<CardAction>
                        {
                            new CardAction { Type = ActionTypes.OpenUrl, Title = "Google Search Result", Value = $"https://www.google.com/search?q={choosedSuggetion}" },
                        },
                    };
                    break;
                case "youtube":
                    card = new ThumbnailCard
                    {
                        Title = $"{choosedSuggetion}",
                        Subtitle = $"Youtube result {choosedSuggetion}",
                        Buttons = new List<CardAction>
                        {
                            new CardAction { Type = ActionTypes.OpenUrl, Title = "YouTube Search Result", Value = $"https://www.youtube.com/results?search_query={choosedSuggetion}" },
                        },
                    };
                    break;
                default:
                    break;
            }


            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };

            MessagingExtensionActionResponse result = new()
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new List<MessagingExtensionAttachment> { attachment },
                }
            };

            return Task.FromResult(result);
        }

    }
}
