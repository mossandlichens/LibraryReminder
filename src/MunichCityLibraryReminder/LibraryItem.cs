namespace MunichCityLibraryReminder
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Collections.ObjectModel;

    public class LibraryItem
    {
        public LibraryItem()
        {

        }

        public LibraryItem(SimpleBrowser.HtmlResult item)
        {
            if (item != null)
            { 
                var elements = item.XElement.Elements();
                DueDate = elements.ElementAt<XElement>(1).Value.Trim();
                Library = elements.ElementAt<XElement>(2).Value.Trim();
                Title = elements.ElementAt<XElement>(3).Value.Trim();
                Notes = elements.ElementAt<XElement>(4).Value.Trim();
            }
        }

        [DisplayName ("Due date")]
        public string DueDate { get; set; }
        public string Library { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }

        public static void Save(List<LibraryItem> list, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<LibraryItem>));
                ser.Serialize(fs, list);
                fs.Flush();
            }
        }

        public static List<LibraryItem> Load(string fileName)
        {
            List<LibraryItem> result;

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<LibraryItem>));
                result = (List<LibraryItem>)ser.Deserialize(fs);
            }

            return result;
        }

        public override string ToString()
        {
            return this.Title.ToString();
        }


    }
}
