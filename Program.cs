using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net;
using ConsoleTables;

namespace BlackDot_ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter a keyword to search for books in Wordery and BookDepository...");

            startSearching(Console.ReadLine());
            Console.ReadLine();
        }

        private static async Task startSearching(string searchTerm)
        {
            var lstSearchResults = new List<SearchResults>();
            var lstMoreSearchResults = new List<SearchResults>();

            var urlWordery = "https://wordery.com/search?term=" + Convert.ToString(searchTerm);
            var urlBookDepository = "https://www.bookdepository.com/search?searchTerm=" + Convert.ToString(searchTerm);
            var urlWaterstones = "https://www.waterstones.com/books/search/term/" + Convert.ToString(searchTerm);

            var httpClient = new HttpClient();

            var htmlDocWordery = await httpClient.GetStringAsync(urlWordery);
            var htmlDocDepository = await httpClient.GetStringAsync(urlBookDepository);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlDocWordery);         

            lstSearchResults = searchWordery(htmlDocument);
            htmlDocument.LoadHtml(htmlDocDepository);
            lstMoreSearchResults = searchBookDepository(htmlDocument);
            lstSearchResults.AddRange(lstMoreSearchResults);
            //lstMoreSearchResults.OrderBy(o => o.Heading);
            lstSearchResults.Sort((x, y) => x.Heading.CompareTo(y.Heading));

            ConsoleTable.From<SearchResults>(lstSearchResults).Write();

        }


        public static List<SearchResults> searchWordery(HtmlDocument htmlDoc)
        {
            StringBuilder sb = new StringBuilder();
            var lstSearchResultsWordery = new List<SearchResults>();
            var divNoResults = htmlDoc.DocumentNode.Descendants("div").Where(n => n.GetAttributeValue("class", "").Equals("c-zero-results")).ToList();

            if (divNoResults.Count == 0)
            { 

                var divs = htmlDoc.DocumentNode.Descendants("li")
                  .Where(node => node.GetAttributeValue("class", "").Equals("o-book-list__book"));


                foreach (var children in divs)
                {
                    var grandChildren = children.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("c-book__body"));

                    foreach (var grandChild in grandChildren)
                    {
                        if(grandChild.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("c-book__price c-price")).FirstOrDefault() == null)
                        {   
                            sb.Append("N/A");
                        }
                        else
                        {
                            sb.Append(grandChild.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("c-book__price c-price")).FirstOrDefault().InnerText.Trim());
                        }

                        var searchResults = new SearchResults
                        {
                            Source = "Wordery",
                            Heading = grandChild.Descendants("a").FirstOrDefault().ChildAttributes("content").FirstOrDefault().Value.Trim(),
                            Price = sb.ToString()
                        };
                        lstSearchResultsWordery.Add(searchResults);
                        sb.Clear();
                    }
                }
            }
            else
            {
                lstSearchResultsWordery.Add(new SearchResults { Heading = "Oops!! No search results from Wordery!" });
            }

            return lstSearchResultsWordery;
        }

        public static List<SearchResults> searchBookDepository(HtmlDocument htmlDoc)
        {
            var divs = htmlDoc.DocumentNode.Descendants("div")
              .Where(node => node.GetAttributeValue("class", "").Equals("tab search"));

            var nodeAdvSearch = htmlDoc.DocumentNode.Descendants("div").Where(n => n.GetAttributeValue("class", "").Equals("content")).FirstOrDefault();


            var lstSearchResultsBookDepository = new List<SearchResults>();

            if (nodeAdvSearch == null)
            {
                foreach (var children in divs)
                {

                    var granChildren = children.ChildNodes.Where(node => node.Name.Equals("div") && node.GetAttributeValue("class", "").Equals("book-item"));

                    var greatGrandChildren = children.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("item-info"));
                    foreach (var greatGrandChild in greatGrandChildren)
                    {

                        string source = greatGrandChild.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("price-wrap")).FirstOrDefault().Descendants("p").FirstOrDefault().InnerText.Replace("&nbsp", "").Trim('\n').Trim().Trim('\t');
                        string result = string.Concat(source.Where(c => !char.IsWhiteSpace(c)));
                        var searchResults = new SearchResults
                        {
                            Source = "BookDepository",
                            Heading = greatGrandChild.Descendants("h3").Where(node => node.GetAttributeValue("class", "").Equals("title")).FirstOrDefault().InnerText.Trim('\n').Trim(),
                            Price = result
                        };
                        lstSearchResultsBookDepository.Add(searchResults);
                    }

                }
            }
            else
            {
                lstSearchResultsBookDepository.Add(new SearchResults { Heading = "ops!! No search results from BookDepository!" });
            }

            return lstSearchResultsBookDepository;
        }
    }
}
