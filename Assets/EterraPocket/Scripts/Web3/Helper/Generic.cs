using Eterra.NetApiExt.Generated.Model.primitive_types;
using Eterra.NetApiExt.Generated.Model.sp_core.crypto;
using Eterra.NetApiExt.Generated.Model.sp_runtime.multiaddress;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Substrate.Integration.Helper
{
  public static class Generic
  {
    /// <summary>
    /// Unit to decimals
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    public static BigInteger UnitToDecimals(double amount, int decimals)
        => new BigInteger(Convert.ToUInt64(amount * Math.Pow(10, decimals)));

    /// <summary>
    /// Convert H256 to hex string
    /// </summary>
    /// <param name="h256"></param>
    /// <returns></returns>
    public static string ToHexString(this H256 h256)
    {
      return Utils.Bytes2HexString(h256.Value.Value.Select(p => p.Value).ToArray());
    }

    /// <summary>
    /// Convert address string to public key
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte[] ToPublicKey(this string address)
    {
      return Utils.GetPublicKeyFrom(address);
    }

    /// <summary>
    /// Convert AccountId32 to address string
    /// </summary>
    /// <param name="account32"></param>
    /// <param name="ss58"></param>
    /// <returns></returns>
    public static string ToAddress(this AccountId32 account32, short ss58 = 42)
    {
      var pubKey = account32.Value.Value.Select(p => p.Value).ToArray();
      return pubKey.ToAddress(ss58);
    }

    /// <summary>
    /// Convert public key to address string
    /// </summary>
    /// <param name="publicKey"></param>
    /// <param name="ss58"></param>
    /// <returns></returns>
    public static string ToAddress(this byte[] publicKey, short ss58 = 42)
    {
      return Utils.GetAddressFrom(publicKey, ss58);
    }

    /// <summary>
    /// Convert public key to AccountId32
    /// </summary>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    public static AccountId32 ToAccountId32(this byte[] publicKey)
    {
      var account32 = new AccountId32();
      account32.Create(publicKey);

      return account32;
    }

    /// <summary>
    /// Convert Account to AccountId32
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public static AccountId32 ToAccountId32(this Account account)
    {
      var account32 = new AccountId32();
      account32.Create(account.Bytes);

      return account32;
    }

    /// <summary>
    /// Convert address string to AccountId32
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static AccountId32 ToAccountId32(this string address)
    {
      var account32 = new AccountId32();
      account32.Create(address.ToPublicKey());

      return account32;
    }

    /// <summary>
    /// Convert AccountId32 to EnumMultiAddress
    /// </summary>
    /// <param name="accountId32"></param>
    /// <returns></returns>
    public static EnumMultiAddress ToEnumMultiAddress(this AccountId32 accountId32)
    {
      var multiAddress = new EnumMultiAddress();
      multiAddress.Create(MultiAddress.Id, accountId32);

      return multiAddress;
    }

    /// <summary>
    /// Convert hash string to H256
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static H256 ToH256(this string hash)
    {
      var h256 = new H256();
      h256.Create(hash);
      return h256;
    }

    /// <summary>
    /// Convert string to H256
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static H256 ToHash(this string name)
    {
      var nameHash = HashExtension.Twox256(Encoding.UTF8.GetBytes(name));

      var h256 = new H256();
      h256.Create(nameHash);
      return h256;
    }

    /// <summary>
    /// Convert byte to U8
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static U8 ToU8(this byte number)
    {
      var u8 = new U8();
      u8.Create(number);
      return u8;
    }

    /// <summary>
    /// Convert char to U8
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public static U8 ToU8(this char character)
    {
      var u8 = new U8();
      u8.Create(BitConverter.GetBytes(character)[0]);
      return u8;
    }

    /// <summary>
    /// Convert ushort to U16
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static U16 ToU16(this ushort number)
    {
      var u16 = new U16();
      u16.Create(number);
      return u16;
    }

    /// <summary>
    /// Convert uint to U32
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static U32 ToU32(this uint number)
    {
      var u32 = new U32();
      u32.Create(number);
      return u32;
    }

    /// <summary>
    /// Convert BigInteger to U128
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static U128 ToU128(this BigInteger number)
    {
      var u128 = new U128();
      u128.Create(number);
      return u128;
    }

    /// <summary>
    /// Convert byte array to U8 array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static U8[] ToU8Array(this byte[] bytes)
    {
      return bytes.Select(p => p.ToU8()).ToArray();
    }

    /// <summary>
    /// Convert string to U8 array
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static U8[] ToU8Array(this string str)
    {
      return str.Select(p => p.ToU8()).ToArray();
    }

    /// <summary>
    /// Convert ushort array to U16 array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static U16[] ToU16Array(this ushort[] bytes)
    {
      return bytes.Select(p => p.ToU16()).ToArray();
    }

    /// <summary>
    /// Convert uint array to U32 array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static U32[] ToU32Array(this uint[] bytes)
    {
      return bytes.Select(p => p.ToU32()).ToArray();
    }

    /// <summary>
    /// Convert U8 to BaseOpt
    /// </summary>
    /// <param name="u8"></param>
    /// <returns></returns>
    public static BaseOpt<U8> ToBaseOpt(this U8 u8)
    {
      var baseOpt = new BaseOpt<U8>();
      baseOpt.Create(u8);
      return baseOpt;
    }

    /// <summary>
    /// Convert H256 to byte array
    /// </summary>
    /// <param name="h256"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this H256 h256)
    {
      return h256.Value.Value.ToBytes();
    }

    /// <summary>
    /// Convert BaseVec of U8 to byte array
    /// </summary>
    /// <param name="baseVecU8"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this BaseVec<U8> baseVecU8)
    {
      return baseVecU8.Value.ToBytes();
    }

    /// <summary>
    /// Convert U8 array to byte array
    /// </summary>
    /// <param name="arrayU8"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this U8[] arrayU8)
    {
      return arrayU8.Select(p => p.Value).ToArray();
    }

    /// <summary>
    /// Build chunks of a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullList"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> BuildChunksOf<T>(IEnumerable<T> fullList, int batchSize)
    {
      int total = 0;
      while (total < fullList.Count())
      {
        yield return fullList.Skip(total).Take(batchSize);
        total += batchSize;
      }
    }
  }
}