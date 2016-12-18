using System;
using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using OptigemLdapSync.Models;
using static iTextSharp.text.Utilities;

namespace OptigemLdapSync
{
    internal class NewUserReport
    {
        private const float BaseFontSize = 10;

        private readonly TempFilemanager fileManager;

        protected Font TitleFont { get; } = FontFactory.GetFont("Arial", 1.2F * BaseFontSize, Font.BOLD);

        protected Font StandardFont { get; } = FontFactory.GetFont("Arial", BaseFontSize, Font.NORMAL);

        protected Font StandardBoldFont { get; } = FontFactory.GetFont("Arial", BaseFontSize, Font.BOLD);

        protected IHyphenationEvent Hyphenation = new HyphenationAuto("de", "DE", 2, 2);

        public NewUserReport(TempFilemanager fileManager)
        {
            if (fileManager == null)
                throw new ArgumentException(nameof(fileManager));

            this.fileManager = fileManager;
        }

        public string Create(PersonModel person)
        {
            var pdfdoc = new Document();
            pdfdoc.SetPageSize(PageSize.A4);
            pdfdoc.SetMargins(MillimetersToPoints(25), MillimetersToPoints(20), MillimetersToPoints(25), MillimetersToPoints(15));

            var culture = CultureInfo.GetCultureInfo("de-DE");

            var stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(pdfdoc, stream);
            pdfdoc.Open();

            Image logo = Image.GetInstance(Properties.Resources.FegLogo, System.Drawing.Imaging.ImageFormat.Png);
            logo.ScaleToFit(MillimetersToPoints(55f), MillimetersToPoints(500f));
            logo.SetAbsolutePosition(pdfdoc.PageSize.Width - MillimetersToPoints(73f) - MillimetersToPoints(55f), pdfdoc.PageSize.Height - MillimetersToPoints(23f));
            pdfdoc.Add(logo);

            var header = new PdfPTable(1)
            {
                WidthPercentage = 100,
                HeaderRows = 0
            };

            header.DefaultCell.Border = Rectangle.NO_BORDER;

            var paragraph = new Paragraph(this.CreatePhrase("Zugangsdaten zum Intranet", this.TitleFont))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 2,
                SpacingAfter = 2
            };

            header.AddCell(paragraph);
            var cell = header.GetRow(0).GetCells()[0];
            cell.BackgroundColor = new BaseColor(191, 191, 191);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.PaddingTop = 2;
            cell.PaddingBottom = 5;

            header.CompleteRow();

            pdfdoc.Add(header);

            pdfdoc.Add(new Paragraph(this.CreatePhrase(person.Briefanrede + ",")) { SpacingBefore = MillimetersToPoints(5) });
            pdfdoc.Add(new Paragraph(this.CreatePhrase("die Internetseite der Gemeinde ist unter")) { SpacingBefore = MillimetersToPoints(2) });

            paragraph = new Paragraph(this.CreatePhrase("www.feg-giessen.de", StandardBoldFont))
            {
                SpacingBefore = MillimetersToPoints(2),
                Alignment = Element.ALIGN_CENTER
            };

            pdfdoc.Add(paragraph);

            paragraph = new Paragraph(this.CreatePhrase("Über die öffentlichen Informationen hinaus gibt es auch noch geschützte, interne Bereiche, die weitere nützliche und aktuelle Zusatzinformationen enthalten. Diese sind nur für Gemeindemitglieder gedacht und deshalb nur mit entsprechender Zugangsberechtigung erreichbar, sobald das zugehörige Passwort mitgeteilt wurde. Folgende Schritte sind dann dazu erforderlich:"));
            paragraph.SpacingBefore = MillimetersToPoints(2);
            paragraph.Alignment = Element.ALIGN_JUSTIFIED;
            pdfdoc.Add(paragraph);

            var table = new PdfPTable(2)
            {
                WidthPercentage = 100,
                HeaderRows = 0,
                SpacingBefore = MillimetersToPoints(5),
                SpacingAfter = MillimetersToPoints(5)
            };

            table.DefaultCell.Border = Rectangle.NO_BORDER;

            table.SetTotalWidth(new[] { 1.2f, 5f });

            table.AddCell(this.CreatePhrase("Benutzername", this.StandardBoldFont));
            table.AddCell(this.CreatePhrase(person.Username));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("Passwort", this.StandardBoldFont));
            table.AddCell(this.CreatePhrase("(kommt per E-Mail)"));
            table.CompleteRow();

            pdfdoc.Add(table);

            pdfdoc.Add(new Paragraph(this.CreatePhrase("1.) Nach einem Klick rechts oben auf \"Login\" erscheint das Anmeldeformular")));
            pdfdoc.Add(new Paragraph(this.CreatePhrase("2.) Der o.g. Benutzername kann mit Kleinbuchstaben und Punkt in der Mitte eingegeben werden")));
            pdfdoc.Add(new Paragraph(this.CreatePhrase("3.) Das per E-Mail zugeschickte persönliche, geheime Passwort ist einzugeben")));
            pdfdoc.Add(new Paragraph(this.CreatePhrase("4.) Klick auf \"Anmelden\" gewährt den Intranetzugang, erkennbar durch den Login-Namen oben rechts")));

            pdfdoc.Add(new Paragraph(this.CreatePhrase("Nachdem der untere Abschnitt dieses Schreibens mit Kenntnisnahme der Datenschutzerklärung unterschrieben im Gemeindebüro (Fach) angekommen ist, wird an die bekannte bzw. noch anzugebende E-Mail-Adresse das Passwort verschickt (bitte vertraulich behandeln).")) { SpacingBefore = MillimetersToPoints(4), Alignment = Element.ALIGN_JUSTIFIED });
            pdfdoc.Add(new Paragraph(this.CreatePhrase("Und nun freuen wir uns über viele Besucher - und Rückmeldungen, wenn etwas gefällt, etwas fehlt, etwas falsch ist, oder irgendwas noch nicht ganz klappt :-). Das Online-Internet-Team ist unter internet@feg-giessen.de zu erreichen.")) { SpacingBefore = MillimetersToPoints(2), Alignment = Element.ALIGN_JUSTIFIED });
            pdfdoc.Add(new Paragraph(this.CreatePhrase("Freunde, Gäste, Sucher und Besucher können natürlich gerne auf die öffentliche Website hingewiesen werden. Dort sind stets die aktuellen Informationen über unsere Gemeinde zu finden.")) { SpacingBefore = MillimetersToPoints(2), SpacingAfter = MillimetersToPoints(5), Alignment = Element.ALIGN_JUSTIFIED });
            
            CustomDashedLineSeparator separator = new CustomDashedLineSeparator
            {
                Dash = 10,
                Gap = 7,
                LineWidth = 1
            };
            pdfdoc.Add(new Chunk(separator));

            paragraph = new Paragraph(this.CreatePhrase("An das Gemeindebüro der FeG-Gießen", this.TitleFont))
            {
                SpacingBefore = MillimetersToPoints(5),
                Alignment = Element.ALIGN_CENTER
            };
            pdfdoc.Add(paragraph);

            paragraph = new Paragraph(this.CreatePhrase("Bitte die Angaben unten prüfen, und ggf. rechts daneben berichtigen, falls nicht korrekt."))
            {
                Alignment = Element.ALIGN_CENTER
            };
            pdfdoc.Add(paragraph);

            table = new PdfPTable(2)
            {
                WidthPercentage = 100,
                HeaderRows = 0,
                SpacingBefore = MillimetersToPoints(7),
                SpacingAfter = MillimetersToPoints(4)
            };

            table.DefaultCell.Border = Rectangle.NO_BORDER;

            table.SetTotalWidth(new[] { 1f, 5f });

            table.AddCell(this.CreatePhrase("Straße", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Strasse));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("PLZ Ort", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Plz + " " + person.Ort));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("Zusatzort", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Zusatzort));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("Land", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Land));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("E-Mail", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.EMail));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("Telefon", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Telefon));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("Telefax", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Telefax));
            table.CompleteRow();
            table.AddCell(this.CreatePhrase("Handy", this.StandardFont));
            table.AddCell(this.CreatePhrase(person.Mobiltelefon));
            table.CompleteRow();

            pdfdoc.Add(table);

            pdfdoc.Add(new Paragraph(this.CreatePhrase("Die beigefügte Erklärung zum Datenschutz habe ich zur Kenntnis genommen, erkläre mein Einverständnis dazu, und bitte um Zusendung des Passworts.")));

            pdfdoc.Add(new Paragraph(this.CreatePhrase("Datum, Unterschrift: __________________________________________")) { SpacingBefore = MillimetersToPoints(10) });

            paragraph = new Paragraph(this.CreatePhrase(person.Vorname + " " + person.Nachname))
            {
                IndentationLeft = MillimetersToPoints(50)
            };
            pdfdoc.Add(paragraph);

            pdfdoc.Close();
            writer.Flush();

            byte[] data = stream.ToArray();

            string filename = this.fileManager.GetTempFile(person.Username.Replace(".", "-") + ".pdf");
            using (var fs = File.OpenWrite(filename))
            {
                fs.Write(data, 0, (int)data.Length);

                stream.Dispose();
                fs.Flush();
            }

            return filename;
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

        private class CustomDashedLineSeparator : DottedLineSeparator
        {
            public float Dash { get; set; } = 5;

            public float Phase { get; set; } = 2.5f;

            public override void Draw(PdfContentByte canvas, float llx, float lly, float urx, float ury, float y)
            {
                canvas.SaveState();
                canvas.SetLineWidth(lineWidth);
                canvas.SetLineDash(Dash, gap, Phase);
                DrawLine(canvas, llx, urx, y);
                canvas.RestoreState();
            }
        }
    }
}
