﻿using Moq;
using NBitcoin;
using Stratis.Bitcoin.BlockPulling;
using Stratis.Bitcoin.Notifications;
using Stratis.Bitcoin.Tests.Logging;
using System.Threading;
using System.Threading.Tasks;
using Stratis.Bitcoin.Common;
using Stratis.Bitcoin.Common.Hosting;
using Xunit;

namespace Stratis.Bitcoin.Tests.Notifications
{
    public class BlockNotificationTest : LogsTestBase
    {
        [Fact]
        public void NotifyStartHashNotOnChainCompletes()
        {
            var startBlockId = new uint256(156);
            var chain = new Mock<ConcurrentChain>();
            chain.Setup(c => c.GetBlock(startBlockId))
                .Returns((ChainedBlock)null);

            var notification = new BlockNotification(chain.Object, new Mock<ILookaheadBlockPuller>().Object, new Signals(), new AsyncLoopFactory(), new NodeLifetime());

            notification.Notify();
        }

        [Fact]
        public void NotifySetsPullerLocationToBlockMatchingStartHash()
        {
            var startBlockId = new uint256(156);
            var chain = new Mock<ConcurrentChain>();
            var header = new BlockHeader();
            chain.Setup(c => c.GetBlock(startBlockId))
                .Returns(new ChainedBlock(header, 0));

            var stub = new Mock<ILookaheadBlockPuller>();
            var lifetime = new NodeLifetime();
            stub.Setup(s => s.NextBlock(lifetime.ApplicationStopping))
                .Returns((Block)null);

            var notification = new BlockNotification(chain.Object, stub.Object, new Signals(), new AsyncLoopFactory(), lifetime);

            notification.Notify();
            notification.SyncFrom(startBlockId);
            notification.SyncFrom(startBlockId);
            stub.Verify(s => s.SetLocation(It.Is<ChainedBlock>(c => c.Height == 0 && c.Header.GetHash() == header.GetHash())));
        }

        [Fact]
        public async Task NotifyWithoutSyncFromRunsWithoutBroadcastingBlocks()
        {
            var lifetime = new NodeLifetime();
            new CancellationTokenSource(100).Token.Register(() => lifetime.StopApplication());

            var startBlockId = new uint256(156);
            var chain = new Mock<ConcurrentChain>();
            var header = new BlockHeader();
            chain.Setup(c => c.GetBlock(startBlockId))
                .Returns(new ChainedBlock(header, 0));
            var stub = new Mock<ILookaheadBlockPuller>();
            stub.SetupSequence(s => s.NextBlock(lifetime.ApplicationStopping))
                .Returns(new Block())
                .Returns(new Block())
                .Returns((Block)null);

            var signals = new Mock<ISignals>();
            var signalerMock = new Mock<ISignaler<Block>>();
            signals.Setup(s => s.Blocks)
                .Returns(signalerMock.Object);

            var notification = new BlockNotification(chain.Object, stub.Object, signals.Object, new AsyncLoopFactory(), lifetime);

            await notification.Notify();

            signalerMock.Verify(s => s.Broadcast(It.IsAny<Block>()), Times.Exactly(0));
        }

        [Fact]
        public async Task NotifyWithSyncFromSetBroadcastsOnNextBlock()
        {
            var lifetime = new NodeLifetime();
            new CancellationTokenSource(100).Token.Register(() => lifetime.StopApplication());

            var startBlockId = new uint256(156);
            var chain = new Mock<ConcurrentChain>();
            var header = new BlockHeader();
            chain.Setup(c => c.GetBlock(startBlockId))
                .Returns(new ChainedBlock(header, 0));

            var stub = new Mock<ILookaheadBlockPuller>();
            stub.SetupSequence(s => s.NextBlock(lifetime.ApplicationStopping))
                .Returns(new Block())
                .Returns(new Block())
                .Returns((Block)null);

            var signals = new Mock<ISignals>();
            var signalerMock = new Mock<ISignaler<Block>>();
            signals.Setup(s => s.Blocks)
                .Returns(signalerMock.Object);
            
            var notification = new BlockNotification(chain.Object, stub.Object, signals.Object, new AsyncLoopFactory(), lifetime);

            notification.SyncFrom(startBlockId);
            await notification.Notify();            
            
            signalerMock.Verify(s => s.Broadcast(It.IsAny<Block>()), Times.Exactly(2));
        }
    }
}