using System.Collections.Generic;
namespace GraceMusic.Core.Interfaces;

public interface IRepository<T> where T : class
{
    List<T> LoadAll();
    void SaveAll(List<T> items);
}