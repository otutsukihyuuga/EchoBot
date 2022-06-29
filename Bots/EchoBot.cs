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
using System.Data.SqlClient;
using AdaptiveCards;
using System.Data;

namespace EchoBot1.Bots
{
    class Series
    {
        public string name;
        public string genre;
    }
    class Result
    {
        public string Document;
        public string Project;
        public string Answer;
        public string Context;
        public string Description;
        public string number;
        public string searchIcon;
        public string type;
    }
    public class EchoBot : TeamsActivityHandler
    {
        Dictionary<int, string> number2word = new() {
                                  {1, "one"},
                                  {2, "two"},
                                {3, "three"},
            { 4,"four"},
            { 5,"five"}
        };
        string helloImage;
        string searchIcon;
        private IConfiguration configuration;
        SqlConnection connection = new SqlConnection("Server=tcp:suggestion-db.database.windows.net,1433;Initial Catalog=suggestion-db;Persist Security Info=False;User ID=pratham;Password=P11112001@p;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=1000;");

        //db functions
        List<string> GetTypeofContents(string query, string user)
        {
            List<string> Contents = new();
            try
            {
                connection.Open();
                using SqlCommand command = new("GetTypeofContents", connection);
                command.Parameters.Add("@query", SqlDbType.NVarChar).Value = query;
                command.Parameters.Add("@user", SqlDbType.NVarChar).Value = user;
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string content = reader["TypeOfContent"].ToString();
                    Contents.Add(content);
                }
            }
            finally
            {
                connection.Close();
            }
            return Contents;
        }
        List<(string, string, string, string, string, string)> GetAnswer(string query,string TypeofContent, string user)
        {
            string storedProcedure;
            if (TypeofContent == "Question")
                storedProcedure = "GetTop5Question";
            else
                storedProcedure = "GetTop5";

            List<(string, string, string, string, string, string)> answers = new();

            try
            {
                connection.Open();
                using SqlCommand command = new(storedProcedure, connection);
                command.Parameters.Add("@query", SqlDbType.NVarChar).Value = query;
                if (TypeofContent != "Question")
                    command.Parameters.Add("@TypeofContent", SqlDbType.NVarChar).Value = TypeofContent;
                command.Parameters.Add("@user", SqlDbType.NVarChar).Value = user;
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    (string, string, string, string, string, string) answer;
                    answer.Item1 = reader["ID"].ToString();
                    if (TypeofContent == "Question")
                        answer.Item2 = reader["Answer"].ToString();
                    else
                        answer.Item2 = reader["Title"].ToString();
                    answer.Item3 = reader["DocumentNo"].ToString();
                    answer.Item4 = reader["Project"].ToString();
                    answer.Item5 = reader["ProjectDescription"].ToString();
                    if (TypeofContent == "Question")
                        answer.Item6 = TypeofContent;
                    else
                        answer.Item6 = reader["TypeOfContent"].ToString();
                    answers.Add(answer);
                }
            }
            finally
            {
                connection.Close();
            }

            return answers;
        }

        public EchoBot(IConfiguration iconfig)
        {
            configuration = iconfig;
            helloImage = configuration.GetSection("BaseUrl").Value + configuration.GetSection("HelloImage").Value;
            searchIcon = configuration.GetSection("BaseUrl").Value + configuration.GetSection("searchIcon").Value;
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
                new Series{name = "One Piece", genre="Pirats World-Building"},
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

            string user = turnContext.Activity.From.Id;
            var packages = GetSuggestions(text,user);
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

        private IEnumerable<(string,string)> GetSuggestions(string text, string user)
        {
            List<(string, string)> suggestions = new()
            {
                ($"Search All for {text}",text),
            };
            List<string> TypeofContents = GetTypeofContents(text,user);
            foreach (string TypeofContent in TypeofContents)
            {
                suggestions.Add(($"Search {TypeofContent} for {text}", text));
            }
            return suggestions;
        }
        private AdaptiveCard getResults(string TypeofContent, string query, string user)
        {
            var paths = new[] { ".", "Resources", "results.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            AdaptiveCardTemplate adaptiveCardTemplate = new AdaptiveCardTemplate(adaptiveCardJson);
            //load data
            var answer = GetAnswer(query, TypeofContent, user);
            Result[] results = new Result[answer.Count];
            for (int i = 0; i < answer.Count; i++)
            {
                results[i] = new Result();
                results[i].Document = answer[i].Item3;
                results[i].Project = answer[i].Item4;
                results[i].Answer = answer[i].Item2;
                results[i].Context = answer[i].Item6;
                results[i].Description = answer[i].Item5;
                results[i].number = number2word[(i + 1)];
                results[i].searchIcon = searchIcon;
                if (results[i].Context == "Question")
                    results[i].type = "Answer";
                else
                    results[i].type = "Project Title";
            }

            var Data = new
            {
                query = query,
                result = results,
                TypeofContent = TypeofContent
            };
            adaptiveCardJson = adaptiveCardTemplate.Expand(Data);
            return JsonConvert.DeserializeObject<AdaptiveCard>(adaptiveCardJson);
        }
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            var (choosedSuggetion,q) = query.ToObject<(string,string)>();
            string user = turnContext.Activity.From.Name;
            string beforeFor = choosedSuggetion.Split("for")[0];
            string TypeofContent = beforeFor.Substring(beforeFor.IndexOf(" ")+1).Trim();
            var card = getResults(TypeofContent, q, user);

            var preview = new ThumbnailCard
            {
                Title = $"{TypeofContent} for {q}",
            };

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
                Preview = preview.ToAttachment(),
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
