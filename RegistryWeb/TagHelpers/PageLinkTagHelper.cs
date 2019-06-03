using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;

namespace RegistryWeb.TagHelpers
{
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public ViewOptions.PageOptions PageModel { get; set; }
        public string PageAction { get; set; }

        [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]
        public Dictionary<string, object> PageUrlValues { get; set; } = new Dictionary<string, object>();

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "nav";

            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");

            var firstDisplayedPageIndex = PageModel.CurrentPage - 4 > 1 ? PageModel.CurrentPage - 4 : 1;
            var lastDisplayedPageIndex = firstDisplayedPageIndex + 8 > PageModel.TotalPages ? PageModel.TotalPages : firstDisplayedPageIndex + 8;
            if (lastDisplayedPageIndex - firstDisplayedPageIndex < 8 && firstDisplayedPageIndex > 1)
                firstDisplayedPageIndex = Math.Max(1, lastDisplayedPageIndex - 8);

            if (firstDisplayedPageIndex != 1)
            {
                var startNumber = CreateLinkTag(1, urlHelper);
                tag.InnerHtml.AppendHtml(startNumber);
            }
            for (var i = firstDisplayedPageIndex; i <= lastDisplayedPageIndex; i++)
            {
                var numberLink = CreateLinkTag(i, urlHelper);
                tag.InnerHtml.AppendHtml(numberLink);
            }
            if (lastDisplayedPageIndex != PageModel.TotalPages)
            {
                var endNumber = CreateLinkTag(PageModel.TotalPages, urlHelper);
                tag.InnerHtml.AppendHtml(endNumber);
            }

            output.Content.AppendHtml(tag);
        }

        TagBuilder CreateLinkTag(int page, IUrlHelper urlHelper)
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
