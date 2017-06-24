using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abot.Crawler;
using Abot.Poco;
using System.Net;
using System.Collections;
using System.IO;

namespace ConsoleApp1
{

    class Program
    {
        static int pageCount = 0; //For saving file
        static string savePath = "CrawlResult.txt";

        static void Main(string[] args)
        {
            //Create result file
            if (!File.Exists(savePath))
            {
                StreamWriter sw = File.CreateText(savePath);
                sw.WriteLine("Url\n");
                sw.Write("Details\t");
                sw.Write("People\t");
                sw.WriteLine("Dates\n");
                sw.WriteLine();
                sw.WriteLine("Description\n");
                sw.WriteLine();
                sw.WriteLine("Comments\n");
                sw.WriteLine();
                sw.WriteLine("-----------");
                sw.WriteLine();
                sw.WriteLine();

                sw.Close();
            }


            //Crawling
            PoliteWebCrawler crawler = new PoliteWebCrawler();

            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;
            

            CrawlResult result = crawler.Crawl(new Uri("https://issues.apache.org/jira/browse/CAMEL-10597"));

            if (result.ErrorOccurred)
                Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
            else
                Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

            Console.WriteLine("---Program Complete---");
            Console.ReadKey();
        }

#if true
        static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }
      
        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            else
                Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

            if (string.IsNullOrEmpty(crawledPage.Content.Text))
                Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);

            var htmlAgilityPackDocument = crawledPage.HtmlDocument; //Html Agility Pack parser


            #region Save the page to bin
            try
            {
                if (crawledPage.Uri.ToString().IndexOf("CAMEL") >= 0)
                {
                    WebClient Client = new WebClient();
                    Client.DownloadFile("" + crawledPage.Uri + "", "SavedPageNumber" + pageCount);
                    Console.WriteLine("Content: \n", crawledPage.Content.Text);

                    string htmlText = crawledPage.Content.Text.ToString();

                    if (htmlText.Contains("Type:</strong>"))
                    {
                        Console.WriteLine("Found it\n");

                        //Find Type
                        string dummyHtmlText = htmlText.Substring(htmlText.IndexOf("Type:</strong>"), htmlText.Length - htmlText.IndexOf("Type:</strong>"));
                        for (int i = 0; i < 3; i++)
                        {
                            int number = dummyHtmlText.IndexOf(">");
                            dummyHtmlText = dummyHtmlText.Substring(dummyHtmlText.IndexOf(">") + 1, dummyHtmlText.Length - dummyHtmlText.IndexOf(">") - 1);
                        }
                        String type = dummyHtmlText.Substring(0, dummyHtmlText.IndexOf("\n")).Trim();

                        //Find Assignee
                        dummyHtmlText = htmlText.Substring(htmlText.IndexOf("Assignee:</dt>"), htmlText.Length - htmlText.IndexOf("Assignee:</dt>"));
                        for (int i = 0; i < 9; i++)
                        {
                            int number = dummyHtmlText.IndexOf(">");
                            dummyHtmlText = dummyHtmlText.Substring(dummyHtmlText.IndexOf(">") + 1, dummyHtmlText.Length - dummyHtmlText.IndexOf(">") - 1);
                        }
                        String assignee = dummyHtmlText.Substring(0, dummyHtmlText.IndexOf("<")).Trim();

                        //Find Date
                        dummyHtmlText = htmlText.Substring(htmlText.IndexOf("Created:</dt>"), htmlText.Length - htmlText.IndexOf("Created:</dt>"));
                        for (int i = 0; i < 3; i++)
                        {
                            int number = dummyHtmlText.IndexOf(">");
                            dummyHtmlText = dummyHtmlText.Substring(dummyHtmlText.IndexOf(">") + 1, dummyHtmlText.Length - dummyHtmlText.IndexOf(">") - 1);
                        }
                        String date = dummyHtmlText.Substring(dummyHtmlText.IndexOf("datetime=") + "datetime=".Length + 1,
                            dummyHtmlText.IndexOf("</time>") - dummyHtmlText.IndexOf("datetime=") - "datetime=".Length - 1 - "</time>".Length
                            ).Trim();

                        //Find descriptions block
                        dummyHtmlText = htmlText.Substring(htmlText.IndexOf("<div id=descriptionmodule"), htmlText.Length - htmlText.IndexOf("<div id=descriptionmodule"));
                        String descriptions = dummyHtmlText.Substring(0, dummyHtmlText.IndexOf("<div id=linkingmodule"));

                        //Find Comments block
                        dummyHtmlText = htmlText.Substring(htmlText.IndexOf("issuePanelWrapper"), htmlText.Length - htmlText.IndexOf("issuePanelWrapper"));
                        String comments = dummyHtmlText.Substring(0, dummyHtmlText.IndexOf("viewissuesidebar"));

                        //write result
                        using (StreamWriter sw = File.AppendText(savePath))
                        {
                            sw.WriteLine(crawledPage.Uri.ToString() + "\n");
                            sw.WriteLine("SavedPageNumber" + pageCount + "\n");
                            sw.WriteLine();
                            sw.Write(type + "\t");
                            sw.Write(assignee + "\t");
                            sw.WriteLine(date + "\n");
                            sw.WriteLine();
                            //sw.WriteLine(descriptions + "\n");
                            sw.WriteLine();
                            //sw.WriteLine(comments);
                            sw.WriteLine();
                            sw.WriteLine("-----------");
                            sw.WriteLine();
                            sw.WriteLine();
                        }



                        int debug = 0;

                    }
                }




            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError"+ ex.ToString() + "\n");

                int debug = 1;
            }
            pageCount++;
#endregion

            var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser
        }

        static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }
#endif

     }
}
