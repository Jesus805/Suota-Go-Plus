namespace suota_pgp
{
    public interface IStateManager
    {
        AppState State { get; set; }

        ErrorState ErrorState { get; }
    }
}
