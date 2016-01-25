﻿using System;
using System.IO;
using System.Xml;
using Kronos.Client;
using NLog;
using NLog.Config;

namespace ClientSample
{
    public class Program
    {
        private static ILogger _logger;
        public static void LoggerSetup()
        {
            var reader = XmlReader.Create("NLog.config");
            var config = new XmlLoggingConfiguration(reader, null); //filename is not required.
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public static void Main(string[] args)
        {
            LoggerSetup();

            _logger.Info("Starting program");

            IKronosClient client = KronosClientFactory.CreateClient();
            string key = "key";
            string fileToSend = "C:\\Temp\\file.bin";
            DateTime expiryDate = new DateTime();

            client.InsertToServer(key, File.ReadAllBytes(fileToSend), expiryDate);

            _logger.Info("Closing program");
        }
    }
}