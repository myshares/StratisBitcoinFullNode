﻿using Microsoft.Extensions.DependencyInjection;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.RPC.Controllers;
using Stratis.Bitcoin.Features.RPC.Models;
using Xunit;

namespace Stratis.Bitcoin.Tests.RPC.Controller
{
    public class GetInfoActionTests : BaseRPCControllerTest
    {
        [Fact]
        public void CallWithDependencies()
        {
            string dir = AssureEmptyDir("Stratis.Bitcoin.Tests/TestData/GetInfoActionTests/CallWithDependencies");
            IFullNode fullNode = this.BuildServicedNode(dir);
            FullNodeController controller = fullNode.Services.ServiceProvider.GetService<FullNodeController>();

            GetInfoModel info = controller.GetInfo();

            uint expectedProtocolVersion = (uint)NodeSettings.Default().ProtocolVersion;
            var expectedRelayFee = MempoolValidator.MinRelayTxFee.FeePerK.ToUnit(NBitcoin.MoneyUnit.BTC);
            Assert.NotNull(info);
            Assert.Equal(0, info.blocks);
            Assert.NotEqual<uint>(0, info.version);
            Assert.Equal(expectedProtocolVersion, info.protocolversion);
            Assert.Equal(0, info.timeoffset);
            Assert.Equal(0, info.connections);
            Assert.NotNull(info.proxy);
            Assert.Equal(0, info.difficulty);
            Assert.False(info.testnet);
            Assert.Equal(expectedRelayFee, info.relayfee);
            Assert.Empty(info.errors);
        }

    }
}
