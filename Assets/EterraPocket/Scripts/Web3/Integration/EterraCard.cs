using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.card;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;

namespace Eterra.Integration.Model
{
  /// <summary>
  /// A helper class for managing the Eterra Card fields.
  /// Provides structured access to the Card properties and encoding/decoding methods.
  /// </summary>
  public class EterraCard
  {
    private readonly Card _rawCard;

    public byte Top { get; set; }
    public byte Right { get; set; }
    public byte Bottom { get; set; }
    public byte Left { get; set; }
    public Color? CardColor { get; set; }

    public EterraCard(Card card)
    {
      _rawCard = card;
      Top = card.Top.Value;
      Right = card.Right.Value;
      Bottom = card.Bottom.Value;
      Left = card.Left.Value;
      CardColor = card.Color.Value != null ? card.Color.Value : null;
    }

    public Card ToRawCard()
    {
      return new Card
      {
        Top = new U8(Top),
        Right = new U8(Right),
        Bottom = new U8(Bottom),
        Left = new U8(Left),
        Color = new BaseOpt<EnumColor>(CardColor.HasValue ? (EnumColor)CardColor.Value : default)
      };
    }

    public byte[] Encode() => ToRawCard().Encode();

    public static EterraCard Decode(byte[] byteArray, ref int p)
    {
      var card = new Card();
      card.Decode(byteArray, ref p);
      return new EterraCard(card);
    }

    public override string ToString()
    {
      return $"EterraCard [Top: {Top}, Right: {Right}, Bottom: {Bottom}, Left: {Left}, Color: {CardColor}]";
    }
  }
}