namespace PassingCar.Behaviors;

public class PasswordVaildBehavior : Behavior<Entry>
{
    public static readonly BindableProperty IsValidProperty =
        BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(PasswordVaildBehavior), false);

    public bool IsValid
    {
        get { return (bool)GetValue(IsValidProperty); }
        private set { SetValue(IsValidProperty, value); }
    }

    public int MinimumLength { get; set; } = 8;
    public int MaximumLength { get; set; } = 40;
    public bool RequiresSpecialCharacter { get; set; } = true;
    public bool RequiresDigit { get; set; } = true;
    public bool RequiresUppercase { get; set; } = true;

    protected override void OnAttachedTo(Entry bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.TextChanged += OnTextChanged;
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.TextChanged -= OnTextChanged;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var password = e.NewTextValue;
        IsValid = ValidatePassword(password);
    }

    private bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) ||
            password.Length < MinimumLength ||
            password.Length > MaximumLength)
            return false;

        if (RequiresSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            return false;

        if (RequiresDigit && !password.Any(char.IsDigit))
            return false;

        if (RequiresUppercase && !password.Any(char.IsUpper))
            return false;

        return true;
    }
}

