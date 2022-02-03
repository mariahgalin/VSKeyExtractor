﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace VSKeyExtractor
{
    struct Product
    {
        public string Name { get; }
        public string GUID { get; }
        public string MPC { get; }
        public Product(string Name, string GUID, string MPC)
        {
            this.Name = Name;
            this.GUID = GUID;
            this.MPC = MPC;
        }
    }

    class Program
    {
        static readonly List<Product> Products = new List<Product>
        {
            new Product("Visual Studio 2015 Enterprise"       , "4D8CFBCB-2F6A-4AD2-BABF-10E28F6F2C8F", "07060"),
            new Product("Visual Studio 2015 Professional"     , "4D8CFBCB-2F6A-4AD2-BABF-10E28F6F2C8F", "07062"),

            new Product("Visual Studio 2017 Enterprise"       , "5C505A59-E312-4B89-9508-E162F8150517", "08860"),
            new Product("Visual Studio 2017 Professional"     , "5C505A59-E312-4B89-9508-E162F8150517", "08862"),
            new Product("Visual Studio 2017 Test Professional", "5C505A59-E312-4B89-9508-E162F8150517", "08866"),

            new Product("Visual Studio 2019 Enterprise"       , "41717607-F34E-432C-A138-A3CFD7E25CDA", "09260"),
            new Product("Visual Studio 2019 Professional"     , "41717607-F34E-432C-A138-A3CFD7E25CDA", "09262"),

            new Product("Visual Studio 2022 Enterprise"       , "1299B4B9-DFCC-476D-98F0-F65A2B46C96D", "09660"),
            new Product("Visual Studio 2022 Professional"     , "1299B4B9-DFCC-476D-98F0-F65A2B46C96D", "09662"),
        };

        [STAThread]
        static void Main()
        {
            foreach (var product in Products) ExtractLicense(product);
        }

        private static void ExtractLicense(Product product)
        {
            var encrypted = Registry.GetValue($"HKEY_CLASSES_ROOT\\Licenses\\{product.GUID}\\{product.MPC}", "", null);
            if (encrypted == null) return;
            try
            {
                var secret = ProtectedData.Unprotect((byte[])encrypted, null, DataProtectionScope.CurrentUser);
                var unicode = new UnicodeEncoding();
                var str = unicode.GetString(secret);
                foreach (var sub in str.Split('\0'))
                {
                    var match = Regex.Match(sub, @"\w{5}-\w{5}-\w{5}-\w{5}-\w{5}");
                    if (match.Success)
                    {
                        Console.WriteLine($"Found key for {product.Name}: {match.Captures[0]}");
                        TxtFile("Found key for", product.Name, match.Captures[0].ToString());
                    }
                }
            }
            catch (Exception) { }
        }

        private static string ChoosePath()
        {
            bool anyPath = false;
            string path = "";

            try
            {
                FolderBrowserDialog dialogExplorer = new FolderBrowserDialog();
                dialogExplorer.ShowNewFolderButton = false;

                DialogResult result = dialogExplorer.ShowDialog();
                if (result == DialogResult.OK)
                {
                    path = dialogExplorer.SelectedPath;
                    anyPath = true;

                    dialogExplorer.RootFolder = Environment.SpecialFolder.Desktop;
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message, "Error in Choose directory");
            }

            return path;
        }

        static private string GenerateFileName(string ProductVS)
        {
            string fileName = "";
            string fileNameExtension = ".txt";
            string license = "License";

            //Create the name
            fileName =  ProductVS + " " + license + fileNameExtension;

            return fileName;
        }

        static public void TxtFile(string header, string ProductVS, string key)
        {
            StringBuilder getData = new StringBuilder();

            string fileName = GenerateFileName(ProductVS);

            string rootPath = ChoosePath();
            string fullFilePath = rootPath + "\\" + fileName;

            getData.Append(header + " " + ProductVS + ": " + key + "\n");
            
            File.AppendAllText(fullFilePath, getData.ToString());
        }
    }
}

