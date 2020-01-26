﻿using Emby.Plugins.JavScraper.Baidu;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Emby.Plugins.JavScraper.Configuration
{
    /// <summary>
    /// 配置
    /// </summary>
    public class PluginConfiguration
        : BasePluginConfiguration
    {
        /// <summary>
        /// 版本信息
        /// </summary>
        public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 启用代理
        /// </summary>
        public bool EnableJsProxy { get; set; } = true;

        /// <summary>
        /// JsProxy 代理地址
        /// </summary>
        public string JsProxy { get; set; } = "https://j.javscraper.workers.dev/";

        /// <summary>
        /// 是否包含 JsProxy
        /// </summary>
        public bool HasJsProxy
            => EnableJsProxy && JsProxy.IsWebUrl();

        private const string default_jsProxyBypass = "netcdn.";
        private List<string> _jsProxyBypass;

        /// <summary>
        /// 不走代理的域名
        /// </summary>
        public string JsProxyBypass
        {
            get => _jsProxyBypass?.Any() != true ? default_jsProxyBypass : string.Join(",", _jsProxyBypass);
            set
            {
                _jsProxyBypass = value?.Split(" ,;，；".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim())
                    .Distinct().ToList() ?? new List<string>();
            }
        }

        /// <summary>
        /// 是否不走代理
        /// </summary>
        public bool IsJsProxyBypass(string host)
        {
            if (HasJsProxy == false)
                return true;

            if (string.IsNullOrWhiteSpace(host))
                return false;
            if (_jsProxyBypass == null)
                JsProxyBypass = default_jsProxyBypass;

            return _jsProxyBypass?.Any(v => host.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0) == true;
        }

        private const string default_suren = "ARA,CUTE,DCV,GANA,HOI,JKZ,LUXU,MAAN,MMGH,MIUM,NAMA,NTK,SCUTE,SIMM,SIRO,SQB,SWEET,URF";
        private List<string> _suren;

        /// <summary>
        /// 素人的番号
        /// </summary>
        public string Suren
        {
            get => _suren?.Any() != true ? default_suren : string.Join(",", _suren);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = default_suren;
                _suren = value.Split(" ,;，；".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim())
                    .Distinct().ToList();
            }
        }

        /// <summary>
        /// 素人的番号
        /// </summary>
        private Regex regexSuren = new Regex(@"[\d]{3,4}[a-z]{3,6}[-_][\d]{3,5}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 是不是素人
        /// </summary>
        public bool IsSuren(string no)
        {
            if (string.IsNullOrWhiteSpace(no))
                return false;

            if (_suren?.Any() != true)
                Suren = default_suren;

            if (_suren?.Any(v => no.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0) == true)
                return true;

            return regexSuren.IsMatch(no);
        }

        /// <summary>
        /// 打开百度人体分析
        /// </summary>
        public bool EnableBaiduBodyAnalysis { get; set; }

        /// <summary>
        /// 百度人体分析 ApiKey
        /// </summary>
        public string BaiduBodyAnalysisApiKey { get; set; }

        /// <summary>
        /// 百度人体分析 SecretKey
        /// </summary>
        public string BaiduBodyAnalysisSecretKey { get; set; }

        /// <summary>
        /// 构造代理地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string BuildProxyUrl(string url)
            => string.IsNullOrWhiteSpace(url) == false && HasJsProxy && IsJsProxyBypass(GetHost(url)) == false ? $"{JsProxy.TrimEnd("/")}/http/{url}" : url;

        /// <summary>
        /// 获取域名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetHost(string url)
        {
            try
            {
                return new Uri(url).Host;
            }
            catch { }

            return url;
        }

        private BodyAnalysisService bodyAnalysisService;

        /// <summary>
        /// 获取 百度人体分析服务
        /// </summary>
        /// <param name="jsonSerializer"></param>
        /// <returns></returns>
        public BodyAnalysisService GetBodyAnalysisService(IJsonSerializer jsonSerializer)
        {
            if (EnableBaiduBodyAnalysis == false || string.IsNullOrWhiteSpace(BaiduBodyAnalysisApiKey) || string.IsNullOrWhiteSpace(BaiduBodyAnalysisSecretKey))
                return null;

            if (bodyAnalysisService != null && bodyAnalysisService.ApiKey == BaiduBodyAnalysisApiKey && bodyAnalysisService.SecretKey == BaiduBodyAnalysisSecretKey)
                return bodyAnalysisService;
            BaiduBodyAnalysisApiKey = BaiduBodyAnalysisApiKey.Trim();
            BaiduBodyAnalysisSecretKey = BaiduBodyAnalysisSecretKey.Trim();

            bodyAnalysisService = new BodyAnalysisService(BaiduBodyAnalysisApiKey, BaiduBodyAnalysisSecretKey, jsonSerializer);
            return bodyAnalysisService;
        }
    }
}