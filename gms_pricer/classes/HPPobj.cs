using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace gms_pricer.classes
{

    /// <summary>
    /// Supporting class to handle line structures for LINQ queries
    /// </summary>
    class HPPLine
    {
        public int LineNumber;
        public string LineText;
    }

    /// <summary>
    /// Main entry point class for parsing HPP files
    /// </summary>
    class HPPFile
    {
        /// <summary>
        /// Filename + path to file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// List of SQFCategories inside the file
        /// </summary>
        public List<HPPCategory> Categories = new List<HPPCategory>();

        public HPPFile(string FilePath)
        {
            this.FileName = FilePath;
        }


        /// <summary>
        /// Method to get a list of categories in the file
        /// </summary>
        /// <returns></returns>
        public List<HPPCategory> GetCategories()
        {
            List<HPPCategory> returnValue = new List<HPPCategory>();
            StreamReader sr = new StreamReader(this.FileName);

            List<HPPLine> linesofFile = new List<HPPLine>();

            string line = string.Empty;
            int a = 1;
            while ((line = sr.ReadLine()) != null)
            {
                HPPLine sqLine = new HPPLine();
                sqLine.LineNumber = a;
                sqLine.LineText = line;

                // add to our master list
                linesofFile.Add(sqLine);
                a++;
            }

            // parse out lines containing cats
            List<HPPLine> catLines = (from l in linesofFile
                                      where l.LineText.Contains("Category")
                                     select l).ToList<HPPLine>();

            /*
            // parse out our obj classes
            List<HPPLine> objLines = (from l in linesofFile
                                      where l.LineText.Contains("class") && (!l.LineText.Contains("Category"))
                                      select l).ToList<HPPLine>();
            */
            
            
            
            

            // process our categories, objects, and props
            List<HPPCategory> categories = new List<HPPCategory>();
            foreach (HPPLine catLine in catLines)
            {
                HPPCategory cat = new HPPCategory();
                // process cat
                if (cat.ParseCategoryInformation(catLines, catLine.LineNumber))
                {
                    //Console.WriteLine("Category: " + cat.CategoryName);
                    // DEBUG:
                    if (cat.CategoryName.Contains("700"))
                    {
                        Console.WriteLine("MONEYH CAT");
                    }

                    List<HPPLine> objLines = (from l in linesofFile
                                where l.LineText.Contains("class") && (!l.LineText.Contains("Category")) && (l.LineNumber >= cat.StartLine)
                                select l).ToList<HPPLine>();

                    

                    foreach (HPPLine objLine in objLines)
                    {
                        HPPobj obj = new HPPobj();
                        if (obj.ParseObjectInformation(objLines, objLine.LineNumber))
                        {
                            // check for dupe classnames
                            List<HPPobj> dupeObjects = (from o in cat.HPPItems
                                                         where o.ClassName.Contains(obj.ClassName)
                                                         select o).ToList<HPPobj>();

                            if (dupeObjects.Count <= 0)
                            {
                                // parse out lines containing props
                                List<HPPLine> propLines = (from l in linesofFile
                                                           where l.LineText.Contains("type") || l.LineText.Contains("buy") || l.LineText.Contains("sell") && l.LineNumber >= obj.StartLine && l.LineNumber <= obj.LastLine
                                                           select l).ToList<HPPLine>();

                                // process props for obj
                                obj.ParseObjProps(propLines);

                                // add objects to our cat
                                cat.HPPItems.Add(obj);
                            }
                        }
                    }                                       
                }
                returnValue.Add(cat);
            }
            return returnValue;
        }

        /// <summary>
        /// Main entry point for our object
        /// </summary>
        /// <param name="filename">This can be our current objects file name if not specified</param>
        /// <returns>True False if process was successful</returns>
        public bool parseFile(string filename)
        {
            bool returnValue = true; // assume true
            
            
            

            return returnValue;

        }

    }

    /// <summary>
    /// class for handling our individual categories in a hpp file
    /// </summary>
    class HPPCategory
    {
        /// <summary>
        /// Class name of category ex Category_674
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// List of our current category's items
        /// </summary>
        public List<HPPobj> HPPItems = new List<HPPobj>();

        /// <summary>
        /// The first line where we see the category
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        /// The Next reference to a category
        /// </summary>
        public int LastLine { get; set; }

        /// <summary>
        /// Method to get list of our SQF objects in specified category
        /// </summary>
        /// <param name="CategoryName"></param>
        /// <returns></returns>
        public List<HPPobj> GetHPPobjects(string CategoryName)
        {
            List<HPPobj> returnValue = new List<HPPobj>();

            return returnValue;
        }

        /// <summary>
        /// Main entry point we parse all the info for our category here
        /// </summary>
        /// <param name="CatLines"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public bool ParseCategoryInformation(List<HPPLine> CatLines, int start)
        {
            bool returnValue = true;

            HPPLine currentLine = (from l in CatLines
                                   where l.LineNumber == start
                                   select l).ToList<HPPLine>().First();
            this.StartLine = currentLine.LineNumber;

            // failover for last cat in file or single cat files
            try
            {
                HPPLine nextLine = (from l in CatLines
                                    where l.LineNumber > start
                                    select l).ToList<HPPLine>().First();
                this.LastLine = nextLine.LineNumber;
            }
            catch (InvalidOperationException ex)
            {
                this.LastLine = 99999;
            }

            // parse category name
            string categoryMatchString = "(class)\\s*(?<CategoryName>Category_[a-zA-Z0-9_]+)";

            Regex catRegex = new Regex(categoryMatchString);
            Match catMatch = catRegex.Match(currentLine.LineText);

            if (catMatch.Success)
            {
                this.CategoryName = catMatch.Groups["CategoryName"].ToString();
                returnValue = true;
            }
            else
            {
                returnValue = false;
            }



            return returnValue;
        }
    }
    

    /// <summary>
    /// Class for handling our indvidual object records
    /// </summary>
    class HPPobj
    {
        /// <summary>
        /// Item's Classname ex m9sd
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Type of trade item ex trade_weapons
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Cost to purchase item
        /// </summary>
        public int BuyPrice { get; set; }

        /// <summary>
        /// Cost to sell item
        /// </summary>
        public int SellPrice { get; set; }

        public int StartLine { get; set; }
        public int LastLine { get; set; }

        /// <summary>
        /// Method to handle getting the range of our obj
        /// </summary>
        /// <param name="ObjLines"></param>
        /// <param name="start"></param>
        /// <returns>Result of process</returns>
        public bool ParseObjectInformation(List<HPPLine> ObjLines, int start)
        {
            bool returnValue = true;

            HPPLine currentLine = (from l in ObjLines
                                   where l.LineNumber == start
                                   select l).ToList<HPPLine>().First();
            this.StartLine = currentLine.LineNumber;
            try
            {
                HPPLine nextLine = (from l in ObjLines
                                    where l.LineNumber > start
                                    select l).ToList<HPPLine>().First();
                this.LastLine = nextLine.LineNumber;
            }
            catch (InvalidOperationException ex)
            {
                this.LastLine = 99999;
            }

            // parse out the name of our obj classname
            string classNameMatchString = "\\s*(class)(\\s)(?<ClassName>[a-zA-Z0-9_]+)";
            Regex regex = new Regex(classNameMatchString);
            Match match = regex.Match(currentLine.LineText);

            if (match.Success)
            {
                this.ClassName = match.Groups["ClassName"].Value.ToString();
                //Console.WriteLine("Parsing " + this.ClassName);
            }
            else
            {
                returnValue = false;
            }
            
            return returnValue;
        }

        /// <summary>
        /// Method to handle parsing of the properties themselves in our obj range
        /// </summary>
        /// <param name="PropLines"></param>
        public void ParseObjProps(List<HPPLine> PropLines)
        {

            // get all of the prop lines that are in scope of our obj
            List<HPPLine> linesInScope = (from l in PropLines
                                         where l.LineNumber >= this.StartLine
                                         && l.LineNumber < this.LastLine
                                         select l).ToList<HPPLine>();

            // regex strings for matching
            
            string typeMatchString = "(type)\\s*(=)\\s*\"(?<Type>[a-zA-Z0-9_]+)";
            string buyPriceMatchString = "(buy\\[\\])\\s*(=)\\s*(\\{)(?<Cost>[0-9]+)";
            string sellPriceMatchString = "(sell\\[\\])\\s*(=)\\s*(\\{)(?<Cost>[0-9]+)";

            // disassemble the props for our obj
            Regex typeRegex = new Regex(typeMatchString);
            Regex buyRegex = new Regex(buyPriceMatchString);
            Regex sellRegex = new Regex(sellPriceMatchString);
            

            // get our matches.
            foreach (HPPLine line in linesInScope)
            {
                Match typeMatch = Regex.Match(line.LineText, typeMatchString);
                Match buyMatch = Regex.Match(line.LineText, buyPriceMatchString);
                Match sellMatch = Regex.Match(line.LineText, sellPriceMatchString);

                if (typeMatch.Success)
                {
                    this.Type = typeMatch.Groups["Type"].ToString();
                    //Console.WriteLine("Type: " + this.Type);
                }
                else if (buyMatch.Success)
                {
                    this.BuyPrice = Convert.ToInt32(buyMatch.Groups["Cost"].ToString());
                    //Console.WriteLine("Cost: $" + this.BuyPrice.ToString());
                }
                else if (sellMatch.Success)
                {
                    this.SellPrice = Convert.ToInt32(sellMatch.Groups["Cost"].ToString());
                    //Console.WriteLine("Sell: $" + this.SellPrice.ToString());
                }
            }
        }
    }    
}
