using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PdfReport.Api.Infrastructure.TagHelpers
{
    [HtmlTargetElement("*", Attributes = ValueForAttributeName)]
    public class ValueForAttributeTagHelper : TagHelper
    {
        private const string ValueForAttributeName = "value-for";

        [HtmlAttributeName(ValueForAttributeName)]
        public ModelExpression ValueFor { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ValueFor == null) return;

            var displayValue = ValueFor.Model != null ? ValueFor.Model.ToString() : string.Empty;

            if (ValueFor.ModelExplorer.ModelType == typeof(DateTimeOffset?) ||
                ValueFor.ModelExplorer.ModelType == typeof(DateTimeOffset))
            {
                displayValue = ((DateTimeOffset?)ValueFor.Model).HasValue
                    ? ((DateTimeOffset?)ValueFor.Model)?.DateTime.ToShortDateString()
                    : string.Empty;
            } 
            else if (ValueFor.ModelExplorer.ModelType == typeof(DateTime?) ||
                       ValueFor.ModelExplorer.ModelType == typeof(DateTime))
            {
                displayValue = ((DateTime?)ValueFor.Model).HasValue
                    ? ((DateTime?)ValueFor.Model)?.ToShortDateString()
                    : string.Empty;
            }
            else if (ValueFor.ModelExplorer.ModelType == typeof(decimal?) ||
                     ValueFor.ModelExplorer.ModelType == typeof(decimal))
            {
                displayValue = ((decimal?)ValueFor.Model).HasValue
                    ? ((decimal?)ValueFor.Model)?.ToString("C")
                    : string.Empty;
            }

            output.Content.SetHtmlContent(displayValue);
        }
    }
}
