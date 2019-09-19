using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.TagHelpers
{
    public class LogOwnerProcessTagHelper : TagHelper
    {
        public IEnumerable<LogOwnerProcess> Elements { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";

            TagBuilder trhead = new TagBuilder("tr");
            trhead.InnerHtml.AppendHtml("<th width=\"170px\">Дата</th>");
            trhead.InnerHtml.AppendHtml("<th>Пользователь</th>");
            trhead.InnerHtml.AppendHtml("<th>Операция</th>");
            trhead.InnerHtml.AppendHtml("<th>Объект</th>");
            trhead.InnerHtml.AppendHtml("<th></th>");

            TagBuilder thead = new TagBuilder("thead");
            thead.InnerHtml.AppendHtml(trhead);

            TagBuilder tbody = new TagBuilder("tbody");
            TagBuilder tr = new TagBuilder("tr");
            foreach (var log in Elements)
            {
                tr = new TagBuilder("tr");
                tr.InnerHtml.AppendHtml("<td>" + log.Date + "</td>");
                tr.InnerHtml.AppendHtml("<td>" + log.IdUserNavigation.UserDescription + "</td>");
                tr.InnerHtml.AppendHtml("<td>" + log.IdLogTypeNavigation.Name + "</td>");
                tr.InnerHtml.AppendHtml("<td>" + log.IdLogObjectNavigation.Name + "</td>");
                tr.InnerHtml.AppendHtml("<td></td>");
                tbody.InnerHtml.AppendHtml(tr);
                //foreach (var changeLog in log.Logs)
                //{
                //    tr = new TagBuilder("tr");
                //    tr.InnerHtml.AppendHtml("<td></td>");
                //    tr.InnerHtml.AppendHtml("<td colspan=\"3\">" + changeLog.FieldName + "</td>");
                //    tbody.InnerHtml.AppendHtml(tr);
                //}
            }
            output.Content.AppendHtml(thead);
            output.Content.AppendHtml(tbody);
        }
    }
}
