﻿using System;
using System.IO;
using System.Text;
using Kronos.Core.Communication;
using Kronos.Core.Configuration;
using Kronos.Core.Requests;
using Kronos.Core.Serialization;
using Kronos.Core.StatusCodes;
using Microsoft.Extensions.PlatformAbstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Kronos.Client.Tests
{
    public class KronosClientTests
    {
        [Fact]
        public void Insert_InsertsObjectToStorage()
        {
            string key = "key";
            byte[] package = Encoding.UTF8.GetBytes("package");
            DateTime expiryDate = DateTime.Today.AddDays(1);

            var communicationServiceMock = new Mock<IClientServerConnection>();
            communicationServiceMock.Setup(x => x.SendToServer(It.IsAny<Request>()))
                .Returns(SerializationUtils.Serialize(RequestStatusCode.Ok));

            KronosConfig config = LoadTestConfiguration();
            IKronosClient client = new KronosClient(config, (endpoint) => communicationServiceMock.Object);
            client.Insert(key, package, expiryDate);

            communicationServiceMock.Verify(x => x.SendToServer(It.IsAny<InsertRequest>()), Times.Once);
        }

        [Fact]
        public void Get_ReturnsObject()
        {
            const string word = "lorem ipsum";
            byte[] package = SerializationUtils.Serialize(word);

            var communicationServiceMock = new Mock<IClientServerConnection>();
            communicationServiceMock
                .Setup(x => x.SendToServer(It.IsAny<GetRequest>()))
                .Returns(SerializationUtils.Serialize(package));

            KronosConfig config = LoadTestConfiguration();
            IKronosClient client = new KronosClient(config, (endpoint) => communicationServiceMock.Object);

            byte[] response = client.Get("key");

            string responseString = SerializationUtils.Deserialize<string>(response);
            Assert.Equal(responseString, word);
            communicationServiceMock.Verify(x => x.SendToServer(It.IsAny<GetRequest>()), Times.Once);
        }

        [Fact]
        public void Get_DoestNotReturnObject()
        {
            var communicationServiceMock = new Mock<IClientServerConnection>();
            communicationServiceMock
                .Setup(x => x.SendToServer(It.IsAny<GetRequest>()))
                .Returns(SerializationUtils.Serialize(RequestStatusCode.NotFound));

            KronosConfig config = LoadTestConfiguration();
            IKronosClient client = new KronosClient(config, (endpoint) => communicationServiceMock.Object);

            byte[] response = client.Get("key");

            Assert.Null(response);
            communicationServiceMock.Verify(x => x.SendToServer(It.IsAny<GetRequest>()), Times.Once);
        }

        [Fact]
        public void Delete_CallsSendToServer()
        {
            var communicationServiceMock = new Mock<IClientServerConnection>();
            communicationServiceMock
                .Setup(x => x.SendToServer(It.IsAny<DeleteRequest>()))
                .Returns(SerializationUtils.Serialize(RequestStatusCode.Ok));

            KronosConfig config = LoadTestConfiguration();
            IKronosClient client = new KronosClient(config, (endpoint) => communicationServiceMock.Object);

            client.Delete("key");

            communicationServiceMock.Verify(x => x.SendToServer(It.IsAny<DeleteRequest>()), Times.Once);
        }

        private static KronosConfig LoadTestConfiguration()
        {
            string dir = PlatformServices.Default.Application.ApplicationBasePath;
            string configContent = File.ReadAllText($"{dir}/KronosConfig.json");

            KronosConfig config = JsonConvert.DeserializeObject<KronosConfig>(configContent);
            return config;
        }
    }
}
