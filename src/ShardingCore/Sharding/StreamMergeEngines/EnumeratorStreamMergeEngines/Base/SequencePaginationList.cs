﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Base
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 8:31:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class SequencePaginationList
    {
        private readonly IEnumerable<RouteQueryResult<long>> _routeQueryResults;
        private long? _skip;
        private long? _take;

        public SequencePaginationList(IEnumerable<RouteQueryResult<long>> routeQueryResults)
        {
            _routeQueryResults = routeQueryResults;
        }
        public SequencePaginationList Skip(long? skip)
        {
            if (skip > int.MaxValue)
                throw new ShardingCoreException($"not support skip more than {int.MaxValue}");
            _skip = skip;
            return this;
        }
        public SequencePaginationList Take(long? take)
        {
            if (take > int.MaxValue)
                throw new ShardingCoreException($"not support take more than {int.MaxValue}");
            _take = take;
            return this;
        }

        public ICollection<SequenceResult> ToList()
        {
            ICollection<SequenceResult> routeResults = new LinkedList<SequenceResult>();

            var currentSkip = _skip.GetValueOrDefault();
            var currentTake = _take;
            bool stopSkip = false;
            bool needBreak = false;
            foreach (var routeQueryResult in _routeQueryResults)
            {
                if (!stopSkip)
                {
                    if (routeQueryResult.QueryResult > currentSkip)
                    {
                        stopSkip = true;
                    }
                    else
                    {
                        currentSkip = currentSkip - routeQueryResult.QueryResult;
                        continue;
                    }
                }

                var currentRealSkip = currentSkip;
                var currentRealTake = routeQueryResult.QueryResult-currentSkip;
                if (currentSkip != 0)
                    currentSkip = 0;
                if (currentTake.HasValue)
                {
                    if (currentTake.Value <= currentRealTake)
                    {
                        currentRealTake = currentTake.Value;
                        needBreak = true;
                    }
                    else
                    {
                        currentRealTake = currentTake.Value-currentRealTake;
                    }
                }
                var sequenceResult = new SequenceResult(currentRealSkip, currentRealTake, routeQueryResult.RouteResult);
                routeResults.Add(sequenceResult);

                if (needBreak)
                    break;

            }

            return routeResults;
        }
    }
    public class SequenceResult
    {
        public SequenceResult(long skip, long take, RouteResult routeResult)
        {
            Skip = (int)skip;
            Take = (int)take;
            RouteResult = routeResult;
        }

        public int Skip { get; }
        public int Take { get; }

        public RouteResult RouteResult { get; }
    }
}