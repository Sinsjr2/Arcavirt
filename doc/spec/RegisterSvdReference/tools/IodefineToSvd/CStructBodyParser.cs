namespace IodefineToSvd;

public static class CStructBodyParser {
    static readonly HashSet<string> TypeKeywords = ["unsigned", "char", "short", "long", "int"];

    public static IReadOnlyList<CStructMember> Parse(IReadOnlyList<CToken> tokens) {
        var members = new List<CStructMember>();
        var pos = 0;
        while (pos < tokens.Count) {
            members.Add(ParseMember(tokens, ref pos));
        }
        return members;
    }

    static CStructMember ParseMember(IReadOnlyList<CToken> tokens, ref int pos) {
        return tokens[pos].Text == "union"
            ? ParseUnionMember(tokens, ref pos)
            : ParsePlainMember(tokens, ref pos);
    }

    static CStructMember ParseUnionMember(IReadOnlyList<CToken> tokens, ref int pos) {
        Expect(tokens, ref pos, "union");
        Expect(tokens, ref pos, "{");

        var sizeType = ParseTypeName(tokens, ref pos);
        ExpectIdentifier(tokens, ref pos);
        Expect(tokens, ref pos, ";");

        // RX64MのICU(st_icu)のPIBR0等では、ビットフィールドを説明するstruct部分が
        // 丸ごとコメントアウトされており(該当機能が未実装のため)、
        // "union { unsigned char BYTE; } PIBR0;"のようにstruct自体が存在しない。
        // この場合はフィールド無しのレジスタとして扱う。
        IReadOnlyList<CBitField> fields = [];
        if (tokens[pos].Text == "struct") {
            Expect(tokens, ref pos, "struct");
            Expect(tokens, ref pos, "{");
            fields = ParseBitFieldList(tokens, ref pos);
            Expect(tokens, ref pos, "}");
            ExpectIdentifier(tokens, ref pos);
            Expect(tokens, ref pos, ";");
        }

        Expect(tokens, ref pos, "}");
        var registerName = ExpectIdentifier(tokens, ref pos);

        // RX64MのICU(st_icu)ではIR[256]/DTCER[256]/IER[32]のように、union全体が
        // 配列になっているレジスタ群がある。全要素が同一のビットフィールド構成を
        // 持つ、完全に均一なレジスタ配列であるため、SVDの<dim>で表現する
        // (RegisterLayoutBuilder/SvdDocumentBuilder側で処理)。
        int? arrayCount = null;
        if (tokens[pos].Text == "[") {
            Expect(tokens, ref pos, "[");
            arrayCount = ExpectNumber(tokens, ref pos);
            Expect(tokens, ref pos, "]");
        }
        Expect(tokens, ref pos, ";");

        return new CStructMember(registerName, BaseTypeByteSize(sizeType), IsPadding: false, fields, arrayCount);
    }

    static CStructMember ParsePlainMember(IReadOnlyList<CToken> tokens, ref int pos) {
        var typeName = ParseTypeName(tokens, ref pos);
        var memberName = ExpectIdentifier(tokens, ref pos);

        var elementSize = BaseTypeByteSize(typeName);
        var totalSize = elementSize;
        var isPadding = typeName == "char";

        if (tokens[pos].Text == "[") {
            Expect(tokens, ref pos, "[");
            var count = ExpectNumber(tokens, ref pos);
            Expect(tokens, ref pos, "]");
            totalSize = elementSize * count;
        }
        Expect(tokens, ref pos, ";");

        return new CStructMember(memberName, totalSize, isPadding, Fields: null);
    }

    // RX64Mのst_sci0/st_sci12ではTDRHL/RDRHLのように、ビットフィールドではなく
    // バイト単位のサブメンバー(例: "unsigned char TDRH;")を持つ内部struct
    // ("} BYTE;"で終わる)が現れる。これはSVD上は1レジスタとして扱うだけでよいため、
    // コロンが無いメンバーはフィールドとして記録せず読み飛ばす。
    static IReadOnlyList<CBitField> ParseBitFieldList(IReadOnlyList<CToken> tokens, ref int pos) {
        var fields = new List<CBitField>();
        var bitOffset = 0;
        while (tokens[pos].Text != "}") {
            ParseTypeName(tokens, ref pos);
            string? name = null;
            if (tokens[pos].Kind == CTokenKind.Identifier) {
                name = ExpectIdentifier(tokens, ref pos);
            }

            if (tokens[pos].Text == ":") {
                Expect(tokens, ref pos, ":");
                var width = ExpectNumber(tokens, ref pos);
                Expect(tokens, ref pos, ";");

                if (name is not null) {
                    fields.Add(new CBitField(name, bitOffset, width));
                }
                bitOffset += width;
            } else {
                Expect(tokens, ref pos, ";");
            }
        }
        return fields;
    }

    static string ParseTypeName(IReadOnlyList<CToken> tokens, ref int pos) {
        var parts = new List<string>();
        while (tokens[pos].Kind == CTokenKind.Identifier && TypeKeywords.Contains(tokens[pos].Text)) {
            parts.Add(tokens[pos].Text);
            pos++;
        }
        if (parts.Count == 0) {
            throw new InvalidOperationException($"型名を期待しましたが '{tokens[pos].Text}' でした(位置 {pos})。");
        }
        return string.Join(' ', parts);
    }

    static int BaseTypeByteSize(string typeName) {
        return typeName switch {
            "char" => 1,
            "unsigned char" => 1,
            "unsigned short" => 2,
            "unsigned long" => 4,
            _ => throw new InvalidOperationException($"未対応の型です: {typeName}"),
        };
    }

    static void Expect(IReadOnlyList<CToken> tokens, ref int pos, string text) {
        if (tokens[pos].Text != text) {
            throw new InvalidOperationException($"'{text}' を期待しましたが '{tokens[pos].Text}' でした(位置 {pos})。");
        }
        pos++;
    }

    static string ExpectIdentifier(IReadOnlyList<CToken> tokens, ref int pos) {
        if (tokens[pos].Kind != CTokenKind.Identifier) {
            throw new InvalidOperationException($"識別子を期待しましたが '{tokens[pos].Text}' でした(位置 {pos})。");
        }
        var text = tokens[pos].Text;
        pos++;
        return text;
    }

    static int ExpectNumber(IReadOnlyList<CToken> tokens, ref int pos) {
        if (tokens[pos].Kind != CTokenKind.Number) {
            throw new InvalidOperationException($"数値を期待しましたが '{tokens[pos].Text}' でした(位置 {pos})。");
        }
        var text = tokens[pos].Text;
        pos++;
        return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? Convert.ToInt32(text, 16)
            : int.Parse(text);
    }
}