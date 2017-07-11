namespace Stormpath.Owin.Middleware.Model
{
    public sealed class MeGroupsCollectionModel
    {
        public int Size { get; set; }

        public MeGroupModel[] Items { get; set; }
    }
}
