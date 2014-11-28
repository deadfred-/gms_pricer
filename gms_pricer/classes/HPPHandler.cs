using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gms_pricer.classes
{
    /// <summary>
    /// Class to handle writing files to disk and other things...
    /// </summary>
    class HPPHandler
    {
        public string OutputDirectory { get; set; }
        public List<HPPFile> OutputFiles = new List<HPPFile>();

        /// <summary>
        /// Save our parsed files to the HPP format
        /// </summary>
        /// <returns>false if there was an IO exception</returns>
        public bool SaveFilesToDisk ()
        {
            bool returnValue = true; // assume true
            double priceModifier = 0.30;
            double finalSellPrice = 0;
            double finalBuyPrice = 0;

            // make sure we have files to save
            if (this.OutputFiles.Count() > 0)
            {
                foreach (HPPFile file in this.OutputFiles)
                {
                    try
                    {
                        List<string> fileLines = new List<string>();

                        foreach (HPPCategory cat in file.Categories)
                        {
                            // write cat start line
                            //class Category_478 {
                            string catStartLine = string.Format("class {0} {{",cat.CategoryName);
                            fileLines.Add(catStartLine);
                            
                            foreach (HPPobj obj in cat.HPPItems)
                            {
                                priceModifier = 0.30;
                                if (obj.ClassName.Equals("BAF_Merlin_DZE"))
                                    Console.WriteLine("MATCH");

                                // write our items
                                //class 30Rnd_556x45_StanagSD {
                                string objStartLine = string.Format("\tclass {0} {{", obj.ClassName);
                                fileLines.Add(objStartLine);

                                // price math
                                int price = obj.BuyPrice;                                
                                if (file.FileName.Contains("Hero"))
                                {
                                    priceModifier = 0.25;
                                }

                                if (obj.Type.Equals("trade_weapons"))
                                {
                                    priceModifier = 0.12;
                                }

                                if (obj.ClassName.Equals("ItemBriefcase100oz") 
                                    || obj.ClassName.Equals("ItemGoldBar") 
                                    || obj.ClassName.Equals("ItemGoldBar10oz") 
                                    || obj.ClassName.Equals("ItemSilverBar") 
                                    || obj.ClassName.Equals("ItemSilverBar10oz") 
                                    || obj.ClassName.Equals("ItemCopperBar10oz"))
                                {
                                    priceModifier = 1;
                                }

                                finalSellPrice = Convert.ToInt32(Math.Floor((double)price * priceModifier));

                                // write out our props                                
                                string objPropTypeLine = string.Format("\t\ttype = \"{0}\";",obj.Type);
                                string objPropBuyLine = string.Format("\t\tbuy[] = {{{0}, \"Coins\"}};",obj.BuyPrice);
                                string objPropSellLine = string.Format("\t\tsell[] = {{{0}, \"Coins\"}};", finalSellPrice);

                                fileLines.Add(objPropTypeLine);
                                fileLines.Add(objPropBuyLine);
                                fileLines.Add(objPropSellLine);

                                // write our obj end line
                                string objEndLine = string.Format("\t}};");
                                fileLines.Add(objEndLine);
                            }

                            // write cat close line
                            string catEndLine = "};";
                            fileLines.Add(catEndLine);

                        }

                        // check to see if folder exists
                        string subPath = "out";
                        bool folderExists = Directory.Exists(subPath);

                        if (!folderExists)
                        {
                            Directory.CreateDirectory("out");
                        }

                        // create new file
                        StreamWriter sw = new StreamWriter("out\\" + file.FileName);

                        // write lines
                        foreach (string fileLine in fileLines)
                        {
                            sw.WriteLine(fileLine);
                        }

                        // close file
                        sw.Close();
                    }
                    catch (Exception ex)
                    {
                        returnValue = false;
                    }
                }
            }
            else
            {
                returnValue = false;
            }


            return returnValue;
        }

    }
}
