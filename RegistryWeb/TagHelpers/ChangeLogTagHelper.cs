using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RegistryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.TagHelpers
{
    public class ChangeLogTagHelper : TagHelper
    {
        public IQueryable<GroupChangeLog> Elements { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";

            TagBuilder trhead = new TagBuilder("tr");
            trhead.InnerHtml.AppendHtml("<th width=\"170px\">Дата</th>");
            trhead.InnerHtml.AppendHtml("<th>Пользователь</th>");
            trhead.InnerHtml.AppendHtml("<th>Операция</th>");
            trhead.InnerHtml.AppendHtml("<th></th>");

            TagBuilder thead = new TagBuilder("thead");
            thead.InnerHtml.AppendHtml(trhead);

            TagBuilder tbody = new TagBuilder("tbody");
            TagBuilder tr = new TagBuilder("tr");
            foreach (var groupChangeLog in Elements)
            {
                tr = new TagBuilder("tr");
                tr.InnerHtml.AppendHtml("<td>" + groupChangeLog.OperationTime + "</td>");
                tr.InnerHtml.AppendHtml("<td>" + groupChangeLog.Logs.FirstOrDefault().UserName + "</td>");
                tr.InnerHtml.AppendHtml("<td>" + GetOperation(groupChangeLog) + "</td>");
                tr.InnerHtml.AppendHtml("<td></td>");
                tbody.InnerHtml.AppendHtml(tr);
                foreach (var changeLog in groupChangeLog.Logs)
                {
                    tr = new TagBuilder("tr");
                    tr.InnerHtml.AppendHtml("<td></td>");
                    tr.InnerHtml.AppendHtml("<td colspan=\"3\">" + changeLog.FieldName + "</td>");
                    tbody.InnerHtml.AppendHtml(tr);
                }
            }
            output.Content.AppendHtml(thead);
            output.Content.AppendHtml(tbody);
        }

        private string GetOperation(GroupChangeLog groupChangeLog)
        {
            if (groupChangeLog.TableName == "owner_processes")
            {
                if (groupChangeLog.Logs.Any(l => l.OperationType == "INSERT" && l.FieldName == "id_process"))
                    return "Процесс собственности создан";
                if (groupChangeLog.Logs.Any(l => l.FieldName == "annul_date" && l.FieldNewValue != null))
                    return "Процесс собственности аннулирован ";
            }
            var firstChangeLog = groupChangeLog.Logs.FirstOrDefault();
            return groupChangeLog.TableName + " " + firstChangeLog.OperationType;
        }
    }
}
