namespace IodefineToSvd;

public static class CStructBodyParser {
    static readonly HashSet<string> TypeKeywords = ["unsigned", "char", "short", "long", "int", "void"];

    // RXは32bitアーキテクチャのため、ポインタ型メンバーは指す先の型によらず常に4byte。
    const int PointerByteSize = 4;

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
        var firstMemberName = ExpectIdentifier(tokens, ref pos);
        Expect(tokens, ref pos, ";");

        // RX64MのICU(st_icu)のPIBR0等では、ビットフィールドを説明するstruct部分が
        // 丸ごとコメントアウトされており(該当機能が未実装のため)、
        // "union { unsigned char BYTE; } PIBR0;"のようにstruct自体が存在しない。
        // この場合はフィールド無しのレジスタとして扱う。
        IReadOnlyList<CBitField> fields = [];
        var plainAliasNames = new List<string>();
        if (tokens[pos].Text == "struct") {
            Expect(tokens, ref pos, "struct");
            Expect(tokens, ref pos, "{");
            fields = ParseBitFieldList(tokens, ref pos);
            Expect(tokens, ref pos, "}");
            ExpectIdentifier(tokens, ref pos);
            Expect(tokens, ref pos, ";");
        } else {
            // RX64MのSCIFA(st_scifa)のBRR/MDDRのように、ビットフィールドを持たない
            // プレーンなメンバーが複数並ぶ無名unionのケース(同一オフセットの別名レジスタ)。
            // struct部分が無く、かつ後続にさらに"型 識別子;"が続く場合はここに来る。
            while (tokens[pos].Text != "}") {
                var aliasType = ParseTypeName(tokens, ref pos);
                if (aliasType != sizeType) {
                    throw new InvalidOperationException($"エイリアスunionのメンバー型が不一致です: {sizeType} と {aliasType}");
                }
                var aliasName = ExpectIdentifier(tokens, ref pos);
                Expect(tokens, ref pos, ";");
                plainAliasNames.Add(aliasName);
            }
        }

        Expect(tokens, ref pos, "}");

        if (plainAliasNames.Count > 0) {
            // 無名union(エイリアス表現)は閉じ括弧の直後にレジスタ名を持たない。
            Expect(tokens, ref pos, ";");
            return new CStructMember(firstMemberName, BaseTypeByteSize(sizeType), IsPadding: false, Fields: [], ArrayCount: null, AliasNames: plainAliasNames);
        }

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

        // RX64MのDMAC0/DTC/EDMAC/EXDMAC0/EXDMAC1(DMSAR/DTCVBR/TDLAR/EDMSAR等)では、
        // 転送元・転送先アドレスを保持するレジスタが"void *NAME;"というポインタ型
        // メンバーとして宣言されている。ポインタはRX(32bitアーキテクチャ)では常に
        // 4byte固定であり、指す先の型(void)のサイズとは無関係のため、素の型サイズ
        // ではなくポインタとして扱う。
        var isPointer = tokens[pos].Text == "*";
        if (isPointer) {
            Expect(tokens, ref pos, "*");
        }

        var memberName = ExpectIdentifier(tokens, ref pos);

        var elementSize = isPointer ? PointerByteSize : BaseTypeByteSize(typeName);
        var isPadding = typeName == "char" && !isPointer;

        // RX64MのSYSTEM(st_system)のDPSBKR[32]のように、ビットフィールドを
        // 持たない素の配列メンバーが、各要素が独立した1byteレジスタである
        // ケースがある。ByteSizeは常に1要素分のサイズとし、配列の場合は
        // ArrayCountを設定する(union内配列と同じ表現、
        // RegisterLayoutBuilder/SvdDocumentBuilder側で共通処理される)。
        int? arrayCount = null;
        if (tokens[pos].Text == "[") {
            Expect(tokens, ref pos, "[");
            arrayCount = ExpectNumber(tokens, ref pos);
            Expect(tokens, ref pos, "]");
        }
        Expect(tokens, ref pos, ";");

        return new CStructMember(memberName, elementSize, isPadding, Fields: null, arrayCount);
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