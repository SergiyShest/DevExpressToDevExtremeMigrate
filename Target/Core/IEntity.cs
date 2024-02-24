

using System.ComponentModel.DataAnnotations;
using System.Reflection;


namespace Core

{
    public interface INamed
    {
        string Name { get; }
    }
    public interface INamedInt : INamed
    { 
        int Id { get; }
    }


    public interface IEntity
    { 
        int? Id { get; set; }
    }


}