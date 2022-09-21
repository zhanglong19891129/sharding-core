using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using System;
using System.Collections.Generic;
using WebApplication1.Data;
using WebApplication1.Data.Helpers;
using WebApplication1.Data.Models;
using WebApplication1.Data.Sharding;

namespace WebApplication1.Pages
{
    public class CreateDbKeyModel : PageModel
    {

        private readonly AbstaractShardingDbContext db;
        private readonly IShardingRuntimeContext runtimeContext;

        [BindProperty]
        public string Key { get; set; }

        public CreateDbKeyModel(AbstaractShardingDbContext db, IShardingRuntimeContext runtimeContext)
        {
            this.db = db;
            this.runtimeContext = runtimeContext;
        }

        public void OnGet()
        {
            Key = "";
        }

        public IActionResult OnPost()
        {
            db.TestModelKeys.Add(new TestModelKey { Key = Key });
            db.SaveChanges();

            // ��ȡ��д�뵽����
            var dblist = JsonFileHelper.Read<List<string>>(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName);
            dblist.Add(Key);
            dblist.Sort();
            JsonFileHelper.Save(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName, dblist);

            // ��̬��������Դ
            DynamicShardingHelper.DynamicAppendDataSourceOnly(runtimeContext, Key, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{Key};");
            //DynamicShardingHelper.DynamicAppendDataSourceOnly(runtimeContext, Key, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{Key};");
            db.Database.Migrate();

            return RedirectToPage("DbKeyMan");
        }
    }
}
