﻿using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Net.Peer;

namespace Mineral.Common.Overlay.Server
{
    public class PeerServer
    {
        #region Field
        private IChannel channel = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public async void Start(int port)
        {
            IEventLoopGroup boss_group = new MultithreadEventLoopGroup(1);
            IEventLoopGroup worker_group = new MultithreadEventLoopGroup();

            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();

                bootstrap.Group(boss_group, worker_group);
                bootstrap.Channel<TcpServerSocketChannel>();
                bootstrap.Option(ChannelOption.SoBacklog, 100);
                //bootstrap.Option(ChannelOption.SoKeepalive, true);
                //bootstrap.Option(ChannelOption.MessageSizeEstimator, DefaultMessageSizeEstimator.Default);
                //bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(Args.Instance.Node.ConnectionTimeout));
                bootstrap.Handler(new LoggingHandler("SRV-LTSN"));
                bootstrap.ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                    channel.Pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    channel.Pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                    channel.Pipeline.AddLast("echo", new EchoServerHandler());
                }));

                //bootstrap.ChildHandler(new NettyChannelInitializer("", false));

                Logger.Info("Tcp listener started, bind port : " + port);

                this.channel = await bootstrap.BindAsync(port);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message, e);
            }
            finally
            {
            }
        }

        public async void Close()
        {
            if (this.channel != null && this.channel.Active)
            {
                try
                {
                    Logger.Info("Closing TCP server...");
                    await this.channel.CloseAsync();
                }
                catch (Exception e)
                {
                    Logger.Warning("Closing TCP server failed." + e.Message);
                }
            }
        }
        #endregion
    }
}
