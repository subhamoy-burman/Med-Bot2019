// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using EchoBot2019.Bots;
using EchoBot2019.Dialogs;
using EchoBot2019.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EchoBot2019
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Configure State
            ConfigureState(services);

            ConfigureDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<MainDialog>>();
        }
            
        public void ConfigureDialogs(IServiceCollection services)
        {
            services.AddSingleton<MainDialog>();
        }

        public void ConfigureState(IServiceCollection services)
        {
            services.AddSingleton<IStorage, MemoryStorage>();

            //var storageAccount = "DefaultEndpointsProtocol=https;AccountName=botframeworkmeic;AccountKey=glZ7OpY6k+15FGyNgxeLcdHhULrWqx3K9rNZxGTshx85DRtmne7lWHPvubVQSSss5brxEkeRk1ir+AStZwHV7A==;EndpointSuffix=core.windows.net";
            //var storageContainer = "meic-state-date";

            //services.AddSingleton<IStorage>(new BlobsStorage(storageAccount, storageContainer));

            services.AddSingleton<UserState>();

            services.AddSingleton<ConversationState>();

            services.AddSingleton<StateService>();
              
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
