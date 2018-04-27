using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ContainerManagement
{
    public class DockerManager
    {
        private DockerClient client;

        public DockerManager(string uri)
        {
            client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }

        private async System.Threading.Tasks.Task<CreateContainerResponse> CreateContainer(string parent, string containerId, string[] args)
        {
            return await client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Name = containerId,
                Image = parent,
                AttachStdout = true,
                AttachStdin = false,
                ArgsEscaped = false,
                Cmd = args
            });
        }

        public async System.Threading.Tasks.Task StartContainer(string containerId)
        {
            await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters()
            {

            });
        }

        public async System.Threading.Tasks.Task StopContainer(string containerId)
        {
            await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters()
            {
                WaitBeforeKillSeconds = 1
            });
        }

        public async System.Threading.Tasks.Task RemoveContainer(string containerId)
        {
            await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters()
            {
                Force = true
            });
        }

        public async System.Threading.Tasks.Task InitAsync(string parent, string tag, string containerId)
        {
            Console.WriteLine("Pulling image " + parent + ":" + tag);

            await client.Images.CreateImageAsync(new ImagesCreateParameters()
            {
                FromImage = parent,
                Tag = tag
            }, null, new Progress());

            CreateContainerResponse response = await CreateContainer(parent, containerId, null);

            await StartContainer(containerId);

            var buffer = new byte[1024];

            using (var stream1 = await client.Containers.AttachContainerAsync(response.ID, false, new ContainerAttachParameters()
            {
                Stream = true,
                Stderr = true,
                Stdin = false,
                Stdout = true,
                Logs = "1"
            }))
            {
                (string output, string stderr) = await stream1.ReadOutputToEndAsync(default(CancellationToken));
                Console.WriteLine(output);
            }


            Stream stream = await client.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters()
            {
                ShowStdout = true,
                ShowStderr = true
            });

            StreamReader streamReader = new StreamReader(stream);
            Console.WriteLine(streamReader.ReadToEnd());

            await StopContainer(containerId);

            await RemoveContainer(containerId);

            Console.WriteLine("Done!");
        }

        class Progress : IProgress<JSONMessage>
        {
            public void Report(JSONMessage value)
            {
                Console.WriteLine(value.ID);
                Console.WriteLine(value.Status);
                Console.WriteLine(value.ProgressMessage);
            }
        }
    }
}
