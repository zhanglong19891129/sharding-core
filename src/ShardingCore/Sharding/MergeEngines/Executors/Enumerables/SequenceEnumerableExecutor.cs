﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerables
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 13:30:08
    /// Email: 326308290@qq.com
    internal class SequenceEnumerableExecutor<TResult> : AbstractEnumerableExecutor<TResult>
    {
        private readonly IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> _shardingMerger;
        private readonly IQueryable<TResult> _noPaginationQueryable;
        private readonly bool _async;

        public SequenceEnumerableExecutor(StreamMergeContext streamMergeContext, bool async) : base(streamMergeContext)
        {
            _async = async;
            _noPaginationQueryable = streamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().As<IQueryable<TResult>>();
            _shardingMerger = new SequenceEnumerableShardingMerger<TResult>(streamMergeContext, async);
        }

        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var sqlSequenceRouteUnit = sqlExecutorUnit.RouteUnit.As<SqlSequenceRouteUnit>();
            var sequenceResult = sqlSequenceRouteUnit.SequenceResult;
            var shardingDbContext = streamMergeContext.CreateDbContext(sqlSequenceRouteUnit);
            var newQueryable = _noPaginationQueryable
                .Skip(sequenceResult.Skip)
                .Take(sequenceResult.Take)
                .ReplaceDbContextQueryable(shardingDbContext)
                .As<IQueryable<TResult>>();
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext, streamMergeAsyncEnumerator);
        }

        public override IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> GetShardingMerger()
        {
            return _shardingMerger;
        }
    }
}
