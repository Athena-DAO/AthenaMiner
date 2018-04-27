using Docker.DotNet;
using Docker.DotNet.Models;
using Interfaces;
using System;
using System.Threading;

namespace ContainerManagement
{
    public sealed class DockerManager : IContainerManager
    {
        private static volatile DockerManager instance;
        private static object syncRoot = new object();
        private DockerClient client;

        private DockerManager()
        {

        }

        public static DockerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new DockerManager();
                        }
                    }
                }
                return instance;
            }
        }

        public void InitClient(string uri)
        {
            client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }

        private CreateContainerResponse CreateContainer(string parent, string containerId, string[] args)
        {
            return client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Name = containerId,
                Image = parent,
                AttachStdout = true,
                AttachStdin = false,
                ArgsEscaped = false,
                Cmd = args
            }).Result;
        }

        private void StartContainer(string containerId)
        {
            client.Containers.StartContainerAsync(containerId, new ContainerStartParameters()
            {

            }).Wait();
        }

        private void StopContainer(string containerId)
        {
            client.Containers.StopContainerAsync(containerId, new ContainerStopParameters()
            {
                WaitBeforeKillSeconds = 1
            }).Wait();
        }

        private void RemoveContainer(string containerId)
        {
            client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters()
            {
                Force = true
            }).Wait();
        }

        private void PullImage(string parent, string tag)
        {
            client.Images.CreateImageAsync(new ImagesCreateParameters()
            {
                FromImage = parent,
                Tag = tag
            }, null, new Progress()).Wait();
        }

        public (string, string) RunImage(string parent, string tag, string[] args)
        {
            string containerId = Guid.NewGuid().ToString();
            PullImage(parent, tag);

            CreateContainerResponse response = CreateContainer(parent, containerId, args);
            StartContainer(containerId);

            string stdOut = "", stdErr = "";
            using (var stream1 = client.Containers.AttachContainerAsync(response.ID, false, new ContainerAttachParameters()
            {
                Stream = true,
                Stderr = true,
                Stdin = false,
                Stdout = true,
                Logs = "1"
            }).Result)
            {
                (string output, string stderr) = stream1.ReadOutputToEndAsync(default(CancellationToken)).Result;
                stdOut += output;
                stdErr += stdErr;
            }

            StopContainer(containerId);
            RemoveContainer(containerId);

            return (stdOut, stdErr);
        }

        class Progress : IProgress<JSONMessage>
        {
            public void Report(JSONMessage value)
            {
            }
        }
    }
}
