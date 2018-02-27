using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnShop.Models
{
    [Serializable]
    public class Pledge
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }

        public Pledge(HtmlNode Node, HtmlDocument Document)
        {
            ID = getPledgeID(Node);
            Type = getPledgeType(Node);
            Name = getPledgeName(Node);
            Account = getPledgeAccount(Document);
        }

        public Pledge()
        {

        }

        private int getPledgeID(HtmlNode Node)
        {
            return Convert.ToInt32(Node
                .Descendants("a")
                .FirstOrDefault()
                .Attributes["href"]
                .Value
                .Split('/')
                .Last()
                );
        }

        private string getPledgeType(HtmlNode Node)
        {
            return Node
                .Descendants("h1")
                .FirstOrDefault()
                .InnerHtml
                .Split(new string[] { " - " }, StringSplitOptions.None)[0]
                .Split('<')[0];
        }

        private string getPledgeName(HtmlNode Node)
        {
            try
            {
                return Node
                    .Descendants("h1")
                    .FirstOrDefault()
                    .InnerHtml
                    .Split('<')[0]
                    .Split(new string[] { " - " }, StringSplitOptions.None)[1];
            }
            catch
            {
                try
                {
                    return Node
                        .Descendants("h1")
                        .FirstOrDefault()
                        .InnerHtml
                        .Split(new string[] { " - " }, StringSplitOptions.None)[0]
                        .Split('<')[0];
                }
                catch
                {
                    return Node
                        .Descendants("h1")
                        .FirstOrDefault()
                        .InnerHtml;
                }
            }
        }

        public string getPledgeAccount(HtmlDocument Document)
        {
            HtmlNode Node = Document
                .DocumentNode
                .Descendants("a")
                .Where(d =>
                    d.Attributes["href"] != null &&
                    d.Attributes["href"].Value.Contains("https://robertsspaceindustries.com/citizens/")
                )
                .FirstOrDefault();
            try
            {
                return Node.
                    Attributes["href"].
                    Value.
                    Split('/').
                    Last();
            }
            catch
            {
                return "";
            }
        }
    }
}
