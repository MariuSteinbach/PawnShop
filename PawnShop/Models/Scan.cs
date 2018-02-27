using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnShop.Models
{
    [Serializable]
    public class Scan
    {
        public string Account { get; set; }
        public List<Pledge> Pledges { get; set; }
        public DateTime Date { get; set; }

        public Scan()
        {
            Pledges = new List<Pledge>();
            Date = DateTime.Now;
        }

        public int AddPledges(HtmlDocument Document)
        {
            for(int i = 0; i < countPledges(Document); i++)
            {
                Pledges.Add(new Pledge(getPledgeNode(Document, i), Document));
                if(Account == null)
                {
                    Account = new Pledge(getPledgeNode(Document, i), Document).getPledgeAccount(Document);
                }
            }
            return countPledges(Document);
        }

        public int countPledges(HtmlDocument Document)
        {
            return Document
                .DocumentNode
                .Descendants("article")
                .Where(d =>
                    d.Attributes.Contains("class") &&
                    d.Attributes["class"].Value.Contains("pledge")).Count();
        }

        public HtmlNode getPledgeNode(HtmlDocument Document, int Index)
        {
            return Document
                .DocumentNode
                .Descendants("article")
                .Where(d =>
                    d.Attributes.Contains("class") &&
                    d.Attributes["class"].Value.Contains("pledge")
                ).ElementAt(Index);
        }
    }
}
