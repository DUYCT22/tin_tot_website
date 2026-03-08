using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TinTot.Application.Common
{
    public static class HtmlContentSanitizer
    {
        // Các thẻ nguy hiểm dạng block (có mở và đóng)
        private static readonly Regex DangerousBlockTagsRegex =
            new(@"<(script|style|iframe|object|embed|form|input|button|textarea|select|option|meta|link)[^>]*>.*?</\1>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        // Các thẻ nguy hiểm dạng self-closing
        private static readonly Regex DangerousSelfClosingTagsRegex =
            new(@"<(script|style|iframe|object|embed|form|input|button|textarea|select|option|meta|link)[^>]*/?>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        // Thuộc tính sự kiện (onClick, onLoad, ...)
        private static readonly Regex EventHandlerAttributeRegex =
            new(@"\son\w+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Thuộc tính href/src có chứa javascript:
        private static readonly Regex JsProtocolRegex =
            new(@"\s(href|src)\s*=\s*(['""]?)\s*javascript:[^'"">\s]*\2",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Các thẻ được phép
        private static readonly Regex AllowedTagRegex =
            new(@"</?(p|br|strong|b|em|i|u|ul|ol|li|a)\b[^>]*>",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Bất kỳ thẻ HTML nào
        private static readonly Regex AnyTagRegex =
            new(@"<[^>]+>", RegexOptions.Compiled);

        // Thẻ <a>
        private static readonly Regex AnchorTagRegex =
            new(@"<a\b([^>]*)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Thuộc tính href trong thẻ <a>
        private static readonly Regex HrefAttributeRegex =
            new(@"href\s*=\s*(['""])(.*?)\1",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Sanitize(string? rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml))
            {
                return string.Empty;
            }

            var sanitized = rawHtml.Trim();
            sanitized = DangerousBlockTagsRegex.Replace(sanitized, string.Empty);
            sanitized = DangerousSelfClosingTagsRegex.Replace(sanitized, string.Empty);
            sanitized = EventHandlerAttributeRegex.Replace(sanitized, string.Empty);
            sanitized = JsProtocolRegex.Replace(sanitized, string.Empty);

            sanitized = AnyTagRegex.Replace(sanitized, match => AllowedTagRegex.IsMatch(match.Value) ? match.Value : string.Empty);
            sanitized = AnchorTagRegex.Replace(sanitized, SanitizeAnchorTag);

            return sanitized;
        }

        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var noTags = AnyTagRegex.Replace(html, " ");
            return Regex.Replace(noTags, @"\s+", " ").Trim();
        }

        private static string SanitizeAnchorTag(Match match)
        {
            var attributes = match.Groups[1].Value;
            var hrefMatch = HrefAttributeRegex.Match(attributes);
            if (!hrefMatch.Success)
            {
                return "<a>";
            }

            var hrefValue = hrefMatch.Groups[2].Value.Trim();
            if (string.IsNullOrWhiteSpace(hrefValue))
            {
                return "<a>";
            }

            if (hrefValue.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return "<a>";
            }

            if (Uri.TryCreate(hrefValue, UriKind.Absolute, out var absoluteUri))
            {
                if (absoluteUri.Scheme is not ("http" or "https" or "mailto" or "tel"))
                {
                    return "<a>";
                }
            }
            else if (!hrefValue.StartsWith('/') && !hrefValue.StartsWith('#'))
            {
                return "<a>";
            }

            var encodedHref = WebUtility.HtmlEncode(hrefValue);
            return $"<a href=\"{encodedHref}\" rel=\"nofollow noopener noreferrer\">";
        }
    }
}
