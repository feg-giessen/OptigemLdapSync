using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OptigemLdapSync.Models;

namespace OptigemLdapSync
{
    internal class NewUserReport
    {
        private const float BaseFontSize = 10;

        protected Font TitleFont { get; } = FontFactory.GetFont("Arial", 1.2F * BaseFontSize, Font.BOLD);

        protected Font StandardFont { get; } = FontFactory.GetFont("Arial", BaseFontSize, Font.NORMAL);

        protected Font StandardBoldFont { get; } = FontFactory.GetFont("Arial", BaseFontSize, Font.BOLD);

        protected IHyphenationEvent Hyphenation = new HyphenationAuto("de", "DE", 2, 2);

        public void Create(PersonModel person)
        {
            var pdfdoc = new Document();
            pdfdoc.SetPageSize(PageSize.A4);
            pdfdoc.SetMargins(15, 15, 10, 15);

            var culture = CultureInfo.GetCultureInfo("de-DE");

            var stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(pdfdoc, stream);
            pdfdoc.Open();

            var header = new PdfPTable(1)
            {
                WidthPercentage = 100,
                HeaderRows = 1
            };

            header.Rows.Add(new PdfPRow(new[] 
            {
                new PdfPCell(this.CreatePhrase("Zugangsdaten zum Intranet", this.TitleFont))
                {
                    BackgroundColor = new BaseColor(191, 191, 191)
                }
            }));

            pdfdoc.Add(header);

            pdfdoc.Add(new Paragraph(this.CreatePhrase(person.Briefanrede + ",")));
            pdfdoc.Add(new Paragraph(this.CreatePhrase("die Internetseite der Gemeinde ist unter")));

            var paragraph = new Paragraph(this.CreatePhrase("www.feg-giessen.de", StandardBoldFont))
            {
                Alignment = Element.ALIGN_CENTER
            };

            pdfdoc.Add(paragraph);

            pdfdoc.Add(new Paragraph(this.CreatePhrase("Über die öffentlichen Informationen hinaus gibt es auch noch geschützte, interne Bereiche, die weitere nützliche und aktuelle Zusatzinformationen enthalten. Diese sind nur für Gemeindemitglieder gedacht und deshalb nur mit entsprechender Zugangsberechtigung erreichbar, sobald das zugehörige Passwort mitgeteilt wurde. Folgende Schritte sind dann dazu erforderlich:")));
        }

        protected Phrase CreatePhrase(string text, Font font)
        {
            var p = new Phrase(text, font);
            foreach (Chunk c in p.Chunks)
            {
                c.SetHyphenation(this.Hyphenation);
            }

            return p;
        }

        protected Phrase CreatePhrase(string text)
        {
            return this.CreatePhrase(text, this.StandardFont);
        }
    }
}
