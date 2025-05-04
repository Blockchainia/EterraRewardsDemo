using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types;
using System.Linq;

namespace Eterra.Integration.Helpers
{
  public static class BaseVecHelper
  {
    /// <summary>
    /// Converts a BaseVec<T> to an array of T.
    /// </summary>
    /// <typeparam name="T">The type contained within BaseVec, must implement IType.</typeparam>
    /// <param name="baseVec">The BaseVec to convert.</param>
    /// <returns>An array of T.</returns>
    public static T[] ToArray<T>(BaseVec<T> baseVec) where T : IType, new()
    {
      return baseVec.Value.ToArray();
    }
  }
}