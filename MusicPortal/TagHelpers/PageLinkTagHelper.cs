using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MusicPortal.TagHelpers
{
    public class PageLinkTagHelper : TagHelper
    {
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public string? SortOrder { get; set; }
        public string? CurrentFilter { get; set; }
        public string? Action { get; set; }
        public string? Controller { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div"; 

            output.Content.SetHtmlContent(BuildPageLinks());
        }

        private string BuildPageLinks()
        {
            var links = new List<string>();

            for (int i = 1; i <= TotalPages; i++)
            {
                var url = $"/{Controller}/{Action}?pageNumber={i}&sortOrder={SortOrder}&currentFilter={CurrentFilter}";
                links.Add($"<a href='{url}'>{i}</a>");
            }

            return string.Join(" ", links);
        }
    }
}
