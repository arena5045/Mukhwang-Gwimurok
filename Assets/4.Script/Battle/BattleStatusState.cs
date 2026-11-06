public sealed class BattleStatusState
{
    public BattleStatusData Data { get; }

    public int Amount { get; set; }

    public BattleStatusState(
        BattleStatusData data,
        int amount)
    {
        Data = data;
        Amount = amount;
    }
}