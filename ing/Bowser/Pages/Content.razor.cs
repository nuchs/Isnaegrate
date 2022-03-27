using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Bowser;
using Bowser.Shared;
using Bowser.Data;

namespace Bowser.Pages
{
    public sealed partial class Content :IAsyncDisposable
    {
        protected override async Task OnParametersSetAsync()
        {
            User = repo.First(u => u.Id == UserId);

            await session.StartSession(UserId);

            await base.OnParametersSetAsync();
        }

        public User User { get; set; }

        [Parameter]
        public string UserId { get; set; } = "";

        public void Logout()
        {
            nav.NavigateTo("/");
        }

        public async ValueTask DisposeAsync()
        {
            await session.EndSession(UserId);
        }
    }
}