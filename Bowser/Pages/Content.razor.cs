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
    public partial class Content
    {
        protected override Task OnParametersSetAsync()
        {
            User = repo.First(u => u.Id == UserId);

            return base.OnParametersSetAsync();
        }

        public User User { get; set; }

        [Parameter]
        public string UserId { get; set; } = "";

        public void Logout()
        {
            nav.NavigateTo("/");
        }
    }
}