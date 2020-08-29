using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;

using x = DocumentFormat.OpenXml.Spreadsheet;

using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace CheckOpenXML
{
    internal class ExcelReplace
    {
        public void DoReplaceAndInsert()
        {
            string testPath = @"C:\Users\wangwei\Desktop\TEMP";

            List<string> lstTest = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                lstTest.Add(string.Format("replace string {0}", i + 1));
            }

            var templatePath = Path.Combine(testPath, @"test.xlsx");

            #region replace string

            var resultPath = Path.Combine(testPath, @"result.xlsx");
            File.Copy(templatePath, resultPath, true);
            using (var workbook = SpreadsheetDocument.Open(resultPath, true, new OpenSettings { AutoSave = true }))
            {
                // Replace shared strings
                SharedStringTablePart sharedStringsPart = workbook.WorkbookPart.SharedStringTablePart;
                IEnumerable<x.Text> sharedStringTextElements = sharedStringsPart.SharedStringTable.Descendants<x.Text>();
                DoReplace(sharedStringTextElements, lstTest);

                // Replace inline strings
                IEnumerable<WorksheetPart> worksheetParts = workbook.GetPartsOfType<WorksheetPart>();
                foreach (var worksheet in worksheetParts)
                {
                    var allTextElements = worksheet.Worksheet.Descendants<x.Text>();
                    DoReplace(allTextElements, lstTest);
                }
            }

            #endregion

            #region replace image with new one

            var resultfileWithImage = Path.Combine(testPath, @"resultImage.xlsx");
            File.Copy(resultPath, resultfileWithImage, true);

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(resultfileWithImage, true))
            {
                // iterator each worksheet
                foreach (var part in document.WorkbookPart.GetPartsOfType<WorksheetPart>())
                {
                    // get drawingpart of worksheetpart
                    var drawingPart = part.DrawingsPart;
                    if (drawingPart != null)
                    {
                        // load drawing part to XmlDocument for processing
                        XmlDocument doc = new XmlDocument();
                        doc.Load(drawingPart.GetStream());
                        // add namespace
                        XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                        ns.AddNamespace("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
                        ns.AddNamespace("xdr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
                        XmlNodeList list = doc.SelectNodes("/xdr:wsDr/xdr:twoCellAnchor/xdr:pic/xdr:blipFill/a:blip", ns);
                        List<KeyValuePair<string, string>> pairList = new List<KeyValuePair<string, string>>();
                        // traverse the XmlNodeList to get the name and embed rId
                        foreach (XmlNode node in list)
                        {
                            foreach (XmlAttribute attribute in node.Attributes)
                            {
                                KeyValuePair<string, string> pair = new KeyValuePair<string, string>(attribute.Value, attribute.Name);
                                pairList.Add(pair);
                            }
                        }
                        // Replace images, see http://msdn.microsoft.com/en-us/library/bb508298.aspx
                        foreach (KeyValuePair<string, string> image in pairList)
                        {
                            string id = image.Key;
                            string name = image.Value;
                            OpenXmlPart oldPart = null;
                            // if the attribute name is embed
                            if (name == "r:embed")
                            {
                                // get ImagePart via id
                                oldPart = drawingPart.GetPartById(id);
                                // delete ImagePart
                                drawingPart.DeletePart(oldPart);
                                // add the ImagePart
                                ImagePart newPart = drawingPart.AddImagePart(ImagePartType.Jpeg, id);
                                // replace image
                                string replacement = Path.Combine(testPath, "newImage.jpg");
                                using (FileStream stream = new FileStream(replacement, FileMode.Open))
                                {
                                    // feed data to the newPart
                                    newPart.FeedData(stream);
                                }
                            }
                        }
                        // save xml document to the stream
                        doc.Save(drawingPart.GetStream(FileMode.Create));
                    }
                }
            }

            #endregion

            #region insert into new picture

            var resultInsertImage = Path.Combine(testPath, @"resultInsertImage.xlsx");
            File.Copy(resultPath, resultInsertImage, true);

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(resultInsertImage, true))
            {
                //WorksheetPart worksheetPart = document.WorkbookPart.GetPartsOfType<WorksheetPart>().FirstOrDefault();
                var worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(document.WorkbookPart.Workbook.Sheets.Elements<Sheet>().Single(s => s.Name == "Sheet1").Id.Value);
                AddImage(worksheetPart, Path.Combine(testPath, @"oldImge.jpg"), "test", 8, 3);

                worksheetPart.Worksheet.Save();
            }

            #endregion
        }

        private void DoReplace(IEnumerable<x.Text> textElements, List<string> replacementText)
        {
            foreach (var text in textElements)
            {
                for (int i = 0; i < replacementText.Count; i++)
                {
                    string placeHolder = string.Format("##{0}##", i + 1);
                    if (text.Text.Contains(placeHolder))
                        text.Text = text.Text.Replace(placeHolder, replacementText[i]);
                }
            }
        }

        private void AddImage(WorksheetPart worksheetPart, string imageFileName, string imgDesc, int colNumber, int rowNumber)
        {
            var drawingsPart = worksheetPart.DrawingsPart ?? worksheetPart.AddNewPart<DrawingsPart>();

            if (!worksheetPart.Worksheet.ChildElements.OfType<Drawing>().Any())
            {
                worksheetPart.Worksheet.Append(new Drawing { Id = worksheetPart.GetIdOfPart(drawingsPart) });
            }

            if (drawingsPart.WorksheetDrawing == null)
            {
                drawingsPart.WorksheetDrawing = new Xdr.WorksheetDrawing();
            }

            var worksheetDrawing = drawingsPart.WorksheetDrawing;
            var imagePart = drawingsPart.AddImagePart(ImagePartType.Jpeg);

            using (var imageStream = new FileStream(imageFileName, FileMode.Open))
            {
                imagePart.FeedData(imageStream);
            }

            int bmWidth = 0;
            int bmHeight = 0;
            float bmverticalResolution;
            float bmhorizontalResolution;

            using (Bitmap bm = new Bitmap(imageFileName))
            {
                bmWidth = bm.Width;
                bmHeight = bm.Height;
                bmverticalResolution = bm.HorizontalResolution;
                bmhorizontalResolution = bm.VerticalResolution;
            }

            A.Extents extents = new A.Extents();
            var extentsCx = bmWidth * (long)(914400 / bmverticalResolution);
            var extentsCy = bmHeight * (long)(914400 / bmhorizontalResolution);

            var colOffset = 0;
            var rowOffset = 0;

            var nvps = worksheetDrawing.Descendants<Xdr.NonVisualDrawingProperties>();
            var nvpId = nvps.Count() > 0
                ? (UInt32Value)worksheetDrawing.Descendants<Xdr.NonVisualDrawingProperties>().Max(p => p.Id.Value) + 1
                : 1U;

            var oneCellAnchor = new Xdr.OneCellAnchor(
                new Xdr.FromMarker
                {
                    ColumnId = new Xdr.ColumnId((colNumber - 1).ToString()),
                    RowId = new Xdr.RowId((rowNumber - 1).ToString()),
                    ColumnOffset = new Xdr.ColumnOffset(colOffset.ToString()),
                    RowOffset = new Xdr.RowOffset(rowOffset.ToString())
                },
                new Xdr.Extent { Cx = extentsCx, Cy = extentsCy },
                new Xdr.Picture(
                    new Xdr.NonVisualPictureProperties(
                        new Xdr.NonVisualDrawingProperties { Id = nvpId, Name = "Picture " + nvpId, Description = imgDesc },
                        new Xdr.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true })
                    ),
                    new Xdr.BlipFill(
                        new A.Blip { Embed = drawingsPart.GetIdOfPart(imagePart), CompressionState = A.BlipCompressionValues.Print },
                        new A.Stretch(new A.FillRectangle())
                    ),
                    new Xdr.ShapeProperties(
                        new A.Transform2D(
                            new A.Offset { X = 0, Y = 0 },
                            new A.Extents { Cx = extentsCx, Cy = extentsCy }
                        ),
                        new A.PresetGeometry { Preset = A.ShapeTypeValues.Rectangle }
                    )
                ),
                new Xdr.ClientData()
            );

            worksheetDrawing.Append(oneCellAnchor);
        }
    }
}