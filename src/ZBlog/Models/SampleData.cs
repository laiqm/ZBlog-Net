﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZBlog.Common;

namespace ZBlog.Models
{
    public static class SampleData
    {
        public static async Task InitializeZBlog(IServiceProvider serviceProvider, IConfiguration configuration, bool createUsers = true)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var serverProvider = serviceScope.ServiceProvider;
                var dbContext = serverProvider.GetService<ZBlogDbContext>();

                if (await dbContext.Database.EnsureCreatedAsync())
                {
                    if (createUsers)
                    {
                        await CreateAdminUser(dbContext, configuration);
                    }
                    await InsertTestData(serverProvider);
                }
            }
        }

        private static async Task InsertTestData(IServiceProvider serviceProvider)
        {
            var tag1 = new Tag
            {
                Name = "Tag1"
            };
            var tag2 = new Tag
            {
                Name = "Tag2"
            };
            var catalog1 = new Catalog
            {
                Url = "Code",
                PRI = 10,
                Title = "Code"
            };
            var catalog2 = new Catalog
            {
                Url = "Fun",
                PRI = 9,
                Title = "Fun"
            };
            var dbContext = serviceProvider.GetService<ZBlogDbContext>();
            dbContext.Add(tag1);
            dbContext.Add(tag2);
            dbContext.Add(catalog1);
            dbContext.Add(catalog2);
            for (int i = 1; i < 16; i++)
            {
                var post = new Post
                {
                    Title = $"Test-{i}",
                    Url = $"Test-{i}",
                    Summary = $"# Content-{i}",
                    Content = $"# Content-{i}",
                    CreateTime = DateTime.Now,
                    LastEditTime = DateTime.Now,
                    UserId = 1,
                    Catalog = i % 2 == 0 ? catalog1 : catalog2
                };
                dbContext.Add(post);
                var postTag = new PostTag
                {
                    Post = post,
                    Tag = i % 2 == 0 ? tag1 : tag2
                };
                dbContext.Add(postTag);
            }
            await dbContext.SaveChangesAsync();
        }

        private static async Task CreateAdminUser(ZBlogDbContext dbContext, IConfiguration configuration)
        {
            var user = await dbContext.Users.Where(u => u.Name.Equals("zhangmm", StringComparison.OrdinalIgnoreCase)).FirstOrDefaultAsync();
            if (user == null)
            {
                user = new User
                {
                    Name = configuration["User:Name"],
                    NickName = configuration["User:NickName"],
                    Email = configuration["User:Email"],
                    Password = Util.GetMd5(configuration["User:Password"])
                };
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}