#!/usr/bin/env python3
"""RX64Mデータシート(pdftotext -layout抽出済みテキスト)から、レジスタ節ごとに
リセット値(ビットごとの0/1/x)とビット単位のアクセス権限・Undefined注記を
構造化して抽出する。

使い方:
    pdftotext -layout -f <開始ページ> -l <終了ページ> datasheet.pdf chapter.txt
    python3 extract_registers.py chapter.txt

前提: 章のテキストには "N.N.N   レジスタ名 (略号)" 形式の節見出し、
"Address(es): ..." 行、"bNN ... b0" 形式のビット位置ヘッダーと直後の
"Value after reset: ..." 行、"Bit  Symbol  Bit Name  Description  R/W" 形式の
ビット表が含まれること(RX64M/RX111マニュアルの標準的なレジスタ節の形式)。

出力はレジスタごとのJSON(1行1レジスタ)。ResetValue/ResetMask/Accessは
IodefineToSvdのRegisterDatasheetMetadataと同じ規約(read-writeへの集約・
部分マスク・null)で機械的に算出した「たたき台」であり、人間またはサブ
エージェントによる最終確認を代替するものではない。
"""
import json
import re
import sys

SECTION_RE = re.compile(r'^\s*(\d+\.\d+\.\d+)\s+(.+\S)\s*$')
ADDRESS_RE = re.compile(r'^\s*Address\(es\):\s*(.+)$')
BIT_HEADER_RE = re.compile(r'\bb(\d+)\b')
RESET_ROW_RE = re.compile(r'^\s*Value after reset:\s*(.*)$')
BIT_ROW_RE = re.compile(r'^\s*b(\d+)(?:\s+to\s+b(\d+))?\s+(.*)$')
ACCESS_TOKEN_RE = re.compile(r'^R\(?/?\(?W?\)?\d*$|^W\(?/?\(?R?\)?\d*$|^R$|^W$')


def parse_bit_header_and_value(header_line, value_line):
    """ヘッダー行の"bNN"の出現列位置を基準に、Value after reset行の同じ列位置の
    文字(0/1/x)を読み取る。列位置は次のビットラベルの開始位置までを1ビット分の
    窓とする(シンボル名の長さで列がずれても、ラベル自体の位置は安定するため)。"""
    positions = [(int(m.group(1)), m.start()) for m in BIT_HEADER_RE.finditer(header_line)]
    bits = {}
    for k, (bit_no, col) in enumerate(positions):
        left = col
        right = positions[k + 1][1] if k + 1 < len(positions) else len(value_line)
        window = value_line[left:right]
        ch = None
        for c in window:
            if c in '01x':
                ch = c
                break
        bits[bit_no] = ch
    return bits


def find_reset_blocks(lines, start, end):
    """[start, end)の範囲を"Address(es):"行で区切ったインスタンスグループごとに、
    (ヘッダー行, 値行)の組を集めてビット値を統合する。CS0CRのように同一節内で
    グループごとにリセット値が異なるケース(BSCで実際に発生)を区別するため、
    ビットを1つの辞書にマージせず、グループのリストとして返す。
    32bitレジスタのb31-b16/b15-b0の2ブロックは同一グループ内でマージされる。"""
    groups = []  # list of {"addresses": [...], "bits": {...}}
    current = None
    i = start
    while i < end:
        addr_m = ADDRESS_RE.match(lines[i])
        if addr_m:
            current = {'addresses': [addr_m.group(1).strip()], 'bits': {}}
            groups.append(current)
            i += 1
            continue
        reset_m = RESET_ROW_RE.match(lines[i])
        if reset_m:
            header_line = None
            for j in range(i - 1, max(start, i - 6), -1):
                if BIT_HEADER_RE.search(lines[j]) and lines[j].strip().startswith('b'):
                    header_line = lines[j]
                    break
            if header_line is not None:
                bits = parse_bit_header_and_value(header_line, lines[i])
                if current is None:
                    current = {'addresses': [], 'bits': {}}
                    groups.append(current)
                current['bits'].update(bits)
        i += 1
    return groups


BIT_TABLE_HEADER_RE = re.compile(r'\bSymbol\b.*\bR/W\b|\bBit Name\b')


def find_bit_table(lines, start, end):
    """"b0 SYMBOL BitName ... R/W" 形式のビット表を解析し、ビットごとの
    (symbol, access, undefined) を返す。ビット位置ヘッダー行("b15 b14 ...")を
    誤って表の行と認識しないよう、"Bit  Symbol  Bit Name  ...  R/W"という
    表の見出し行より後だけを走査対象にする。"""
    table_start = None
    for i in range(start, end):
        if BIT_TABLE_HEADER_RE.search(lines[i]):
            table_start = i + 1
            break
    if table_start is None:
        return []

    fields = []
    i = table_start
    while i < end:
        m = BIT_ROW_RE.match(lines[i])
        if m:
            lo = int(m.group(1))
            hi = int(m.group(2)) if m.group(2) else lo
            rest = m.group(3)
            tokens = re.split(r'\s{2,}', rest.strip())
            access = None
            for t in reversed(tokens):
                t = t.strip()
                if ACCESS_TOKEN_RE.match(t):
                    access = t
                    break
            symbol = tokens[0] if tokens else ''
            desc_lines = [rest]
            j = i + 1
            while j < end and not BIT_ROW_RE.match(lines[j]) and lines[j].strip() != '':
                desc_lines.append(lines[j])
                j += 1
            desc_text = ' '.join(desc_lines)
            undefined = bool(re.search(r'\bundefined\b', desc_text, re.IGNORECASE))
            fields.append({
                'bit_lo': min(lo, hi), 'bit_hi': max(lo, hi),
                'symbol': symbol, 'access': access, 'undefined': undefined,
            })
            i = j
        else:
            i += 1
    return fields


def derive_metadata(bits, fields, width):
    """bits(位置->0/1/x)とfields(ビット範囲ごとのaccess/undefined)から、
    ResetValue/ResetMask/Accessのたたき台を算出する。"""
    undefined_bits = set()
    for f in fields:
        if f['undefined']:
            for b in range(f['bit_lo'], f['bit_hi'] + 1):
                undefined_bits.add(b)
    for b, v in bits.items():
        if v == 'x':
            undefined_bits.add(b)

    reset_value = 0
    reset_mask = 0
    for b in range(width):
        v = bits.get(b)
        if b not in undefined_bits and v in ('0', '1'):
            reset_mask |= (1 << b)
            if v == '1':
                reset_value |= (1 << b)

    has_write = any(f['access'] and 'W' in f['access'] for f in fields)
    has_read = any(f['access'] and 'R' in f['access'] for f in fields)
    if has_write and has_read:
        access = 'read-write'
    elif has_write:
        access = 'write-only'
    elif has_read:
        access = 'read-only'
    else:
        access = None

    return {
        'reset_value_candidate': f'0x{reset_value:0{width // 4}X}',
        'reset_mask_candidate': f'0x{reset_mask:0{width // 4}X}',
        'access_candidate': access,
        'undefined_bits': sorted(undefined_bits),
    }


def main():
    path = sys.argv[1]
    with open(path, encoding='utf-8') as f:
        lines = f.readlines()

    section_starts = []
    for i, line in enumerate(lines):
        m = SECTION_RE.match(line)
        if m:
            section_starts.append((i, m.group(1), m.group(2)))

    results = []
    for idx, (start, number, title) in enumerate(section_starts):
        end = section_starts[idx + 1][0] if idx + 1 < len(section_starts) else len(lines)

        groups = find_reset_blocks(lines, start, end)
        fields = find_bit_table(lines, start, end)
        if not groups and not fields:
            continue

        all_bits = set()
        for g in groups:
            all_bits |= set(g['bits'])
        width = 32 if any(b >= 16 for b in all_bits) else 16 if any(b >= 8 for b in all_bits) else 8

        group_results = []
        for g in groups:
            derived = derive_metadata(g['bits'], fields, width)
            group_results.append({
                'addresses': g['addresses'],
                'bits': {str(k): v for k, v in sorted(g['bits'].items())},
                **derived,
            })

        results.append({
            'section': number,
            'title': title,
            'width': width,
            'fields': fields,
            'instance_groups': group_results,
        })

    for r in results:
        print(json.dumps(r, ensure_ascii=False))


if __name__ == '__main__':
    main()
