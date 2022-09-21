﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 12:59:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IDataSourceRouteRuleEngineFactory
    {
        DataSourceRouteResult Route(IQueryable queryable, IShardingDbContext shardingDbContext, Dictionary<Type, IQueryable> queryEntities);
        //DataSourceRouteResult Route(DataSourceRouteRuleContext ruleContext);
    }
}
