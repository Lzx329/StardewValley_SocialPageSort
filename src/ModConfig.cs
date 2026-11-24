namespace SocialPageSort;

/// <summary>排序方式。</summary>
public enum SortMode
{
    Friendship,
    Name,
    Birthday
}

/// <summary>写入 config.json 的设置，按 SMAPI 的约定使用公共可读写属性。</summary>
public sealed class ModConfig
{
    /// <summary>当前排序方式。默认好感排序。</summary>
    public SortMode SortMode { get; set; } = SortMode.Friendship;

    /// <summary>好感排序是否降序。</summary>
    public bool FriendshipDescending { get; set; } = true;

    /// <summary>名字排序是否降序。</summary>
    public bool NameDescending { get; set; }

    /// <summary>生日排序是否降序。</summary>
    public bool BirthdayDescending { get; set; }

    /// <summary>取得该方式记录的方向。</summary>
    public bool GetDescending(SortMode mode)
    {
        return mode switch
        {
            SortMode.Name => this.NameDescending,
            SortMode.Birthday => this.BirthdayDescending,
            _ => this.FriendshipDescending
        };
    }

    /// <summary>设置该方式的方向。</summary>
    public void SetDescending(SortMode mode, bool descending)
    {
        switch (mode)
        {
            case SortMode.Name:
                this.NameDescending = descending;
                break;
            case SortMode.Birthday:
                this.BirthdayDescending = descending;
                break;
            default:
                this.FriendshipDescending = descending;
                break;
        }
    }

    /// <summary>翻转该方式的方向。</summary>
    public void FlipDescending(SortMode mode)
    {
        this.SetDescending(mode, !this.GetDescending(mode));
    }

    /// <summary>该方式被「点击名称」选中时使用的默认方向：好感降序、名字升序、生日升序。</summary>
    public static bool DefaultDescending(SortMode mode)
    {
        return mode == SortMode.Friendship;
    }
}
