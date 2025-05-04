using Eterra.NetApiExt.Generated.Types.Base;
using Eterra.NetApiExt.Generated.Model.pallet_eterra.types.card;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;

namespace Eterra.Integration.Model
{
  public class Board
  {
    private readonly Arr4Arr4BaseOpt _rawBoard;

    public Cell[,] Cells { get; private set; }

    public Board(Arr4Arr4BaseOpt board)
    {
      _rawBoard = board;
      Cells = ConvertToCellArray(board);
    }

    /// <summary>
    /// Initializes the board to a default empty state.
    /// </summary>
    public void Init()
    {
      for (int i = 0; i < 4; i++)
      {
        for (int j = 0; j < 4; j++)
        {
          Cells[i, j] = new Cell(i, j, new CardWrapper(new BaseOpt<Card>(null)));
        }
      }
    }

    private Cell[,] ConvertToCellArray(Arr4Arr4BaseOpt board)
    {
      var cellArray = new Cell[4, 4];

      for (int i = 0; i < 4; i++)
      {
        for (int j = 0; j < 4; j++)
        {
          var value = board.Value[i].Value[j];
          cellArray[i, j] = new Cell(i, j, new CardWrapper(new BaseOpt<Card>(null)));
        }
      }

      return cellArray;
    }

    public Cell GetCell(int x, int y)
    {
      if (x < 0 || x >= 4 || y < 0 || y >= 4)
        throw new IndexOutOfRangeException("Board position out of range.");

      return Cells[x, y];
    }

    public void SetCard(int x, int y, Card card)
    {
      if (x < 0 || x >= 4 || y < 0 || y >= 4)
        throw new IndexOutOfRangeException("Board position out of range.");

      Cells[x, y].Occupant = new CardWrapper(card);
    }

    public bool IsOccupied(int x, int y) => GetCell(x, y).Occupant?.HasCard() == true;

    public void ClearCell(int x, int y) => GetCell(x, y).Occupant = null;

    public Arr4Arr4BaseOpt ToArr4Arr4BaseOpt()
    {
      var newBoard = new Arr4Arr4BaseOpt();
      for (int i = 0; i < 4; i++)
      {
        for (int j = 0; j < 4; j++)
        {
          newBoard.Value[i].Value[j] = new BaseOpt<Card>(Cells[i, j].Occupant?.HasCard() == true ? Cells[i, j].Occupant.ToBaseOpt().Value : null);
        }
      }
      return newBoard;
    }
  }

  public class Cell
  {
    public int X { get; }
    public int Y { get; }
    public CardWrapper? Occupant { get; set; }

    public Cell(int x, int y, CardWrapper? occupant = null)
    {
      X = x;
      Y = y;
      Occupant = occupant;
    }

    public override string ToString()
    {
      return Occupant?.HasCard() == true ? $"({X}, {Y}): Occupied" : $"({X}, {Y}): Empty";
    }
  }
}