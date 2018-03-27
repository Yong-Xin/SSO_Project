using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MvcClient
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
            services.AddMvc(); 
            //闭了JWT的Claim 类型映射, 以便允许well-known claims
            //保证不会修改任何从Authorization Server返回的Claims
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                //使用Cookie作为验证用户的首选方式
                options.DefaultScheme = "Cookies";
                //当用户需要登陆的时候, 将使用的是OpenId Connect Scheme
                options.DefaultChallengeScheme = "oidc";
            })
            //添加了可以处理Cookie的处理器(handler)
            .AddCookie("Cookies")
            //让上面的handler来执行OpenId Connect 协议
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = "Cookies";
                //Identity Server 
                options.Authority = "http://localhost:5000";
                options.RequireHttpsMetadata = false;

                options.ClientId = "mvc_implicit";
                options.ResponseType = "id_token token";
                options.SaveTokens = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //配置的位置一定要在useMVC前
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
