namespace AspectCore.Api.Domain.Entities
{
    public class City
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int IdExterno { get; private set; }

        public City(Guid id, string name, int idExterno)
        {
            Id = id;
            Name = name;
            IdExterno = idExterno;
        }
    }
}
