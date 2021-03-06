﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.RDS;
using SlackAPI.WebSocketMessages;
using SlackAPIService;
using SlackApp.Data;
using SlackApp.Model;

namespace SlackApp.BotResponses
{
    public class BotLimiterResponse : AbstractSocketResponse
    {
        public List<BotCooldownEntry> BotCooldowns = new List<BotCooldownEntry>();

        public BotLimiterResponse(ISlackClient client) : base(client)
        {
        }

        public override void MessageReceiver(NewMessage message)
        {
            try
            {
                if (message.subtype == "bot_message" && !message.channel.StartsWith('D'))
                {
                    var bot_channel = BotCooldowns
                        .Where((x) => { return x.Botname == message.bot_id && x.Channel == message.channel; })
                        .DefaultIfEmpty(new BotCooldownEntry(String.Empty, string.Empty, DateTime.MinValue, BotCooldownEntry.DefaultCooldown))
                        .FirstOrDefault();

                    if (bot_channel.Botname == message.bot_id && bot_channel.Channel == message.channel && bot_channel.CooldownEnd > DateTime.UtcNow)
                    {
                        Client.DeleteMessage(message);
                    }
                    else
                    {
                        BotCooldowns.Remove(bot_channel);
                        BotCooldowns.Add(new BotCooldownEntry(message.bot_id, message.channel, DateTime.UtcNow + bot_channel.CoolDown, bot_channel.CoolDown));
                    }
                }
            } catch (Exception e)
            {
                
            }

        }

        public override void ReloadResponseTriggers()
        {
            throw new NotImplementedException();
        }

        public override void SaveResponseTrigger<T>( T value)
        {
            throw new NotImplementedException();
        }
    }
}
