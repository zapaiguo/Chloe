namespace Chloe.Entity
{
    public interface ICollectionPropertyBuilder
    {
        CollectionProperty Property { get; }
    }

    public interface ICollectionPropertyBuilder<TProperty> : ICollectionPropertyBuilder
    {

    }
}
