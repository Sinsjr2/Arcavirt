/// <summary>
/// 
/// </summary>
/// <param name="NewVelocity">null のときは設定された最大速度を適用します。</param>
public record struct ChangeVelocityCommand(int Position, int? NewVelocity) : IComparable<ChangeVelocityCommand> {
    public int CompareTo(ChangeVelocityCommand other) => Position.CompareTo(other.Position);
}
