using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using OSSFinder.Infrastructure.Extensions;

namespace OSSFinder.Infrastructure.Helpers
{
    public static class HtmlHelpers
    {
        private static readonly Regex InvalidXmlChars =
            new Regex(
                @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
                RegexOptions.Compiled);

        private static readonly Regex Imagetags = new Regex(@"<img\s[^>]*(>|$)",
                                                             RegexOptions.IgnoreCase | RegexOptions.Singleline |
                                                             RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private static readonly Regex AnchorTags = new Regex(@"<a\s[^>]*(>|$)",
                                                              RegexOptions.IgnoreCase | RegexOptions.Singleline |
                                                              RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private static readonly Regex Autolinks =
            new Regex(
                @"(\b(?<!""|>|;)(?:https?|ftp)://[A-Za-z0-9][-A-Za-z0-9+&@#/%?=~_|\[\]\(\)!:,.;]*[-A-Za-z0-9+&@#/%=~_|\[\]])",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Urlprotocol = new Regex(@"^(https?|ftp)://(www\.)?|(/$)", RegexOptions.Compiled);

        private static readonly Regex UrlprotocolSafe = new Regex(@"^https?://", RegexOptions.Compiled);

        private static readonly Regex _sanitizeUrl = new Regex(@"[^-a-z0-9+&@#/%?=~_|!:,.;\(\)]",
                                                               RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _sanitizeUrlAllowSpaces = new Regex(@"[^-a-z0-9+&@#/%?=~_|!:,.;\(\) ]",
                                                                          RegexOptions.IgnoreCase |
                                                                          RegexOptions.Compiled);

        private static readonly Regex Tags = new Regex("<[^>]*(>|$)",
                                                        RegexOptions.Singleline | RegexOptions.ExplicitCapture |
                                                        RegexOptions.Compiled);

        private static readonly Regex Whitelist =
            new Regex(
                @"
            ^</?(b(lockquote)?|code|d(d|t|l|el)|em|h(1|2|3)|i|kbd|li|ol|p(re)?|s(ub|up|trong|trike)?|ul)>$|
            ^<(b|h)r\s?/?>$",
                RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled |
                RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex WhitelistA =
            new Regex(
                @"
            ^<a\s
            href=""(\#\d+|(https?|ftp)://[-a-z0-9+&@#/%?=~_|!:,.;\(\)]+)""
            (\stitle=""[^""<>]+"")?\s?>$|
            ^</a>$",
                RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled |
                RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex WhitelistImg =
            new Regex(
                @"
            ^<img\s
            src=""https?://[-a-z0-9+&@#/%?=~_|!:,.;\(\)]+""
            (\swidth=""\d{1,3}"")?
            (\sheight=""\d{1,3}"")?
            (\salt=""[^""<>]*"")?
            (\stitle=""[^""<>]*"")?
            \s?/?>$",
                RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled |
                RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex Namedtags = new Regex
            (@"</?(?<tagname>\w+)[^>]*(\s|$|>)",
             RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private static readonly Regex RemoveProtocolDomain = new Regex(@"http://[^/]+",
                                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// returnurl=/path/
        /// </summary>
        public static string ReturnQueryString
        {
            get { return Keys.ReturnUrl + "=" + GetReturnUrl(HttpContext.Current.Request.Url.ToString()); }
        }

        /// <summary>
        /// remove any potentially dangerous tags from the provided raw HTML input
        /// </summary>
        public static string RemoveTags(string html)
        {
            if (html.IsNullOrEmpty()) return "";
            return Tags.Replace(html, "");
        }

        /// <summary>
        /// removes specified tag and any contents of that tag (intended for PRE, SCRIPT, etc)
        /// </summary>
        public static string RemoveTagContents(string tag, string html)
        {
            string pattern = "<{0}[^>]*>(.*?)</{0}>".Replace("{0}", tag);
            return Regex.Replace(html, pattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        // filters control characters but allows only properly-formed surrogate sequences

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        public static string RemoveInvalidXmlChars(string text)
        {
            if (text.IsNullOrEmpty()) return "";
            return InvalidXmlChars.Replace(text, "");
        }

        /// <summary>
        /// remove image tags from the provided HTML input
        /// </summary>
        public static string RemoveImageTags(string html)
        {
            if (html.IsNullOrEmpty()) return "";
            return Imagetags.Replace(html, "");
        }

        /// <summary>
        /// returns true if the provided HTML input has an image tag
        /// </summary>
        public static bool HasImageTags(string html)
        {
            if (html.IsNullOrEmpty()) return false;
            return Imagetags.IsMatch(html);
        }

        /// <summary>
        /// returns # of anchor tags OF ANY KIND in the input html
        /// </summary>
        public static int HasAnchorTagsAny(string html)
        {
            if (html.IsNullOrEmpty()) return 0;
            return AnchorTags.Matches(html).Count;
        }

        /// <summary>
        /// returns # of anchor tags to sites outside our network in the input html
        /// </summary>
        public static int HasAnchorTags(string html)
        {
            if (html.IsNullOrEmpty()) return 0;
            return AnchorTags.Matches(html).Cast<Match>().Count(m => true);
        }

        /// <summary>
        /// replace any text of the form example.com with a hyperlink; DANGER, does not nofollow
        /// intended for INTERNAL SITE USE ONLY
        /// </summary>
        public static string HyperlinkUrlsGenerously(string text)
        {
            if (text.IsNullOrEmpty()) return "";
            // bail if we already have a hyperlink
            if (HasAnchorTagsAny(text) > 0) return text;
            // go for it
            return Regex.Replace(text, @"([\w\-\d\.]+\.(com|net|org|edu))", "<a href=\"http://$1\">$1</a>");
        }

        /// <summary>
        /// returns true if the provided text contains a semi-valid URL
        /// </summary>
        public static bool IsUrl(string text)
        {
            return Autolinks.IsMatch(text);
        }

        /// <summary>
        /// auto-hyperlinks any *naked* URLs encountered in the text, with nofollow; 
        /// please note that valid HTML anchors will *not* be linked
        /// </summary>
        public static string HyperlinkUrls(string text, string cssclass, bool nofollow)
        {
            if (text.IsNullOrEmpty()) return "";

            string linkTemplate = @"<a href=""$1""";
            if (cssclass.HasValue())
                linkTemplate += " class=\"" + cssclass + "\"";
            if (nofollow)
                linkTemplate += @" rel=""nofollow""";
            linkTemplate += @">$2</a>";

            var linkTemplateBackup = linkTemplate;
            var offset = 0;
            const int maxlen = 50;

            foreach (Match m in Autolinks.Matches(text))
            {
                var url = m.Value;
                linkTemplate = linkTemplateBackup;

                string link;
                if (url.Length > maxlen)
                {
                    // if this is a stackoverflow-style URL, then let's have a mouseover title element
                    if (Regex.IsMatch(url, @"/questions/\d{3,}/"))
                    {
                        // extract the friendly text title
                        var title = Regex.Match(url, @"\d{3,}/([^/]+)").Groups[1].Value;
                        if (title.HasValue())
                            linkTemplate = linkTemplate.Replace(@">$2",
                                                                @" title=""" + UrlEncode(title).Replace("-", " ") +
                                                                @""">$2");
                    }

                    link = linkTemplate.Replace("$1", url).Replace("$2", ShortenUrl(url, maxlen));
                }
                else
                {
                    link = linkTemplate.Replace("$1", url).Replace("$2", RemoveUrlProtocol(url));
                }

                text = text.Substring(0, m.Index + offset) + link + text.Substring(m.Index + m.Length + offset);
                offset += (link.Length - m.Length);
            }

            return text;
        }

        /// <summary>
        /// auto-hyperlinks any URLs encountered in the text, with nofollow
        /// </summary>
        public static string HyperlinkUrls(string text)
        {
            return HyperlinkUrls(text, null);
        }

        /// <summary>
        /// auto-hyperlinks any URLs encountered in the text, with nofollow
        /// </summary>
        public static string HyperlinkUrls(string text, string cssclass)
        {
            return HyperlinkUrls(text, null, true);
        }


        /// <summary>
        /// makes a http://veryveryvery/very/very/very-long-url.html shorter for display purposes; 
        /// tries to break at slash borders
        /// </summary>
        public static string ShortenUrl(string url, int maxlen)
        {
            url = RemoveUrlProtocol(url);
            if (url.Length < maxlen) return url;

            for (int i = url.Length - 1; i > 0; i--)
            {
                if ((url[i] == '/') && (i < maxlen))
                    return url.Substring(0, i) + "/&hellip;";
            }

            return url.Substring(0, maxlen - 1) + "&hellip;";
        }

        /// <summary>
        /// removes the protocol (and trailing slash, if present) from the URL
        /// </summary>
        private static string RemoveUrlProtocol(string url)
        {
            return Urlprotocol.Replace(url, "");
        }

        /// <summary>
        /// returns Html Encoded string
        /// </summary>
        public static string Encode(string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// returns Url Encoded string
        /// </summary>
        public static string UrlEncode(string html)
        {
            return HttpUtility.UrlEncode(html);
        }

        /// <summary>
        /// removes any &gt; or &lt; characters from the input
        /// </summary>
        public static string RemoveTagChars(string s)
        {
            if (s.IsNullOrEmpty()) return s;
            return s.Replace("<", "").Replace(">", "");
        }

        /// <summary>
        /// returns "safe" URL, stripping anything outside normal charsets for URL
        /// </summary>
        public static string SanitizeUrl(string url)
        {
            if (url.IsNullOrEmpty()) return url;
            return _sanitizeUrl.Replace(url, "");
        }

        /// <summary>
        /// returns "safe" URL, stripping anything outside normal charsets for URL
        /// </summary>
        public static string SanitizeUrlAllowSpaces(string url)
        {
            if (url.IsNullOrEmpty()) return url;
            return _sanitizeUrlAllowSpaces.Replace(url, "");
        }


        /// <summary>
        /// sanitize any potentially dangerous tags from the provided raw HTML input using 
        /// a whitelist based approach, leaving the "safe" HTML tags
        /// CODESNIPPET:4100A61A-1711-4366-B0B0-144D1179A937
        /// </summary>
        public static string Sanitize(string html)
        {
            if (html.IsNullOrEmpty()) return html;

            // match every HTML tag in the input
            MatchCollection tags = Tags.Matches(html);
            for (var i = tags.Count - 1; i > -1; i--)
            {
                Match tag = tags[i];
                string tagname = tag.Value.ToLowerInvariant();

                if (Whitelist.IsMatch(tagname) || WhitelistA.IsMatch(tagname) || WhitelistImg.IsMatch(tagname))
                {
                    continue;
                }

                html = html.Remove(tag.Index, tag.Length);
                Debug.WriteLine("tag sanitized: " + tagname);
            }

            return html;
        }

        /// <summary>
        /// process HTML so it is safe for display and free of XSS vulnerabilities
        /// </summary>
        public static string Safe(string html)
        {
            if (html.IsNullOrEmpty()) return html;
            html = Sanitize(html);
            html = BalanceTags(html);
            return html;
        }

        /// <summary>
        /// ensures <code>url</code> has a valid protocol for being used in a link somewhere
        /// </summary>
        /// <param name="url">the url to check</param>
        /// <returns>the processed url</returns>
        public static string SafeProtocol(string url)
        {
            if (!UrlprotocolSafe.IsMatch(url))
            {
                url = "http://" + url;
            }

            return url;
        }

        /// <summary>
        /// attempt to balance HTML tags in the html string
        /// by removing any unmatched opening or closing tags
        /// IMPORTANT: we *assume* HTML has *already* been 
        /// sanitized and is safe/sane before balancing!
        /// 
        /// CODESNIPPET: A8591DBA-D1D3-11DE-947C-BA5556D89593
        /// </summary>
        public static string BalanceTags(string html)
        {
            if (html.IsNullOrEmpty()) return html;

            // convert everything to lower case; this makes
            // our case insensitive comparisons easier
            MatchCollection tags = Namedtags.Matches(html.ToLowerInvariant());

            // no HTML tags present? nothing to do; exit now
            int tagcount = tags.Count;
            if (tagcount == 0) return html;

            const string ignoredtags = "<p><img><br><li><hr>";
            var tagpaired = new bool[tagcount];
            var tagremove = new bool[tagcount];

            // loop through matched tags in forward order
            for (var ctag = 0; ctag < tagcount; ctag++)
            {
                var tagname = tags[ctag].Groups["tagname"].Value;

                // skip any already paired tags
                // and skip tags in our ignore list; assume they're self-closed
                if (tagpaired[ctag] || ignoredtags.Contains("<" + tagname + ">"))
                    continue;

                var tag = tags[ctag].Value;
                var match = -1;

                if (tag.StartsWith("</"))
                {
                    // this is a closing tag
                    // search backwards (previous tags), look for opening tags
                    for (int ptag = ctag - 1; ptag >= 0; ptag--)
                    {
                        var prevtag = tags[ptag].Value;
                        if (tagpaired[ptag] || !prevtag.Equals("<" + tagname, StringComparison.InvariantCulture))
                        {
                            continue;
                        }

                        // minor optimization; we do a simple possibly incorrect match above
                        // the start tag must be <tag> or <tag{space} to match
                        if (!prevtag.StartsWith("<" + tagname + ">") && !prevtag.StartsWith("<" + tagname + " "))
                        {
                            continue;
                        }

                        match = ptag;
                        break;
                    }
                }
                else
                {
                    // this is an opening tag
                    // search forwards (next tags), look for closing tags
                    for (int ntag = ctag + 1; ntag < tagcount; ntag++)
                    {
                        if (!tagpaired[ntag] &&
                            tags[ntag].Value.Equals("</" + tagname + ">", StringComparison.InvariantCulture))
                        {
                            match = ntag;
                            break;
                        }
                    }
                }

                // we tried, regardless, if we got this far
                tagpaired[ctag] = true;
                if (match == -1)
                    tagremove[ctag] = true; // mark for removal
                else
                    tagpaired[match] = true; // mark paired
            }

            // loop through tags again, this time in reverse order
            // so we can safely delete all orphaned tags from the string
            for (int ctag = tagcount - 1; ctag >= 0; ctag--)
            {
                if (tagremove[ctag])
                {
                    html = html.Remove(tags[ctag].Index, tags[ctag].Length);
                    Debug.WriteLine("unbalanced tag removed: " + tags[ctag]);
                }
            }

            return html;
        }

        /// <summary>
        /// provided a NON-ENCODED url, returns a properly (cough) encoded return URL
        /// </summary>
        public static string GetReturnUrl(string url)
        {
            // prevent double-returning
            if (QueryStringContains(url, Keys.ReturnUrl))
                url = QueryStringRemove(url, Keys.ReturnUrl);

            // remove session key from return URL, if present
            if (QueryStringContains(url, Keys.Session))
                url = QueryStringRemove(url, Keys.Session);

            // remove the http://example.com part of the url
            url = RemoveProtocolDomain.Replace(url, "");

            // allow only whitelisted URL characters, plus spaces
            url = SanitizeUrlAllowSpaces(url);

            // encode it for the URL
            return HttpUtility.UrlEncode(url);
        }

        /// <summary>
        /// fast (and maybe a bit inaccurate) check to see if the querystring contains the specified key
        /// </summary>
        public static bool QueryStringContains(string url, string key)
        {
            return url.Contains(key + "=");
        }

        /// <summary>
        /// removes the specified key, and any value, from the querystring. 
        /// for www.example.com/bar.foo?x=1&y=2&z=3 if you pass "y" you'll get back 
        /// www.example.com/bar.foo?x=1&z=3
        /// </summary>
        public static string QueryStringRemove(string url, string key)
        {
            if (url.IsNullOrEmpty()) return "";
            return Regex.Replace(url, @"[?&]" + key + "=[^&]*", "");
        }

        /// <summary>
        /// returns the value, if any, of the specified key in the querystring
        /// </summary>
        public static string QueryStringValue(string url, string key)
        {
            if (url.IsNullOrEmpty()) return "";
            return Regex.Match(url, key + "=.*").ToString().Replace(key + "=", "");
        }

        /// <summary>
        /// Produces optional, URL-friendly version of a title, "like-this-one". 
        /// hand-tuned for speed, reflects performance refactoring contributed by John Gietzen (user otac0n) 
        /// </summary>
        public static string UrlFriendly(string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            string s;
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    s = c.ToString().ToLowerInvariant();
                    if ("àåáâäãåą".Contains(s))
                    {
                        sb.Append("a");
                    }
                    else if ("èéêëę".Contains(s))
                    {
                        sb.Append("e");
                    }
                    else if ("ìíîïı".Contains(s))
                    {
                        sb.Append("i");
                    }
                    else if ("òóôõöø".Contains(s))
                    {
                        sb.Append("o");
                    }
                    else if ("ùúûü".Contains(s))
                    {
                        sb.Append("u");
                    }
                    else if ("çćč".Contains(s))
                    {
                        sb.Append("c");
                    }
                    else if ("żźž".Contains(s))
                    {
                        sb.Append("z");
                    }
                    else if ("śşš".Contains(s))
                    {
                        sb.Append("s");
                    }
                    else if ("ñń".Contains(s))
                    {
                        sb.Append("n");
                    }
                    else if ("ýŸ".Contains(s))
                    {
                        sb.Append("y");
                    }
                    else if (c == 'ł')
                    {
                        sb.Append("l");
                    }
                    else if (c == 'đ')
                    {
                        sb.Append("d");
                    }
                    else if (c == 'ß')
                    {
                        sb.Append("ss");
                    }
                    else if (c == 'ğ')
                    {
                        sb.Append("g");
                    }
                    prevdash = false;
                }
                if (i == maxlen) break;
            }

            return prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
        }

        /// <summary>
        /// remove entities such as "&gt;" or "&quot;"
        /// </summary>
        public static string RemoveEntities(string html)
        {
            return Regex.Replace(html, @"&([^; ]+);", "");
        }

        /// <summary>
        /// remove double-encoded entities; translates "&amp;gt;" to "&gt;"
        /// </summary>
        public static string DecodeEntities(string html)
        {
            return Regex.Replace(html, @"&amp;([^; ]+);", @"&$1;");
        }

        public static string MakeClasses(Dictionary<string, bool> possibilities)
        {
            var buffer = new StringBuilder();

            foreach (var possibility in possibilities)
            {
                if (possibility.Value)
                {
                    buffer.Append(" ").Append(possibility.Key);
                }
            }

            return buffer.ToString();
        }
    }
}