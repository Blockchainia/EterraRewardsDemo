using Eterra.NetApiExt.Generated.Model.solochain_template_runtime; // Import EnumRuntimeCallusing Eterra.NetApiExt.Generated.Storage;

namespace Substrate.Integration.Call
{
  /// <summary>
  /// Call interface to be implemented by all calls
  /// </summary>
  public interface ICall
  {
    /// <summary>
    /// Convert the call to a runtime call
    /// </summary>
    /// <returns></returns>
    EnumRuntimeCall ToCall();
  }
}