﻿using System;
using System.Collections.Generic;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace P2pNet
{


    public class P2pRedis : P2pNetBase
    {

        public ConnectionMultiplexer RedisCon {get; private set; } = null;

        public P2pRedis(IP2pNetClient _client, string _connectionString) : base(_client, _connectionString)
        {
            RedisCon = ConnectionMultiplexer.Connect(_connectionString);
        }

        protected override string _Join(string mainChannel)
        {
            _Listen(mainChannel);
            return _NewP2pId();
        }

        protected override void _Leave()
        {
            // reset. Seems heavy handed
            RedisCon.Close();
            RedisCon = ConnectionMultiplexer.Connect(connectionStr);
        }
        protected override bool _Send(P2pNetMessage msg)
        {
            string msgJSON = JsonConvert.SerializeObject(msg);
            RedisCon.GetSubscriber().Publish(msg.dstChannel, msgJSON);
            return true;
        }

        protected override void _Listen(string channel)
        {
            RedisCon.GetSubscriber().Subscribe(channel, (rcvChannel, msgJSON) => {
                P2pNetMessage msg = JsonConvert.DeserializeObject<P2pNetMessage>(msgJSON);
                _OnReceivedNetMessage(rcvChannel, msg);
            });
        }

        protected override void _StopListening(string channel)
        {
            RedisCon.GetSubscriber().Unsubscribe(channel);
        }

        private  string _NewP2pId()
        {
            return System.Guid.NewGuid().ToString();
        }

    }
}
