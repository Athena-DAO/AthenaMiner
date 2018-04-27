namespace Interfaces
{
    public interface IContainerManager
    {
        void InitClient(string url);
        (string, string) RunImage(string image, string tag, string[] args);
    }
}
