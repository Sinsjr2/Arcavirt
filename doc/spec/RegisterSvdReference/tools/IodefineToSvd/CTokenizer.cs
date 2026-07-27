namespace IodefineToSvd;

public enum CTokenKind {
    Identifier,
    Number,
    Symbol,
}

public readonly record struct CToken(CTokenKind Kind, string Text);

public static class CTokenizer {
    public static IReadOnlyList<CToken> Tokenize(string text) {
        var tokens = new List<CToken>();
        var i = 0;
        while (i < text.Length) {
            var c = text[i];
            if (char.IsWhiteSpace(c)) {
                i++;
                continue;
            }
            if (c == '#') {
                while (i < text.Length && text[i] != '\n') {
                    i++;
                }
                continue;
            }
            if (char.IsLetter(c) || c == '_') {
                var start = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) {
                    i++;
                }
                tokens.Add(new CToken(CTokenKind.Identifier, text[start..i]));
                continue;
            }
            if (char.IsDigit(c)) {
                var start = i;
                while (i < text.Length && char.IsLetterOrDigit(text[i])) {
                    i++;
                }
                tokens.Add(new CToken(CTokenKind.Number, text[start..i]));
                continue;
            }
            if ("{}();:*[]".IndexOf(c) >= 0) {
                tokens.Add(new CToken(CTokenKind.Symbol, c.ToString()));
                i++;
                continue;
            }
            throw new InvalidOperationException($"未知の文字です: '{c}' (位置 {i})");
        }
        return tokens;
    }
}