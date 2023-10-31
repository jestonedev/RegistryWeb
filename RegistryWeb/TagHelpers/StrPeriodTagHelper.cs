using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.TagHelpers
{
    public class StrPeriodTagHelper : TagHelper
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var period = "";
            if (From != null)
            {
                period += "с " + From.Value.ToString("dd.MM.yyyy");
            }
            if (From != null && To != null)
            {
                period += " ";
            }
            if (To != null)
            {
                period += "по " + To.Value.ToString("dd.MM.yyyy");
            }
            if (period == "")
            {
                period = "н/а";
            }
            
            
            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            var content = await output.GetChildContentAsync();
            output.Content.Append(content.GetContent());
            output.Content.Append(period);
        }
    }
}
