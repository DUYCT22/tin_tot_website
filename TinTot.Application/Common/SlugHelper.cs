using System.Globalization;
using System.Text;

namespace TinTot.Application.Common
{
    public static class SlugHelper
    {
        public static string ToSlug(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "item";
            }

            var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c == 'đ' ? 'd' : c);
                    continue;
                }

                if (char.IsWhiteSpace(c) || c is '-' or '_' or '.')
                {
                    sb.Append('-');
                }
            }

            var slug = sb.ToString().Trim('-');
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
        }
    }
}
