﻿using NBitcoin;
using System.Threading;
using System.Threading.Tasks;

namespace Stratis.Bitcoin.Features.BlockStore.LoopSteps
{
    /// <summary>
    /// Check if the next chained block already exists in the BlockRepository.
    /// <para>
    /// If the block exists in the repository the step 
    /// will return a Continue result which execute a 
    /// "Continue" on the while loop
    /// </para>
    /// <para>
    /// If the block does not exists in the repository the step 
    /// will return a Next() result which'll cause the BlockStoreLoop to execute 
    /// the next step (<seealso cref="ProcessPendingStorageStep"/>)
    /// </para>
    /// </summary>
    internal sealed class CheckNextChainedBlockExistStep : BlockStoreLoopStep
    {
        internal CheckNextChainedBlockExistStep(BlockStoreLoop blockStoreLoop)
            : base(blockStoreLoop)
        {
        }

        /// <inheritdoc/>
        internal override async Task<BlockStoreLoopStepResult> ExecuteAsync(ChainedBlock nextChainedBlock, CancellationToken cancellationToken, bool disposeMode)
        {
            if (await this.BlockStoreLoop.BlockRepository.ExistAsync(nextChainedBlock.HashBlock))
            {
                await this.BlockStoreLoop.BlockRepository.SetBlockHash(nextChainedBlock.HashBlock);

                this.BlockStoreLoop.SetStoreTip(nextChainedBlock);

                return BlockStoreLoopStepResult.Continue();
            }

            return BlockStoreLoopStepResult.Next();
        }
    }
}