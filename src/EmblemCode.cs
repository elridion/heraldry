namespace Heraldry
{
    /// <summary>
    /// A banner's emblem as players type it: a symbol id, e.g. "42". The normalised text is what goes
    /// into the ZDO, so it stays readable to anyone who inspects it.
    /// </summary>
    internal readonly struct EmblemCode
    {
        public const int None = -1;

        public readonly int Symbol;

        public EmblemCode(int symbol)
        {
            Symbol = symbol;
        }

        public bool IsEmpty => Symbol == None;

        // Empty text is a valid "no emblem". Anything else needs a known symbol.
        public static bool TryParse(string text, out EmblemCode code)
        {
            code = new EmblemCode(None);
            string trimmed = (text ?? "").Trim();
            if (trimmed.Length == 0) return true;
            if (!int.TryParse(trimmed, out int symbol) || !EmblemLibrary.HasSymbol(symbol)) return false;

            code = new EmblemCode(symbol);
            return true;
        }

        public override string ToString() => IsEmpty ? "" : Symbol.ToString();
    }
}
