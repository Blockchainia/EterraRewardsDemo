using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using Eterra.NetApiExt.Generated.Model.solochain_template_runtime; // Import EnumRuntimeCallusing Eterra.NetApiExt.Generated.Storage;

using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Numerics;

namespace Substrate.Integration.Call
{
  /// <summary>
  /// Pallet Balances
  /// </summary>
  public static class PalletBalances
  {
    /// <summary>
    /// Balances transfer keep alive call
    /// </summary>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public static EnumRuntimeCall BalancesTransferKeepAlive(AccountId32 target, BigInteger amount)
    {
      var baseU128 = new BaseCom<U128>();
      baseU128.Create(amount);

      var multiAddress = new EnumMultiAddress();
      multiAddress.Create(MultiAddress.Id, target);

      var baseTubleParams = new BaseTuple<EnumMultiAddress, BaseCom<U128>>();
      baseTubleParams.Create(multiAddress, baseU128);

      var enumPalletCall = new Eterra.NetApiExt.Generated.Model.pallet_balances.pallet.EnumCall();
      enumPalletCall.Create(Eterra.NetApiExt.Generated.Model.pallet_balances.pallet.Call.transfer_keep_alive, baseTubleParams);

      var enumCall = new EnumRuntimeCall();
      enumCall.Create(RuntimeCall.Balances, enumPalletCall);

      return enumCall;
    }

    /// <summary>
    /// Balances transfer call
    /// </summary>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public static EnumRuntimeCall BalancesTransfer(AccountId32 target, BigInteger amount)
    {
      var baseU128 = new BaseCom<U128>();
      baseU128.Create(amount);

      var multiAddress = new EnumMultiAddress();
      multiAddress.Create(MultiAddress.Id, target);

      var baseTubleParams = new BaseTuple<EnumMultiAddress, BaseCom<U128>>();
      baseTubleParams.Create(multiAddress, baseU128);

      var enumPalletCall = new Eterra.NetApiExt.Generated.Model.pallet_balances.pallet.EnumCall();
      enumPalletCall.Create(Eterra.NetApiExt.Generated.Model.pallet_balances.pallet.Call.transfer_keep_alive, baseTubleParams);

      var enumCall = new EnumRuntimeCall();
      enumCall.Create(RuntimeCall.Balances, enumPalletCall);

      return enumCall;
    }
  }
}