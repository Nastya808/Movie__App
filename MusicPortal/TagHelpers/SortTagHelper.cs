using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MusicPortal.TagHelpers
{
    public class SortTagHelper : TagHelper
    {
        public string SortField { get; set; } = string.Empty;
        public string? CurrentSort { get; set; }
        public string? Action { get; set; }
        public string? Controller { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", BuildSortUrl());

            output.Content.SetContent(output.Content.GetContent());
        }

        private string BuildSortUrl()
        {
            var sortOrder = (SortField == CurrentSort) ? $"{SortField}_desc" : SortField;
            return $"/{Controller}/{Action}?sortOrder={sortOrder}";
        }
    }
}
