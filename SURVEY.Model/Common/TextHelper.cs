using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SURVEY.Model.Common
{
    public static class TextHelper
    {
        private static readonly Regex _multiWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex _slugInvalidCharsRegex = new("[^a-z0-9-]", RegexOptions.Compiled);
        private static readonly Regex _multipleDashRegex = new("-+", RegexOptions.Compiled);

        // Loại bỏ null / chuỗi trắng => trả về string.Empty
        public static string Safe(string? input) => string.IsNullOrWhiteSpace(input) ? string.Empty : input!;

        // Trim + gom nhiều khoảng trắng (space, tab, newline) thành 1 space
        public static string NormalizeWhitespace(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var replaced = _multiWhitespaceRegex.Replace(input!, " ");
            return replaced.Trim();
        }

        // Xoá \r \n \t và trim
        public static string ToSingleLine(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var sb = new StringBuilder(input!.Length);
            foreach (var ch in input!)
            {
                if (ch == '\r' || ch == '\n' || ch == '\t') continue;
                sb.Append(ch);
            }
            return NormalizeWhitespace(sb.ToString());
        }

        // Trả về null nếu chuỗi null / trắng sau khi trim
        public static string? NullIfWhiteSpace(string? input) => string.IsNullOrWhiteSpace(input) ? null : input!.Trim();

        // Chuẩn hoá dấu xuống dòng về 1 kiểu tùy chọn (mặc định \n)
        public static string NormalizeLineEndings(string? input, string newline = "\n")
        {
            if (input == null) return string.Empty;
            // Thay CRLF, CR thành LF tạm, sau đó nếu newline khác LF thì thay thế
            var normalized = input.Replace("\r\n", "\n").Replace("\r", "\n");
            return newline == "\n" ? normalized : normalized.Replace("\n", newline);
        }

        // Loại bỏ ký tự điều khiển không in được (ASCII < 32 trừ \r \n \t) và trim
        public static string RemoveControlChars(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var sb = new StringBuilder(input!.Length);
            foreach (var ch in input!)
            {
                if (char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t') continue;
                sb.Append(ch);
            }
            return sb.ToString().Trim();
        }

        // Cắt chuỗi theo độ dài tối đa, thêm hậu tố nếu bị cắt
        public static string Truncate(string? input, int maxLength, string suffix = "}")
        {
            if (maxLength <= 0) return string.Empty;
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (input!.Length <= maxLength) return input!;
            var cutLength = maxLength - suffix.Length;
            if (cutLength <= 0) return input!.Substring(0, maxLength);
            return input!.Substring(0, cutLength) + suffix;
        }

        // Loại bỏ dấu tiếng Việt / ký tự có dấu => Latin cơ bản
        public static string RemoveDiacritics(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var normalized = input!.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // Tạo slug: lowercase, bỏ dấu, thay khoảng trắng thành '-', loại bỏ ký tự đặc biệt
        public static string Slugify(string? input)
        {
            var clean = NormalizeWhitespace(RemoveDiacritics(input)).ToLowerInvariant();
            clean = clean.Replace(' ', '-');
            clean = _slugInvalidCharsRegex.Replace(clean, string.Empty);
            clean = _multipleDashRegex.Replace(clean, "-");
            return clean.Trim('-');
        }

        // Ghép nhiều chuỗi với khoảng trắng chuẩn hoá
        public static string JoinNormalized(params string?[] parts)
        {
            var filtered = parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim());
            return NormalizeWhitespace(string.Join(' ', filtered));
        }

        // So sánh chuỗi không phân biệt khoảng trắng thừa + không phân biệt hoa thường + bỏ dấu
        public static bool EqualsNormalized(string? a, string? b)
        {
            var na = Slugify(a);
            var nb = Slugify(b);
            return string.Equals(na, nb, StringComparison.Ordinal);
        }

        // Chuẩn hoá cho hiển thị 1 dòng ngắn (remove control chars + truncate)
        public static string NormalizeForDisplay(string? input, int maxLength = 120)
        {
            var cleaned = ToSingleLine(RemoveControlChars(input));
            return Truncate(cleaned, maxLength);
        }
    }
}
