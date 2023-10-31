using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace RegistryWeb.TagHelpers
{
    public class PageLinkTagHelper : TagHelper
    {
        public ViewOptions.PageOptions PageModel { get; set; }
        public string PageAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "nav";

            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");

            var firstDisplayedPageIndex = PageModel.CurrentPage - 4 > 1 ? PageModel.CurrentPage - 4 : 1;
            var lastDisplayedPageIndex = firstDisplayedPageIndex + 8 > PageModel.TotalPages ? PageModel.TotalPages : firstDisplayedPageIndex + 8;
            if (lastDisplayedPageIndex - firstDisplayedPageIndex < 8 && firstDisplayedPageIndex > 1)
                firstDisplayedPageIndex = Math.Max(1, lastDisplayedPageIndex - 8);

            if (firstDisplayedPageIndex != 1)
            {
                var startNumber = CreateLinkTag(1);
                tag.InnerHtml.AppendHtml(startNumber);
            }
            for (var i = firstDisplayedPageIndex; i <= lastDisplayedPageIndex; i++)
            {
                var numberLink = CreateLinkTag(i);
                tag.InnerHtml.AppendHtml(numberLink);
            }
            if (lastDisplayedPageIndex != PageModel.TotalPages)
            {
                var endNumber = CreateLinkTag(PageModel.TotalPages);
                tag.InnerHtml.AppendHtml(endNumber);
            }

            output.Content.AppendHtml(tag);
        }

        TagBuilder CreateLinkTag(int page)
        {
            TagBuilder link = new TagBuilder("a");
            if (page == 1)
                link.AddCssClass("mr-2");
            if (page == PageModel.TotalPages)
                link.AddCssClass("ml-2");
            link.AddCssClass("page-link");
            link.Attributes["data-page"] = page.ToString();
            link.Attributes["href"] = "#";
            link.InnerHtml.Append(page.ToString());

            TagBuilder item = new TagBuilder("li");
            item.AddCssClass("page-item");
            if (page == PageModel.CurrentPage)
                item.AddCssClass("active");
            item.InnerHtml.AppendHtml(link);

            return item;
        }
    }
}
