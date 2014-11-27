using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gms_pricer.classes;
using System.IO;

namespace gms_pricer
{
    class Program
    {
        static void Main(string[] args)
        {
            // get list of files in directory
            string dirPath = @"C:\Users\admin\Documents\GitHub\BigTDayZEpoch\MPMissions\DayZ_Epoch_11_vp.Chernarus\Scripts\Trader_Items\Category";
            string[] files = Directory.GetFiles(dirPath);            
            List<string> logLines = new List<string>();
            List<HPPFile> parsedFiles = new List<HPPFile>();

            double priceModifier = 0.25;
            double finalSellPrice = 0;

            foreach (string file in files)
            {
                Console.WriteLine(string.Format("Opening file: {0}", file));
                if (file.Contains("NeutralAssault"))
                    Console.WriteLine("DEBUG: MATCH");
                logLines.Add(string.Format("Opening file: {0}", file));
                logLines.Add("");

                // parse file
                HPPFile hpFile = new HPPFile(file);
                List<HPPCategory> cats = hpFile.GetCategories();
                // save cat list to files
                hpFile.Categories = cats;

                // save file info to file
                FileInfo fi = new FileInfo(file);
                hpFile.FileName = fi.Name;

                // add to our master list
                parsedFiles.Add(hpFile);

                // test of HPPHandler
                HPPHandler handler = new HPPHandler();

                handler.OutputFiles = parsedFiles;
                handler.SaveFilesToDisk();

                // set prices 
                foreach (HPPCategory cat in cats)
                {
                    foreach (HPPobj obj in cat.HPPItems)
                    {
                        // get price
                        int price = obj.BuyPrice;
                        finalSellPrice = Convert.ToInt32(Math.Floor((double)price * priceModifier));
                        string line = string.Format("Item:\t{0}\tBuy:\t{1}\tNewSell:\t{2}\tOldSell:\t{3}",obj.ClassName, obj.BuyPrice.ToString(), finalSellPrice.ToString(), obj.SellPrice.ToString());

                        // add our record to the log
                        logLines.Add(line);

                        // TODO: Commit changes to disk

                        // DEBUG
                        Console.WriteLine(line);
                    }
                }

                

            }
            

            // finally log the lines to a file
            StreamWriter sw = new StreamWriter("newPrices.txt");
            foreach (string logLine in logLines)
            {
                sw.WriteLine(logLine);
            }
            sw.Close();

            
            /*
            HPPFile file = new HPPFile(@"C:\Users\admin\Documents\GitHub\BigTDayZEpoch\MPMissions\DayZ_Epoch_11_vp.Chernarus\Scripts\Trader_Items\Category\NeutralBlackMarketWeapons.hpp"); // TODO: Set up construct
            List<HPPCategory> cats = file.GetCategories();
            */
            Console.ReadKey();


        }
    }
}
