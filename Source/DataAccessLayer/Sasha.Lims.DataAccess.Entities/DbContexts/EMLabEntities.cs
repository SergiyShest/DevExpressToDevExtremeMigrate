namespace Sasha.Lims.DataAccess.Entities.Entity
{
    /// <summary>
    /// Partial class to define EMLabEntities constructor with connection string as parameter
    /// </summary>
    public partial class EMLabEntities
    {
        public EMLabEntities(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }
    }
}
