using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.card;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Eterra.Integration.Model
{
  /// <summary>
  /// A helper class to handle conversions between BaseOpt<Card> and Card.
  /// </summary>
  public class CardWrapper
  {
    /// <summary>
    /// The wrapped Card object (can be null if optional).
    /// </summary>
    public Card? WrappedCard { get; private set; }

    /// <summary>
    /// Constructor from an optional BaseOpt<Card>.
    /// </summary>
    /// <param name="optionalCard">The optional card data from Substrate.</param>
    public CardWrapper(BaseOpt<Card> optionalCard)
    {
      WrappedCard = optionalCard.Value; // Unwrap the optional value
    }

    /// <summary>
    /// Constructor from an existing Card object.
    /// </summary>
    /// <param name="card">The Card object to wrap.</param>
    public CardWrapper(Card card)
    {
      WrappedCard = card;
    }

    /// <summary>
    /// Converts this wrapper into a BaseOpt<Card> for Substrate storage.
    /// </summary>
    /// <returns>A BaseOpt<Card> containing the wrapped Card.</returns>
    public BaseOpt<Card> ToBaseOpt()
    {
      return new BaseOpt<Card>(WrappedCard);
    }

    /// <summary>
    /// Checks if this wrapper contains a valid card.
    /// </summary>
    /// <returns>True if a card is present, false otherwise.</returns>
    public bool HasCard()
    {
      return WrappedCard != null;
    }

    /// <summary>
    /// Returns a string representation of the card.
    /// </summary>
    public override string ToString()
    {
      return HasCard() ? WrappedCard.ToString() : "No Card Present";
    }
  }
}