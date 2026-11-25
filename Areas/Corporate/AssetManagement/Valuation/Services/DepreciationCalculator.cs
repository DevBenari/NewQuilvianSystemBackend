namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed record DepreciationBookState(
    decimal CapitalizedCost,
    decimal AccumulatedDepreciation,
    decimal AccumulatedImpairment,
    decimal ResidualValue,
    int UsefulLifeMonths,
    int DepreciatedMonths)
{
    public decimal NetBookValue => CapitalizedCost - AccumulatedDepreciation - AccumulatedImpairment;

    public int RemainingMonths => Math.Max(0, UsefulLifeMonths - DepreciatedMonths);
}

public static class DepreciationCalculator
{
    public static decimal RoundRupiah(decimal value) => decimal.Round(value, 0, MidpointRounding.AwayFromZero);

    public static decimal MonthlyAmount(decimal netBookValue, decimal residualValue, int remainingMonths)
    {
        var depreciable = netBookValue - residualValue;
        if (depreciable <= 0m || remainingMonths <= 0)
        {
            return 0m;
        }

        if (remainingMonths == 1)
        {
            return depreciable;
        }

        return Math.Min(depreciable, RoundRupiah(depreciable / remainingMonths));
    }

    public static decimal MonthlyAmount(DepreciationBookState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return MonthlyAmount(state.NetBookValue, state.ResidualValue, state.RemainingMonths);
    }

    public static DepreciationBookState Apply(DepreciationBookState state, decimal amount, int months) =>
        state with
        {
            AccumulatedDepreciation = state.AccumulatedDepreciation + amount,
            DepreciatedMonths = state.DepreciatedMonths + months
        };

    public static IReadOnlyList<decimal> Schedule(DepreciationBookState state, int months)
    {
        ArgumentNullException.ThrowIfNull(state);
        var amounts = new List<decimal>();
        var current = state;
        for (var index = 0; index < months; index++)
        {
            var amount = MonthlyAmount(current);
            if (amount <= 0m)
            {
                break;
            }

            amounts.Add(amount);
            current = Apply(current, amount, 1);
        }

        return amounts;
    }

    public static decimal ExpectedAccumulation(decimal capitalizedCost, decimal residualValue, int usefulLifeMonths, int depreciatedMonths) =>
        Schedule(new DepreciationBookState(capitalizedCost, 0m, 0m, residualValue, usefulLifeMonths, 0), depreciatedMonths).Sum();

    public static decimal PriceCorrectionAdjustment(DepreciationBookState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var expected = ExpectedAccumulation(state.CapitalizedCost, state.ResidualValue, state.UsefulLifeMonths, state.DepreciatedMonths);
        return expected - state.AccumulatedDepreciation;
    }
}
