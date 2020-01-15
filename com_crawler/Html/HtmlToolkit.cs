﻿// This source code is a part of Community Crawler Project.
// Copyright (C) 2020. rollrat. Licensed under the MIT Licence.

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace com_crawler.Html
{
    /// <summary>
    /// Parse extended xpaths that parse HTML.
    /// </summary>
    public class HtmlToolkit
    {
        string pattern;
        HtmlNode root;

        /// <summary>
        /// Create new instance of html-toolkit.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="root_node">The top node of the location to search for the xpath.</param>
        /// <returns></returns>
        public static HtmlToolkit Create(string pattern, HtmlNode root_node)
            => new HtmlToolkit(pattern, root_node);
        public HtmlToolkit(string pattern, HtmlNode root_node)
        {
            this.pattern = pattern;
            root = root_node;
            parse_pattern();
        }

        public List<string> Result { get; private set; }

        /// <summary>
        /// Parse expath
        /// 
        /// Example)
        /// /html[1]/body[1]/div[8]/div[3]/div[10]/div[1]/div[2]/div[1]/div[5]/div[1]/span[1]/div[1]/table[1]/tr[1]/td[{3+i*1}]/a[1]
        /// </summary>
        private void parse_pattern()
        {
            var tokens = split_token(pattern);

            Result = new List<string>();
        }

        /// <summary>
        /// Tokenize e-xpath.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private List<string> split_token(string str)
        {
            var result = new List<string>();
            for (int i = 0; i < str.Length; i++)
            {
                var builder = new StringBuilder();
                bool skip = false;
                for (; i < str.Length; i++)
                {
                    if (str[i] == ',' && skip == false)
                    {
                        result.Add(builder.ToString());
                        break;
                    }
                    if (str[i] == '[')
                        skip = true;
                    else if (str[i] == ']' && skip)
                        skip = false;
                    builder.Append(str[i]);
                }
                if (i == str.Length)
                    result.Add(builder.ToString());
            }
            return result;
        }
    }
}
