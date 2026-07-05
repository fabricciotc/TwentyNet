namespace TwentyNet.BFF.Hubs;

public interface IWorkspaceClient
{
    Task ObjectRecordChanged(string objectName, Guid recordId, string changeType);
}
