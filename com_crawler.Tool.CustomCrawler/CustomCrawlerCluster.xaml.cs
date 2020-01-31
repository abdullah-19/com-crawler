﻿/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Community Crawler Developer

***/

using CefSharp;
using CefSharp.Wpf;
using com_crawler.Html;
using com_crawler.Utils;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace com_crawler.Tool.CustomCrawler
{
    /// <summary>
    /// CustomCrawlerCluster.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomCrawlerCluster : Window
    {
        ChromiumWebBrowser browser;
        string url;
        HtmlTree tree;
        CallbackCCW cbccw;

        public CustomCrawlerCluster(string url, HtmlTree tree)
        {
            InitializeComponent();

            //Cef.Initialize(new CefSettings());
            browser = new ChromiumWebBrowser(string.Empty);
            browserContainer.Content = browser;
            browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;

            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("ccw", cbccw = new CallbackCCW(this), isAsync: true);

            this.url = url;
            this.tree = tree;

            ResultList.DataContext = new CustomCrawlerClusterDataGridViewModel();
            ResultList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<CustomCrawlerClusterDataGridItemViewModel>(ResultList).SortHandler);
            CaptureList.DataContext = new CustomCrawlerClusterCaptureDataGridViewModel();
            CaptureList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<CustomCrawlerClusterCaptureDataGridItemViewModel>(CaptureList).SortHandler);
            PatternList.DataContext = new CustomCrawlerClusterPatternDataGridViewModel();
            PatternList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<CustomCrawlerClusterPatternDataGridItemViewModel>(PatternList).SortHandler);

            for (int i = 0; i <= tree.Height; i++)
            {
                for (int j = 0; j < tree[i].Count; j++)
                {
                    if (tree[i][j].Name != "#comment" && tree[i][j].Name != "#text")
                    {
                        tree[i][j].SetAttributeValue("ccw_tag", $"ccw_{i}_{j}");
                        tree[i][j].SetAttributeValue("onmouseenter", $"ccw.hoverelem('ccw_{i}_{j}')");
                        tree[i][j].SetAttributeValue("onmouseleave", $"ccw.hoverelem('ccw_{i}_{j}')");
                    }
                }
            }

            KeyDown += CustomCrawlerCluster_KeyDown;
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                refresh();
            }
            catch { }
        }

        private void CustomCrawlerCluster_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                if (locking)
                {
                    F2.Text = "F2: Lock";
                    locking = false;
                }
                else
                {
                    F2.Text = "F2: UnLock";
                    locking = true;
                }
            }
            else if (e.Key == Key.F3)
            {
                depth--;
                if (depth < 0)
                    depth = 0;
                Depth.Text = $"Depth={depth}";
                cbccw.adjust();
            }
            else if (e.Key == Key.F4)
            {
                depth++;
                Depth.Text = $"Depth={depth}";
                cbccw.adjust();
            }
            else if (e.Key == Key.F5)
            {
                new CustomCrawlerTree(cbccw.selected_node, new List<HtmlNode> { cbccw.selected_node }, this).Show();
            }
            else if (e.Key == Key.F6)
            {
                if (cbccw.selected_node != null)
                {
                    AppendCapture(cbccw.selected_node.XPath);
                }
            }
            else if (e.Key == Key.F7)
            {
                if (cbccw.selected_node != null)
                {
                    var locking = this.locking;
                    this.locking = true;
                    (new CustomCrawlerClusterCapture() { Owner = this }).ShowDialog();
                    this.locking = locking;
                }
            }
            else if (e.Key == Key.Add)
            {
                if (browser.ZoomLevel <= 3.0)
                {
                    browser.ZoomInCommand.Execute(null);
                }
            }
            else if (e.Key == Key.Subtract)
            {
                if (browser.ZoomLevel >= -3.0)
                {
                    browser.ZoomOutCommand.Execute(null);
                }
            }
            else if (e.Key == Key.F8)
            {
                if (cbccw.selected_node != null)
                {
                    var tar = cbccw.selected_node.GetAttributeValue("ccw_tag", "");
                    if (Marking.Contains(tar))
                    {
                        Marking.Remove(tar);
                        cbccw.before_border = "";
                        browser.EvaluateScriptAsync($"document.querySelector('[ccw_tag={tar}]').style.border = '';").Wait();
                    }
                    else
                    {
                        Marking.Add(cbccw.selected_node.GetAttributeValue("ccw_tag", ""));
                        refresh_marking();
                    }
                }
            }
        }

        private void refresh()
        {
            browser.LoadHtml(tree[0][0].OuterHtml, url);
            Thread.Sleep(100);
            refresh_marking();
        }

        public List<string> Marking = new List<string>();
        private void refresh_marking()
        {
            var builder = new StringBuilder();
            foreach (var mm in Marking)
                builder.Append($"document.querySelector('[ccw_tag={mm}]').style.border = '0.2em solid orange';");
            browser.EvaluateScriptAsync(builder.ToString()).Wait();
        }

        #region Tree, Capture, Pattern

        public void AppendCapture(string info)
        {
            var index = CaptureList.Items.Count + 1;
            (CaptureList.DataContext as CustomCrawlerClusterCaptureDataGridViewModel).Items.Add(new CustomCrawlerClusterCaptureDataGridItemViewModel
            {
                Index = index.ToString(),
                Info = info,
                DateTime = DateTime.Now.ToString("h:mm ss"),
                Node = cbccw.selected_node,
            });
        }

        public void SelectNode(HtmlNode node)
        {
            F2.Text = "F2: UnLock";
            locking = true;

            var ij = tree.UnRef(node);
            var dd = depth;
            depth = 0;
            cbccw.hoverelem($"ccw_{ij.Item1}_{ij.Item2}", true);
            depth = dd;
            browser.EvaluateScriptAsync($"document.querySelector('[ccw_tag=ccw_{ij.Item1}_{ij.Item2}]').scrollIntoView(true);").Wait();
        }

        private void CaptureList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (CaptureList.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show($"Are you sure you want to delete {CaptureList.SelectedItems.Count} items?", "Cluster", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        CaptureList.SelectedItems.Cast<object>().ToList().ForEach(x => (CaptureList.DataContext as CustomCrawlerClusterCaptureDataGridViewModel).Items.Remove(x as CustomCrawlerClusterCaptureDataGridItemViewModel));
                    }
                }
            }
        }

        private void ExtractPatterns_Click(object sender, RoutedEventArgs e)
        {
            if (CaptureList.SelectedItems.Count <= 1)
            {
                MessageBox.Show("Select two and more items!", "Cluster", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selected = CaptureList.SelectedItems.OfType<CustomCrawlerClusterCaptureDataGridItemViewModel>();
            var lca = tree.GetLCANode(selected.Select(x => (tree[x.Node], x.Node).ToTuple()).ToList());

            try
            {
                var pattern = new Pattern
                {
                    LCA = lca,
                    Nodes = selected.Select(x => (x.Info, x.Node)).ToList(),
                    SubPatternsString = selected.Select(x => make_string(x.Node)).ToList(),
                    Content = make_string(lca),
                    Info = make_string(lca, selected.ToDictionary(x => x.Node, x => "@" + x.Info))
                };

                (PatternList.DataContext as CustomCrawlerClusterPatternDataGridViewModel).Items.Add(new CustomCrawlerClusterPatternDataGridItemViewModel
                {
                    Index = (PatternList.Items.Count + 1).ToString(),
                    Pattern = pattern.Info,
                    Patterns = pattern
                });
            }
            catch
            {
                MessageBox.Show("Do not select same elements!", "Cluster", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        Dictionary<HtmlNode, string> msdp = new Dictionary<HtmlNode, string>();
        private string make_string(HtmlNode node)
        {
            if (node.ChildNodes.Count == 0)
            {
                if (node.Name == "#text")
                    return "#";
                return $"({node.Name})";
            }
            if (msdp.ContainsKey(node))
                return msdp[node];
            var ms = $"({node.Name}{string.Join("", node.ChildNodes.ToList().Where(x => x.Name != "#comment").Select(x => make_string(x)))})";
            msdp.Add(node, ms);
            return ms;
        }

        private string make_string(HtmlNode node, Dictionary<HtmlNode, string> snodes)
        {
            if (snodes != null && snodes.ContainsKey(node))
                return snodes[node];
            if (node.ChildNodes.Count == 0)
            {
                if (node.Name == "#text")
                    return "#";
                return $"({node.Name})";
            }
            return $"({node.Name}{string.Join("", node.ChildNodes.ToList().Where(x => x.Name != "#comment").Select(x => make_string(x, snodes)))})";
        }

        public class Pattern
        {
            public HtmlNode LCA { get; set; }
            public List<(string, HtmlNode)> Nodes { get; set; }
            public List<string> SubPatternsString { get; set; }
            public string Content { get; set; }
            public string Info { get; set; }
        }

        private void CaptureList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (CaptureList.SelectedItems.Count > 0)
            {
                var node = (CaptureList.SelectedItems[0] as CustomCrawlerClusterCaptureDataGridItemViewModel).Node;

                if (section)
                {
                    refresh();
                    section = false;
                }

                browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '0em';").Wait();
                before = $"ccw_tag={node.GetAttributeValue("ccw_tag", "")}";
                browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '1em solid #FDFF47';").Wait();
                browser.EvaluateScriptAsync($"document.querySelector('[{before}]').scrollIntoView(true);").Wait();
            }
        }

        string before_find = "";
        private void FindPatternsOnPage_Click(object sender, RoutedEventArgs e)
        {
            if (PatternList.SelectedItems.Count != 1)
            {
                MessageBox.Show("Selects only one item!", "Cluster", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build nodes
            make_string(tree.RootNode);

            var pattern = PatternList.SelectedItems[0] as CustomCrawlerClusterPatternDataGridItemViewModel;
            var candidate = new List<(HtmlNode, string)>();

            if (!AllowRoughly.IsChecked.Value)
            {
                foreach (var pp in msdp)
                    if (pp.Value == pattern.Patterns.Content)
                        candidate.Add((pp.Key, "100.0%"));
            }
            else
            {
                foreach (var pp in msdp)
                {
                    var distance = Strings.ComputeLevenshteinDistance(pp.Value, pattern.Patterns.Content);
                    var per = 100.0 - distance / (double)Math.Max(pp.Value.Length, pattern.Patterns.Content.Length) * 100.0;
                    if (per >= AccuracyPattern.Value)
                        candidate.Add((pp.Key, per.ToString("0.0") + "%"));
                }
            }

            var builder = new StringBuilder();

            browser.EvaluateScriptAsync(before_find).Wait();
            candidate.ForEach(x => builder.Append($"document.querySelector('[ccw_tag={x.Item1.GetAttributeValue("ccw_tag", "")}]').style.border = '0.2em solid #FDFF47';"));
            browser.EvaluateScriptAsync(builder.ToString()).Wait();
            browser.EvaluateScriptAsync($"document.querySelector('[ccw_tag={candidate[0].Item1.GetAttributeValue("ccw_tag", "")}]').scrollIntoView(true);").Wait();
            before_find = string.Join("", candidate.Select(x => $"document.querySelector('[ccw_tag={x.Item1.GetAttributeValue("ccw_tag", "")}]').style.border = '';"));

            MessageBox.Show($"Found {candidate.Count} three identical patterns!\r\n" + string.Join("\r\n", candidate.Select(x => $"({x.Item2}) {x.Item1.XPath}")), "Cluster", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PatternAccuracy != null)
                PatternAccuracy.Text = (sender as Slider).Value.ToString("0.0") + "%";
        }

        #endregion

        #region Cef Callback

        bool locking = false;
        int depth = 0;

        public class CallbackCCW
        {
            CustomCrawlerCluster instance;
            string before = "";
            public string before_border = "";
            string latest_elem = "";
            public HtmlNode selected_node;
            public CallbackCCW(CustomCrawlerCluster instance)
            {
                this.instance = instance;
            }
            public void hoverelem(string elem, bool adjust = false)
            {
                if (instance.locking && !adjust)
                    return;
                latest_elem = elem;
                var i = Convert.ToInt32(elem.Split('_')[1]);
                var j = Convert.ToInt32(elem.Split('_')[2]);
                for (int k = 0; k < instance.depth; k++)
                {
                    if (instance.tree[i][j].ParentNode == instance.tree.RootNode)
                        break;
                    var rr = instance.tree.UnRef(instance.tree[i][j].ParentNode);
                    (i, j) = rr;
                }
                selected_node = instance.tree[i][j];
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    try
                    {
                        instance.refresh_marking();
                        instance.hover_item.Text = instance.tree[i][j].XPath;
                        instance.browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '{before_border}';").Wait();
                        before = $"ccw_tag=ccw_{i}_{j}";
                        before_border = instance.browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border").Result.Result.ToString();
                        instance.browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '0.2em solid red';").Wait();
                    }
                    catch { }
                }));
            }
            public void adjust()
            {
                hoverelem(latest_elem, true);
            }
        }

        #endregion

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<CustomCrawlerClusterDataGridItemViewModel>();
            if ((Functions.SelectedItem as ComboBoxItem).Content.ToString() == "LinearClustering")
            {
                var rr = tree.LinearClustering();

                for (int i = 0; i < rr.Count; i++)
                {
                    list.Add(new CustomCrawlerClusterDataGridItemViewModel
                    {
                        Index = (i + 1).ToString(),
                        Count = rr[i].Item1.ToString("#,#"),
                        Accuracy = rr[i].Item2.ToString(),
                        Header = rr[i].Item3.Name + "+" + string.Join("/", rr[i].Item4.Select(x => x.Name)),
                        Node = rr[i].Item3
                    });
                }

                C2.Header = "Count";
                C3.Header = "Accuracy";
                C4.Header = "Header";
            }
            else if ((Functions.SelectedItem as ComboBoxItem).Content.ToString() == "StylistClustering")
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        refresh();
                    }));
                    Thread.Sleep(500);
                });

                stylist_clustering(ref list);

                C2.Header = "Count"; // count of element
                C3.Header = "Use(%)"; // use space
                C4.Header = "Area"; // count of range
            }
            ResultList.DataContext = new CustomCrawlerClusterDataGridViewModel(list);
        }

        string before = "";
        bool section = false;

        private void ResultList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (ResultList.SelectedItems.Count > 0)
            {
                if ((Functions.SelectedItem as ComboBoxItem).Content.ToString() == "LinearClustering")
                {
                    var node = (ResultList.SelectedItems[0] as CustomCrawlerClusterDataGridItemViewModel).Node;

                    if (section)
                    {
                        refresh();
                        section = false;
                    }

                    browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '0em';").Wait();
                    before = $"ccw_tag={node.GetAttributeValue("ccw_tag", "")}";
                    browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '1em solid #FDFF47';").Wait();
                    browser.EvaluateScriptAsync($"document.querySelector('[{before}]').scrollIntoView(true);").Wait();
                }
                else if ((Functions.SelectedItem as ComboBoxItem).Content.ToString() == "StylistClustering")
                {
                    var node = (ResultList.SelectedItems[0] as CustomCrawlerClusterDataGridItemViewModel).Node;

                    if (section)
                    {
                        refresh();
                        section = false;
                    }

                    browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '0em';").Wait();
                    before = $"ccw_tag={node.GetAttributeValue("ccw_tag", "")}";
                    browser.EvaluateScriptAsync($"document.querySelector('[{before}]').style.border = '1em solid #FDFF47';").Wait();
                    browser.EvaluateScriptAsync($"document.querySelector('[{before}]').scrollIntoView(true);").Wait();
                }
            }
        }

        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultList.SelectedItems.Count > 0)
            {
                if ((Functions.SelectedItem as ComboBoxItem).Content.ToString() == "LinearClustering")
                {
                    var node = (ResultList.SelectedItems[0] as CustomCrawlerClusterDataGridItemViewModel).Node;

                    if (node.Name == "tbody")
                        browser.LoadHtml($"<table>{node.OuterHtml}</table>", url);
                    else
                        browser.LoadHtml(node.OuterHtml, url);

                    section = true;
                }
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            //browser.
        }

        #region Stylist Clustering

        private void stylist_clustering(ref List<CustomCrawlerClusterDataGridItemViewModel> result)
        {
            var pps = new List<List<(int?, int?, HtmlNode)>>();
            var ppsd = new Dictionary<HtmlNode, (int, int)>();
            for (int i = 0; i <= tree.Height; i++)
            {
                var pp = new List<(int?, int?, HtmlNode)>();
                for (int j = 0; j < tree[i].Count; j++)
                {
                    var w = browser.EvaluateScriptAsync($"document.querySelector('[ccw_tag=ccw_{i}_{j}]').clientWidth").Result.Result;
                    var h = browser.EvaluateScriptAsync($"document.querySelector('[ccw_tag=ccw_{i}_{j}]').clientHeight").Result.Result;

                    pp.Add((w as int?, h as int?, tree[i][j]));
                    ppsd.Add(tree[i][j], (i, j));
                }
                pps.Add(pp);
            }

            // area, use, use%, count
            var rr = new List<(int, int, double, int, HtmlNode)>();
            var max_area = 0;

            for (int i = 0; i <= tree.Height; i++)
                for (int j = 0; j < tree[i].Count; j++)
                {
                    if (!pps[i][j].Item1.HasValue)
                        continue;
                    int area = pps[i][j].Item1.Value * pps[i][j].Item2.Value;
                    int cnt = 0;
                    int use = 0;
                    foreach (var child in tree[i][j].ChildNodes)
                    {
                        var ij = ppsd[child];
                        if (!pps[ij.Item1][ij.Item2].Item1.HasValue)
                            continue;
                        cnt++;
                        use += pps[ij.Item1][ij.Item2].Item1.Value * pps[ij.Item1][ij.Item2].Item2.Value;
                    }
                    if (use == 0)
                        continue;
                    max_area = Math.Max(max_area, area);

                    rr.Add((area, use, use / (double)area * 100.0, cnt, tree[i][j]));
                }

            for (int i = 0; i < rr.Count; i++)
            {
                result.Add(new CustomCrawlerClusterDataGridItemViewModel
                {
                    Index = (i + 1).ToString(),
                    Count = rr[i].Item4.ToString(),
                    Accuracy = $"{rr[i].Item2.ToString("#,0")} ({rr[i].Item3.ToString("#0.0")} %)",
                    Header = $"{rr[i].Item1.ToString("#,0")} ({(rr[i].Item1 / (double)max_area * 100.0).ToString("#0.0")} %)",
                    Node = rr[i].Item5
                });
            }
        }

        #endregion
    }
}
