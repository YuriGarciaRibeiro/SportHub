using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Extensions.Validation;

public static class PasswordValidatorExtensions
{
    public sealed class PasswordPolicyOptions
    {
        public int MinimumLength { get; init; } = 12;
        public int RequiredCategories { get; init; } = 3;
        public bool DisallowWhitespace { get; init; } = true;
        public int MaxRepeatedRun { get; init; } = 3;
        public int MinSequentialLengthToReject { get; init; } = 4;
        public bool CheckCommonPasswords { get; init; } = true;
        public ISet<string> CommonPasswords { get; init; } = DefaultCommonPasswords;
    }

    private static readonly HashSet<string> DefaultCommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "123456","123456789","12345678","12345","111111","qwerty","password",
        "123123","abc123","iloveyou","admin","welcome","senha","000000",
        "qwertyuiop","letmein","monkey","dragon","passw0rd","qazwsx"
    };

    private static readonly string[] KeyboardRows =
    {
        "qwertyuiop","asdfghjkl","zxcvbnm","1234567890"
    };

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<T, IEnumerable<string>>? forbiddenSubstrings = null,
        PasswordPolicyOptions? policy = null)
    {
        policy ??= new PasswordPolicyOptions();

        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(policy.MinimumLength).WithMessage($"Password must be at least {policy.MinimumLength} characters long.")
            .Must(p => HasRequiredCategories(p, policy.RequiredCategories))
                .WithMessage($"Password must contain at least {policy.RequiredCategories} of the following: uppercase, lowercase, digit, special character.")
            .Must(p => !policy.DisallowWhitespace || !HasWhitespace(p))
                .WithMessage("Password must not contain whitespace.")
            .Must(p => !HasRepeatedRun(p, policy.MaxRepeatedRun))
                .WithMessage($"Password must not contain {policy.MaxRepeatedRun} or more identical characters in a row.")
            .Must(p => !HasSequentialPatterns(p, policy.MinSequentialLengthToReject))
                .WithMessage($"Password must not contain sequential patterns of length {policy.MinSequentialLengthToReject} or more (e.g., 'abcd', '4321', 'qwer').")
            .Must(p => !policy.CheckCommonPasswords || !IsCommonPassword(p, policy.CommonPasswords))
                .WithMessage("Password is too common and easily guessable.")
            .Must((instance, p) => !ContainsForbidden(p, forbiddenSubstrings?.Invoke(instance)))
                .WithMessage("Password must not contain personal information (e.g., name or email parts).");
    }


    private static bool HasRequiredCategories(string? s, int required)
    {
        if (string.IsNullOrEmpty(s)) return false;
        bool hasUpper = s.Any(char.IsUpper);
        bool hasLower = s.Any(char.IsLower);
        bool hasDigit = s.Any(char.IsDigit);
        bool hasSymbol = s.Any(c => !char.IsLetterOrDigit(c));
        int count = (hasUpper?1:0) + (hasLower?1:0) + (hasDigit?1:0) + (hasSymbol?1:0);
        return count >= required;
    }

    private static bool HasWhitespace(string? s) => s?.Any(char.IsWhiteSpace) == true;

    private static bool HasRepeatedRun(string? s, int maxRun)
    {
        if (string.IsNullOrEmpty(s) || maxRun <= 1) return false;
        int run = 1;
        for (int i = 1; i < s.Length; i++)
        {
            run = (s[i] == s[i-1]) ? run + 1 : 1;
            if (run >= maxRun) return true;
        }
        return false;
    }

    private static bool HasSequentialPatterns(string? s, int minLen)
    {
        if (string.IsNullOrEmpty(s) || minLen <= 2) return false;
        string lower = s.ToLowerInvariant();

        // alpha & digit sequences ascending/descending
        if (HasAlphabeticOrNumericSequence(lower, minLen)) return true;

        // keyboard rows (e.g., qwerty)
        foreach (var row in KeyboardRows)
        {
            if (ContainsSequence(lower, row, minLen) || ContainsSequence(lower, Reverse(row), minLen))
                return true;
        }
        return false;
    }

    private static bool HasAlphabeticOrNumericSequence(string s, int minLen)
    {
        int ascRun = 1, descRun = 1;
        for (int i = 1; i < s.Length; i++)
        {
            char prev = s[i - 1], cur = s[i];
            bool bothLetters = char.IsLetter(prev) && char.IsLetter(cur);
            bool bothDigits  = char.IsDigit(prev)  && char.IsDigit(cur);

            if (bothLetters || bothDigits)
            {
                if (cur == prev + 1) { ascRun++; descRun = 1; }
                else if (cur == prev - 1) { descRun++; ascRun = 1; }
                else { ascRun = descRun = 1; }

                if (ascRun >= minLen || descRun >= minLen) return true;
            }
            else
            {
                ascRun = descRun = 1;
            }
        }
        return false;
    }

    private static bool ContainsSequence(string haystack, string needleRow, int minLen)
    {
        // check any contiguous window of length >= minLen present in needleRow
        for (int len = minLen; len <= needleRow.Length; len++)
        {
            for (int start = 0; start + len <= needleRow.Length; start++)
            {
                string seg = needleRow.Substring(start, len);
                if (haystack.Contains(seg)) return true;
            }
        }
        return false;
    }

    private static string Reverse(string s)
    {
        var arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }

    private static bool IsCommonPassword(string? s, ISet<string> list)
        => !string.IsNullOrEmpty(s) && list.Contains(s);

    private static bool ContainsForbidden(string? password, IEnumerable<string>? forbidden)
    {
        if (string.IsNullOrWhiteSpace(password) || forbidden is null) return false;
        string p = password.ToLowerInvariant();
        foreach (var token in forbidden.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var t = token!.Trim().ToLowerInvariant();
            if (t.Length >= 3 && p.Contains(t)) // ignora tokens muito curtos para evitar falsos positivos
                return true;
        }
        return false;
    }

    // (Opcional) manter sua regra antiga para compatibilidade
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder.StrongPassword(); // delega para a versão robusta
}
