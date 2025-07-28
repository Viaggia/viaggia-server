namespace viaggia_server.Repositories
{
    public interface ISoftDeletable
    {
        bool IsActive { get; set; }
    }
}
