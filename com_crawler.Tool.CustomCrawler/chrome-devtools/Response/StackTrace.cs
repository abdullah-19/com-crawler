﻿/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Community Crawler Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com_crawler.Tool.CustomCrawler.chrome_devtools.Response
{
    public class StackTrace
    {
        [JsonProperty(PropertyName = "desciption")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "callFrames")]
        public CallFrame[] CallFrames { get; set; }
        [JsonProperty(PropertyName = "parent")]
        public StackTrace Parent { get; set; }
        [JsonProperty(PropertyName = "parentId")]
        public StackTraceId ParentId { get; set; }
    }

    public class StackTraceId
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "debuggerId")]
        public string UniqueDebuggerId { get; set; }
    }
}
